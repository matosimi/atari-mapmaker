using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtariMapMaker
{
    public static class SubmapManager
    {
        /// <summary>
        /// Load a submap from file
        /// </summary>
        public static AtariMap LoadSubmap(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Submap file not found: {filePath}");

            AtariJson.ParseAtrmap(filePath);
            var parsedData = AtariJson.ParsedData;

            if (parsedData == null)
                throw new Exception("Failed to parse submap file");

            // Validate submap dimensions (must be at least 2x2 chars)
            if (parsedData.MapScreenSize.Width < 2 || parsedData.MapScreenSize.Height < 2)
                throw new Exception("Submap screen size must be at least 2x2 characters");

            AtariMap submap = new AtariMap(parsedData.MapSize, parsedData.MapScreenSize)
            {
                Data = parsedData.MapData.Select(i => (byte)i).ToArray()
            };

            // Load fonts if available
            if (parsedData.FontDataArray != null && parsedData.FontDataArray.Length > 0)
            {
                for (int i = 0; i < parsedData.FontDataArray.Length && i < 8; i++)
                {
                    if (parsedData.FontDataArray[i] != null)
                    {
                        submap.SetFontData(parsedData.FontDataArray[i].Select(j => (byte)j).ToArray(), i);
                    }
                }
            }
            else if (parsedData.FontData != null)
            {
                submap.SetFontData(parsedData.FontData.Select(i => (byte)i).ToArray(), 0);
            }

            // Load font line mapping (per-screen)
            if (parsedData.FontLineMappingPerScreen != null)
            {
                submap.FontLineMappingPerScreen = parsedData.FontLineMappingPerScreen.Select(i => (byte)i).ToArray();
            }
            else
            {
                // Initialize to all font 0
                submap.SetFontForAllLines(0);
            }
            
            // Load font mapping references
            if (parsedData.FontLineMappingReferences != null)
            {
                submap.FontLineMappingReferences = parsedData.FontLineMappingReferences;
            }
            else
            {
                // Initialize all screens to reference screen 0,0 by default
                submap.FontLineMappingReferences = new Dictionary<string, ScreenReference>();
                for (int sy = 0; sy < submap.MapSize.Height; sy++)
                {
                    for (int sx = 0; sx < submap.MapSize.Width; sx++)
                    {
                        string key = $"{sx},{sy}";
                        submap.FontLineMappingReferences[key] = new ScreenReference(0, 0);
                    }
                }
            }

            // Load DLI data
            if (parsedData.DliData != null)
                submap.ColorData = parsedData.DliData.Select(i => (byte)i).ToArray();
            else
                submap.InitDliColorFullMap();

            return submap;
        }

        /// <summary>
        /// Validate that a submap is compatible with tilemap usage
        /// </summary>
        public static bool ValidateSubmap(AtariMap submap, int requiredTileWidth, int requiredTileHeight)
        {
            if (submap == null) return false;
            if (submap.ScreenSize.Width < requiredTileWidth || submap.ScreenSize.Height < requiredTileHeight)
                return false;
            return true;
        }

        /// <summary>
        /// Get screen index from submap for a given tile position
        /// </summary>
        public static int GetSubmapScreenIndex(AtariMap submap, int tileX, int tileY)
        {
            if (submap == null) return 0;
            int screensPerRow = submap.MapSize.Width;
            int screenIndex = tileY * screensPerRow + tileX;
            if (screenIndex >= submap.MapSize.Width * submap.MapSize.Height)
                return 0; // Default to first screen if out of bounds
            return screenIndex;
        }
    }
}
