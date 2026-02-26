using System.Collections.Generic;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class LocalizationEntry
{
    public long Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public Dictionary<string, string?> TranslationsByLocale { get; } = new();
}

