using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace AtariMapMaker
{
    public class AtariClipboard
    {
        private Bitmap clipboard;
        private byte[,] data;
        private AtariMap dataSource;
        private Graphics gr;
        private int clipboardWidth;
        private int clipboardHeight;
        private int zoom;
        private bool valid = false;
        private Bitmap underClipBoardImage;
        private Graphics gruc;  //underclipboardimage gfx

        public AtariClipboard()
        { 
        }

        public Bitmap UnderClipBoardImage
        {
            get
            {
                return this.underClipBoardImage;
            }
            set
            {
                underClipBoardImage = value;
            }
        }

        public Graphics UnderImageGraphics
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

        public bool isValid
        {
            get
            {
                return valid;
            }
            set
            {
                this.valid = value;
            }
        }

        public void SetDataSource(AtariMap myMap)
        {
            this.dataSource = myMap;
            
        }

        public void Copy(Bitmap srcBmp, Rectangle mouseSelection, int offset, int zoom)
        {
            if (dataSource == null)
                return;
            this.zoom = zoom;

            //graficka cast
            if (clipboard != null)
            {
                clipboard.Dispose();
            }
            clipboard = new Bitmap(mouseSelection.Width, mouseSelection.Height);
            gr = Graphics.FromImage(clipboard);
            gr.DrawImage(srcBmp, new Rectangle(0, 0, mouseSelection.Width, mouseSelection.Height), mouseSelection, GraphicsUnit.Pixel);
            gr.Dispose();

            //datova cast
            clipboardWidth = mouseSelection.Width / (8 * zoom);
            clipboardHeight = mouseSelection.Height / (8 * zoom);
            data = new byte[clipboardWidth, clipboardHeight];
            int xo = mouseSelection.X / (8*zoom);
            int yo = mouseSelection.Y / (8 * zoom);
            for (int y = 0; y < clipboardHeight; y++)
                for (int x = 0; x < clipboardWidth; x++)
                    data[x, y] = dataSource.Data[offset + x + xo + (y + yo) * dataSource.Stride];            
        }

        public void Paste(int offset)
        {
            if (valid)
            {
                for (int y = 0; y < clipboardHeight; y++)
                    for (int x = 0; x < clipboardWidth; x++)
                        dataSource.Data[offset + x + y * dataSource.Stride] = data[x, y];
            }
        }

        public Bitmap GetImage()
        {
            return clipboard;
        }
    }
}
