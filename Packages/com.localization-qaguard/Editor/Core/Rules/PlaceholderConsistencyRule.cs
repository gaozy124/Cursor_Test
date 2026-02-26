using System.Collections.Generic;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;

namespace LocalizationQAGuard.Editor.Core.Rules;

public sealed class PlaceholderConsistencyRule : ICheckRule
{
    public const string RuleName = "LQG004";

    public string Id => RuleName;
    public string Description => "Placeholder set mismatch between locales.";

    public bool IsEnabled(ScanContext context) => context.CheckPlaceholderConsistency;

    public IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        foreach (var collection in snapshot.Collections)
        {
            var referenceLocale = ResolveReferenceLocale(collection, context.ReferenceLocaleCode);
            if (string.IsNullOrWhiteSpace(referenceLocale))
            {
                continue;
            }

            foreach (var entry in collection.Entries)
            {
                if (!entry.TranslationsByLocale.TryGetValue(referenceLocale, out var referenceText))
                {
                    continue;
                }

                var referencePlaceholders = PlaceholderParser.Extract(referenceText);
                if (referencePlaceholders.Count == 0)
                {
                    continue;
                }

                foreach (var locale in collection.Locales)
                {
                    if (locale == referenceLocale)
                    {
                        continue;
                    }

                    if (!entry.TranslationsByLocale.TryGetValue(locale, out var text) || string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    var currentPlaceholders = PlaceholderParser.Extract(text);
                    if (referencePlaceholders.SetEquals(currentPlaceholders))
                    {
                        continue;
                    }

                    var expected = string.Join(", ", referencePlaceholders.OrderBy(x => x));
                    var actual = string.Join(", ", currentPlaceholders.OrderBy(x => x));

                    yield return new ScanIssue
                    {
                        RuleId = Id,
                        Severity = ScanSeverity.Error,
                        CollectionName = collection.Name,
                        Key = entry.Key,
                        LocaleCode = locale,
                        AssetPath = collection.SharedDataPath,
                        Message = $"Placeholder mismatch. Expected [{expected}], found [{actual}].",
                        SuggestedFix = "Align placeholders with the reference locale text."
                    };
                }
            }
        }
    }

    private static string ResolveReferenceLocale(LocalizationCollection collection, string preferredLocale)
    {
        if (!string.IsNullOrWhiteSpace(preferredLocale) && collection.Locales.Contains(preferredLocale))
        {
            return preferredLocale;
        }

        return collection.Locales.FirstOrDefault() ?? string.Empty;
    }
}

