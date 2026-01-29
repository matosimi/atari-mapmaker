using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AtariMapMaker
{
    public static class MetadataParser
    {
        /// <summary>
        /// Parse metadata text using regex pattern and extract named groups
        /// </summary>
        public static List<MetadataLayerItem> ParseMetadata(string rawText, string regexPattern)
        {
            List<MetadataLayerItem> items = new List<MetadataLayerItem>();

            if (string.IsNullOrEmpty(rawText) || string.IsNullOrEmpty(regexPattern))
                return items;

            try
            {
                Regex regex = new Regex(regexPattern, RegexOptions.Multiline);
                MatchCollection matches = regex.Matches(rawText);

                foreach (Match match in matches)
                {
                    MetadataLayerItem item = new MetadataLayerItem();

                    // Extract named groups
                    int x = 0;
                    int y = 0;
                    int value = 0;
                    byte color = 0;

                    if (match.Groups["x"].Success)
                        int.TryParse(match.Groups["x"].Value, out x);

                    if (match.Groups["y"].Success)
                        int.TryParse(match.Groups["y"].Value, out y);

                    item.X = x;
                    item.Y = y;

                    if (match.Groups["text"].Success)
                        item.Text = match.Groups["text"].Value;

                    if (match.Groups["value"].Success)
                        int.TryParse(match.Groups["value"].Value, out value);
                    item.Value = value;

                    if (match.Groups["color"].Success)
                        byte.TryParse(match.Groups["color"].Value, out color);
                    item.Color = color;

                    items.Add(item);
                }
            }
            catch (Exception)
            {
                // Invalid regex pattern - return empty list
            }

            return items;
        }

        /// <summary>
        /// Update parsed items for a screen's metadata
        /// </summary>
        public static void UpdateScreenMetadata(AtariMap map, int screenX, int screenY)
        {
            if (map == null || map.ScreenMetadata == null)
                return;

            string key = $"{screenX},{screenY}";
            if (!map.ScreenMetadata.ContainsKey(key))
                return;

            ScreenMetadata metadata = map.ScreenMetadata[key];
            if (metadata == null)
                return;

            metadata.ParsedItems = ParseMetadata(metadata.RawText, metadata.RegexPattern);
        }

        /// <summary>
        /// Get metadata items for a specific screen
        /// </summary>
        public static List<MetadataLayerItem> GetScreenMetadataItems(AtariMap map, int screenX, int screenY)
        {
            if (map == null || map.ScreenMetadata == null)
                return new List<MetadataLayerItem>();

            string key = $"{screenX},{screenY}";
            if (!map.ScreenMetadata.ContainsKey(key))
                return new List<MetadataLayerItem>();

            ScreenMetadata metadata = map.ScreenMetadata[key];
            if (metadata == null || metadata.ParsedItems == null)
                return new List<MetadataLayerItem>();

            return metadata.ParsedItems;
        }
    }
}
