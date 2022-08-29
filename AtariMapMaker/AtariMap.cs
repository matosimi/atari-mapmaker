using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AtariMapMaker
{
    [Serializable]
    public class AtariMap
    {
        private byte[] data;
        private Size screenSize;    //velkost obrazovky v znakoch
        private Size dataSize;      //velkost dat v znakoch
        private Size screens;       //pocet screenov v datach (velkost mapy)
        private int offset;
        private readonly byte[,,] colorData;   //screenNumber,linenumber,colorNumber

        public AtariMap(Size screens, Size screenSize)
        {
            this.screens = screens;
            this.dataSize = new Size(screens.Width * screenSize.Width, screens.Height * screenSize.Height);
            this.data = new byte[dataSize.Width * dataSize.Height];
            this.screenSize = screenSize;
            this.colorData = new byte[screens.Width * screens.Height, screenSize.Height, 5];
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
                return this.screenSize.Width * this.screens.Width;
            }
        }

        //set single color
        public void SetColorData(int screenx, int screeny, byte lineNumber, byte colorNumber, byte colorIndexFromPalette)
        {
            this.colorData[screeny * screens.Width + screenx, lineNumber, colorNumber] = colorIndexFromPalette;
        }

        //set all colors multiple lines based on the given 5 colors
        public void SetColorData(int screenx, int screeny, byte startingLine, byte[] color5)
        {
            for (int j = startingLine; j < screenSize.Height; j++)
                for (int i = 0; i < 5; i++)
                    this.colorData[screeny * screens.Width + screenx, j, i] = color5[i];
        }

        public Size ScreenSize
        {
            get
            {
                return this.screenSize;
            }

        }

        public Size Screens
        {
            get
            {
                return this.screens;
            }
        }

        public byte[] Data
        {
            get 
            {
                    return data;
            }
            set 
            {
                    data = value;
            }

        }
    }
}
