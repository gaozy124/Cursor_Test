using System.Collections.Generic;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class LocalizationCollection
{
    public string Name { get; init; } = string.Empty;
    public string SharedDataPath { get; init; } = string.Empty;
    public List<string> Locales { get; } = new();
    public List<LocalizationEntry> Entries { get; } = new();
    public List<DuplicateKeyRecord> DuplicateKeys { get; } = new();
}

