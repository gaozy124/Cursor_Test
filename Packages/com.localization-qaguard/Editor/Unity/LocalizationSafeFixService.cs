using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using UnityEditor;
using UnityEngine;

namespace LocalizationQAGuard.Editor.Unity;

public sealed class LocalizationSafeFixService
{
    private static readonly string[] EntryValueCandidates =
    {
        "m_Localized",
        "m_Text",
        "m_Value",
        "value"
    };

    public SafeFixResult ApplyEmptyTranslationFixes(
        LocalizationProjectSnapshot snapshot,
        ScanContext context,
        bool dryRun)
    {
        var result = new SafeFixResult { DryRun = dryRun };
        if (snapshot.Collections.Count == 0)
        {
            return result;
        }

        var tablePathsBySharedDataPath = FindStringTablePathsBySharedDataPath();

        foreach (var collection in snapshot.Collections)
        {
            var referenceLocale = ResolveReferenceLocale(collection, context.ReferenceLocaleCode);
            if (string.IsNullOrWhiteSpace(referenceLocale))
            {
                continue;
            }

            if (!tablePathsBySharedDataPath.TryGetValue(collection.SharedDataPath, out var localeTablePathMap))
            {
                continue;
            }

            foreach (var entry in collection.Entries)
            {
                if (!entry.TranslationsByLocale.TryGetValue(referenceLocale, out var referenceText) ||
                    string.IsNullOrWhiteSpace(referenceText))
                {
                    continue;
                }

                foreach (var locale in collection.Locales)
                {
                    if (locale == referenceLocale)
                    {
                        continue;
                    }

                    if (!entry.TranslationsByLocale.TryGetValue(locale, out var currentText) ||
                        !string.IsNullOrWhiteSpace(currentText))
                    {
                        continue;
                    }

                    result.CandidateCount++;

                    if (dryRun)
                    {
                        continue;
                    }

                    if (!localeTablePathMap.TryGetValue(locale, out var tablePath))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    var changed = TrySetEntryValue(tablePath, entry.Id, referenceText!);
                    if (changed)
                    {
                        result.AppliedCount++;
                    }
                    else
                    {
                        result.SkippedCount++;
                    }
                }
            }
        }

        if (!dryRun && result.AppliedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> FindStringTablePathsBySharedDataPath()
    {
        var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var guids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (mainType?.Name != "StringTable")
            {
                continue;
            }

            var tableAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (tableAsset == null)
            {
                continue;
            }

            var so = new SerializedObject(tableAsset);
            var sharedDataRef = so.FindProperty("m_SharedData")?.objectReferenceValue;
            if (sharedDataRef == null)
            {
                continue;
            }

            var sharedDataPath = AssetDatabase.GetAssetPath(sharedDataRef);
            if (string.IsNullOrWhiteSpace(sharedDataPath))
            {
                continue;
            }

            var localeCode = ExtractLocaleCode(so, path);
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                localeCode = "unknown";
            }

            if (!map.TryGetValue(sharedDataPath, out var localeMap))
            {
                localeMap = new Dictionary<string, string>(StringComparer.Ordinal);
                map[sharedDataPath] = localeMap;
            }

            localeMap[localeCode] = path;
        }

        return map;
    }

    private static bool TrySetEntryValue(string stringTableAssetPath, long entryId, string newValue)
    {
        var tableAsset = AssetDatabase.LoadMainAssetAtPath(stringTableAssetPath);
        if (tableAsset == null)
        {
            return false;
        }

        var so = new SerializedObject(tableAsset);
        var tableDataProp = so.FindProperty("m_TableData");
        if (tableDataProp == null || !tableDataProp.isArray)
        {
            return false;
        }

        for (var i = 0; i < tableDataProp.arraySize; i++)
        {
            var entryProp = tableDataProp.GetArrayElementAtIndex(i);
            var idProp = entryProp.FindPropertyRelative("m_Id");
            if (idProp == null || idProp.longValue != entryId)
            {
                continue;
            }

            if (!TryWriteEntryStringValue(entryProp, newValue))
            {
                return false;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tableAsset);
            return true;
        }

        return false;
    }

    private static bool TryWriteEntryStringValue(SerializedProperty entryProp, string value)
    {
        foreach (var candidate in EntryValueCandidates)
        {
            var childProp = entryProp.FindPropertyRelative(candidate);
            if (childProp == null)
            {
                continue;
            }

            var writableStringProp = FindWritableStringProperty(childProp);
            if (writableStringProp == null)
            {
                continue;
            }

            writableStringProp.stringValue = value;
            return true;
        }

        return false;
    }

    private static SerializedProperty? FindWritableStringProperty(SerializedProperty property)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            return property;
        }

        if (!property.hasVisibleChildren)
        {
            return null;
        }

        var copy = property.Copy();
        var end = copy.GetEndProperty();
        var enterChildren = true;

        while (copy.NextVisible(enterChildren) && !SerializedProperty.EqualContents(copy, end))
        {
            if (copy.propertyType == SerializedPropertyType.String)
            {
                return copy;
            }

            enterChildren = false;
        }

        return null;
    }

    private static string ResolveReferenceLocale(LocalizationCollection collection, string preferredLocale)
    {
        if (!string.IsNullOrWhiteSpace(preferredLocale) && collection.Locales.Contains(preferredLocale))
        {
            return preferredLocale;
        }

        return collection.Locales.FirstOrDefault() ?? string.Empty;
    }

    private static string ExtractLocaleCode(SerializedObject tableSo, string tableAssetPath)
    {
        var localeIdentifier = tableSo.FindProperty("m_LocaleIdentifier");
        if (localeIdentifier != null)
        {
            var codeProp = localeIdentifier.FindPropertyRelative("m_Code");
            if (codeProp != null && !string.IsNullOrWhiteSpace(codeProp.stringValue))
            {
                return codeProp.stringValue;
            }
        }

        var localeName = tableSo.FindProperty("m_LocaleName");
        if (localeName != null && !string.IsNullOrWhiteSpace(localeName.stringValue))
        {
            return localeName.stringValue;
        }

        var fileName = System.IO.Path.GetFileNameWithoutExtension(tableAssetPath);
        return fileName.Split('_').LastOrDefault() ?? fileName;
    }
}

