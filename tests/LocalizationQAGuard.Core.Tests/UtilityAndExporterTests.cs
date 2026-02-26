using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;
using Xunit;

namespace LocalizationQAGuard.Core.Tests;

public sealed class UtilityAndExporterTests
{
    [Fact]
    public void PlaceholderParser_ExtractsNamedAndIndexedPlaceholders()
    {
        var placeholders = PlaceholderParser.Extract("Hi {name}, score {0}, literal {{ignored}}");

        Assert.Equal(2, placeholders.Count);
        Assert.Contains("name", placeholders);
        Assert.Contains("0", placeholders);
    }

    [Theory]
    [InlineData("Price {0}", true)]
    [InlineData("{{Hello}}", true)]
    [InlineData("Broken {0", false)]
    [InlineData("Broken }", false)]
    public void PlaceholderParser_DetectsBalancedBraces(string text, bool expected)
    {
        Assert.Equal(expected, PlaceholderParser.HasBalancedBraces(text));
    }

    [Fact]
    public void CsvAndJsonExporter_WriteExpectedOutputs()
    {
        var report = new ScanReport
        {
            Issues =
            [
                new ScanIssue
                {
                    RuleId = "LQG001",
                    Severity = ScanSeverity.Error,
                    CollectionName = "UI",
                    Key = "shop.buy",
                    LocaleCode = "fr",
                    Message = "Placeholder mismatch, expected {amount}",
                    SuggestedFix = "Align placeholders",
                    AssetPath = "Assets/Localization/UI Shared.asset"
                }
            ]
        };

        var basePath = Path.Combine(Path.GetTempPath(), $"lqg-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(basePath);
        var csvPath = Path.Combine(basePath, "report.csv");
        var jsonPath = Path.Combine(basePath, "report.json");

        CsvReportExporter.Export(csvPath, report);
        JsonReportExporter.Export(jsonPath, report);

        var csv = File.ReadAllText(csvPath);
        var json = File.ReadAllText(jsonPath);

        Assert.Contains("LQG001", csv);
        Assert.Contains("shop.buy", csv);
        Assert.Contains("\"RuleId\": \"LQG001\"", json);
    }
}

