using System.Collections.Generic;
using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Services;

namespace LocalizationQAGuard.Editor.Core.Rules;

public sealed class SmartStringSyntaxRule : ICheckRule
{
    public const string RuleName = "LQG005";

    public string Id => RuleName;
    public string Description => "Potential smart string formatting syntax issues.";

    public bool IsEnabled(ScanContext context) => context.CheckSmartStringSyntax;

    public IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        foreach (var collection in snapshot.Collections)
        {
            foreach (var entry in collection.Entries)
            {
                foreach (var pair in entry.TranslationsByLocale)
                {
                    var text = pair.Value;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    if (!LooksLikeFormattedText(text))
                    {
                        continue;
                    }

                    if (PlaceholderParser.HasBalancedBraces(text))
                    {
                        continue;
                    }

                    yield return new ScanIssue
                    {
                        RuleId = Id,
                        Severity = ScanSeverity.Error,
                        CollectionName = collection.Name,
                        Key = entry.Key,
                        LocaleCode = pair.Key,
                        AssetPath = collection.SharedDataPath,
                        Message = "Unbalanced braces detected in formatted string.",
                        SuggestedFix = "Fix brace balance or escape literals with double braces."
                    };
                }
            }
        }
    }

    private static bool LooksLikeFormattedText(string text) =>
        text.Contains('{') || text.Contains('}');
}

