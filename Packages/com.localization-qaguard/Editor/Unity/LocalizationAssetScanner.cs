using System;
using System.Collections.Generic;
using System.Linq;
using LocalizationQAGuard.Editor.Core.Models;
using UnityEditor;
using UnityEngine;

namespace LocalizationQAGuard.Editor.Unity;

public sealed class LocalizationAssetScanner
{
    private static readonly string[] EntryValueCandidates =
    {
        "m_Localized",
        "m_Text",
        "m_Value",
        "value"
    };

    public LocalizationProjectSnapshot ScanProject(ScanContext context)
    {
        var snapshot = new LocalizationProjectSnapshot();
        var collections = BuildCollections();
        snapshot.Collections.AddRange(collections);

        if (context.ScanTextReferencesInProject)
        {
            var allKeys = collections
                .SelectMany(collection => collection.Entries)
                .Select(entry => entry.Key)
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Distinct()
                .ToList();

            var usedKeys = new KeyUsageScanner().FindUsedKeys(allKeys);
            snapshot.UsedKeys.UnionWith(usedKeys);
        }

        return snapshot;
    }

    private static List<LocalizationCollection> BuildCollections()
    {
        var tableDataBySharedPath = ParseStringTablesBySharedPath();
        var sharedDataPaths = FindAssetsByTypeName("SharedTableData");
        var collections = new List<LocalizationCollection>();

        foreach (var sharedDataPath in sharedDataPaths)
        {
            var sharedAsset = AssetDatabase.LoadMainAssetAtPath(sharedDataPath);
            if (sharedAsset == null)
            {
                continue;
            }

            var sharedSo = new SerializedObject(sharedAsset);
            var entriesProp = sharedSo.FindProperty("m_Entries");
            if (entriesProp == null || !entriesProp.isArray)
            {
                continue;
            }

            var keyRecords = new List<(long Id, string Key)>();
            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var record = entriesProp.GetArrayElementAtIndex(i);
                var idProp = record.FindPropertyRelative("m_Id");
                var keyProp = record.FindPropertyRelative("m_Key");
                if (idProp == null || keyProp == null)
                {
                    continue;
                }

                var key = keyProp.stringValue ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                keyRecords.Add((idProp.longValue, key));
            }

            var duplicateKeyRecords = keyRecords
                .GroupBy(record => record.Key, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => new DuplicateKeyRecord
                {
                    Key = group.Key,
                    Ids = group.Select(x => x.Id).ToList()
                })
                .ToList();

            var collection = new LocalizationCollection
            {
                Name = sharedAsset.name,
                SharedDataPath = sharedDataPath
            };

            collection.DuplicateKeys.AddRange(duplicateKeyRecords);

            if (!tableDataBySharedPath.TryGetValue(sharedDataPath, out var localeTables))
            {
                localeTables = new Dictionary<string, Dictionary<long, string?>>();
            }

            collection.Locales.AddRange(localeTables.Keys.OrderBy(x => x, StringComparer.Ordinal));

            foreach (var keyRecord in keyRecords)
            {
                var entry = new LocalizationEntry
                {
                    Id = keyRecord.Id,
                    Key = keyRecord.Key
                };

                foreach (var localeTable in localeTables)
                {
                    if (localeTable.Value.TryGetValue(keyRecord.Id, out var translatedValue))
                    {
                        entry.TranslationsByLocale[localeTable.Key] = translatedValue;
                    }
                }

                collection.Entries.Add(entry);
            }

            collections.Add(collection);
        }

        return collections.OrderBy(x => x.Name, StringComparer.Ordinal).ToList();
    }

    private static Dictionary<string, Dictionary<string, Dictionary<long, string?>>> ParseStringTablesBySharedPath()
    {
        var stringTablePaths = FindAssetsByTypeName("StringTable");
        var tableDataBySharedPath = new Dictionary<string, Dictionary<string, Dictionary<long, string?>>>(StringComparer.Ordinal);

        foreach (var stringTablePath in stringTablePaths)
        {
            var tableAsset = AssetDatabase.LoadMainAssetAtPath(stringTablePath);
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

            var localeCode = ExtractLocaleCode(so, stringTablePath);
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                localeCode = "unknown";
            }

            var entries = ParseStringTableEntries(so);

            if (!tableDataBySharedPath.TryGetValue(sharedDataPath, out var localeMap))
            {
                localeMap = new Dictionary<string, Dictionary<long, string?>>(StringComparer.Ordinal);
                tableDataBySharedPath[sharedDataPath] = localeMap;
            }

            localeMap[localeCode] = entries;
        }

        return tableDataBySharedPath;
    }

    private static Dictionary<long, string?> ParseStringTableEntries(SerializedObject tableSo)
    {
        var tableDataProp = tableSo.FindProperty("m_TableData");
        var entries = new Dictionary<long, string?>();
        if (tableDataProp == null || !tableDataProp.isArray)
        {
            return entries;
        }

        for (var i = 0; i < tableDataProp.arraySize; i++)
        {
            var entry = tableDataProp.GetArrayElementAtIndex(i);
            var idProp = entry.FindPropertyRelative("m_Id");
            if (idProp == null)
            {
                continue;
            }

            entries[idProp.longValue] = FindFirstString(entry);
        }

        return entries;
    }

    private static string? FindFirstString(SerializedProperty property)
    {
        foreach (var candidate in EntryValueCandidates)
        {
            var child = property.FindPropertyRelative(candidate);
            var value = ExtractString(child);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private static string? ExtractString(SerializedProperty? property)
    {
        if (property == null)
        {
            return null;
        }

        if (property.propertyType == SerializedPropertyType.String)
        {
            return property.stringValue;
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
                return copy.stringValue;
            }

            enterChildren = false;
        }

        return null;
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

    private static List<string> FindAssetsByTypeName(string typeName)
    {
        var guids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });
        var matches = new List<string>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (mainType?.Name == typeName)
            {
                matches.Add(path);
            }
        }

        return matches;
    }
}

