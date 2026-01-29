using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AtariMapMaker
{
    [Serializable]
    public class AtariMap
    {
        public byte[] Data { get; set; }
        public Size ScreenSize { get; }    //velkost obrazovky v znakoch
        private Size dataSize;      //velkost dat v znakoch
        public Size MapSize { get; }       //pocet screenov v datach (velkost mapy)
        private int offset;
        public byte[] ColorData { get; set; }   //screens*lines*5 colors
        
        // New properties for v2.0+
        public byte[][] FontDataArray { get; set; }  // Multiple fonts (max 8)
        public string[] FontFileNames { get; set; }  // Font file names (one per slot)
        public byte[] FontLineMapping { get; set; }  // Font index per line (one per screen line)
        public bool FontTemplateLocked { get; set; }
        public string FontTemplatePattern { get; set; }
        public bool MultiFontEnabled { get; set; }  // Enable/disable multifont features
        public string MapDescription { get; set; }
        public Dictionary<string, string> ScreenDescriptions { get; set; }
        public Dictionary<string, ScreenMetadata> ScreenMetadata { get; set; }
        public string SubmapPath { get; set; }
        public bool IsTilemap { get; set; }
        public TilemapData TilemapInfo { get; set; }
        public BitmapTilesetData BitmapTileset { get; set; }
        public Dictionary<string, LibraryElement> ElementLibrary { get; set; }
        public List<ScreenLink> ScreenLinks { get; set; }
        
        public AtariMap(Size mapSize, Size screenSize)
        {
            this.MapSize = mapSize;
            this.ScreenSize = screenSize;
            this.dataSize = new Size(mapSize.Width * screenSize.Width, mapSize.Height * screenSize.Height);
            this.Data = new byte[dataSize.Width * dataSize.Height];
            InitDliColorFullMap();
            InitializeV2Properties();
        }
        
        private void InitializeV2Properties()
        {
            FontDataArray = new byte[8][];  // Max 8 fonts
            FontFileNames = new string[8];  // Font file names
            FontLineMapping = new byte[ScreenSize.Height];
            FontTemplateLocked = false;
            FontTemplatePattern = "All Font0";
            MultiFontEnabled = false;  // Default to single font mode
            MapDescription = "";
            ScreenDescriptions = new Dictionary<string, string>();
            ScreenMetadata = new Dictionary<string, ScreenMetadata>();
            SubmapPath = null;
            IsTilemap = false;
            TilemapInfo = null;
            BitmapTileset = null;
            ElementLibrary = new Dictionary<string, LibraryElement>();
            ScreenLinks = new List<ScreenLink>();
        }

        public void CopyDliColorsFullScreen(int localScreenNumber, AtariMap targetMap, int targetScreenNumber)
        {
            int length = this.ScreenSize.Height * 5;
            int sourceOffset = localScreenNumber * length;
            int destOffset = targetScreenNumber * length;
            for (int i = 0; i < length; i++)
                targetMap.ColorData[destOffset + i] = ColorData[sourceOffset + i];
        }

        public void InitDliColorFullMap()
        {
            int totalLength = MapSize.Width * MapSize.Height * ScreenSize.Height * 5;
            this.ColorData = new byte[totalLength];
            for (int i = 0; i < totalLength; i+=5)
                ColorData[i] = Globals.DEFAULT_COLOR; //0.color in each row indicates that no DLI was used 
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int OffsetX
        {
            get { return offset % Stride; }
        }

        public int OffsetY
        {
            get { return offset / Stride; }
        }

        /// <summary>
        /// Vrati pocet bytov tvoriacich 1 riadok v datach mapy
        /// </summary>
        /// <returns></returns>
        public int Stride
        {
            get
            {
                return this.ScreenSize.Width * this.MapSize.Width;
            }
        }

        //set single color for multiple lines
        public void SetDliColorMultiple(int screenx, int screeny, int startingLine, int lines, int colorNumber, byte colorIndexFromPalette)
        {
            int screenOffset = (screeny * MapSize.Width + screenx) * ScreenSize.Height * 5;
            if (lines < 0) lines = ScreenSize.Height - startingLine;
            if (startingLine + lines > ScreenSize.Height) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                this.ColorData[screenOffset + (startingLine + j) * 5 + colorNumber] = colorIndexFromPalette;
        }
        /// <summary>
        /// Get byte[5] color structure for given offset (char in AtariMap)
        /// </summary>
        /// <param name="charOffset"></param>
        /// <returns></returns>
        public byte[] GetDliColor5(int charOffset)
        {
            int row = charOffset / Stride;
            int column = charOffset % Stride;
            int line = row % ScreenSize.Height;
            int screenNumber = column / ScreenSize.Width + (row / ScreenSize.Height)*MapSize.Width;
            
            int dliOffset = screenNumber * ScreenSize.Height * 5 + line * 5;

            byte[] retValue = new byte[5];
            for (int i = 0; i < 5; i++)
                retValue[i] = ColorData[dliOffset + i];
            return retValue;
        }

        public byte[] GetDliColor5(Point clickedChar)
        {
            int charOffset = clickedChar.X + Stride*clickedChar.Y;
            return GetDliColor5(charOffset);
        }

        //set all colors multiple lines based on the given 5 colors
        public void SetDliColor5Multiple(int screenx, int screeny, int startingLine, int lines, byte[] color5)
        {
            int screenOffset = (screeny * MapSize.Width + screenx) * ScreenSize.Height * 5;
            if (lines < 0) lines = ScreenSize.Height - startingLine;
            if (startingLine + lines > ScreenSize.Height) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                for (int i = 0; i < 5; i++)
                    this.ColorData[screenOffset + (startingLine + j) * 5 + i] = color5[i];
        }

        public void SetDliColor(int screenx, int screeny, int line, int colorNumber, byte colorIndex)
        {
            int screenOffset = (screeny * MapSize.Width + screenx) * ScreenSize.Height * 5;
            this.ColorData[screenOffset + line * 5 + colorNumber] = colorIndex;
        }

        public void SwapChar(byte char1, byte char2, bool globalChange, Point screenToUse)
        {
            if (globalChange)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i] == char1) Data[i] = char2;
                    else if (Data[i] == char2) Data[i] = char1;
                }
            }
            else
            {
                int offset = Stride * ScreenSize.Height * screenToUse.Y + ScreenSize.Width * screenToUse.X;
                for (int y = 0; y < ScreenSize.Height; y++)
                    for (int x = 0; x < ScreenSize.Width; x++)
                    {
                        int i = offset + y * Stride + x;
                        if (Data[i] == char1) Data[i] = char2;
                        else if (Data[i] == char2) Data[i] = char1;
                    }
            }
        }

        public (int,int) CharOccurence(Point screenToAnalyze, int posx, int posy, byte charVal)
        {
            int index = 0;
            int count = 0;
            int offset = Stride * ScreenSize.Height * screenToAnalyze.Y + ScreenSize.Width * screenToAnalyze.X;
            for (int y = 0; y < ScreenSize.Height; y++)
                for (int x = 0; x < ScreenSize.Width; x++)
                {
                    if (y == posy && x == posx) index = count;
                    if (Data[offset + y * Stride + x] == charVal) count++;
                }
            return (index, count);
        }

        public void ClearScreen(Point screenToClear)
        {
            int offset = Stride * ScreenSize.Height * screenToClear.Y + ScreenSize.Width * screenToClear.X;
            for (int y = 0; y < ScreenSize.Height; y++)
                for (int x = 0; x < ScreenSize.Width; x++)
                    Data[offset + y * Stride + x] = 0;
        }

        public void FlipScreen(Point screenToFlip, bool horizontal)
        {
            int offset = Stride * ScreenSize.Height * screenToFlip.Y + ScreenSize.Width * screenToFlip.X;
            if (horizontal)
            {
                for (int y = 0; y < ScreenSize.Height; y++)
                    for (int x = 0; x < ScreenSize.Width / 2; x++)
                        (Data[offset + y * Stride + x], Data[offset + y * Stride + ScreenSize.Width - 1 - x]) = (Data[offset + y * Stride + ScreenSize.Width - 1 - x], Data[offset + y * Stride + x]);
            }
            else //vertical
            {
                for (int y = 0; y < ScreenSize.Height / 2 ; y++)
                    for (int x = 0; x < ScreenSize.Width; x++)
                        (Data[offset + y * Stride + x], Data[offset + (ScreenSize.Height - 1 - y) * Stride + x]) = (Data[offset + (ScreenSize.Height - 1 - y) * Stride + x], Data[offset + y * Stride + x]);

            }
        }

        // Font management methods
        public byte GetFontForLine(int line)
        {
            if (FontLineMapping == null || line < 0 || line >= FontLineMapping.Length)
                return 0;
            return FontLineMapping[line];
        }

        public void SetFontForLine(int line, byte fontIndex)
        {
            if (FontLineMapping == null)
                FontLineMapping = new byte[ScreenSize.Height];
            if (line >= 0 && line < FontLineMapping.Length && fontIndex < 8)
                FontLineMapping[line] = fontIndex;
        }

        public void SetFontForAllLines(byte fontIndex)
        {
            if (FontLineMapping == null)
                FontLineMapping = new byte[ScreenSize.Height];
            for (int i = 0; i < FontLineMapping.Length; i++)
                FontLineMapping[i] = fontIndex;
        }

        public int GetAvailableFontSlot()
        {
            if (FontDataArray == null)
                FontDataArray = new byte[8][];
            for (int i = 0; i < FontDataArray.Length; i++)
            {
                if (FontDataArray[i] == null)
                    return i;
            }
            return -1; // No available slot
        }

        public void SetFontData(byte[] fontData, int fontIndex, string fileName = null)
        {
            if (FontDataArray == null)
                FontDataArray = new byte[8][];
            if (FontFileNames == null)
                FontFileNames = new string[8];
            if (fontIndex >= 0 && fontIndex < FontDataArray.Length)
            {
                FontDataArray[fontIndex] = fontData;
                FontFileNames[fontIndex] = fileName;
            }
        }

        public void ClearFontSlot(int fontIndex)
        {
            if (FontDataArray != null && fontIndex >= 0 && fontIndex < FontDataArray.Length)
            {
                FontDataArray[fontIndex] = null;
                if (FontFileNames != null)
                    FontFileNames[fontIndex] = null;
            }
        }

        public byte[] GetFontData(int fontIndex)
        {
            if (FontDataArray == null || fontIndex < 0 || fontIndex >= FontDataArray.Length)
                return null;
            return FontDataArray[fontIndex];
        }
    }
}
