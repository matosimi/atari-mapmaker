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
        private byte[,,] colorData;   //screenNumber,linenumber,colorNumber

        public AtariMap(Size screens, Size screenSize)
        {
            this.screens = screens;
            this.dataSize = new Size(screens.Width * screenSize.Width, screens.Height * screenSize.Height);
            this.data = new byte[dataSize.Width * dataSize.Height];
            this.screenSize = screenSize;
            InitColorData();
        }

        public void InitColorData()
        {
            this.colorData = new byte[screens.Width * screens.Height, screenSize.Height, 5];
            for (int i = 0; i < screens.Width * screens.Height; i++)
                for (int j = 0; j < screenSize.Height; j++)
                    for (int k = 0; k < 5; k++)
                        colorData[i, j, k] = AtariFontRenderer.Color5[k];
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
        /// <summary>
        /// Get byte[5] color structure for given offset (char in AtariMap)
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] GetColorData(int offset)
        {
            int row = offset / Stride;
            int column = offset % Stride;
            int line = row % screenSize.Height;
            int screenNumber = column / screenSize.Width + (row / screenSize.Height)*screens.Width;
            byte[] retValue = new byte[5];
            
            for (int i = 0; i < 5; i++)
                retValue[i] = colorData[screenNumber, line, i];
            return retValue;
        }

        //set all colors multiple lines based on the given 5 colors
        public void SetColorData(int screenx, int screeny, byte startingLine, byte[] color5)
        {
            for (int j = startingLine; j < screenSize.Height; j++)
                for (int i = 0; i < 5; i++)
                    this.colorData[screeny * screens.Width + screenx, j, i] = color5[i];
        }

        public void SetColor(int screenx, int screeny, int line, int colorNumber, byte colorIndex)
        {
            this.colorData[screeny * screens.Width + screenx, line, colorNumber] = colorIndex;
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
