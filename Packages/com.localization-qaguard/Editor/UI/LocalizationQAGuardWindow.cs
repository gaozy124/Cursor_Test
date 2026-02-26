using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;
using LocalizationQAGuard.Editor.Unity;
using UnityEditor;
using UnityEngine;

namespace LocalizationQAGuard.Editor.UI;

public sealed class LocalizationQAGuardWindow : EditorWindow
{
    private readonly LocalizationScanService _scanService = new();
    private readonly LocalizationSafeFixService _safeFixService = new();
    private ScanContext _context = new();
    private ScanReport _lastReport = new();
    private LocalizationProjectSnapshot _lastSnapshot = new();
    private Vector2 _scrollPosition;
    private string _searchText = string.Empty;
    private int _severityFilterIndex;
    private bool _isScanning;
    private bool _fixDryRun = true;

    private static readonly string[] SeverityFilters = { "All", "Error", "Warning", "Info" };

    [MenuItem("Tools/Localization QA Guard/Open")]
    public static void Open()
    {
        var window = GetWindow<LocalizationQAGuardWindow>("Localization QA Guard");
        window.minSize = new Vector2(980, 560);
        window.Show();
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawSettings();
        DrawActions();
        DrawSummary();
        DrawIssueList();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Localization QA Guard", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Validate localization quality before release.", EditorStyles.miniLabel);
    }

    private void DrawSettings()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Scan Settings", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _context.CheckMissingTranslations = EditorGUILayout.ToggleLeft("Check missing translations", _context.CheckMissingTranslations);
            _context.CheckEmptyTranslations = EditorGUILayout.ToggleLeft("Check empty translations", _context.CheckEmptyTranslations);
            _context.CheckDuplicateKeys = EditorGUILayout.ToggleLeft("Check duplicate keys", _context.CheckDuplicateKeys);
            _context.CheckPlaceholderConsistency = EditorGUILayout.ToggleLeft("Check placeholder consistency", _context.CheckPlaceholderConsistency);
            _context.CheckSmartStringSyntax = EditorGUILayout.ToggleLeft("Check smart string syntax", _context.CheckSmartStringSyntax);
            _context.CheckUnusedKeys = EditorGUILayout.ToggleLeft("Check unused keys", _context.CheckUnusedKeys);
            _context.ScanTextReferencesInProject = EditorGUILayout.ToggleLeft("Scan project text references (for unused key rule)", _context.ScanTextReferencesInProject);
            _context.ReferenceLocaleCode = EditorGUILayout.TextField(new GUIContent("Reference locale", "Leave empty to auto-select first locale per collection."), _context.ReferenceLocaleCode);
        }
    }

    private void DrawActions()
    {
        EditorGUILayout.Space(6);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginDisabledGroup(_isScanning);
            if (GUILayout.Button("Run Scan", GUILayout.Height(28)))
            {
                RunScan();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_lastReport.Issues.Count == 0);
            if (GUILayout.Button("Export JSON", GUILayout.Height(28)))
            {
                ExportJson();
            }

            if (GUILayout.Button("Export CSV", GUILayout.Height(28)))
            {
                ExportCsv();
            }
            EditorGUI.EndDisabledGroup();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            _fixDryRun = EditorGUILayout.ToggleLeft("Dry run safe fix", _fixDryRun, GUILayout.Width(140));
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(_lastSnapshot.Collections.Count == 0 || _isScanning);
            if (GUILayout.Button("Apply Safe Fixes (Empty → Reference)", GUILayout.Height(24), GUILayout.Width(280)))
            {
                ApplySafeFixes();
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    private void DrawSummary()
    {
        EditorGUILayout.Space(6);
        using (new EditorGUILayout.HorizontalScope("box"))
        {
            EditorGUILayout.LabelField($"Collections: {_lastSnapshot.Collections.Count}", GUILayout.Width(140));
            EditorGUILayout.LabelField($"Issues: {_lastReport.Issues.Count}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"Errors: {_lastReport.ErrorCount}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"Warnings: {_lastReport.WarningCount}", GUILayout.Width(110));
            EditorGUILayout.LabelField($"Info: {_lastReport.InfoCount}", GUILayout.Width(80));
            GUILayout.FlexibleSpace();
        }

        if (_lastReport.Issues.Count > 0)
        {
            var generatedAt = _lastReport.Metadata.GeneratedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            EditorGUILayout.LabelField($"Last Scan: {generatedAt} | Project: {_lastReport.Metadata.ProjectName}", EditorStyles.miniLabel);
        }
    }

    private void DrawIssueList()
    {
        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            _searchText = EditorGUILayout.TextField("Search", _searchText);
            _severityFilterIndex = EditorGUILayout.Popup("Severity", _severityFilterIndex, SeverityFilters, GUILayout.Width(220));
        }

        EditorGUILayout.Space(4);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

        var filteredIssues = GetFilteredIssues();
        if (filteredIssues.Count == 0)
        {
            EditorGUILayout.HelpBox("No issues match current filters.", MessageType.Info);
        }
        else
        {
            foreach (var issue in filteredIssues)
            {
                DrawIssueRow(issue);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawIssueRow(ScanIssue issue)
    {
        var color = issue.Severity switch
        {
            ScanSeverity.Error => new Color(0.56f, 0.18f, 0.18f, 0.18f),
            ScanSeverity.Warning => new Color(0.59f, 0.45f, 0.08f, 0.18f),
            _ => new Color(0.12f, 0.28f, 0.52f, 0.14f)
        };

        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        using (new EditorGUILayout.VerticalScope("box"))
        {
            GUI.backgroundColor = oldColor;
            EditorGUILayout.LabelField($"[{issue.Severity}] {issue.RuleId} | {issue.CollectionName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Key: {issue.Key}   Locale: {issue.LocaleCode}");
            EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Fix: {issue.SuggestedFix}", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrWhiteSpace(issue.AssetPath) && GUILayout.Button("Ping Asset", GUILayout.Width(95)))
                {
                    PingAsset(issue.AssetPath);
                }
            }
        }
    }

    private List<ScanIssue> GetFilteredIssues()
    {
        IEnumerable<ScanIssue> query = _lastReport.Issues;

        if (_severityFilterIndex > 0)
        {
            var selectedSeverity = _severityFilterIndex switch
            {
                1 => ScanSeverity.Error,
                2 => ScanSeverity.Warning,
                _ => ScanSeverity.Info
            };
            query = query.Where(issue => issue.Severity == selectedSeverity);
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            query = query.Where(issue =>
                issue.RuleId.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                issue.CollectionName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                issue.Key.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                issue.LocaleCode.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                issue.Message.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    private void RunScan()
    {
        _isScanning = true;
        try
        {
            var (snapshot, report) = _scanService.Run(_context);
            _lastSnapshot = snapshot;
            _lastReport = report;
            Repaint();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Localization QA Guard] Scan failed: {ex}");
            EditorUtility.DisplayDialog("Localization QA Guard", $"Scan failed:\n{ex.Message}", "OK");
        }
        finally
        {
            _isScanning = false;
        }
    }

    private void ExportJson()
    {
        var path = EditorUtility.SaveFilePanel("Export JSON report", Application.dataPath, "localization-qa-report", "json");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        JsonReportExporter.Export(path, _lastReport);
        Reveal(path);
    }

    private void ExportCsv()
    {
        var path = EditorUtility.SaveFilePanel("Export CSV report", Application.dataPath, "localization-qa-report", "csv");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        CsvReportExporter.Export(path, _lastReport);
        Reveal(path);
    }

    private static void PingAsset(string assetPath)
    {
        var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (obj != null)
        {
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }
    }

    private static void Reveal(string path)
    {
        Debug.Log($"[Localization QA Guard] Report exported: {path}");
        if (File.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }

    private void ApplySafeFixes()
    {
        try
        {
            var fixResult = _safeFixService.ApplyEmptyTranslationFixes(_lastSnapshot, _context, _fixDryRun);
            var title = _fixDryRun ? "Safe Fix Dry Run Complete" : "Safe Fix Complete";
            var body =
                $"Candidates: {fixResult.CandidateCount}\n" +
                $"Applied: {fixResult.AppliedCount}\n" +
                $"Skipped: {fixResult.SkippedCount}";

            EditorUtility.DisplayDialog("Localization QA Guard", body, "OK");
            Debug.Log($"[Localization QA Guard] {title}. {body}");

            if (!_fixDryRun && fixResult.AppliedCount > 0)
            {
                RunScan();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Localization QA Guard] Safe fix failed: {ex}");
            EditorUtility.DisplayDialog("Localization QA Guard", $"Safe fix failed:\n{ex.Message}", "OK");
        }
    }
}

