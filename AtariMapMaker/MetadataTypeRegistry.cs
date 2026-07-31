using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AtariMapMaker
{
    /// <summary>
    /// Global 1:1 metadata type byte ↔ label on <see cref="AtariMap.MetadataTypeLabels"/>.
    /// </summary>
    public static class MetadataTypeRegistry
    {
        private static readonly Regex TypePrefixRegex = new Regex(@"^\$([0-9A-Fa-f]{1,2})\s*(.*)$", RegexOptions.Compiled);

        public static void EnsureLabelsDictionary(AtariMap map)
        {
            if (map.MetadataTypeLabels == null)
                map.MetadataTypeLabels = new Dictionary<byte, string>();
        }

        /// <summary>
        /// Call after loading a map: legacy maps get types 00.. from first-seen unique texts;
        /// maps with labels reconcile item.Text with registry.
        /// </summary>
        public static void SyncAfterLoad(AtariMap map)
        {
            if (map == null) return;
            EnsureLabelsDictionary(map);
            if (map.ScreenMetadata == null) return;

            if (map.MetadataTypeLabels.Count == 0)
            {
                RebuildFromLegacyFirstSeenOrder(map);
                return;
            }

            foreach (var item in EnumerateAllItems(map))
            {
                if (map.MetadataTypeLabels.TryGetValue(item.Type, out string regLabel))
                {
                    if (!string.Equals(item.Text, regLabel, StringComparison.Ordinal))
                        item.Text = regLabel;
                }
                else
                {
                    byte? found = FindTypeByLabel(map, item.Text);
                    if (found.HasValue)
                    {
                        item.Type = found.Value;
                        item.Text = map.MetadataTypeLabels[item.Type];
                    }
                    else
                    {
                        try
                        {
                            item.Type = RegisterNewLabel(map, item.Text ?? "");
                            item.Text = map.MetadataTypeLabels[item.Type];
                        }
                        catch (InvalidOperationException)
                        {
                            item.Type = 0;
                            if (!map.MetadataTypeLabels.ContainsKey(0))
                                map.MetadataTypeLabels[0] = item.Text ?? "";
                            item.Text = map.MetadataTypeLabels[0];
                        }
                    }
                }
            }
        }

        private static void RebuildFromLegacyFirstSeenOrder(AtariMap map)
        {
            map.MetadataTypeLabels.Clear();
            foreach (var item in EnumerateItemsSorted(map))
            {
                try
                {
                    RegisterNewLabel(map, item.Text ?? "");
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            foreach (var item in EnumerateAllItems(map))
            {
                byte? ft = FindTypeByLabel(map, item.Text ?? "");
                if (!ft.HasValue) continue;
                item.Type = ft.Value;
                if (map.MetadataTypeLabels.TryGetValue(item.Type, out string lab))
                    item.Text = lab;
            }
        }

        public static IEnumerable<MetadataLayerItem> EnumerateItemsSorted(AtariMap map)
        {
            if (map.ScreenMetadata == null) yield break;
            foreach (var key in map.ScreenMetadata.Keys.OrderBy(ParseScreenKey))
            {
                if (!map.ScreenMetadata.TryGetValue(key, out var meta) || meta?.ParsedItems == null) continue;
                foreach (var item in meta.ParsedItems)
                    yield return item;
            }
        }

        private static string ParseScreenKey(string key)
        {
            var parts = key.Split(',');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                return (y * 65536 + x).ToString("D10", CultureInfo.InvariantCulture);
            return key;
        }

        public static IEnumerable<MetadataLayerItem> EnumerateAllItems(AtariMap map)
        {
            if (map.ScreenMetadata == null) yield break;
            foreach (var meta in map.ScreenMetadata.Values)
            {
                if (meta?.ParsedItems == null) continue;
                foreach (var item in meta.ParsedItems)
                    yield return item;
            }
        }

        /// <summary>
        /// Removes <see cref="AtariMap.MetadataTypeLabels"/> entries for type bytes that no metadata item references.
        /// Does not change any item's <see cref="MetadataLayerItem.Type"/>.
        /// </summary>
        /// <returns>Number of dictionary entries removed.</returns>
        public static int PruneUnusedTypeLabels(AtariMap map)
        {
            if (map == null) return 0;
            EnsureLabelsDictionary(map);
            if (map.MetadataTypeLabels.Count == 0) return 0;
            var used = new HashSet<byte>();
            foreach (var item in EnumerateAllItems(map))
                used.Add(item.Type);
            int removed = 0;
            foreach (byte key in map.MetadataTypeLabels.Keys.ToList())
            {
                if (!used.Contains(key))
                {
                    map.MetadataTypeLabels.Remove(key);
                    removed++;
                }
            }
            return removed;
        }

        public static byte? FindTypeByLabel(AtariMap map, string label)
        {
            if (map?.MetadataTypeLabels == null || label == null) return null;
            foreach (var kv in map.MetadataTypeLabels)
            {
                if (string.Equals(kv.Value, label, StringComparison.Ordinal))
                    return kv.Key;
            }
            return null;
        }

        /// <summary>Smallest unused type byte, or throws if full.</summary>
        public static byte AllocateTypeByte(AtariMap map)
        {
            EnsureLabelsDictionary(map);
            for (int b = 0; b < 256; b++)
            {
                if (!map.MetadataTypeLabels.ContainsKey((byte)b))
                    return (byte)b;
            }
            throw new InvalidOperationException("All 256 metadata type bytes are in use.");
        }

        /// <summary>Registers label if new; returns existing type byte if label already exists.</summary>
        public static byte RegisterNewLabel(AtariMap map, string label)
        {
            label = NormalizeLabel(label);
            EnsureLabelsDictionary(map);
            var existing = FindTypeByLabel(map, label);
            if (existing.HasValue) return existing.Value;
            byte nb = AllocateTypeByte(map);
            map.MetadataTypeLabels[nb] = label;
            return nb;
        }

        public static string NormalizeLabel(string s) => (s ?? "").Trim();

        public static void SetLabelForTypeAndSyncItems(AtariMap map, byte type, string newLabel)
        {
            EnsureLabelsDictionary(map);
            newLabel = NormalizeLabel(newLabel);
            foreach (var kv in map.MetadataTypeLabels)
            {
                if (kv.Key != type && string.Equals(kv.Value, newLabel, StringComparison.Ordinal))
                    throw new InvalidOperationException($"Label \"{newLabel}\" is already assigned to type ${kv.Key:X2}.");
            }
            map.MetadataTypeLabels[type] = newLabel;
            foreach (var item in EnumerateAllItems(map))
            {
                if (item.Type == type)
                    item.Text = newLabel;
            }
        }

        public static byte? GetRepresentativeColorForType(AtariMap map, byte type)
        {
            foreach (var item in EnumerateAllItems(map))
            {
                if (item.Type == type)
                    return item.Color;
            }
            return null;
        }

        /// <summary>Apply combo / free-text input; updates registry when label changes for a type prefix.</summary>
        public static bool TryResolveInput(AtariMap map, string input, out byte type, out string label, out string error)
        {
            type = 0;
            label = "";
            error = null;
            input = NormalizeLabel(input);
            if (map == null) { error = "No map."; return false; }
            EnsureLabelsDictionary(map);

            var m = TypePrefixRegex.Match(input);
            if (m.Success)
            {
                if (!byte.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out type))
                {
                    error = "Invalid type byte.";
                    return false;
                }
                string rest = NormalizeLabel(m.Groups[2].Value);
                if (!map.MetadataTypeLabels.TryGetValue(type, out string currentReg))
                {
                    if (!string.IsNullOrEmpty(rest))
                    {
                        byte? conflict = FindTypeByLabel(map, rest);
                        if (conflict.HasValue && conflict.Value != type)
                        {
                            error = $"Label \"{rest}\" is already assigned to type ${conflict.Value:X2}.";
                            return false;
                        }
                    }
                    else
                        rest = "T" + type.ToString("X2");
                    map.MetadataTypeLabels[type] = rest;
                    label = rest;
                    return true;
                }
                if (string.IsNullOrEmpty(rest))
                {
                    label = currentReg;
                    return true;
                }
                if (!string.Equals(rest, currentReg, StringComparison.Ordinal))
                {
                    try
                    {
                        SetLabelForTypeAndSyncItems(map, type, rest);
                    }
                    catch (InvalidOperationException ex)
                    {
                        error = ex.Message;
                        return false;
                    }
                }
                label = map.MetadataTypeLabels[type];
                return true;
            }

            byte? byLabel = FindTypeByLabel(map, input);
            if (byLabel.HasValue)
            {
                type = byLabel.Value;
                label = map.MetadataTypeLabels[type];
                return true;
            }

            try
            {
                type = RegisterNewLabel(map, input);
                label = map.MetadataTypeLabels[type];
                return true;
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
                return false;
            }
        }

    }

    /// <summary>List entry for metadata type combo.</summary>
    internal sealed class MetadataTypeListEntry
    {
        public byte Type { get; }
        public string Label { get; }

        public MetadataTypeListEntry(byte type, string label)
        {
            Type = type;
            Label = label ?? "";
        }

        public override string ToString() => "$" + Type.ToString("X2") + "  " + Label;
    }
}
