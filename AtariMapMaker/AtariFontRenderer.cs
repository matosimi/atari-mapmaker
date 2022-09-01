using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace AtariMapMaker
{
    [Serializable]

    public struct AtariFont
    {
        public byte[] data;
        public Bitmap bitmap;
    } 
    public static class AtariFontRenderer
    {
        private static readonly byte[] COLOR5 = { 40, 202, 148, 70, 0 };
        private static byte[] color5 = COLOR5;
        private static string lastFontFile;
        public static readonly Dictionary<Globals.FontType, AtariFont> fonts = new Dictionary<Globals.FontType, AtariFont>();
        
        public static string LastFontFile
        {
            get { return lastFontFile; }
            set { lastFontFile = value; }
        }

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
            }
        }

        public static void LoadFont(String fontname, Globals.FontType fontType)
        {
            byte[] fontData = new byte[1024*2];
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

            myMap.Offset -= deltaX + deltaY * myMap.Stride;
            return true;
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
                    } else {
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
        /// Try avoid calling this method outside AtariPictureTools
        /// outBmp has to be already properly sized (windowCharsHorizontal*8, windowCharsVertical*8)
        /// </summary>
        /// <param name="myMap"></param>
        /// <param name="adrOffset"></param>
        /// <param name="font"></param>
        /// <param name="outBmp"></param>
        public static void RenderMapData(AtariMap myMap, Globals.FontType fontType, Bitmap outBmp)
        {
            if (outBmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new Exception("Output bitmap of RenderMapData MUST be 8bppIndexed palette!");
            
            AtariFont font = fonts[fontType];

            byte[] data = myMap.Data;
            int adrOffset = myMap.Offset;
            if (adrOffset < 0)
            {
                return;
            }
            
            int width = outBmp.Width / 8;
            int height = outBmp.Height / 8;

            if (myMap.OffsetX + width > myMap.Stride)
                width = myMap.Stride - myMap.OffsetX;
            if (myMap.OffsetY + height > myMap.Screens.Height * myMap.ScreenSize.Height)
                height = myMap.ScreenSize.Height * myMap.Screens.Height - myMap.OffsetY;

            //Bitmap bmp = new Bitmap(bmpSize.Width, bmpSize.Height, PixelFormat.Format8bppIndexed);
            //outBmp.Palette = AtariPalette.GetPalette();
            BitmapData bmd = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            BitmapData fntd = font.bitmap.LockBits(new Rectangle(0, 0, font.bitmap.Width, font.bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            int index;

            unsafe
            {
                byte* row = (byte*)bmd.Scan0;
                for (int y = 0; y < height; y++)
                {
                    byte* fntRow = (byte*)fntd.Scan0;
                    for (int scln = 0; scln < 8; scln++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            index = adrOffset + x; // +y * width; //index znaku co sa ma kreslit v data
                            byte[] dliColor5 = myMap.GetColorData(index);
                            for (int c = 0; c < 8; c++)
                                row[x*8+c] = (index < data.Length) ? SwapColor(fntRow[data[index] * 8 + c ], dliColor5) : (byte)0;
                        }
                        fntRow += fntd.Stride;
                        row += bmd.Stride;
                    }
                    adrOffset += myMap.Stride; 
                }
            }
            font.bitmap.UnlockBits(fntd);
            outBmp.UnlockBits(bmd);
            return;
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
                if (paletteIndex == COLOR5[i]) return myMap.GetColorData(offset)[i]; // color5[i];
            throw new Exception("Messed up font.bitmap!");
        }
    }
}
