namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class ScanIssue
{
    public string RuleId { get; init; } = string.Empty;
    public ScanSeverity Severity { get; init; } = ScanSeverity.Warning;
    public string CollectionName { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string LocaleCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string SuggestedFix { get; init; } = string.Empty;
    public string AssetPath { get; init; } = string.Empty;
}

