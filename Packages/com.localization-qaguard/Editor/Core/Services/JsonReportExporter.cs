using System.IO;
using System.Text.Json;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Services;

public static class JsonReportExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void Export(string outputPath, ScanReport report)
    {
        var json = JsonSerializer.Serialize(report, Options);
        File.WriteAllText(outputPath, json);
    }
}

