using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LocalizationQAGuard.Editor.Core.Services;

public static class PlaceholderParser
{
    private static readonly Regex PlaceholderRegex =
        new(@"\{([a-zA-Z_][a-zA-Z0-9_]*|\d+)(?:[^{}]*)\}", RegexOptions.Compiled);

    public static HashSet<string> Extract(string? text)
    {
        var result = new HashSet<string>();
        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        var unescaped = text.Replace("{{", string.Empty).Replace("}}", string.Empty);
        var matches = PlaceholderRegex.Matches(unescaped);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                result.Add(match.Groups[1].Value);
            }
        }

        return result;
    }

    public static bool HasBalancedBraces(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        var depth = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (i + 1 < text.Length && text[i] == '{' && text[i + 1] == '{')
            {
                i++;
                continue;
            }

            if (i + 1 < text.Length && text[i] == '}' && text[i + 1] == '}')
            {
                i++;
                continue;
            }

            if (text[i] == '{')
            {
                depth++;
            }
            else if (text[i] == '}')
            {
                depth--;
                if (depth < 0)
                {
                    return false;
                }
            }
        }

        return depth == 0;
    }
}

