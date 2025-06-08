using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace AtariMapMaker
{
    public static class AtariClipboard
    {
        private static byte[,] data;
        private static AtariMap dataSource;
        private static Graphics gr;
        public static int ClipboardWidth { get; private set; }
        public static int ClipboardHeight { get; private set; }
        public static Bitmap ClipboardImage { get; private set; }
        public static Bitmap UnderClipBoardImage { get; set; }
        public static Graphics UnderImageGraphics { get; set; }
        public static bool IsValid { get; set; }
        public static void SetDataSource(AtariMap myMap)
        {
            dataSource = myMap;
        }

        public static void Copy(Bitmap srcBmp, Rectangle mouseSelection, int offset)
        {
            if (dataSource == null)
                return;
            
            //graphical part
            if (ClipboardImage != null)
            {
                ClipboardImage.Dispose();
            }
            ClipboardImage = new Bitmap(mouseSelection.Width, mouseSelection.Height);
            gr = Graphics.FromImage(ClipboardImage);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            gr.DrawImage(srcBmp, Globals.OriginateRectangle(mouseSelection), Globals.UnzoomRectangle(mouseSelection), GraphicsUnit.Pixel);
            gr.Dispose();

            //data part
            ClipboardWidth = mouseSelection.Width / Globals.CharSize;
            ClipboardHeight = mouseSelection.Height / Globals.CharSize;
            data = new byte[ClipboardWidth, ClipboardHeight];
            int xo = mouseSelection.X / Globals.CharSize;
            int yo = mouseSelection.Y / Globals.CharSize;
            for (int y = 0; y < ClipboardHeight; y++)
                for (int x = 0; x < ClipboardWidth; x++)
                    data[x, y] = dataSource.Data[offset + x + xo + (y + yo) * dataSource.Stride];            
        }

        public static void Paste(int offset)
        {
            if (offset + (ClipboardWidth - 1) + (ClipboardHeight - 1) * dataSource.Stride < dataSource.Data.Length)
            {
                if (IsValid)
                {
                    for (int y = 0; y < ClipboardHeight; y++)
                        for (int x = 0; x < ClipboardWidth; x++)
                            dataSource.Data[offset + x + y * dataSource.Stride] = data[x, y];
                }
            }
        }
    }
}
