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
        
        // Tile index storage for tilemaps (stores tile indexes instead of individual chars)
        // If Use16BitIndexes is true, this stores ushort values (2 bytes per tile)
        // If false, Data array stores byte values (1 byte per tile)
        // For tilemaps, Data array represents tile grid, not character grid
        public ushort[] TileIndexes { get; set; }  // Optional: separate array for 16-bit indexes
        
        // Character data array for tilemaps (expanded from tiles for fast rendering)
        // This array stores the actual character data expanded from tile indexes
        // Size: (MapSize.Width * ScreenSize.Width * TileWidth) x (MapSize.Height * ScreenSize.Height * TileHeight)
        // When a tile is placed, its characters are expanded into this array
        public byte[] CharData { get; set; }  // Character data for tilemaps (for fast display)
        
        // Cached submap to avoid repeated parsing during rendering
        private AtariMap cachedSubmap;
        private string cachedSubmapPath;
        
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
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * screenCharHeight];
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
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int sourceScreenCharHeight = this.ScreenSize.Height;
            if (this.IsTilemap && this.TilemapInfo != null && this.TilemapInfo.TileHeight > 0)
            {
                sourceScreenCharHeight = this.ScreenSize.Height * this.TilemapInfo.TileHeight;
            }
            
            int targetScreenCharHeight = targetMap.ScreenSize.Height;
            if (targetMap.IsTilemap && targetMap.TilemapInfo != null && targetMap.TilemapInfo.TileHeight > 0)
            {
                targetScreenCharHeight = targetMap.ScreenSize.Height * targetMap.TilemapInfo.TileHeight;
            }
            
            // Use the minimum of source and target heights to avoid out-of-bounds
            int length = Math.Min(sourceScreenCharHeight, targetScreenCharHeight) * 5;
            int sourceOffset = localScreenNumber * sourceScreenCharHeight * 5;
            int destOffset = targetScreenNumber * targetScreenCharHeight * 5;
            for (int i = 0; i < length; i++)
                targetMap.ColorData[destOffset + i] = ColorData[sourceOffset + i];
        }

        public void InitDliColorFullMap()
        {
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            int totalLength = MapSize.Width * MapSize.Height * screenCharHeight * 5;
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
            get 
            { 
                // For tilemaps, offset is in character coordinates, so use CharStride
                if (IsTilemap && CharData != null)
                    return offset % CharStride;
                return offset % Stride; 
            }
        }

        public int OffsetY
        {
            get 
            { 
                // For tilemaps, offset is in character coordinates, so use CharStride
                if (IsTilemap && CharData != null)
                    return offset / CharStride;
                return offset / Stride; 
            }
        }

        /// <summary>
        /// Vrati pocet bytov tvoriacich 1 riadok v datach mapy
        /// For tilemaps: returns stride in tile units (ScreenSize.Width * MapSize.Width tiles)
        /// For normal maps: returns stride in character units (ScreenSize.Width * MapSize.Width chars)
        /// </summary>
        /// <returns></returns>
        public int Stride
        {
            get
            {
                return this.ScreenSize.Width * this.MapSize.Width;
            }
        }
        
        /// <summary>
        /// Get character stride for tilemaps (in character units)
        /// For tilemaps: returns ScreenSize.Width * MapSize.Width * TileWidth (chars per row)
        /// For normal maps: returns Stride (same as Stride property)
        /// </summary>
        public int CharStride
        {
            get
            {
                if (IsTilemap && TilemapInfo != null)
                {
                    return ScreenSize.Width * MapSize.Width * TilemapInfo.TileWidth;
                }
                return Stride;
            }
        }

        //set single color for multiple lines
        public void SetDliColorMultiple(int screenx, int screeny, int startingLine, int lines, int colorNumber, byte colorIndexFromPalette)
        {
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            int screenOffset = (screeny * MapSize.Width + screenx) * screenCharHeight * 5;
            if (lines < 0) lines = screenCharHeight - startingLine;
            if (startingLine + lines > screenCharHeight) throw new Exception($"The screen does not have that many ({lines}) lines.");
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
            // For tilemaps, use CharStride and convert ScreenSize from tiles to characters
            int stride = Stride;
            int screenCharWidth = ScreenSize.Width;
            int screenCharHeight = ScreenSize.Height;
            
            if (IsTilemap && TilemapInfo != null)
            {
                stride = CharStride;
                screenCharWidth = ScreenSize.Width * TilemapInfo.TileWidth;
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            int row = charOffset / stride;
            int column = charOffset % stride;
            int line = row % screenCharHeight;
            
            // Calculate screen coordinates (same formula as SetDliColor uses)
            int screenX = column / screenCharWidth;
            int screenY = row / screenCharHeight;
            
            // Use same offset calculation as SetDliColor: (screeny * MapSize.Width + screenx) * screenCharHeight * 5
            int screenOffset = (screenY * MapSize.Width + screenX) * screenCharHeight * 5;
            int dliOffset = screenOffset + line * 5;

            byte[] retValue = new byte[5];
            // Check bounds to prevent IndexOutOfRangeException
            if (ColorData != null && dliOffset + 4 < ColorData.Length)
            {
                for (int i = 0; i < 5; i++)
                    retValue[i] = ColorData[dliOffset + i];
            }
            else
            {
                // Return default colors if out of bounds
                for (int i = 0; i < 5; i++)
                    retValue[i] = Globals.DEFAULT_COLOR;
            }
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
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            int screenOffset = (screeny * MapSize.Width + screenx) * screenCharHeight * 5;
            if (lines < 0) lines = screenCharHeight - startingLine;
            if (startingLine + lines > screenCharHeight) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                for (int i = 0; i < 5; i++)
                    this.ColorData[screenOffset + (startingLine + j) * 5 + i] = color5[i];
        }

        public void SetDliColor(int screenx, int screeny, int line, int colorNumber, byte colorIndex)
        {
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            int screenOffset = (screeny * MapSize.Width + screenx) * screenCharHeight * 5;
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
            // For tilemaps, font numbers are read-only and come from submap tiles in column 0
            if (IsTilemap && TilemapInfo != null && !string.IsNullOrEmpty(SubmapPath))
            {
                try
                {
                    // Use cached submap to avoid repeated parsing
                    if (cachedSubmap == null || cachedSubmapPath != SubmapPath)
                    {
                        cachedSubmap = SubmapManager.LoadSubmap(SubmapPath);
                        cachedSubmapPath = SubmapPath;
                    }
                    
                    AtariMap submap = cachedSubmap;
                    int tileWidth = TilemapInfo.TileWidth;
                    int tileHeight = TilemapInfo.TileHeight;
                    
                    // Calculate which tile row this character line belongs to within the screen
                    int tileRowInScreen = line / tileHeight;
                    int lineInTile = line % tileHeight;
                    
                    // Get the tile at column 0 of this tile row within the screen
                    // Screen coordinates: screenx, screeny
                    // Tile coordinates within map: (screenx * ScreenSize.Width + 0, screeny * ScreenSize.Height + tileRowInScreen)
                    int mapTileX = screenx * ScreenSize.Width + 0; // Always column 0
                    int mapTileY = screeny * ScreenSize.Height + tileRowInScreen;
                    
                    // Get tile index from Data array (tilemap stores tile indexes, not characters)
                    int tileDataIndex = mapTileY * Stride + mapTileX;
                    if (tileDataIndex < 0 || tileDataIndex >= Data.Length)
                        return 0; // Out of bounds, default to font 0
                    
                    byte tileIndex = Data[tileDataIndex];
                    
                    // Get font from submap screen with this tile index
                    // Submap screens are numbered sequentially (row-major by default)
                    int submapScreenIndex = tileIndex;
                    int submapScreenX = submapScreenIndex % submap.MapSize.Width;
                    int submapScreenY = submapScreenIndex / submap.MapSize.Width;
                    
                    // Check if this submap screen references another screen for font mapping
                    Point actualSubmapScreen = submap.GetReferencedScreen(submapScreenX, submapScreenY);
                    int actualSubmapScreenX = actualSubmapScreen.X;
                    int actualSubmapScreenY = actualSubmapScreen.Y;
                    
                    // Clamp to valid range
                    if (actualSubmapScreenX < 0) actualSubmapScreenX = 0;
                    if (actualSubmapScreenX >= submap.MapSize.Width) actualSubmapScreenX = submap.MapSize.Width - 1;
                    if (actualSubmapScreenY < 0) actualSubmapScreenY = 0;
                    if (actualSubmapScreenY >= submap.MapSize.Height) actualSubmapScreenY = submap.MapSize.Height - 1;
                    
                    // Get font number for this line within the (possibly referenced) submap tile (screen)
                    if (submap.FontLineMappingPerScreen != null && submap.MapSize.Width > 0 && submap.MapSize.Height > 0)
                    {
                        int submapScreenOffset = (actualSubmapScreenY * submap.MapSize.Width + actualSubmapScreenX) * submap.ScreenSize.Height;
                        int submapIndex = submapScreenOffset + lineInTile;
                        if (submapIndex >= 0 && submapIndex < submap.FontLineMappingPerScreen.Length)
                            return submap.FontLineMappingPerScreen[submapIndex];
                    }
                    return 0; // Default to font 0
                }
                catch
                {
                    // If submap loading fails, fall through to normal font mapping
                }
            }
            
            // For normal maps (not tilemaps), use stored font mapping
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
            
            // For normal maps, ScreenSize.Height is already in character lines
            int screenCharHeight = ScreenSize.Height;
            
            int screenOffset = (actualScreenY * MapSize.Width + actualScreenX) * screenCharHeight;
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
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            if (FontLineMappingPerScreen == null)
                FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * screenCharHeight];
            
            int screenOffset = (actualScreenY * MapSize.Width + actualScreenX) * screenCharHeight;
            int index = screenOffset + line;
            if (index >= 0 && index < FontLineMappingPerScreen.Length && fontIndex < 8)
                FontLineMappingPerScreen[index] = fontIndex;
        }

        public void SetFontForAllLines(byte fontIndex)
        {
            // Set for all screens
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            if (FontLineMappingPerScreen == null)
                FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * screenCharHeight];
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
            
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int screenCharHeight = ScreenSize.Height;
            if (IsTilemap && TilemapInfo != null && TilemapInfo.TileHeight > 0)
            {
                screenCharHeight = ScreenSize.Height * TilemapInfo.TileHeight;
            }
            
            if (FontLineMappingPerScreen == null)
                FontLineMappingPerScreen = new byte[MapSize.Width * MapSize.Height * screenCharHeight];
            
            int screenOffset = (actualScreenY * MapSize.Width + actualScreenX) * screenCharHeight;
            for (int i = 0; i < screenCharHeight; i++)
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
        
        /// <summary>
        /// Clear the cached submap (call when SubmapPath changes)
        /// </summary>
        public void ClearSubmapCache()
        {
            cachedSubmap = null;
            cachedSubmapPath = null;
        }
        
        /// <summary>
        /// Initialize CharData array for tilemaps (expanded character data from tiles)
        /// CharData can only be initialized after TilemapInfo is set (tile dimensions from submap)
        /// </summary>
        public void InitializeCharData()
        {
            if (!IsTilemap || TilemapInfo == null || TilemapInfo.TileWidth == 0 || TilemapInfo.TileHeight == 0)
                return;
                
            int tileWidth = TilemapInfo.TileWidth;
            int tileHeight = TilemapInfo.TileHeight;
            
            // CharData size: (MapSize.Width * ScreenSize.Width * tileWidth) x (MapSize.Height * ScreenSize.Height * tileHeight)
            // MapSize: number of screens (e.g., 2x2)
            // ScreenSize: tiles per screen (e.g., 5x5)
            // Total tiles: (2 * 5) x (2 * 5) = 10x10 tiles
            // Total chars: (10 * tileWidth) x (10 * tileHeight) = 20x20 chars (for 2x2 tiles)
            int charWidth = MapSize.Width * ScreenSize.Width * tileWidth;
            int charHeight = MapSize.Height * ScreenSize.Height * tileHeight;
            
            CharData = new byte[charWidth * charHeight];
            
            // Initialize to all zeros (empty)
            for (int i = 0; i < CharData.Length; i++)
                CharData[i] = 0;
        }
        
        /// <summary>
        /// Expand a tile from submap into CharData at the specified tile position
        /// </summary>
        public void ExpandTileToCharData(int tileX, int tileY, byte tileIndex)
        {
            if (!IsTilemap || TilemapInfo == null || CharData == null)
                return;
                
            if (cachedSubmap == null || cachedSubmapPath != SubmapPath)
            {
                if (!string.IsNullOrEmpty(SubmapPath))
                {
                    cachedSubmap = SubmapManager.LoadSubmap(SubmapPath);
                    cachedSubmapPath = SubmapPath;
                }
                else
                    return;
            }
            
            AtariMap submap = cachedSubmap;
            int tileWidth = TilemapInfo.TileWidth;
            int tileHeight = TilemapInfo.TileHeight;
            
            // Get submap screen coordinates for this tile index
            int submapScreenX = tileIndex % submap.MapSize.Width;
            int submapScreenY = tileIndex / submap.MapSize.Width;
            
            // Calculate character position in CharData
            int charStride = MapSize.Width * ScreenSize.Width * tileWidth;
            int charStartX = tileX * tileWidth;
            int charStartY = tileY * tileHeight;
            
            // Copy tile data from submap to CharData
            for (int ty = 0; ty < tileHeight; ty++)
            {
                for (int tx = 0; tx < tileWidth; tx++)
                {
                    // Source: submap screen
                    int submapCharX = submapScreenX * submap.ScreenSize.Width + tx;
                    int submapCharY = submapScreenY * submap.ScreenSize.Height + ty;
                    int submapIndex = submapCharX + submapCharY * submap.Stride;
                    
                    // Destination: CharData
                    int charX = charStartX + tx;
                    int charY = charStartY + ty;
                    int charIndex = charX + charY * charStride;
                    
                    if (submapIndex < submap.Data.Length && charIndex < CharData.Length)
                    {
                        CharData[charIndex] = submap.Data[submapIndex];
                    }
                }
            }
        }
        
        /// <summary>
        /// Regenerate CharData from all tile indexes in the Data array
        /// This should be called after importing tile indexes to update the character representation
        /// </summary>
        public void RegenerateCharDataFromTiles()
        {
            if (!IsTilemap || TilemapInfo == null || Data == null)
                return;
                
            // First ensure CharData is initialized
            if (CharData == null)
            {
                InitializeCharData();
            }
            
            // Iterate through all tiles and expand each one
            int tileWidth = MapSize.Width * ScreenSize.Width;
            int tileHeight = MapSize.Height * ScreenSize.Height;
            
            for (int tileY = 0; tileY < tileHeight; tileY++)
            {
                for (int tileX = 0; tileX < tileWidth; tileX++)
                {
                    int tileIndex = tileY * Stride + tileX;
                    if (tileIndex < Data.Length)
                    {
                        byte tileIdx = Data[tileIndex];
                        ExpandTileToCharData(tileX, tileY, tileIdx);
                    }
                }
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
