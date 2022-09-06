using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtariMapMaker
{
    public static class Globals
    {
        public enum FontType { Screen, Dli };
        public enum WindowType { Editor, CharPicker, Dli};
        public static Size editorWindowSizeInChars;
        private static readonly int[] zoomMultiplier = new int[] { 1, 2, 3, 4 };    //100%,200%,400%
        private static int zoomIndex = 1;
        public enum ClipBoardEnum { none, color, color5, colorAll };
        public const byte DEFAULT_COLOR = 255;
        public static int Zoom
        {
            get { return zoomMultiplier[zoomIndex]; }
            set { zoomIndex = value; }
        }
        /// <summary>
        /// Size of character in pixels based on selected Zoom
        /// </summary>
        public static int CharSize
        {
            get { return 8 * Zoom; }
        }

        /// <summary>
        /// Divide rectangle coordinates by zoom amount
        /// </summary>
        /// <param name="sourceRect"></param>
        /// <returns></returns>
        public static Rectangle UnzoomRectangle(Rectangle sourceRect)
        {
            return new Rectangle(sourceRect.X / Zoom, sourceRect.Y / Zoom, sourceRect.Width / Zoom, sourceRect.Height / Zoom);
        }

        /// <summary>
        /// Change rectangle origin to 0,0
        /// </summary>
        /// <param name="sourceRect"></param>
        /// <returns></returns>
        public static Rectangle OriginateRectangle(Rectangle sourceRect)
        {
            return new Rectangle(0, 0, sourceRect.Width, sourceRect.Height); 
        }
    }
}
