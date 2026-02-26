using LocalizationQAGuard.Editor.Core.Models;
using LocalizationQAGuard.Editor.Core.Rules;
using LocalizationQAGuard.Editor.Core.Services;
using Xunit;

namespace LocalizationQAGuard.Core.Tests;

public sealed class RuleEngineTests
{
    [Fact]
    public void MissingTranslationRule_FlagsMissingLocaleEntry()
    {
        var snapshot = BuildSnapshot(
            locales: ["en", "fr"],
            key: "menu.play",
            translations: new Dictionary<string, string?> { { "en", "Play" } });

        var rule = new MissingTranslationRule();
        var issues = rule.Evaluate(snapshot, new ScanContext()).ToList();

        Assert.Single(issues);
        Assert.Equal("fr", issues[0].LocaleCode);
        Assert.Equal(ScanSeverity.Error, issues[0].Severity);
    }

    [Fact]
    public void PlaceholderConsistencyRule_FlagsMismatch()
    {
        var snapshot = BuildSnapshot(
            locales: ["en", "fr"],
            key: "shop.price",
            translations: new Dictionary<string, string?>
            {
                { "en", "Price: {amount}" },
                { "fr", "Prix : {value}" }
            });

        var rule = new PlaceholderConsistencyRule();
        var issues = rule.Evaluate(snapshot, new ScanContext { ReferenceLocaleCode = "en" }).ToList();

        Assert.Single(issues);
        Assert.Equal("fr", issues[0].LocaleCode);
    }

    [Fact]
    public void RuleEngine_AppliesMultipleRulesAndReturnsCounts()
    {
        var collection = new LocalizationCollection
        {
            Name = "UI",
            SharedDataPath = "Assets/Localization/UI Shared.asset"
        };
        collection.Locales.AddRange(["en", "fr"]);
        collection.DuplicateKeys.Add(new DuplicateKeyRecord
        {
            Key = "menu.quit",
            Ids = [101, 208]
        });

        var entry = new LocalizationEntry
        {
            Id = 101,
            Key = "menu.quit"
        };
        entry.TranslationsByLocale["en"] = "Quit";
        collection.Entries.Add(entry);

        var snapshot = new LocalizationProjectSnapshot();
        snapshot.Collections.Add(collection);

        var report = new RuleEngine().Evaluate(snapshot, new ScanContext
        {
            CheckUnusedKeys = false,
            ScanTextReferencesInProject = false
        });

        Assert.Equal(2, report.ErrorCount); // duplicate + missing fr
    }

    private static LocalizationProjectSnapshot BuildSnapshot(
        IReadOnlyList<string> locales,
        string key,
        Dictionary<string, string?> translations)
    {
        var collection = new LocalizationCollection
        {
            Name = "UI",
            SharedDataPath = "Assets/Localization/UI Shared.asset"
        };

        collection.Locales.AddRange(locales);

        var entry = new LocalizationEntry
        {
            Id = 1,
            Key = key
        };
        foreach (var pair in translations)
        {
            entry.TranslationsByLocale[pair.Key] = pair.Value;
        }

        collection.Entries.Add(entry);

        var snapshot = new LocalizationProjectSnapshot();
        snapshot.Collections.Add(collection);
        return snapshot;
    }
}

