using System;
using System.Collections.Generic;
using System.Linq;

namespace AtariMapMaker
{
    public static class TilemapEditor
    {
        /// <summary>
        /// Apply numbering pattern to tiles
        /// </summary>
        public static void ApplyNumberingPattern(AtariMap tilemap, string pattern)
        {
            if (tilemap == null || !tilemap.IsTilemap || tilemap.TilemapInfo == null)
                return;

            if (tilemap.TilemapInfo.TileByteValues == null)
                tilemap.TilemapInfo.TileByteValues = new Dictionary<string, TileInfo>();

            int tileIndex = 0;

            switch (pattern.ToLower())
            {
                case "row-major":
                    // Increment left-to-right, top-to-bottom
                    for (int y = 0; y < tilemap.MapSize.Height; y++)
                    {
                        for (int x = 0; x < tilemap.MapSize.Width; x++)
                        {
                            string key = $"{x},{y}";
                            if (!tilemap.TilemapInfo.TileByteValues.ContainsKey(key))
                                tilemap.TilemapInfo.TileByteValues[key] = new TileInfo();

                            tilemap.TilemapInfo.TileByteValues[key].ByteValue1 = (byte)(tileIndex & 0xFF);
                            tilemap.TilemapInfo.TileByteValues[key].ByteValue2 = (ushort)tileIndex;
                            tileIndex++;
                        }
                    }
                    break;

                case "column-major":
                    // Increment top-to-bottom, left-to-right
                    for (int x = 0; x < tilemap.MapSize.Width; x++)
                    {
                        for (int y = 0; y < tilemap.MapSize.Height; y++)
                        {
                            string key = $"{x},{y}";
                            if (!tilemap.TilemapInfo.TileByteValues.ContainsKey(key))
                                tilemap.TilemapInfo.TileByteValues[key] = new TileInfo();

                            tilemap.TilemapInfo.TileByteValues[key].ByteValue1 = (byte)(tileIndex & 0xFF);
                            tilemap.TilemapInfo.TileByteValues[key].ByteValue2 = (ushort)tileIndex;
                            tileIndex++;
                        }
                    }
                    break;

                default:
                    // Default to row-major
                    ApplyNumberingPattern(tilemap, "row-major");
                    break;
            }

            tilemap.TilemapInfo.NumberingPattern = pattern;
        }

        /// <summary>
        /// Get tile info for a specific tile position
        /// </summary>
        public static TileInfo GetTileInfo(AtariMap tilemap, int tileX, int tileY)
        {
            if (tilemap == null || !tilemap.IsTilemap || tilemap.TilemapInfo == null)
                return null;

            string key = $"{tileX},{tileY}";
            if (tilemap.TilemapInfo.TileByteValues != null && tilemap.TilemapInfo.TileByteValues.ContainsKey(key))
                return tilemap.TilemapInfo.TileByteValues[key];

            // Return default tile info
            return new TileInfo { ByteValue1 = 0, ByteValue2 = 0 };
        }

        /// <summary>
        /// Set tile byte values for a specific tile
        /// </summary>
        public static void SetTileByteValues(AtariMap tilemap, int tileX, int tileY, byte byteValue1, ushort byteValue2)
        {
            if (tilemap == null || !tilemap.IsTilemap || tilemap.TilemapInfo == null)
                return;

            if (tilemap.TilemapInfo.TileByteValues == null)
                tilemap.TilemapInfo.TileByteValues = new Dictionary<string, TileInfo>();

            string key = $"{tileX},{tileY}";
            if (!tilemap.TilemapInfo.TileByteValues.ContainsKey(key))
                tilemap.TilemapInfo.TileByteValues[key] = new TileInfo();

            tilemap.TilemapInfo.TileByteValues[key].ByteValue1 = byteValue1;
            tilemap.TilemapInfo.TileByteValues[key].ByteValue2 = byteValue2;
        }

        /// <summary>
        /// Renumber tiles using the current numbering pattern
        /// </summary>
        public static void RenumberTiles(AtariMap tilemap)
        {
            if (tilemap == null || !tilemap.IsTilemap || tilemap.TilemapInfo == null)
                return;

            string pattern = tilemap.TilemapInfo.NumberingPattern;
            if (string.IsNullOrEmpty(pattern))
                pattern = "row-major";

            ApplyNumberingPattern(tilemap, pattern);
        }
    }
}
