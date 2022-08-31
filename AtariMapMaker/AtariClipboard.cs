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
        private static Bitmap clipboard;
        private static byte[,] data;
        private static AtariMap dataSource;
        private static Graphics gr;
        private static int clipboardWidth;
        private static int clipboardHeight;
        private static bool valid = false;
        private static Bitmap underClipBoardImage;
        private static Graphics gruc;  //underclipboardimage gfx

        public static Bitmap UnderClipBoardImage
        {
            get
            {
                return underClipBoardImage;
            }
            set
            {
                underClipBoardImage = value;
            }
        }

        public static Graphics UnderImageGraphics
        {
            get
            {
                return gruc;
            }
            set
            {
                gruc = value;
            }
        }

        public static bool IsValid
        {
            get
            {
                return valid;
            }
            set
            {
                valid = value;
            }
        }

        public static void SetDataSource(AtariMap myMap)
        {
            dataSource = myMap;
            
        }

        public static void Copy(Bitmap srcBmp, Rectangle mouseSelection, int offset)
        {
            if (dataSource == null)
                return;
            
            //graficka cast
            if (clipboard != null)
            {
                clipboard.Dispose();
            }
            clipboard = new Bitmap(mouseSelection.Width, mouseSelection.Height);
            gr = Graphics.FromImage(clipboard);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            gr.DrawImage(srcBmp, Globals.OriginateRectangle(mouseSelection), Globals.UnzoomRectangle(mouseSelection), GraphicsUnit.Pixel);
            gr.Dispose();

            //datova cast
            clipboardWidth = mouseSelection.Width / Globals.CharSize;
            clipboardHeight = mouseSelection.Height / Globals.CharSize;
            data = new byte[clipboardWidth, clipboardHeight];
            int xo = mouseSelection.X / Globals.CharSize;
            int yo = mouseSelection.Y / Globals.CharSize;
            for (int y = 0; y < clipboardHeight; y++)
                for (int x = 0; x < clipboardWidth; x++)
                    data[x, y] = dataSource.Data[offset + x + xo + (y + yo) * dataSource.Stride];            
        }

        public static void Paste(int offset)
        {
            if (offset + (clipboardWidth - 1) + (clipboardHeight - 1) * dataSource.Stride < dataSource.Data.Length)
            {
                if (valid)
                {
                    for (int y = 0; y < clipboardHeight; y++)
                        for (int x = 0; x < clipboardWidth; x++)
                            dataSource.Data[offset + x + y * dataSource.Stride] = data[x, y];
                }
            }
        }

        public static Bitmap ClipboardImage

        {
            get
            {
                return clipboard;
            }
        }
    }
}
