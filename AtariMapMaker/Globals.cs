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
        public enum ClipBoardEnum { none, color, color5, colorAll, fontAll };
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

        /// <summary>When true, metadata layer overlay is drawn on the map.</summary>
        public static bool MetadataLayerVisible { get; set; }

        /// <summary>When true, map clicks add/edit metadata instead of editing the character map.</summary>
        public static bool MetadataLayerEditable { get; set; }

        /// <summary>
        /// Blend between charmap and metadata (0–100, step 5). 0 = solid map only, 100 = metadata only, 50 = both at 50%.
        /// </summary>
        public static int MetadataLayerBlendPercent { get; set; } = 50;

        /// <summary>When true, metadata text (white labels) is drawn on the overlay; when false, only value/color indicator is shown.</summary>
        public static bool MetadataLayerShowText { get; set; }

        /// <summary>When true with metadata layer on, draw lines between metadata items sharing the same palette color.</summary>
        public static bool MetadataLayerShowColorLinks { get; set; }

        /// <summary>When true with metadata layer on, draw lines between metadata items sharing the same value (color 0x0E).</summary>
        public static bool MetadataLayerShowValueLinks { get; set; }
    }
}
