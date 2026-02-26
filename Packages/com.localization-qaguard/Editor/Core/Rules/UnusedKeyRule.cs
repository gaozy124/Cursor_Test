using System.Collections.Generic;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Rules;

public sealed class UnusedKeyRule : ICheckRule
{
    public const string RuleName = "LQG006";

    public string Id => RuleName;
    public string Description => "Key not found in scanned project references.";

    public bool IsEnabled(ScanContext context) => context.CheckUnusedKeys && context.ScanTextReferencesInProject;

    public IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        foreach (var collection in snapshot.Collections)
        {
            foreach (var entry in collection.Entries)
            {
                if (snapshot.UsedKeys.Contains(entry.Key))
                {
                    continue;
                }

                yield return new ScanIssue
                {
                    RuleId = Id,
                    Severity = ScanSeverity.Info,
                    CollectionName = collection.Name,
                    Key = entry.Key,
                    LocaleCode = "-",
                    AssetPath = collection.SharedDataPath,
                    Message = "Key appears unused in scanned project text references.",
                    SuggestedFix = "Review usage; remove key if confirmed unused."
                };
            }
        }
    }
}

