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
        public int Value { get; set; }
        public byte Color { get; set; }
    }

    /// <summary>Holds a single copied metadata item for paste (CTRL+click to copy, click to paste).</summary>
    public static class MetadataItemClipboard
    {
        public static MetadataLayerItem CopiedItem { get; private set; }
        public static bool HasItem => CopiedItem != null;
        public static void Copy(MetadataLayerItem item)
        {
            if (item == null) { CopiedItem = null; return; }
            CopiedItem = new MetadataLayerItem { Text = item.Text ?? "", Value = item.Value, Color = item.Color };
        }
        public static void Clear() { CopiedItem = null; }
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
    }

    public class ScreenLink
    {
        public Point SourceScreen { get; set; }
        public Point LinkedScreen { get; set; }
        public float Transparency { get; set; }
    }
}
