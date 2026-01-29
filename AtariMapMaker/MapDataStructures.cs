using System;
using System.Collections.Generic;
using System.Drawing;

namespace AtariMapMaker
{
    public class ScreenMetadata
    {
        public string RawText { get; set; }
        public string RegexPattern { get; set; }
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

    public class TilemapData
    {
        public int TileWidth { get; set; }  // In characters
        public int TileHeight { get; set; }
        public Dictionary<string, TileInfo> TileByteValues { get; set; }  // Key: "x,y"
        public string NumberingPattern { get; set; }  // "row-major", "column-major", etc.
        public bool ShowByteOverlay { get; set; }
        public float ByteOverlayTransparency { get; set; }

        public TilemapData()
        {
            TileByteValues = new Dictionary<string, TileInfo>();
            NumberingPattern = "row-major";
            ShowByteOverlay = false;
            ByteOverlayTransparency = 0.5f;
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
