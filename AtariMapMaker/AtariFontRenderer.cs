using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace AtariMapMaker
{
    public struct AtariFont
    {
        public byte[] data;
        public Bitmap bitmap;
    }
    public static class AtariFontRenderer
    {
        private static readonly byte[] COLOR5 = { 40, 202, 148, 70, 0 };
        // ALPA: PF0–3, BAK, PF3 alter, PF0 alter, PF2 alter, PF1 alter (PF1 alter defaults to PF1)
        private static readonly byte[] COLOR9 = { 40, 202, 148, 70, 0, 80, 15, 52, 148 };
        private static byte[] color5 = COLOR5;
        private static string lastFontFile;
        public static readonly Dictionary<Globals.FontType, AtariFont> fonts = new Dictionary<Globals.FontType, AtariFont>();
        private static readonly Dictionary<int, AtariFont> cachedFonts = new Dictionary<int, AtariFont>();  // Cache for font index -> font bitmap
        private static bool useDli;

        /// <summary>When set, CharPicker window uses this font index from CharPickerFontSourceMap instead of default.</summary>
        public static int? CharPickerFontIndex { get; set; }
        public static AtariMap CharPickerFontSourceMap { get; set; }
        /// <summary>When set, CharPicker rendering uses these colors instead of the global Color5 palette.</summary>
        public static byte[] CharPickerColorOverride { get; set; }
        public static string LastFontFile
        {
            get { return lastFontFile; }
            set { lastFontFile = value; }
        }

        public static void SetAlpa(bool doIt = true)
        {
            color5 = doIt ? (byte[])COLOR9.Clone() : (byte[])COLOR5.Clone();
        }

        /// <summary>
        /// Color5 index of the ALPA alternate for playfield 0–3, or -1 if none (e.g. BAK).
        /// Mapping: PF0→6, PF1→8, PF2→7, PF3→5.
        /// </summary>
        public static int GetAlpaAlternateColorIndex(int playfieldIndex)
        {
            switch (playfieldIndex)
            {
                case 0: return 6;
                case 1: return 8;
                case 2: return 7;
                case 3: return 5;
                default: return -1;
            }
        }

        /// <summary>Current global ALPA alternate for PF0–3; if missing (old 8-color maps), returns primary PF1 for PF1 alter.</summary>
        public static byte GetAlpaAlternateColor(int playfieldIndex)
        {
            int alt = GetAlpaAlternateColorIndex(playfieldIndex);
            if (alt < 0)
                return color5[Math.Max(0, Math.Min(playfieldIndex, color5.Length - 1))];
            if (alt < color5.Length)
                return color5[alt];
            // PF1 alter not present: same as normal PF1
            if (playfieldIndex == 1 && color5.Length > 1)
                return color5[1];
            return color5[Math.Min(playfieldIndex, color5.Length - 1)];
        }

        /// <summary>
        /// Ensures ALPA palettes have PF1 alter (index 8). Older .atrmap files with 8 colors
        /// get PF1 alter implied as the same value as normal PF1.
        /// </summary>
        public static void NormalizeAlpaColors()
        {
            if (color5 == null || color5.Length <= 5 || color5.Length >= 9)
                return;
            byte[] expanded = new byte[9];
            Array.Copy(COLOR9, expanded, 9);
            Array.Copy(color5, expanded, color5.Length);
            if (color5.Length < 9)
                expanded[8] = color5.Length > 1 ? color5[1] : COLOR9[1];
            color5 = expanded;
        }
        public static bool UseDli { get { return useDli; } set { useDli = value; } }

        public static void RedrawFontImage(Globals.FontType fontType)
        {
            AtariFont font = fonts[fontType];
            font.bitmap = CreateFontImage(true, font.data);
            fonts[fontType] = font;
        }
        public static void SetFontData(byte[] data, Globals.FontType fontType)
        {
            if (fonts.ContainsKey(fontType))
                fonts.Remove(fontType);

            AtariFont atariFont = new AtariFont
            {
                data = data,
                bitmap = CreateFontImage(true, data)
            };
            fonts.Add(fontType, atariFont);
        }

        public static byte[] Color5
        {
            get
            {
                return color5;
            }
            set
            {
                color5 = value;
                NormalizeAlpaColors();
            }
        }

        public static void LoadFont(String fontname, Globals.FontType fontType)
        {
            byte[] fontData = new byte[1024 * 2];
            FileStream fs = new FileStream(fontname, FileMode.Open);
            fs.Read(fontData, 0, 1024);
            fs.Close();
            for (int a = 0; a < 1024; a++)
            {
                fontData[a + 1024] = (byte)(fontData[a] ^ 0x80);
            }
            lastFontFile = fontname;
            SetFontData(fontData, fontType);
        }

        /// <summary>
        /// Calculate new offset based on the difference of mouse down and mouse move coordinates 
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <param name="myMap"></param>
        /// <returns></returns>
        public static bool CalculateOffset(int deltaX, int deltaY, AtariMap myMap)
        {
            if (deltaX == 0 && deltaY == 0) //small movement (no movement)
                return false;

            int subX = myMap.OffsetX - deltaX;
            int subY = myMap.OffsetY - deltaY;

            //compensation of top/left bounds
            if (subX < 0)
                deltaX = myMap.OffsetX;
            if (subY < 0)
                deltaY = myMap.OffsetY;

            // For tilemaps, use CharStride (character units) instead of Stride (tile units)
            // because OffsetX/OffsetY are in character units for rendering CharData
            int stride = myMap.Stride;
            if (myMap.IsTilemap && myMap.CharStride > 0)
            {
                stride = myMap.CharStride;
            }

            int offsetChange = deltaX + deltaY * stride;
            myMap.Offset -= offsetChange;
            return offsetChange != 0;
        }

        //8bpp indexed
        private static Bitmap CreateFontImage(bool colorMode, byte[] fontData) //2 or 4
        {
            Bitmap bmp = new Bitmap(256 * 8, 8, PixelFormat.Format8bppIndexed)
            {
                Palette = AtariPalette.GetIndexedColor5Palette()
            };

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, 8 * 256, 8), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            for (int y = 0; y < bmd.Height; y++)
            {
                unsafe
                {
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);

                    if (colorMode)
                    {
                        //color gr.12 (dl 4)
                        for (int x = 0; x < (bmd.Stride >> 3); x++)
                        {
                            byte value = fontData[((x << 3) + y) % 1024]; // /8 
                            byte point;
                            for (byte o = 3; o != 255; o--)
                            {
                                point = (byte)((value & 0x03) - 1);
                                value >>= 2;
                                if (point == 255)
                                    point = 4;
                                if (x > 127 && point == 2)
                                    point = 3;

                                row[(x << 3) + (o << 1)] = point; //COLOR5[point];
                                row[(x << 3) + (o << 1) + 1] = point; //COLOR5[point];
                            }
                        }
                    }
                    else
                    {
                        //mono gr.0 (dl 2)
                        for (int x = 0; x < (bmd.Stride >> 3); x++) // /8
                        {
                            byte value = fontData[(x << 3) + y]; // /8 
                            byte point;
                            for (byte o = 7; o != 255; o--)
                            {
                                point = (byte)(value & 0x01);
                                value >>= 1;
                                row[(x << 3) + o] = COLOR5[point];
                            }
                        }
                    }
                }

            }
            bmp.UnlockBits(bmd);
            return bmp;
        }

        /// <summary>
        /// Resolve font bitmap for a font slot index (0–7), with caching and fallbacks.
        /// </summary>
        private static AtariFont GetFontByIndex(AtariMap myMap, byte fontIndex, Globals.FontType fontType)
        {
            if (!myMap.MultiFontEnabled)
                return fonts[fontType];

            fontIndex = (byte)(fontIndex & 0x07);

            if (myMap.FontDataArray != null && fontIndex < myMap.FontDataArray.Length && myMap.FontDataArray[fontIndex] != null)
            {
                if (cachedFonts.ContainsKey(fontIndex))
                    return cachedFonts[fontIndex];

                AtariFont customFont = new AtariFont
                {
                    data = myMap.FontDataArray[fontIndex],
                    bitmap = CreateFontImage(true, myMap.FontDataArray[fontIndex])
                };
                cachedFonts[fontIndex] = customFont;
                return customFont;
            }

            // Font slot undefined — inherit from font 0, then default screen font
            if (myMap.FontDataArray != null && myMap.FontDataArray[0] != null)
            {
                if (cachedFonts.ContainsKey(0))
                    return cachedFonts[0];

                AtariFont font0 = new AtariFont
                {
                    data = myMap.FontDataArray[0],
                    bitmap = CreateFontImage(true, myMap.FontDataArray[0])
                };
                cachedFonts[0] = font0;
                return font0;
            }
            return fonts[fontType];
        }

        /// <summary>
        /// Get font bitmap for a specific line, supporting per-line font selection
        /// </summary>
        private static AtariFont GetFontForLine(AtariMap myMap, int screenx, int screeny, int line, Globals.FontType fontType)
        {
            if (!myMap.MultiFontEnabled)
                return fonts[fontType];
            byte fontIndex = myMap.GetFontForLine(screenx, screeny, line);
            return GetFontByIndex(myMap, fontIndex, fontType);
        }

        /// <summary>
        /// Clear cached fonts (call when fonts are updated)
        /// </summary>
        public static void ClearFontCache()
        {
            foreach (var font in cachedFonts.Values)
            {
                if (font.bitmap != null)
                    font.bitmap.Dispose();
            }
            cachedFonts.Clear();
        }

        /// <summary>
        /// Renders clipboard character data to an 8bpp bitmap.
        /// When <paramref name="fontMap"/> and <paramref name="fontIndices"/> are set, uses per-cell fonts from the map.
        /// When <paramref name="screenColors"/> is set (length ≥ 5), palette indices 0–4 map to those Atari colors
        /// (active screen colors); otherwise uses the global Color5 palette via GetIndexedColor5Palette.
        /// </summary>
        public static void RenderClipboardData(byte[,] data, int width, int height, Bitmap outBmp,
            AtariMap fontMap = null, byte[,] fontIndices = null, byte[] screenColors = null)
        {
            if (data == null || outBmp.PixelFormat != PixelFormat.Format8bppIndexed)
                return;
            if (outBmp.Width < width * 8 || outBmp.Height < height * 8)
                return;

            bool usePerCellFonts = fontMap != null && fontMap.MultiFontEnabled && fontIndices != null
                && fontMap.FontDataArray != null;

            // Build display palette from screen colors or global Color5
            if (screenColors != null && screenColors.Length >= 5)
            {
                ColorPalette pal = outBmp.Palette;
                for (int i = 0; i < 5; i++)
                    pal.Entries[i] = AtariPalette.GetColor(screenColors[i]);
                outBmp.Palette = pal;
            }
            else
            {
                outBmp.Palette = AtariPalette.GetIndexedColor5Palette();
            }

            AtariFont defaultFont = fonts[Globals.FontType.Screen];
            BitmapData outData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            BitmapData fntData = null;
            Bitmap lockedFontBmp = defaultFont.bitmap;
            int lastFontIdx = -1;
            try
            {
                fntData = lockedFontBmp.LockBits(new Rectangle(0, 0, lockedFontBmp.Width, lockedFontBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                unsafe
                {
                    byte* outRow = (byte*)outData.Scan0;
                    for (int cy = 0; cy < height; cy++)
                    {
                        for (int py = 0; py < 8; py++)
                        {
                            for (int cx = 0; cx < width; cx++)
                            {
                                bool hMirror = false;
                                if (usePerCellFonts)
                                {
                                    byte fontByte = fontIndices[cx, cy];
                                    int needed = fontByte & AtariMap.CharsetIndexMask;
                                    hMirror = (fontByte & AtariMap.CharsetMirrorFlag) != 0;
                                    if (needed != lastFontIdx)
                                    {
                                        AtariFont nf = GetFontByIndex(fontMap, (byte)needed, Globals.FontType.Screen);
                                        if (nf.bitmap != lockedFontBmp)
                                        {
                                            lockedFontBmp.UnlockBits(fntData);
                                            fntData = null;
                                            lockedFontBmp = nf.bitmap;
                                            fntData = lockedFontBmp.LockBits(new Rectangle(0, 0, lockedFontBmp.Width, lockedFontBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                                        }
                                        lastFontIdx = needed;
                                    }
                                }

                                byte* fntRow = (byte*)fntData.Scan0 + (py * fntData.Stride);
                                byte charValue = data[cx, cy];
                                if (charValue * 8 + 7 >= fntData.Width)
                                    charValue = 0;
                                for (int px = 0; px < 8; px++)
                                {
                                    int srcPx = hMirror ? (7 - px) : px;
                                    byte pixel = fntRow[charValue * 8 + srcPx];
                                    outRow[(cy * 8 + py) * outData.Stride + cx * 8 + px] = pixel;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (fntData != null && lockedFontBmp != null)
                {
                    try { lockedFontBmp.UnlockBits(fntData); } catch { }
                }
                outBmp.UnlockBits(outData);
            }
        }

        /// <summary>
        /// Try avoid calling this method outside AtariPictureTools
        /// outBmp has to be already properly sized (windowCharsHorizontal*8, windowCharsVertical*8)
        /// </summary>
        /// <param name="myMap"></param>
        /// <param name="adrOffset"></param>
        /// <param name="font"></param>
        /// <param name="outBmp"></param>
        public static void RenderMapData(AtariMap myMap, Globals.FontType fontType, Bitmap outBmp, Globals.WindowType? windowType = null)
        {
            byte offMapColor = (color5[4] & 0x0f) < 0x04 ? (byte)(color5[4] + 0x04) : (byte)(color5[4] - 0x04);
            if (outBmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new Exception("Output bitmap of RenderMapData MUST be 8bppIndexed palette!");

            bool useCharPickerFont = (windowType == Globals.WindowType.CharPicker && CharPickerFontIndex != null && CharPickerFontSourceMap != null &&
                CharPickerFontSourceMap.FontDataArray != null && CharPickerFontIndex.Value >= 0 && CharPickerFontIndex.Value < CharPickerFontSourceMap.FontDataArray.Length &&
                CharPickerFontSourceMap.FontDataArray[CharPickerFontIndex.Value] != null);

            AtariFont defaultFont = fonts[fontType];
            if (useCharPickerFont)
            {
                int idx = CharPickerFontIndex.Value;
                if (cachedFonts.ContainsKey(idx))
                    defaultFont = cachedFonts[idx];
                else
                {
                    var cf = new AtariFont { data = CharPickerFontSourceMap.FontDataArray[idx], bitmap = CreateFontImage(true, CharPickerFontSourceMap.FontDataArray[idx]) };
                    cachedFonts[idx] = cf;
                    defaultFont = cf;
                }
            }

            // For tilemaps, use CharData array (expanded character data) instead of Data array (tile indexes)
            byte[] data;
            if (myMap.IsTilemap && myMap.CharData != null && myMap.CharData.Length > 0)
            {
                data = myMap.CharData;
                // For CharData, stride is in character units: MapSize.Width * ScreenSize.Width * TileWidth
                // But we need to use the character stride, not tile stride
                // Offset is already in character coordinates for CharData
            }
            else
            {
                data = myMap.Data;
            }
            
            int adrOffset = myMap.Offset;
            if (adrOffset < 0)
            {
                return;
            }

            int width = outBmp.Width / 8;
            int height = outBmp.Height / 8;
            int widthFull = width;
            int heightFull = height;
            
            // For tilemaps using CharData, stride is in character units
            int charStride = myMap.CharStride;
            int charHeight = myMap.MapSize.Height * myMap.ScreenSize.Height;
            
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                // For tilemaps, ScreenSize is in tiles, so multiply by tile dimensions
                charHeight = myMap.MapSize.Height * myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            
            if (myMap.OffsetX + width > charStride)
            {
                width = charStride - myMap.OffsetX;
            }
            if (myMap.OffsetY + height > charHeight)
            {
                height = charHeight - myMap.OffsetY;
                height = Math.Max(height, 0);
            }

            BitmapData bmd = null;
            BitmapData fntd = null;
            Bitmap lockedFontBmp = null;
            AtariFont currentFont = defaultFont;
            byte[] activeColors = color5;
            if (windowType == Globals.WindowType.CharPicker && CharPickerColorOverride != null && CharPickerColorOverride.Length > 0)
                activeColors = CharPickerColorOverride;
            try
            {
                bmd = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                int index;

                unsafe
                {
                    byte* row = (byte*)bmd.Scan0;
                    lockedFontBmp = currentFont.bitmap;
                    fntd = lockedFontBmp.LockBits(new Rectangle(0, 0, lockedFontBmp.Width, lockedFontBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                    
                    // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
                    int screenCharHeight = myMap.ScreenSize.Height;
                    if (myMap.IsTilemap && myMap.TilemapInfo != null && myMap.TilemapInfo.TileHeight > 0)
                    {
                        screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                    }
                    
                    bool freeCharmap = myMap.FreeCharmapMode && myMap.CharFontData != null && !myMap.IsTilemap
                        && windowType != Globals.WindowType.CharPicker;

                    for (int y = 0; y < height; y++)
                    {
                        // Determine which font to use for this line
                        int absoluteLine = myMap.OffsetY + y;
                        int lineInScreen = absoluteLine % screenCharHeight;
                        int screenY = absoluteLine / screenCharHeight;
                        // Clamp screen Y to valid range
                        if (screenY < 0) screenY = 0;
                        if (screenY >= myMap.MapSize.Height) screenY = myMap.MapSize.Height - 1;
                        
                        // Track current screen X / font slot to detect when font must change
                        int lastScreenX = -1;
                        int lastFontIndex = -1;
                        
                        // For tilemaps, ScreenSize.Width is in tiles, so convert to character lines
                        int screenCharWidth = myMap.ScreenSize.Width;
                        if (myMap.IsTilemap && myMap.TilemapInfo != null && myMap.TilemapInfo.TileWidth > 0)
                        {
                            screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                        }
                        
                        for (int scln = 0; scln < 8; scln++)
                        {
                            byte* fntRow = (byte*)fntd.Scan0 + (scln * fntd.Stride);
                            for (int x = 0; x < width; x++)
                            {
                                // Calculate which screen this character belongs to
                                int absoluteX = myMap.OffsetX + x;
                                int screenX = absoluteX / screenCharWidth;
                                // Clamp screen X to valid range
                                if (screenX < 0) screenX = 0;
                                if (screenX >= myMap.MapSize.Width) screenX = myMap.MapSize.Width - 1;

                                index = adrOffset + x;
                                bool hMirror = false;

                                // Switch font: per-cell in free charmap mode, else per screen/line
                                if (!useCharPickerFont)
                                {
                                    int neededFontIndex = -1;
                                    if (freeCharmap)
                                    {
                                        if (index >= 0 && index < myMap.CharFontData.Length)
                                        {
                                            byte fontByte = myMap.CharFontData[index];
                                            neededFontIndex = fontByte & AtariMap.CharsetIndexMask;
                                            hMirror = (fontByte & AtariMap.CharsetMirrorFlag) != 0;
                                        }
                                        else
                                            neededFontIndex = 0;
                                    }
                                    else if (screenX != lastScreenX)
                                    {
                                        neededFontIndex = myMap.GetFontForLine(screenX, screenY, lineInScreen);
                                        lastScreenX = screenX;
                                    }

                                    if (neededFontIndex >= 0 && neededFontIndex != lastFontIndex)
                                    {
                                        AtariFont lineFont = GetFontByIndex(myMap, (byte)neededFontIndex, fontType);
                                        if (lineFont.bitmap != currentFont.bitmap)
                                        {
                                            lockedFontBmp.UnlockBits(fntd);
                                            fntd = null;
                                            currentFont = lineFont;
                                            lockedFontBmp = currentFont.bitmap;
                                            fntd = lockedFontBmp.LockBits(new Rectangle(0, 0, lockedFontBmp.Width, lockedFontBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                                            fntRow = (byte*)fntd.Scan0 + (scln * fntd.Stride);
                                        }
                                        lastFontIndex = neededFontIndex;
                                    }
                                }
                                
                                // Bounds check for data array
                                if (index < 0 || index >= data.Length)
                                    continue;
                                
                                byte charValue = data[index];
                                
                                // Bounds check for font data (font has 256 characters, each 8 bytes wide)
                                if (charValue * 8 + 7 >= fntd.Stride * fntd.Height)
                                    charValue = 0; // Use character 0 if out of bounds
                                
                                byte[] dliColor5 = useDli ? myMap.GetDliColor5(index) : activeColors;
                                if (dliColor5[0] == Globals.DEFAULT_COLOR) dliColor5 = activeColors;
                                bool oddScanline = (scln & 0x1) == 1;
                                for (int c = 0; c < 8; c++)
                                {
                                    int srcC = hMirror ? (7 - c) : c;
                                    int colorIndex = fntRow[charValue * 8 + srcC];
                                    byte color = ResolveAlpaColor(dliColor5, colorIndex, oddScanline, activeColors);
                                    row[x * 8 + c] = (index < data.Length) ? color : (byte)0;
                                }
                            }
                            //fill offMap space with the offmap color
                            for (int x = width; x < widthFull; x++)
                                for (int c = 0; c < 8; c++)
                                    row[x * 8 + c] = offMapColor;

                            row += bmd.Stride;
                        }
                        // For tilemaps with CharData, use character stride; otherwise use tile stride
                        adrOffset += charStride;
                    }
                    
                    int offUnderPixelAmount = (heightFull - height) * 8 * 8 * widthFull;
                    for (int i = 0; i < offUnderPixelAmount; i++)
                        row[i] = offMapColor;
                }
            }
            finally
            {
                if (fntd != null && lockedFontBmp != null)
                {
                    try { lockedFontBmp.UnlockBits(fntd); } catch { /* already unlocked */ }
                    fntd = null;
                }
                if (bmd != null)
                {
                    try { outBmp.UnlockBits(bmd); } catch { /* already unlocked */ }
                    bmd = null;
                }
            }
        }

        /// <summary>
        /// Resolve a playfield color for the current scanline. ALPA alternates come from the
        /// per-line DLI array when present (indices 5–8); otherwise from the active palette.
        /// Missing PF1 alter falls back to normal PF1.
        /// </summary>
        private static byte ResolveAlpaColor(byte[] lineColors, int colorIndex, bool oddScanline, byte[] paletteColors = null)
        {
            byte[] alpaSource = paletteColors ?? color5;
            if (alpaSource.Length <= 5 || !oddScanline)
                return lineColors[colorIndex];

            int altIndex;
            switch (colorIndex)
            {
                case 3: altIndex = 5; break; // PF3 alter
                case 0: altIndex = 6; break; // PF0 alter
                case 2: altIndex = 7; break; // PF2 alter
                case 1: altIndex = 8; break; // PF1 alter
                default: return lineColors[colorIndex];
            }

            if (altIndex < lineColors.Length)
                return lineColors[altIndex];
            if (altIndex < alpaSource.Length)
                return alpaSource[altIndex];
            // PF1 alter not present: same as normal PF1 (from DLI line or palette)
            if (colorIndex == 1)
                return lineColors.Length > 1 ? lineColors[1] : alpaSource[1];
            return lineColors[colorIndex];
        }

        private static byte SwapColor(byte paletteIndex)
        {
            for (int i = 0; i < COLOR5.Length; i++)
                if (paletteIndex == COLOR5[i]) return color5[i];
            throw new Exception("Messed up font.bitmap!");
        }

        private static byte SwapColor(byte paletteIndex, byte[] dliColor5)
        {
            for (int i = 0; i < COLOR5.Length; i++)
                if (paletteIndex == COLOR5[i]) return dliColor5[i];
            throw new Exception("Messed up font.bitmap!");
        }

        private static byte SwapColor(byte paletteIndex, int offset, AtariMap myMap)
        {
            for (int i = 0; i < COLOR5.Length; i++)
                if (paletteIndex == COLOR5[i]) return myMap.GetDliColor5(offset)[i]; // color5[i];
            throw new Exception("Messed up font.bitmap!");
        }
    }
}
