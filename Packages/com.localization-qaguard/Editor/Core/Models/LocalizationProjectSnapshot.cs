using System.Collections.Generic;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class LocalizationProjectSnapshot
{
    public List<LocalizationCollection> Collections { get; } = new();
    public HashSet<string> UsedKeys { get; } = new();
}

