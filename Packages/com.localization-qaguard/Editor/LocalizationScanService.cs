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
        var report = _ruleEngine.Evaluate(snapshot, context);
        return (snapshot, report);
    }
}

