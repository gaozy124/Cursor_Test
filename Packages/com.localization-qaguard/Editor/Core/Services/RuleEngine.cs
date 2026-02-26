using System.Collections.Generic;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Rules;

namespace LocalizationQAGuard.Editor.Core.Services;

public sealed class RuleEngine
{
    private readonly IReadOnlyList<ICheckRule> _rules;

    public RuleEngine(IReadOnlyList<ICheckRule>? rules = null)
    {
        _rules = rules ?? new List<ICheckRule>
        {
            new DuplicateKeyRule(),
            new MissingTranslationRule(),
            new EmptyTranslationRule(),
            new PlaceholderConsistencyRule(),
            new SmartStringSyntaxRule(),
            new UnusedKeyRule()
        };
    }

    public ScanReport Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context)
    {
        var issues = new List<ScanIssue>();
        foreach (var rule in _rules)
        {
            if (!rule.IsEnabled(context) || context.IgnoredRuleIds.Contains(rule.Id))
            {
                continue;
            }

            issues.AddRange(rule.Evaluate(snapshot, context));
        }

        var distinctIssues = issues
            .GroupBy(x => $"{x.RuleId}|{x.CollectionName}|{x.Key}|{x.LocaleCode}|{x.Message}")
            .Select(x => x.First())
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.CollectionName)
            .ThenBy(x => x.Key)
            .ThenBy(x => x.LocaleCode)
            .ToList();

        return new ScanReport { Issues = distinctIssues };
    }
}

