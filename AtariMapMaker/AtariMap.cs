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

        public void CopyColorData(int localScreenNumber, AtariMap targetMap, int targetScreenNumber)
        {
            byte[] color5 = new byte[5];
            for (int i = 0; i < this.ScreenSize.Height; i++)
            {
                for (int j = 0; j < 5; j++)
                    color5[j] = colorData[localScreenNumber, i, j];
                targetMap.SetColorData(targetScreenNumber % targetMap.ScreenSize.Width, targetScreenNumber / targetMap.screenSize.Width, i, 1, color5);
            }
        }

        public void InitColorData()
        {
            this.colorData = new byte[screens.Width * screens.Height, screenSize.Height, 5];
            for (int i = 0; i < screens.Width * screens.Height; i++)
                for (int j = 0; j < screenSize.Height; j++)
                    colorData[i, j, 0] = Globals.DEFAULT_COLOR; //indicates that no DLI was used 
                    //for (int k = 0; k < 5; k++)
                    //    colorData[i, j, k] = AtariFontRenderer.Color5[k];
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

        //set single color for multiple lines
        public void SetColorData(int screenx, int screeny, int startingLine, int lines, int colorNumber, byte colorIndexFromPalette)
        {
            if (lines < 0) lines = screenSize.Height - startingLine;
            if (startingLine + lines > screenSize.Height) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                this.colorData[screeny * screens.Width + screenx, startingLine + j, colorNumber] = colorIndexFromPalette;
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

        public byte[] GetColorData(Point clickedChar)
        {
            int offset = clickedChar.X + Stride*clickedChar.Y;
            return GetColorData(offset);
        }

        //set all colors multiple lines based on the given 5 colors
        public void SetColorData(int screenx, int screeny, int startingLine, int lines, byte[] color5)
        {
            if (lines < 0) lines = screenSize.Height - startingLine;
            if (startingLine + lines > screenSize.Height) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                for (int i = 0; i < 5; i++)
                    this.colorData[screeny * screens.Width + screenx, startingLine + j, i] = color5[i];
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
