using System.Collections.Generic;
using LocalizationQAGuard.Editor.Core.Models;

namespace LocalizationQAGuard.Editor.Core.Rules;

public interface ICheckRule
{
    string Id { get; }
    string Description { get; }
    bool IsEnabled(ScanContext context);
    IEnumerable<ScanIssue> Evaluate(LocalizationProjectSnapshot snapshot, ScanContext context);
}

