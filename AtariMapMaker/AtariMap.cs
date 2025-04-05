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
        public byte[] Data { get; set; }
        public Size ScreenSize { get; }    //velkost obrazovky v znakoch
        private Size dataSize;      //velkost dat v znakoch
        public Size MapSize { get; }       //pocet screenov v datach (velkost mapy)
        private int offset;
        public byte[] ColorData { get; set; }   //screens*lines*5 colors
        public AtariMap(Size mapSize, Size screenSize)
        {
            this.MapSize = mapSize;
            this.ScreenSize = screenSize;
            this.dataSize = new Size(mapSize.Width * screenSize.Width, mapSize.Height * screenSize.Height);
            this.Data = new byte[dataSize.Width * dataSize.Height];
            InitDliColorFullMap();
        }

        public void CopyDliColorsFullScreen(int localScreenNumber, AtariMap targetMap, int targetScreenNumber)
        {
            int length = this.ScreenSize.Height * 5;
            int sourceOffset = localScreenNumber * length;
            int destOffset = targetScreenNumber * length;
            //TODO: fix crash when edit dli shown on the screen out of map bounds
            for (int i = 0; i < length; i++)
                targetMap.ColorData[destOffset + i] = ColorData[sourceOffset + i];
        }

        public void InitDliColorFullMap()
        {
            int totalLength = MapSize.Width * MapSize.Height * ScreenSize.Height * 5;
            this.ColorData = new byte[totalLength];
            for (int i = 0; i < totalLength; i+=5)
                ColorData[i] = Globals.DEFAULT_COLOR; //0.color in each row indicates that no DLI was used 
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
                return this.ScreenSize.Width * this.MapSize.Width;
            }
        }

        //set single color for multiple lines
        public void SetDliColorMultiple(int screenx, int screeny, int startingLine, int lines, int colorNumber, byte colorIndexFromPalette)
        {
            int screenOffset = (screeny * MapSize.Width + screenx) * ScreenSize.Height * 5;
            if (lines < 0) lines = ScreenSize.Height - startingLine;
            if (startingLine + lines > ScreenSize.Height) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                this.ColorData[screenOffset + (startingLine + j) * 5 + colorNumber] = colorIndexFromPalette;
        }
        /// <summary>
        /// Get byte[5] color structure for given offset (char in AtariMap)
        /// </summary>
        /// <param name="charOffset"></param>
        /// <returns></returns>
        public byte[] GetDliColor5(int charOffset)
        {
            int row = charOffset / Stride;
            int column = charOffset % Stride;
            int line = row % ScreenSize.Height;
            int screenNumber = column / ScreenSize.Width + (row / ScreenSize.Height)*MapSize.Width;
            
            int dliOffset = screenNumber * ScreenSize.Height * 5 + line * 5;

            byte[] retValue = new byte[5];
            for (int i = 0; i < 5; i++)
                retValue[i] = ColorData[dliOffset + i];
            return retValue;
        }

        public byte[] GetDliColor5(Point clickedChar)
        {
            int charOffset = clickedChar.X + Stride*clickedChar.Y;
            return GetDliColor5(charOffset);
        }

        //set all colors multiple lines based on the given 5 colors
        public void SetDliColor5Multiple(int screenx, int screeny, int startingLine, int lines, byte[] color5)
        {
            int screenOffset = (screeny * MapSize.Width + screenx) * ScreenSize.Height * 5;
            if (lines < 0) lines = ScreenSize.Height - startingLine;
            if (startingLine + lines > ScreenSize.Height) throw new Exception($"The screen does not have that many ({lines}) lines.");
            for (int j = 0; j < lines; j++)
                for (int i = 0; i < 5; i++)
                    this.ColorData[screenOffset + (startingLine + j) * 5 + i] = color5[i];
        }

        public void SetDliColor(int screenx, int screeny, int line, int colorNumber, byte colorIndex)
        {
            int screenOffset = (screeny * MapSize.Width + screenx) * ScreenSize.Height * 5;
            this.ColorData[screenOffset + line * 5 + colorNumber] = colorIndex;
        }

        public void SwapChar(byte char1, byte char2, bool globalChange)
        {
            if (globalChange)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i] == char1) Data[i] = char2;
                    else if (Data[i] == char2) Data[i] = char1;
                }
            }
            else
            {
                //poop
            }
        }

    }
}
