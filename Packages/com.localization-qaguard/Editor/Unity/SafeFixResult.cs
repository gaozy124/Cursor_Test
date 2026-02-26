namespace LocalizationQAGuard.Editor.Unity;

public sealed class SafeFixResult
{
    public bool DryRun { get; init; }
    public int CandidateCount { get; set; }
    public int AppliedCount { get; set; }
    public int SkippedCount { get; set; }
}

