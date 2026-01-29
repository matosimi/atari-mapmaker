using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AtariMapMaker
{
    public static class ElementLibraryManager
    {
        /// <summary>
        /// Save a selection to the element library
        /// </summary>
        public static void SaveToLibrary(AtariMap map, Rectangle selection, string name)
        {
            if (map == null || string.IsNullOrEmpty(name))
                return;

            if (map.ElementLibrary == null)
                map.ElementLibrary = new Dictionary<string, LibraryElement>();

            // Extract data from selection
            byte[] data = ExtractSelectionData(map, selection);
            Size size = new Size(selection.Width, selection.Height);

            LibraryElement element = new LibraryElement
            {
                Name = name,
                Size = size,
                Data = data
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
                            map.Data[destIndex] = value;
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
