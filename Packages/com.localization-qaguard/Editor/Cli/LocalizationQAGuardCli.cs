using System;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;
using UnityEditor;
using UnityEngine;

namespace LocalizationQAGuard.Editor.Cli;

public static class LocalizationQAGuardCli
{
    private const string OutputArg = "--lqg-output=";
    private const string FormatArg = "--lqg-format=";
    private const string FailOnArg = "--lqg-fail-on=";
    private const string CheckUnusedArg = "--lqg-check-unused=";
    private const string ReferenceLocaleArg = "--lqg-reference-locale=";

    [MenuItem("Tools/Localization QA Guard/Run CLI Scan")]
    public static void RunInteractive()
    {
        Run(exitEditor: false);
    }

    public static void RunFromCommandLine()
    {
        Run(exitEditor: true);
    }

    private static void Run(bool exitEditor)
    {
        try
        {
            var args = Environment.GetCommandLineArgs();
            var context = BuildContextFromArgs(args);
            var service = new LocalizationScanService();
            var (_, report) = service.Run(context);

            var outputPath = GetArgValue(args, OutputArg);
            var format = (GetArgValue(args, FormatArg) ?? "json").Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                if (format == "csv")
                {
                    CsvReportExporter.Export(outputPath!, report);
                }
                else
                {
                    JsonReportExporter.Export(outputPath!, report);
                }
            }

            Debug.Log($"[Localization QA Guard] Scan complete. Errors={report.ErrorCount}, Warnings={report.WarningCount}, Info={report.InfoCount}");

            var exitCode = ExitCodePolicy.Compute(report, GetArgValue(args, FailOnArg));
            if (exitEditor)
            {
                EditorApplication.Exit(exitCode);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Localization QA Guard] CLI run failed: {ex}");
            if (exitEditor)
            {
                EditorApplication.Exit(99);
            }
        }
    }

    private static ScanContext BuildContextFromArgs(string[] args)
    {
        var context = new ScanContext();

        var referenceLocale = GetArgValue(args, ReferenceLocaleArg);
        if (!string.IsNullOrWhiteSpace(referenceLocale))
        {
            context.ReferenceLocaleCode = referenceLocale!;
        }

        var checkUnused = GetArgValue(args, CheckUnusedArg);
        if (!string.IsNullOrWhiteSpace(checkUnused) && bool.TryParse(checkUnused, out var parsedCheckUnused))
        {
            context.CheckUnusedKeys = parsedCheckUnused;
            context.ScanTextReferencesInProject = parsedCheckUnused;
        }

        return context;
    }

    private static string? GetArgValue(string[] args, string prefix)
    {
        return args.FirstOrDefault(arg => arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            ?.Substring(prefix.Length);
    }
}

