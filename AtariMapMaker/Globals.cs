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
        public static int Zoom
        {
            get { return zoomMultiplier[zoomIndex]; }
            set { zoomIndex = value; }
        }
        public static int CharSize
        {
            get { return 8 * Zoom; }
        }
    }
}
