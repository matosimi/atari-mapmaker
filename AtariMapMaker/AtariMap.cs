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
        //private Point origin;       //vztazny bod screenu [0,0]


        public AtariMap(Size screens, Size screenSize)
        {
            this.screens = screens;
            this.dataSize = new Size(screens.Width * screenSize.Width, screens.Height * screenSize.Height);
            this.data = new byte[dataSize.Width * dataSize.Height];
            this.screenSize = screenSize;

            //new Random().NextBytes(data);
            
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
