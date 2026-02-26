using System.Collections.Generic;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class ScanContext
{
    public bool CheckMissingTranslations { get; set; } = true;
    public bool CheckEmptyTranslations { get; set; } = true;
    public bool CheckDuplicateKeys { get; set; } = true;
    public bool CheckPlaceholderConsistency { get; set; } = true;
    public bool CheckSmartStringSyntax { get; set; } = true;
    public bool CheckUnusedKeys { get; set; } = true;
    public bool ScanTextReferencesInProject { get; set; } = true;

    public string ReferenceLocaleCode { get; set; } = string.Empty;

    public HashSet<string> IgnoredRuleIds { get; } = new();
}

