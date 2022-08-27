using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AtariMapMaker
{
    public struct AtariWindow
    {
        public AtariMap map;
        public Bitmap fontRendererDataImage;
        public Bitmap destinationImage;
        public Graphics graphics;
        public int mapOffset;
    }
    public static class AtariPictureTools
    {
        //private static AtariMap myMap;
        //private static Graphics gr;
        private static Rectangle mouseSelection = new Rectangle();
        //private static Bitmap destImage;
        private static Point prevMouseLoc;
        private static readonly Pen screenSeparatorPen = new Pen(Color.Red);
        private static readonly Pen selectionPen = new Pen(Color.Lime);
        private static readonly Color gridColor = Color.White;
        private static bool drawScreenBorders = true;
        private static bool drawGrid = true;
        private static Point dliFormOrigin;
        public static Dictionary<Globals.WindowType, AtariWindow> windows = new Dictionary<Globals.WindowType, AtariWindow>();

        public static void AssignWindow(Globals.WindowType windowType, Bitmap destintationImage, AtariMap windowMap)
        {
            AtariWindow window = windows.ContainsKey(windowType) ? windows[windowType] : new AtariWindow();
            window.map = windowMap;
            window.destinationImage = destintationImage;
            window.graphics = Graphics.FromImage(destintationImage);
            window.graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        }

        public static void SetMap(Globals.WindowType window, AtariMap map)
        {
            windows[window].map = map;
            windows[window].mapOffset = 0;
        }

        public static void SetGridVisibility(bool _drawScreenBorders, bool _drawGrid)
        {
            drawGrid = _drawGrid;
            drawScreenBorders = _drawScreenBorders;
        }
 
        public static void SelectionStart(Point firstCorner, Globals.WindowType window)
        {
            mouseSelection.X = firstCorner.X - (firstCorner.X % Globals.CharSize);
            mouseSelection.Y = firstCorner.Y - (firstCorner.Y % Globals.CharSize);
            mouseSelection.Width = Globals.CharSize;
            mouseSelection.Height = Globals.CharSize;
            DrawSelection(window);
        }

        public static void SelectionChange(Point newCorner, Globals.WindowType window)
        {
            int mx = newCorner.X - (newCorner.X % Globals.CharSize);
            int my = newCorner.Y - (newCorner.Y % Globals.CharSize);
            if (Math.Abs(mx - mouseSelection.X) != mouseSelection.Width ||
                Math.Abs(my - mouseSelection.Y) != mouseSelection.Height)
            {

                mouseSelection.Width = Globals.CharSize + mx - mouseSelection.X;
                mouseSelection.Height = Globals.CharSize + my - mouseSelection.Y;

                if (mouseSelection.Width + mouseSelection.X > windows[window].fontRendererDataImage.Width * Globals.Zoom ||
                    mouseSelection.Height + mouseSelection.Y > windows[window].fontRendererDataImage.Height * Globals.Zoom)
                {
                    //selection out of bounds - do not copy, do not draw selection
                    mouseSelection.Width = 0;
                    mouseSelection.Height = 0;
                }

                // gr.DrawImage(dataImage, 0, 0, dataImage.Width * zoom, dataImage.Height * zoom);
                Redraw(window);

                DrawSelection(window);
            }
        }

        public static bool SelectionEnd(Globals.WindowType window)
        {
            if (mouseSelection.Width == 0 || mouseSelection.Height == 0)
                return false;
            if (AtariClipboard.UnderClipBoardImage != null)
                AtariClipboard.UnderClipBoardImage.Dispose();
            if (AtariClipboard.UnderImageGraphics != null)
                AtariClipboard.UnderImageGraphics.Dispose();


            if (mouseSelection.Width < 0)
            {
                mouseSelection.X += mouseSelection.Width;
                mouseSelection.Width = -mouseSelection.Width;
            }
            if (mouseSelection.Height < 0)
            {
                mouseSelection.Y += mouseSelection.Height;
                mouseSelection.Height = -mouseSelection.Height;
            }

            AtariClipboard.SetDataSource(windows[window].map);
            Redraw(window, true, false, false);
            AtariClipboard.Copy(windows[window].destinationImage, mouseSelection, AtariFontRenderer.offset);
            
            Redraw(window, false, true, true);
            AtariClipboard.UnderClipBoardImage = new Bitmap(AtariClipboard.GetImage());
            AtariClipboard.UnderImageGraphics = Graphics.FromImage(AtariClipboard.UnderClipBoardImage);
            return true;
        }

        public static Point PreviousMouseLocation
        {
            get
            {
                return prevMouseLoc;
            }
            set
            {
                prevMouseLoc = value;
            }
        }

        private static void DrawSelection(Globals.WindowType window)
        {
            windows[window].graphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y);
            windows[window].graphics.DrawLine(selectionPen, mouseSelection.X + mouseSelection.Width, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            windows[window].graphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y + mouseSelection.Height, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            windows[window].graphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X, mouseSelection.Y + mouseSelection.Height);
        }

        public static void Redraw(Globals.WindowType windowType)
        {
            Redraw(windowType, true, drawScreenBorders, drawGrid);
        }

        public static Point GetDliFormOrigin()
        {
            return dliFormOrigin;
        }

        public static void Redraw(Globals.WindowType window, bool drawData, bool drawScreenBorders, bool drawGrid)
        {
            Bitmap destImage = windows[window].destinationImage;
            AtariMap myMap = windows[window].map;
            Graphics gr = windows[window].graphics;

            if (drawData)
                gr.DrawImage(windows[window].fontRendererDataImage, 0, 0, windows[window].fontRendererDataImage.Width * Globals.Zoom, windows[window].fontRendererDataImage.Height * Globals.Zoom);

            //separatory screenov
            if (drawScreenBorders)
            {
                for (int x = 0; x <= (destImage.Size.Width / Globals.CharSize) / myMap.ScreenSize.Width; x++)
                    gr.DrawLine(screenSeparatorPen, (-AtariFontRenderer.OffsetX % myMap.ScreenSize.Width + (x + 1) * myMap.ScreenSize.Width) * Globals.CharSize,
                                                 0, (-AtariFontRenderer.OffsetX % myMap.ScreenSize.Width + (x + 1) * myMap.ScreenSize.Width) * Globals.CharSize,
                                                 destImage.Size.Height);
                dliFormOrigin = new Point((-AtariFontRenderer.OffsetX % myMap.ScreenSize.Width + (0 + 1) * myMap.ScreenSize.Width) * Globals.CharSize,
                (-AtariFontRenderer.OffsetY % myMap.ScreenSize.Height + (0 + 1) * myMap.ScreenSize.Height) * Globals.CharSize);



                for (int y = 0; y <= (destImage.Size.Height / Globals.CharSize) / myMap.ScreenSize.Height; y++)
                    gr.DrawLine(screenSeparatorPen, 0, (-AtariFontRenderer.OffsetY % myMap.ScreenSize.Height + (y + 1) * myMap.ScreenSize.Height) * Globals.CharSize,
                                      destImage.Width, (-AtariFontRenderer.OffsetY % myMap.ScreenSize.Height + (y + 1) * myMap.ScreenSize.Height) * Globals.CharSize);
            }
            //grid
            if (drawGrid)
                for (int x = 0; x < (destImage.Size.Width / Globals.CharSize); x++)
                    for (int y = 0; y < (destImage.Size.Height / Globals.CharSize); y++)
                        if (AtariFontRenderer.OffsetX + x < myMap.Stride && AtariFontRenderer.OffsetY + y < myMap.Screens.Height * myMap.ScreenSize.Height)
                            destImage.SetPixel(x * Globals.CharSize, y * Globals.CharSize, gridColor);
        }

        public static void Scroll(Point newMouseLoc, Globals.WindowType window)
        {
           
            bool newoffset = AtariFontRenderer.CalculateOffset((newMouseLoc.X - prevMouseLoc.X) / Globals.CharSize, (newMouseLoc.Y - prevMouseLoc.Y) / Globals.CharSize, windows[window].map);

            if (newoffset)
            {

                prevMouseLoc.X = newMouseLoc.X;
                prevMouseLoc.Y = newMouseLoc.Y;

                AtariFontRenderer.RenderMapData(windows[window].map, windows[window].mapOffset, AtariFontRenderer.fonts[Globals.FontType.Screen], windows[window].fontRendererDataImage);
                Redraw(window);
            }
        }

        public static void DrawClipBoard(Point location)
        {
            AtariClipboard.UnderImageGraphics.DrawImage(destImage, 0, 0, new Rectangle(location.X - location.X % Globals.CharSize, location.Y - location.Y % Globals.CharSize, AtariClipboard.GetImage().Width, AtariClipboard.GetImage().Height), GraphicsUnit.Pixel);
            gr.DrawImage(AtariClipboard.GetImage(), location.X - location.X % Globals.CharSize, location.Y - location.Y % Globals.CharSize);
        }

        public static void DrawUnderClipBoard(Point location)
        {
            gr.DrawImage(AtariClipboard.UnderClipBoardImage, location.X - location .X % Globals.CharSize, location .Y - location .Y % Globals.CharSize);
        }

        public static void SetDestImage(Bitmap _destImage, Graphics _gr)
        {
            destImage = _destImage;
            gr = _gr;
        }
    }
}
