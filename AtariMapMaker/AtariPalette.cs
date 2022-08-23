using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace AtariMapMaker
{
    [Serializable]
    public class AtariPalette
    {
        [NonSerialized]
        private readonly ColorPalette myPalette;

        public AtariPalette()
        {
            Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
            myPalette = bmp.Palette;
            bmp.Dispose();
        }

        public ColorPalette GetPalette()
        {
            return this.myPalette;
        }

        public Color GetColor(int index)
        {
            return myPalette.Entries[index];
        }

        public void Load(byte[] rawdata)
        {
            for (int a = 0; a < 256; a++)
                myPalette.Entries[a] = Color.FromArgb(255, rawdata[a * 3], rawdata[a * 3 + 1], rawdata[a * 3 + 2]);
        }
    }
}
