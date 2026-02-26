using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;
using Xunit;

namespace LocalizationQAGuard.Core.Tests;

public sealed class ExitCodePolicyTests
{
    [Theory]
    [InlineData("none", 0)]
    [InlineData("error", 2)]
    [InlineData("warning", 2)]
    [InlineData("unknown", 0)]
    public void Compute_WithErrorReport_RespectsFailOn(string failOn, int expected)
    {
        var report = new ScanReport
        {
            Issues =
            [
                new ScanIssue { RuleId = "LQG002", Severity = ScanSeverity.Error }
            ]
        };

        var exitCode = ExitCodePolicy.Compute(report, failOn);
        Assert.Equal(expected, exitCode);
    }

    [Theory]
    [InlineData("none", 0)]
    [InlineData("error", 0)]
    [InlineData("warning", 2)]
    public void Compute_WithWarningOnlyReport_RespectsFailOn(string failOn, int expected)
    {
        var report = new ScanReport
        {
            Issues =
            [
                new ScanIssue { RuleId = "LQG003", Severity = ScanSeverity.Warning }
            ]
        };

        var exitCode = ExitCodePolicy.Compute(report, failOn);
        Assert.Equal(expected, exitCode);
    }
}

