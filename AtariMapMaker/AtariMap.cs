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
        public byte[] FontLineMappingPerScreen { get; set; }  // Font index per line (per-screen: MapSize.Width * MapSize.Height * ScreenSize.Height)
        public Dictionary<string, ScreenReference> FontLineMappingReferences { get; set; }  // Key: "x,y" -> Value: referenced screen coordinates (x,y)
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
            FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * ScreenSize.Height];
            FontLineMappingReferences = new Dictionary<string, ScreenReference>();
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
            
            // Initialize all screens to reference screen 0,0 by default
            for (int sy = 0; sy < MapSize.Height; sy++)
            {
                for (int sx = 0; sx < MapSize.Width; sx++)
                {
                    string key = $"{sx},{sy}";
                    FontLineMappingReferences[key] = new ScreenReference(0, 0);
                }
            }
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
        // Check if a screen references another screen's font mapping
        private Point GetReferencedScreen(int screenx, int screeny)
        {
            if (FontLineMappingReferences == null)
                return new Point(screenx, screeny);  // No reference, use own screen
            
            string key = $"{screenx},{screeny}";
            if (FontLineMappingReferences.ContainsKey(key))
            {
                ScreenReference refScreen = FontLineMappingReferences[key];
                return new Point(refScreen.X, refScreen.Y);
            }
            
            return new Point(screenx, screeny);  // No reference, use own screen
        }

        // Set whether a screen references another screen
        public void SetFontMappingReference(int screenx, int screeny, bool useReference, int refScreenX, int refScreenY)
        {
            if (FontLineMappingReferences == null)
                FontLineMappingReferences = new Dictionary<string, ScreenReference>();
            
            string key = $"{screenx},{screeny}";
            if (useReference)
            {
                FontLineMappingReferences[key] = new ScreenReference(refScreenX, refScreenY);
            }
            else
            {
                FontLineMappingReferences.Remove(key);
            }
        }

        // Get whether a screen references another screen
        public bool GetFontMappingReference(int screenx, int screeny, out int refScreenX, out int refScreenY)
        {
            refScreenX = screenx;
            refScreenY = screeny;
            
            if (FontLineMappingReferences == null)
                return false;
            
            string key = $"{screenx},{screeny}";
            if (FontLineMappingReferences.ContainsKey(key))
            {
                ScreenReference refScreen = FontLineMappingReferences[key];
                if (refScreen.X != screenx || refScreen.Y != screeny)
                {
                    refScreenX = refScreen.X;
                    refScreenY = refScreen.Y;
                    return true;
                }
            }
            
            return false;
        }

        // Legacy method for backward compatibility - defaults to screen 0,0
        public byte GetFontForLine(int line)
        {
            return GetFontForLine(0, 0, line);
        }

        // Get font for a line, checking references
        public byte GetFontForLine(int screenx, int screeny, int line)
        {
            // Check if this screen references another screen
            Point actualScreen = GetReferencedScreen(screenx, screeny);
            int actualScreenX = actualScreen.X;
            int actualScreenY = actualScreen.Y;
            
            // Clamp to valid range
            if (actualScreenX < 0) actualScreenX = 0;
            if (actualScreenX >= MapSize.Width) actualScreenX = MapSize.Width - 1;
            if (actualScreenY < 0) actualScreenY = 0;
            if (actualScreenY >= MapSize.Height) actualScreenY = MapSize.Height - 1;
            
            // Get font from the actual screen (which may be the referenced one)
            if (FontLineMappingPerScreen == null)
                return 0;
            
            int screenOffset = (actualScreenY * MapSize.Width + actualScreenX) * ScreenSize.Height;
            int index = screenOffset + line;
            if (index >= 0 && index < FontLineMappingPerScreen.Length)
                return FontLineMappingPerScreen[index];
            return 0;
        }

        // Legacy method for backward compatibility - defaults to screen 0,0
        public void SetFontForLine(int line, byte fontIndex)
        {
            SetFontForLine(0, 0, line, fontIndex);
        }

        // Set font for a line, checking references
        // Note: When a screen references another, setting will modify the referenced screen's data
        public void SetFontForLine(int screenx, int screeny, int line, byte fontIndex)
        {
            // Check if this screen references another screen
            Point actualScreen = GetReferencedScreen(screenx, screeny);
            int actualScreenX = actualScreen.X;
            int actualScreenY = actualScreen.Y;
            
            // Clamp to valid range
            if (actualScreenX < 0) actualScreenX = 0;
            if (actualScreenX >= MapSize.Width) actualScreenX = MapSize.Width - 1;
            if (actualScreenY < 0) actualScreenY = 0;
            if (actualScreenY >= MapSize.Height) actualScreenY = MapSize.Height - 1;
            
            // Set font in the actual screen (which may be the referenced one)
            if (FontLineMappingPerScreen == null)
                FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * ScreenSize.Height];
            
            int screenOffset = (actualScreenY * MapSize.Width + actualScreenX) * ScreenSize.Height;
            int index = screenOffset + line;
            if (index >= 0 && index < FontLineMappingPerScreen.Length && fontIndex < 8)
                FontLineMappingPerScreen[index] = fontIndex;
        }

        public void SetFontForAllLines(byte fontIndex)
        {
            // Set for all screens
            if (FontLineMappingPerScreen == null)
                FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * ScreenSize.Height];
            for (int i = 0; i < FontLineMappingPerScreen.Length; i++)
                FontLineMappingPerScreen[i] = fontIndex;
        }

        // Set font for all lines in a specific screen (respects references)
        public void SetFontForAllLinesInScreen(int screenx, int screeny, byte fontIndex)
        {
            // Check if this screen references another screen
            Point actualScreen = GetReferencedScreen(screenx, screeny);
            int actualScreenX = actualScreen.X;
            int actualScreenY = actualScreen.Y;
            
            // Clamp to valid range
            if (actualScreenX < 0) actualScreenX = 0;
            if (actualScreenX >= MapSize.Width) actualScreenX = MapSize.Width - 1;
            if (actualScreenY < 0) actualScreenY = 0;
            if (actualScreenY >= MapSize.Height) actualScreenY = MapSize.Height - 1;
            
            if (FontLineMappingPerScreen == null)
                FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * ScreenSize.Height];
            
            int screenOffset = (actualScreenY * MapSize.Width + actualScreenX) * ScreenSize.Height;
            for (int i = 0; i < ScreenSize.Height; i++)
            {
                int index = screenOffset + i;
                if (index >= 0 && index < FontLineMappingPerScreen.Length)
                    FontLineMappingPerScreen[index] = fontIndex;
            }
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
