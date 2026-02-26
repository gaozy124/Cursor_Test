using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Services;

public static class ExitCodePolicy
{
    public static int Compute(ScanReport report, string? failOnArg)
    {
        var failOn = (failOnArg ?? "error").Trim().ToLowerInvariant();
        return failOn switch
        {
            "none" => 0,
            "warning" when report.ErrorCount > 0 || report.WarningCount > 0 => 2,
            "error" when report.ErrorCount > 0 => 2,
            _ => 0
        };
    }
}

