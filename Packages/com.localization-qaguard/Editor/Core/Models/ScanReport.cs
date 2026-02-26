using System.Collections.Generic;
using System.Linq;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class ScanReport
{
    public IReadOnlyList<ScanIssue> Issues { get; init; } = new List<ScanIssue>();

    public int ErrorCount => Issues.Count(x => x.Severity == ScanSeverity.Error);
    public int WarningCount => Issues.Count(x => x.Severity == ScanSeverity.Warning);
    public int InfoCount => Issues.Count(x => x.Severity == ScanSeverity.Info);
}

