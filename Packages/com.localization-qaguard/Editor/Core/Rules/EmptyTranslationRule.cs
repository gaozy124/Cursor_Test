using System.Collections.Generic;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Rules;

public sealed class EmptyTranslationRule : ICheckRule
{
    public const string RuleName = "LQG003";

    public string Id => RuleName;
    public string Description => "Existing translation value is empty.";

    public bool IsEnabled(ScanContext context) => context.CheckEmptyTranslations;

    public IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        foreach (var collection in snapshot.Collections)
        {
            foreach (var entry in collection.Entries)
            {
                foreach (var pair in entry.TranslationsByLocale)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Value))
                    {
                        continue;
                    }

                    yield return new ScanIssue
                    {
                        RuleId = Id,
                        Severity = ScanSeverity.Warning,
                        CollectionName = collection.Name,
                        Key = entry.Key,
                        LocaleCode = pair.Key,
                        AssetPath = collection.SharedDataPath,
                        Message = $"Translation is present but empty for locale '{pair.Key}'.",
                        SuggestedFix = "Provide a translation or remove the entry if intentionally blank."
                    };
                }
            }
        }
    }
}

