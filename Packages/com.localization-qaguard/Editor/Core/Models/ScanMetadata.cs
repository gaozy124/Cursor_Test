using System;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class ScanMetadata
{
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public string ProjectPath { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public int CollectionCount { get; init; }
    public int EntryCount { get; init; }
}

