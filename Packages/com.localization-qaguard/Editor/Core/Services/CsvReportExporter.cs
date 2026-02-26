using System.Collections.Generic;
using System.IO;
using System.Text;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Services;

public static class CsvReportExporter
{
    private static readonly string[] Headers =
    {
        "RuleId",
        "Severity",
        "Collection",
        "Key",
        "Locale",
        "Message",
        "SuggestedFix",
        "AssetPath"
    };

    public static void Export(string outputPath, ScanReport report)
    {
        var lines = new List<string>
        {
            $"# GeneratedAtUtc,{Escape(report.Metadata.GeneratedAtUtc.ToString("O"))}",
            $"# ProjectName,{Escape(report.Metadata.ProjectName)}",
            $"# ProjectPath,{Escape(report.Metadata.ProjectPath)}",
            $"# CollectionCount,{report.Metadata.CollectionCount}",
            $"# EntryCount,{report.Metadata.EntryCount}",
            $"# IssueCount,{report.Issues.Count}",
            $"# ErrorCount,{report.ErrorCount}",
            $"# WarningCount,{report.WarningCount}",
            $"# InfoCount,{report.InfoCount}",
            string.Empty,
            string.Join(",", Headers)
        };

        foreach (var issue in report.Issues)
        {
            lines.Add(string.Join(",",
                Escape(issue.RuleId),
                Escape(issue.Severity.ToString()),
                Escape(issue.CollectionName),
                Escape(issue.Key),
                Escape(issue.LocaleCode),
                Escape(issue.Message),
                Escape(issue.SuggestedFix),
                Escape(issue.AssetPath)));
        }

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    private static string Escape(string? value)
    {
        var sanitized = value ?? string.Empty;
        if (!sanitized.Contains(',') && !sanitized.Contains('"') && !sanitized.Contains('\n'))
        {
            return sanitized;
        }

        return $"\"{sanitized.Replace("\"", "\"\"")}\"";
    }
}

