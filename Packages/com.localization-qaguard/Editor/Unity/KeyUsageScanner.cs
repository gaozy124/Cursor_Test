using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LocalizationQAGuard.Editor.Unity;

public sealed class KeyUsageScanner
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".unity",
        ".prefab",
        ".asset",
        ".json",
        ".txt",
        ".uxml",
        ".uss"
    };

    public HashSet<string> FindUsedKeys(IEnumerable<string> keys)
    {
        var remaining = new HashSet<string>(keys.Where(key => !string.IsNullOrWhiteSpace(key)), StringComparer.Ordinal);
        var used = new HashSet<string>(StringComparer.Ordinal);
        if (remaining.Count == 0)
        {
            return used;
        }

        var projectAssetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        if (!Directory.Exists(projectAssetsPath))
        {
            Debug.LogWarning("[Localization QA Guard] Could not find Assets folder to scan key usage.");
            return used;
        }

        foreach (var filePath in Directory.EnumerateFiles(projectAssetsPath, "*.*", SearchOption.AllDirectories))
        {
            if (remaining.Count == 0)
            {
                break;
            }

            var extension = Path.GetExtension(filePath);
            if (!SupportedExtensions.Contains(extension))
            {
                continue;
            }

            string content;
            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var key in remaining.ToArray())
            {
                if (!content.Contains(key, StringComparison.Ordinal))
                {
                    continue;
                }

                used.Add(key);
                remaining.Remove(key);
            }
        }

        return used;
    }
}

