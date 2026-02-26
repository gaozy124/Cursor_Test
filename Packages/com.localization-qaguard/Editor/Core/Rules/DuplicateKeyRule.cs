using System.Collections.Generic;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Rules;

public sealed class DuplicateKeyRule : ICheckRule
{
    public const string RuleName = "LQG001";

    public string Id => RuleName;
    public string Description => "Duplicate localization keys in shared table data.";

    public bool IsEnabled(ScanContext context) => context.CheckDuplicateKeys;

    public IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        foreach (var collection in snapshot.Collections)
        {
            foreach (var duplicate in collection.DuplicateKeys)
            {
                yield return new ScanIssue
                {
                    RuleId = Id,
                    Severity = ScanSeverity.Error,
                    CollectionName = collection.Name,
                    Key = duplicate.Key,
                    LocaleCode = "-",
                    AssetPath = collection.SharedDataPath,
                    Message = $"Duplicate key '{duplicate.Key}' has {duplicate.Ids.Count} entries in shared data.",
                    SuggestedFix = "Rename or merge duplicate keys so every key is unique."
                };
            }
        }
    }
}

