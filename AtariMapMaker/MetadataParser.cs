using System;
using System.Collections.Generic;

namespace AtariMapMaker
{
    public static class MetadataParser
    {
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
