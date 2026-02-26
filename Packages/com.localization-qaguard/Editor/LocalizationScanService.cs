using System.IO;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;
using LocalizationQAGuard.Editor.Unity;

namespace LocalizationQAGuard.Editor;

public sealed class LocalizationScanService
{
    private readonly LocalizationAssetScanner _scanner = new();
    private readonly RuleEngine _ruleEngine = new();

    public (LocalizationProjectSnapshot Snapshot, ScanReport Report) Run(ScanContext context)
    {
        var snapshot = _scanner.ScanProject(context);
        var baseReport = _ruleEngine.Evaluate(snapshot, context);
        var metadata = new ScanMetadata
        {
            GeneratedAtUtc = System.DateTimeOffset.UtcNow,
            ProjectPath = Directory.GetCurrentDirectory(),
            ProjectName = Path.GetFileName(Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar)),
            CollectionCount = snapshot.Collections.Count,
            EntryCount = snapshot.Collections.Sum(collection => collection.Entries.Count)
        };

        var report = new ScanReport
        {
            Metadata = metadata,
            Issues = baseReport.Issues
        };

        return (snapshot, report);
    }
}

