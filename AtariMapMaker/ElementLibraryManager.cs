using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AtariMapMaker
{
    public static class ElementLibraryManager
    {
        /// <summary>
        /// Save the current clipboard contents to the element library.
        /// </summary>
        public static void SaveClipboardToLibrary(AtariMap map, string name)
        {
            if (map == null || string.IsNullOrEmpty(name))
                return;
            if (!AtariClipboard.IsValid)
                return;

            int w = AtariClipboard.ClipboardWidth;
            int h = AtariClipboard.ClipboardHeight;
            byte[,] clipboardData = AtariClipboard.GetData();
            if (clipboardData == null || w <= 0 || h <= 0)
                return;

            if (map.ElementLibrary == null)
                map.ElementLibrary = new Dictionary<string, LibraryElement>();

            byte[] data = new byte[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    data[y * w + x] = clipboardData[x, y];

            byte[] fontBytes = null;
            byte[,] clipFonts = AtariClipboard.GetFontData();
            if (clipFonts != null && map.FreeCharmapMode && !AtariClipboard.IsTileIndexes)
            {
                fontBytes = new byte[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        fontBytes[y * w + x] = (byte)(clipFonts[x, y] & 0x07);
            }

            LibraryElement element = new LibraryElement
            {
                Name = name,
                Size = new Size(w, h),
                Data = data,
                FontData = fontBytes,
                IsTileData = AtariClipboard.IsTileIndexes
            };

            map.ElementLibrary[name] = element;
        }

        /// <summary>
        /// Save a rectangular region of the map to the element library (e.g. when selection is in map coordinates).
        /// </summary>
        public static void SaveToLibrary(AtariMap map, Rectangle selection, string name)
        {
            if (map == null || string.IsNullOrEmpty(name))
                return;

            if (map.ElementLibrary == null)
                map.ElementLibrary = new Dictionary<string, LibraryElement>();

            byte[] data = ExtractSelectionData(map, selection);
            Size size = new Size(selection.Width, selection.Height);
            byte[] fontBytes = null;
            if (map.FreeCharmapMode && map.CharFontData != null && !map.IsTilemap)
                fontBytes = ExtractSelectionFontData(map, selection);

            LibraryElement element = new LibraryElement
            {
                Name = name,
                Size = size,
                Data = data,
                FontData = fontBytes
            };

            map.ElementLibrary[name] = element;
        }

        /// <summary>
        /// Extract data from a rectangular selection
        /// </summary>
        private static byte[] ExtractSelectionData(AtariMap map, Rectangle selection)
        {
            byte[] data = new byte[selection.Width * selection.Height];

            for (int y = 0; y < selection.Height; y++)
            {
                for (int x = 0; x < selection.Width; x++)
                {
                    int charX = selection.X + x;
                    int charY = selection.Y + y;

                    if (charX < map.Stride && charY < map.MapSize.Height * map.ScreenSize.Height)
                    {
                        int index = charX + charY * map.Stride;
                        if (index < map.Data.Length)
                            data[y * selection.Width + x] = map.Data[index];
                    }
                }
            }

            return data;
        }

        private static byte[] ExtractSelectionFontData(AtariMap map, Rectangle selection)
        {
            byte[] fonts = new byte[selection.Width * selection.Height];
            if (map.CharFontData == null)
                return fonts;

            for (int y = 0; y < selection.Height; y++)
            {
                for (int x = 0; x < selection.Width; x++)
                {
                    int charX = selection.X + x;
                    int charY = selection.Y + y;
                    if (charX < map.CharStride && charY < map.MapSize.Height * map.ScreenSize.Height)
                    {
                        int index = charX + charY * map.CharStride;
                        if (index < map.CharFontData.Length)
                            fonts[y * selection.Width + x] = (byte)(map.CharFontData[index] & 0x07);
                    }
                }
            }
            return fonts;
        }

        /// <summary>
        /// Paste element from library with optional transparency (skip zero bytes)
        /// </summary>
        public static void PasteFromLibrary(AtariMap map, string elementName, Point destination, bool skipZeroBytes)
        {
            if (map == null || map.ElementLibrary == null || !map.ElementLibrary.ContainsKey(elementName))
                return;

            LibraryElement element = map.ElementLibrary[elementName];
            if (element == null || element.Data == null)
                return;

            if (map.FreeCharmapMode && !map.IsTilemap)
                map.EnsureCharFontData();

            for (int y = 0; y < element.Size.Height; y++)
            {
                for (int x = 0; x < element.Size.Width; x++)
                {
                    int sourceIndex = y * element.Size.Width + x;
                    if (sourceIndex >= element.Data.Length)
                        continue;

                    byte value = element.Data[sourceIndex];

                    // Skip zero bytes if transparency is enabled
                    if (skipZeroBytes && value == 0)
                        continue;

                    int destX = destination.X + x;
                    int destY = destination.Y + y;

                    if (destX < map.Stride && destY < map.MapSize.Height * map.ScreenSize.Height)
                    {
                        int destIndex = destX + destY * map.Stride;
                        if (destIndex < map.Data.Length)
                        {
                            map.Data[destIndex] = value;
                            if (map.IsTilemap && map.TilemapInfo != null)
                                map.ExpandTileToCharData(destX, destY, value);
                            if (map.FreeCharmapMode && map.CharFontData != null && element.FontData != null
                                && sourceIndex < element.FontData.Length && destIndex < map.CharFontData.Length)
                            {
                                map.CharFontData[destIndex] = (byte)(element.FontData[sourceIndex] & 0x07);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Rename an element in the library
        /// </summary>
        public static bool RenameElement(AtariMap map, string oldName, string newName)
        {
            if (map == null || map.ElementLibrary == null || !map.ElementLibrary.ContainsKey(oldName))
                return false;

            if (map.ElementLibrary.ContainsKey(newName))
                return false; // New name already exists

            LibraryElement element = map.ElementLibrary[oldName];
            map.ElementLibrary.Remove(oldName);
            element.Name = newName;
            map.ElementLibrary[newName] = element;

            return true;
        }

        /// <summary>
        /// Delete an element from the library
        /// </summary>
        public static bool DeleteElement(AtariMap map, string elementName)
        {
            if (map == null || map.ElementLibrary == null || !map.ElementLibrary.ContainsKey(elementName))
                return false;

            map.ElementLibrary.Remove(elementName);
            return true;
        }

        /// <summary>
        /// Get all element names in the library
        /// </summary>
        public static List<string> GetElementNames(AtariMap map)
        {
            if (map == null || map.ElementLibrary == null)
                return new List<string>();

            return map.ElementLibrary.Keys.ToList();
        }
    }
}
