using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace AtariMapMaker
{
    class Graphics8bit 
    {
        private Bitmap bmp;
        private BitmapData bmd;

        /// <summary>
        /// Konstruktor z 8bit obrazku
        /// </summary>
        /// <param name="bmp"></param>
        public Graphics8bit(Bitmap bmp)
        {
            this.bmp = bmp;
        }

        /// <summary>
        /// Lockbits - volat pred kreslenim
        /// </summary>
        public void Lock()
        {
            bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        }
        /// <summary>
        /// Unlockbits - volat po dokonceni kreslenia!!!
        /// </summary>
        public void Unlock()
        {
            bmp.UnlockBits(bmd);
        }

        /// <summary>
        /// Vertikalna ciara
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        public void DrawVLine(int x, int y1, int y2, byte colorIndex)
        {
            if (x > bmp.Width -1 || x < 0)
                return;
            if (y1 < 0)
                y1 = 0;
            if (y1 > bmp.Height - 1)
                y1 = bmp.Height - 1;
            if (y2 < 0)
                y2 = 0;
            if (y2 > bmp.Height - 1)
                y2 = bmp.Height - 1;

            if (y1 > y2)
            {
                int y = y2;
                y2 = y1;
                y1 = y;
            }
            
            unsafe
            {
                byte* row = (byte*)bmd.Scan0 + y1 * bmd.Stride;
                for (int a = 0; a < y2 - y1; a++)
                {
                    row[x] = colorIndex;
                    row += bmd.Stride;
                }

            }
        }

        /// <summary>
        /// Horiznotalna ciara
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y"></param>
        /// <param name="colorIndex"></param>
        public void DrawHLine(int x1, int x2, int y, byte colorIndex)
        {
            if (y > bmp.Height -1|| y < 0)
                return;
            if (x1 < 0)
                x1 = 0;
            if (x1 > bmp.Width -1)
                x1 = bmp.Width - 1;
            if (x2 < 0)
                x2 = 0;
            if (x2 > bmp.Width - 1)
                x2 = bmp.Width - 1;

            if (x1 > x2)
            {
                int x = x1;
                x1 = x2;
                x2 = x;
            }

            unsafe
            {
                byte* row = (byte*)bmd.Scan0 + y * bmd.Stride;
                for (int a = x1; a < x2; a++)
                    row[a] = colorIndex;
            }
        }

        /// <summary>
        /// Bod
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colorIndex"></param>
        public void DrawPoint(int x, int y, byte colorIndex)
        {
            unsafe
            { 
                byte* row = (byte*)bmd.Scan0 + y*bmd.Stride;
                row[x] = colorIndex;
            }
        }

        /// <summary>
        /// Obdlznik
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="colorIndex"></param>
        public void DrawRectangle(int x1, int y1, int x2, int y2, byte colorIndex)
        {
            DrawHLine(x1, x2, y1, colorIndex);
            DrawHLine(x1, x2, y2, colorIndex);
            DrawVLine(x1, y1, y2, colorIndex);
            DrawVLine(x2, y1, y2, colorIndex);
        }

        /// <summary>
        /// 8-bit Image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="srcBmp"></param>
        public void DrawImage(int x, int y, Bitmap srcBmp, int zoom)
        {
            BitmapData srcBmd = srcBmp.LockBits(new Rectangle(0,0,srcBmp.Width,srcBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* row = (byte*)bmd.Scan0 + y * bmd.Stride;
                byte* srcRow = (byte*)srcBmd.Scan0;
                for (int yy = y; yy < y + srcBmp.Height; yy++)
                {
                    for (int xx = 0; xx < srcBmp.Width; xx++)
                    {
                        for (int zz = 0; zz < zoom; zz++)
                        {
                            row[(xx + x)*zoom + zz] = srcRow[xx];
                        }
                    }
                    for (int zz = 0; zz < zoom - 1; zz++)
                    {
                        for (int xx = 0; xx < bmp.Width; xx++)
                        {
                            row[xx + bmd.Stride] = row[xx];
                        }
                        row += bmd.Stride;

                    }
                    row += bmd.Stride;
                    srcRow += srcBmd.Stride;
                }
            }

            srcBmp.UnlockBits(srcBmd);
        }

        /// <summary>
        /// Nakresli cast source obrazku definovanu regionom
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="srcBmp"></param>
        /// <param name="zoom"></param>
        /// <param name="region"></param>
        public void DrawImageRegion(Bitmap srcBmp, Rectangle region)
        {
            BitmapData srcBmd = srcBmp.LockBits(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* row = (byte*)bmd.Scan0;
                byte* srcRow = (byte*)srcBmd.Scan0 + srcBmd.Stride * region.Y;
                for (int yy = 0; yy < region.Height; yy++)
                {
                    for (int xx = 0; xx < region.Width; xx++)
                    {
                        row[xx] = srcRow[xx + region.X];
                    }                    
                    row += bmd.Stride;
                    srcRow += srcBmd.Stride;
                }
            }

            srcBmp.UnlockBits(srcBmd);
        }
    }
}
