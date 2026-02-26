using System.Collections.Generic;

namespace LocalizationQAGuard.Editor.Core.Models;

public sealed class DuplicateKeyRecord
{
    public string Key { get; init; } = string.Empty;
    public IReadOnlyList<long> Ids { get; init; } = new List<long>();
}

