using System.Collections.Generic;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Rules;

public sealed class MissingTranslationRule : ICheckRule
{
    public const string RuleName = "LQG002";

    public string Id => RuleName;
    public string Description => "Missing translation for key/locale combination.";

    public bool IsEnabled(ScanContext context) => context.CheckMissingTranslations;

    public IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        foreach (var collection in snapshot.Collections)
        {
            foreach (var entry in collection.Entries)
            {
                foreach (var locale in collection.Locales)
                {
                    if (entry.TranslationsByLocale.ContainsKey(locale))
                    {
                        continue;
                    }

                    yield return new ScanIssue
                    {
                        RuleId = Id,
                        Severity = ScanSeverity.Error,
                        CollectionName = collection.Name,
                        Key = entry.Key,
                        LocaleCode = locale,
                        AssetPath = collection.SharedDataPath,
                        Message = $"Missing translation for locale '{locale}'.",
                        SuggestedFix = "Add a translation in this locale, or intentionally suppress with project policy."
                    };
                }
            }
        }
    }
}

