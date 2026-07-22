using System;
using System.Collections.Generic;
using System.Drawing;

namespace AtariMapMaker
{
    public class ScreenReference
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        public ScreenReference() { }
        public ScreenReference(int x, int y) { X = x; Y = y; }
        
        public static implicit operator Point(ScreenReference r) => new Point(r.X, r.Y);
        public static implicit operator ScreenReference(Point p) => new ScreenReference(p.X, p.Y);
    }
    public class ScreenMetadata
    {
        /// <summary>Metadata elements for this screen (user-defined via map layer or list dialog).</summary>
        public List<MetadataLayerItem> ParsedItems { get; set; }

        public ScreenMetadata()
        {
            ParsedItems = new List<MetadataLayerItem>();
        }
    }

    public class MetadataLayerItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        /// <summary>Global metadata object type byte; <see cref="AtariMap.MetadataTypeLabels"/> defines the label for this type.</summary>
        public byte Type { get; set; }
        public int Value { get; set; }
        public byte Color { get; set; }
    }

    /// <summary>Holds a single copied metadata item for paste (CTRL+click copy, click paste) or move (ALT+click grab, one click to place).</summary>
    public static class MetadataItemClipboard
    {
        public static MetadataLayerItem CopiedItem { get; private set; }
        public static bool HasItem => CopiedItem != null;
        /// <summary>When true, the next place removes the item from the source cell once (move), not duplicate paste.</summary>
        public static bool IsMovePending { get; private set; }
        public static int MoveSourceScreenX { get; private set; }
        public static int MoveSourceScreenY { get; private set; }
        public static int MoveSourceCellX { get; private set; }
        public static int MoveSourceCellY { get; private set; }

        public static void Copy(MetadataLayerItem item)
        {
            if (item == null) { CopiedItem = null; ClearMove(); return; }
            CopiedItem = new MetadataLayerItem { Text = item.Text ?? "", Type = item.Type, Value = item.Value, Color = item.Color };
            ClearMove();
        }

        /// <summary>Grab metadata for a single move; next click without CTRL places it and clears.</summary>
        public static void BeginMove(MetadataLayerItem item, int screenX, int screenY, int cellX, int cellY)
        {
            if (item == null) return;
            CopiedItem = new MetadataLayerItem { Text = item.Text ?? "", Type = item.Type, Value = item.Value, Color = item.Color };
            IsMovePending = true;
            MoveSourceScreenX = screenX;
            MoveSourceScreenY = screenY;
            MoveSourceCellX = cellX;
            MoveSourceCellY = cellY;
        }

        static void ClearMove()
        {
            IsMovePending = false;
            MoveSourceScreenX = MoveSourceScreenY = MoveSourceCellX = MoveSourceCellY = 0;
        }

        public static void Clear()
        {
            CopiedItem = null;
            ClearMove();
        }
    }

    /// <summary>Multi-item clipboard for Screen Metadata dialog Copy/Paste between screens.</summary>
    public static class ScreenMetadataListClipboard
    {
        private static readonly List<MetadataLayerItem> items = new List<MetadataLayerItem>();

        public static int Count => items.Count;
        public static IReadOnlyList<MetadataLayerItem> Items => items;

        public static void Copy(IEnumerable<MetadataLayerItem> source)
        {
            items.Clear();
            if (source == null) return;
            foreach (var item in source)
            {
                if (item == null) continue;
                items.Add(new MetadataLayerItem
                {
                    X = item.X,
                    Y = item.Y,
                    Text = item.Text ?? "",
                    Type = item.Type,
                    Value = item.Value,
                    Color = item.Color
                });
            }
        }

        public static void Clear()
        {
            items.Clear();
        }
    }

    public class TilemapData
    {
        public int TileWidth { get; set; }  // In characters
        public int TileHeight { get; set; }
        public Dictionary<string, TileInfo> TileByteValues { get; set; }  // Key: "x,y"
        public string NumberingPattern { get; set; }  // "row-major", "column-major", etc.
        public bool ShowByteOverlay { get; set; }
        public float ByteOverlayTransparency { get; set; }
        public bool Use16BitIndexes { get; set; }  // If true, use 16-bit tile indexes (supports >256 tiles)

        public TilemapData()
        {
            TileByteValues = new Dictionary<string, TileInfo>();
            NumberingPattern = "row-major";
            ShowByteOverlay = false;
            ByteOverlayTransparency = 0.5f;
            Use16BitIndexes = false;  // Default to 8-bit (256 tiles max)
        }
    }

    public class TileInfo
    {
        public byte ByteValue1 { get; set; }
        public ushort ByteValue2 { get; set; }
    }

    public class BitmapTilesetData
    {
        public int TileWidth { get; set; }  // In pixels
        public int TileHeight { get; set; }
        public byte[] BitmapData { get; set; }  // Raw pixel data
        public int BitmapWidth { get; set; }
        public int BitmapHeight { get; set; }
    }

    public class LibraryElement
    {
        public string Name { get; set; }
        public Size Size { get; set; }
        public byte[] Data { get; set; }
        /// <summary>True when element was saved from a tilemap (clipboard had tile indexes).</summary>
        public bool IsTileData { get; set; }
    }

    public class ScreenLink
    {
        public Point SourceScreen { get; set; }
        public Point LinkedScreen { get; set; }
        public float Transparency { get; set; }
    }
}
