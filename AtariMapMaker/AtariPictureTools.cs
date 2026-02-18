using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;
using System.Drawing.Imaging;

namespace AtariMapMaker
{
    public struct AtariWindow
    {
        public AtariMap map;
        public Bitmap fontRendererFontImage;
        public Bitmap fontRendererMapImage;
        public Graphics pictureBoxGraphics;
        public Globals.FontType fontType;
    }
    public static class AtariPictureTools
    {
        //private static AtariMap myMap;
        //private static Graphics gr;
        private static Rectangle mouseSelection = new Rectangle();
        //private static Bitmap destImage;
        private static Point prevMouseLoc;
        private static int previousOffset;
        private static Point? previousClipboardLocation = null;  // Track previous clipboard position to restore it
        private static Point? previousClipboardGridCell = null;  // Track previous grid cell to optimize redraws
        private static readonly Pen screenSeparatorPen = new Pen(Color.Red);
        private static readonly Pen selectionPen = new Pen(Color.Lime);
        private static readonly Color gridColor = Color.White;
        private static bool drawScreenBorders = true;
        private static bool drawGrid = true;
        private static Point dliFormOrigin;
        public static Dictionary<Globals.WindowType, AtariWindow> windows = new Dictionary<Globals.WindowType, AtariWindow>();

        public static void AssignWindow(Globals.WindowType windowType, Bitmap destinationPictureBoxImage, AtariMap windowMap)
        {
            Globals.FontType myFontType = windowType == Globals.WindowType.Dli ? Globals.FontType.Dli : Globals.FontType.Screen;
            if (windows.ContainsKey(windowType))
                windows.Remove(windowType);

            // Ensure font bitmap exists and is not disposed (e.g. after new map or font reload)
            if (!AtariFontRenderer.fonts.ContainsKey(myFontType) || AtariFontRenderer.fonts[myFontType].bitmap == null ||
                AtariFontRenderer.fonts[myFontType].bitmap.Width <= 0)
            {
                byte[] defaultFontData = DefaultFontResource.GetDefaultFontDataFromResources();
                AtariFontRenderer.SetFontData(defaultFontData, Globals.FontType.Screen);
            }
            Bitmap fontBmp = AtariFontRenderer.fonts[myFontType].bitmap;

            int emptyWidth = destinationPictureBoxImage.Width % (8 * Globals.Zoom);
            int emptyHeight = destinationPictureBoxImage.Height % (8 * Globals.Zoom);
            AtariWindow window = new AtariWindow()
            {
                fontRendererMapImage = new Bitmap((destinationPictureBoxImage.Width - emptyWidth) / Globals.Zoom, (destinationPictureBoxImage.Height - emptyHeight) / Globals.Zoom, System.Drawing.Imaging.PixelFormat.Format8bppIndexed),
                pictureBoxGraphics = Graphics.FromImage(destinationPictureBoxImage),
                map = windowMap,
                fontType = myFontType,
                fontRendererFontImage = fontBmp
            };
            window.fontRendererMapImage.Palette = AtariPalette.GetPalette();
            window.pictureBoxGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            window.pictureBoxGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            windows.Add(windowType, window);
        }
       
        public static void SetGridVisibility(bool _drawScreenBorders, bool _drawGrid)
        {
            drawGrid = _drawGrid;
            drawScreenBorders = _drawScreenBorders;
        }
 
        public static void SelectionStart(Point firstCorner, Globals.WindowType window)
        {
            AtariMap myMap = windows[window].map;
            
            // For tilemaps, snap to tile boundaries; otherwise snap to char boundaries
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                int tileWidth = myMap.TilemapInfo.TileWidth;
                int tileHeight = myMap.TilemapInfo.TileHeight;
                int tilePixelWidth = tileWidth * Globals.CharSize;
                int tilePixelHeight = tileHeight * Globals.CharSize;
                
                mouseSelection.X = (firstCorner.X / tilePixelWidth) * tilePixelWidth;
                mouseSelection.Y = (firstCorner.Y / tilePixelHeight) * tilePixelHeight;
                mouseSelection.Width = tilePixelWidth;
                mouseSelection.Height = tilePixelHeight;
            }
            else
            {
                mouseSelection.X = firstCorner.X - (firstCorner.X % Globals.CharSize);
                mouseSelection.Y = firstCorner.Y - (firstCorner.Y % Globals.CharSize);
                mouseSelection.Width = Globals.CharSize;
                mouseSelection.Height = Globals.CharSize;
            }
            DrawSelection(window);
        }

        public static void SelectionChange(Point newCorner, Globals.WindowType window)
        {
            AtariMap myMap = windows[window].map;
            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            
            // For tilemaps, snap to tile boundaries
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                int tileWidth = myMap.TilemapInfo.TileWidth;
                int tileHeight = myMap.TilemapInfo.TileHeight;
                alignSizeX = tileWidth * Globals.CharSize;
                alignSizeY = tileHeight * Globals.CharSize;
            }
            
            int mx = (newCorner.X / alignSizeX) * alignSizeX;
            int my = (newCorner.Y / alignSizeY) * alignSizeY;
            
            if (Math.Abs(mx - mouseSelection.X) != mouseSelection.Width ||
                Math.Abs(my - mouseSelection.Y) != mouseSelection.Height)
            {
                mouseSelection.Width = alignSizeX + mx - mouseSelection.X;
                mouseSelection.Height = alignSizeY + my - mouseSelection.Y;
                if (mx - mouseSelection.X < 0)
                {
                    mouseSelection.Width -= alignSizeX;
                }
                if (my - mouseSelection.Y < 0)
                {
                    mouseSelection.Height -= alignSizeY;
                }

                //selection out of picturebox bounds - do not copy, do not draw selection
                if (mouseSelection.Width + mouseSelection.X > windows[window].fontRendererMapImage.Width * Globals.Zoom ||
                    mouseSelection.Height + mouseSelection.Y > windows[window].fontRendererMapImage.Height * Globals.Zoom ||
                    newCorner.X < 0 ||
                    newCorner.Y < 0)
                {
                    mouseSelection.Width = 0;
                    mouseSelection.Height = 0;
                }

                //selection out of data bounds - do not copy, do not draw selection
                AtariMap map = windows[window].map;
                int maxCharHeight = map.MapSize.Height * map.ScreenSize.Height;
                int maxCharStride = map.Stride;
                if (map.IsTilemap && map.TilemapInfo != null)
                {
                    maxCharHeight = map.MapSize.Height * map.ScreenSize.Height * map.TilemapInfo.TileHeight;
                    maxCharStride = map.CharStride;
                }
                
                if (mouseSelection.X / Globals.CharSize + map.OffsetX > maxCharStride ||
                    mouseSelection.Y / Globals.CharSize + map.OffsetY > maxCharHeight ||
                    (mouseSelection.X + mouseSelection.Width) / Globals.CharSize + map.OffsetX > maxCharStride ||
                    (mouseSelection.Y + mouseSelection.Height)/ Globals.CharSize + map.OffsetY > maxCharHeight)
                {
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
            AtariClipboard.Copy(windows[window].fontRendererMapImage, mouseSelection, windows[window].map.Offset);
            AtariClipboard.UnderClipBoardImage = new Bitmap(AtariClipboard.ClipboardImage);
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
        
        public static Point? PreviousClipboardLocation
        {
            get
            {
                return previousClipboardLocation;
            }
            set
            {
                previousClipboardLocation = value;
                previousClipboardGridCell = null;  // Reset grid cell when location is reset
            }
        }
        
        public static Point? PreviousClipboardGridCell
        {
            get
            {
                return previousClipboardGridCell;
            }
            set
            {
                previousClipboardGridCell = value;
            }
        }

        public static int PreviousOffset
        {
            set { previousOffset = value; }
        }

        private static void DrawSelection(Globals.WindowType window)
        {
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y);
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X + mouseSelection.Width, mouseSelection.Y, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y + mouseSelection.Height, mouseSelection.X + mouseSelection.Width, mouseSelection.Y + mouseSelection.Height);
            windows[window].pictureBoxGraphics.DrawLine(selectionPen, mouseSelection.X, mouseSelection.Y, mouseSelection.X, mouseSelection.Y + mouseSelection.Height);
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
            Redraw(window, drawData, drawScreenBorders, drawGrid, Point.Empty, false, Point.Empty);
        }

        public static void Redraw(Globals.WindowType window, bool drawData, bool drawScreenBorders, bool drawGrid, Point currentScreen, bool isLocked, Point lockedScreen)
        {
            Bitmap mapImage = windows[window].fontRendererMapImage;
            AtariMap myMap = windows[window].map;
            Graphics gr = windows[window].pictureBoxGraphics;
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            if (drawData)
            {
                // Clear the destination bitmap before rendering to ensure fresh data
                gr.Clear(Color.FromArgb(AtariPalette.GetPalette().Entries[0].ToArgb()));
                AtariFontRenderer.RenderMapData(myMap, windows[window].fontType, mapImage, window);
                Rectangle destRect = new Rectangle(0, 0, mapImage.Width * Globals.Zoom, mapImage.Height * Globals.Zoom);
                Rectangle srcRect = new Rectangle(0, 0, mapImage.Width, mapImage.Height);
                int screenCharWidth = myMap.ScreenSize.Width;
                int screenCharHeight = myMap.ScreenSize.Height;
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                    screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                }
                Point effectiveCurrentScreen = currentScreen;
                if (window == Globals.WindowType.Editor && effectiveCurrentScreen == Point.Empty && screenCharWidth > 0 && screenCharHeight > 0)
                {
                    effectiveCurrentScreen = new Point(myMap.OffsetX / screenCharWidth, myMap.OffsetY / screenCharHeight);
                    if (effectiveCurrentScreen.X < 0) effectiveCurrentScreen.X = 0;
                    if (effectiveCurrentScreen.Y < 0) effectiveCurrentScreen.Y = 0;
                    if (effectiveCurrentScreen.X >= myMap.MapSize.Width) effectiveCurrentScreen.X = myMap.MapSize.Width - 1;
                    if (effectiveCurrentScreen.Y >= myMap.MapSize.Height) effectiveCurrentScreen.Y = myMap.MapSize.Height - 1;
                }
                bool drawLinkOverlay = (window == Globals.WindowType.Editor && myMap.ScreenLinks != null &&
                    effectiveCurrentScreen.X >= 0 && effectiveCurrentScreen.Y >= 0 &&
                    effectiveCurrentScreen.X < myMap.MapSize.Width && effectiveCurrentScreen.Y < myMap.MapSize.Height &&
                    myMap.ScreenLinks.Any(l => l.SourceScreen.X == effectiveCurrentScreen.X && l.SourceScreen.Y == effectiveCurrentScreen.Y));
                if (window == Globals.WindowType.Editor && Globals.MetadataLayerVisible)
                {
                    // Draw map at 50% opacity so metadata text is visible on top
                    var cm = new ColorMatrix();
                    cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = 1f;
                    cm.Matrix33 = 0.5f;
                    cm.Matrix44 = 1f;
                    using (var ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(cm);
                        gr.DrawImage(mapImage, destRect, 0, 0, mapImage.Width, mapImage.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                else
                {
                    gr.DrawImage(mapImage, destRect, srcRect, GraphicsUnit.Pixel);
                }
                // Screen link overlay: draw linked screen with transparency ON TOP of the current screen area (so it is visible)
                if (drawLinkOverlay)
                {
                    var link = myMap.ScreenLinks.First(l => l.SourceScreen.X == effectiveCurrentScreen.X && l.SourceScreen.Y == effectiveCurrentScreen.Y);
                    int csSrcX = (effectiveCurrentScreen.X * screenCharWidth - myMap.OffsetX) * 8;
                    int csSrcY = (effectiveCurrentScreen.Y * screenCharHeight - myMap.OffsetY) * 8;
                    int csW = screenCharWidth * 8;
                    int csH = screenCharHeight * 8;
                    int csDstX = (effectiveCurrentScreen.X * screenCharWidth - myMap.OffsetX) * Globals.CharSize;
                    int csDstY = (effectiveCurrentScreen.Y * screenCharHeight - myMap.OffsetY) * Globals.CharSize;
                    int csDstW = screenCharWidth * Globals.CharSize;
                    int csDstH = screenCharHeight * Globals.CharSize;
                    if (csSrcX >= 0 && csSrcY >= 0 && csSrcX + csW <= mapImage.Width && csSrcY + csH <= mapImage.Height &&
                        csDstX + csDstW > 0 && csDstY + csDstH > 0 && csDstX < destRect.Width && csDstY < destRect.Height)
                    {
                        Rectangle currentScreenDestRect = new Rectangle(csDstX, csDstY, csDstW, csDstH);
                        ScreenLinkRenderer.DrawLinkedScreenIntoRect(myMap, mapImage, gr, link, currentScreenDestRect);
                    }
                }
            }
            //separatory screenov
            if (drawScreenBorders)
            {
                // For tilemaps, ScreenSize is in tiles, so convert to character units
                int screenCharWidth = myMap.ScreenSize.Width;
                int screenCharHeight = myMap.ScreenSize.Height;
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                    screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                }
                
                for (int x = 0; x <= (mapImage.Size.Width / 8) / screenCharWidth; x++)
                    gr.DrawLine(screenSeparatorPen, (-myMap.OffsetX % screenCharWidth + (x + 1) * screenCharWidth) * Globals.CharSize,
                                                 0, (-myMap.OffsetX % screenCharWidth + (x + 1) * screenCharWidth) * Globals.CharSize,
                                                 mapImage.Size.Height * Globals.Zoom);
                dliFormOrigin = new Point((-myMap.OffsetX % screenCharWidth + (0 + 1) * screenCharWidth) * Globals.CharSize,
                (-myMap.OffsetY % screenCharHeight + (0 + 1) * screenCharHeight) * Globals.CharSize);



                for (int y = 0; y <= (mapImage.Size.Height / 8) / screenCharHeight; y++)
                    gr.DrawLine(screenSeparatorPen, 0, (-myMap.OffsetY % screenCharHeight + (y + 1) * screenCharHeight) * Globals.CharSize,
                                      mapImage.Width * Globals.Zoom, (-myMap.OffsetY % screenCharHeight + (y + 1) * screenCharHeight) * Globals.CharSize);
            }
            //grid
            if (drawGrid)
            {
                Brush gridBrush = new SolidBrush(gridColor);
                int gridStepX = 1;
                int gridStepY = 1;
                
                // If tilemap is enabled, use tile size for grid
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    gridStepX = myMap.TilemapInfo.TileWidth;
                    gridStepY = myMap.TilemapInfo.TileHeight;
                }
                
                // For tilemaps, calculate character height for bounds checking
                int maxCharStride = myMap.Stride;
                int maxCharHeight = myMap.MapSize.Height * myMap.ScreenSize.Height;
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    maxCharStride = myMap.CharStride;
                    maxCharHeight = myMap.MapSize.Height * myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                }
                
                for (int x = 0; x < (mapImage.Size.Width / 8); x += gridStepX)
                    for (int y = 0; y < (mapImage.Size.Height / 8); y += gridStepY)
                        if (myMap.OffsetX + x < maxCharStride && myMap.OffsetY + y < maxCharHeight)
                            gr.FillRectangle(gridBrush, x * Globals.CharSize, y * Globals.CharSize, 1, 1);
                            //destImage.SetPixel(x * Globals.CharSize, y * Globals.CharSize, gridColor);
            }
            
            // Draw yellow L-shaped corners for current screen (on top of red borders)
            // Check if currentScreen is valid (within map bounds) instead of != Point.Empty
            // This allows screen 0,0 to be drawn
            if (window == Globals.WindowType.Editor && 
                currentScreen.X >= 0 && currentScreen.Y >= 0 && 
                currentScreen.X < myMap.MapSize.Width && currentScreen.Y < myMap.MapSize.Height)
            {
                DrawCurrentScreenCorners(gr, myMap, currentScreen);
            }
            
            // Draw "locked" text above/below locked screen (use bounds check so screen 0,0 is included)
            if (window == Globals.WindowType.Editor && isLocked &&
                lockedScreen.X >= 0 && lockedScreen.Y >= 0 &&
                lockedScreen.X < myMap.MapSize.Width && lockedScreen.Y < myMap.MapSize.Height)
            {
                DrawLockedText(gr, myMap, lockedScreen);
            }

            // Draw metadata layer overlay when enabled
            if (window == Globals.WindowType.Editor && Globals.MetadataLayerVisible)
            {
                Rectangle viewport = new Rectangle(0, 0, mapImage.Width * Globals.Zoom, mapImage.Height * Globals.Zoom);
                MetadataLayerRenderer.RenderMetadataLayer(myMap, gr, viewport, Globals.Zoom);
            }
        }
        
        private static void DrawCurrentScreenCorners(Graphics gr, AtariMap myMap, Point currentScreen)
        {
            Pen yellowPen = new Pen(Color.Yellow, 2);
            // 2 chars or 2 tiles in each direction for the L-shape
            int cornerSizeX = 2 * Globals.CharSize;
            int cornerSizeY = 2 * Globals.CharSize;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                cornerSizeX = 2 * myMap.TilemapInfo.TileWidth * Globals.CharSize;
                cornerSizeY = 2 * myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            
            // For tilemaps, ScreenSize is in tiles, so convert to character units
            int screenCharWidth = myMap.ScreenSize.Width;
            int screenCharHeight = myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            
            // Calculate screen position in pixels - need to account for scrolling offset
            int screenStartX = currentScreen.X * screenCharWidth;
            int screenStartY = currentScreen.Y * screenCharHeight;
            int screenPixelX = (screenStartX - myMap.OffsetX) * Globals.CharSize;
            int screenPixelY = (screenStartY - myMap.OffsetY) * Globals.CharSize;
            int screenPixelWidth = screenCharWidth * Globals.CharSize;
            int screenPixelHeight = screenCharHeight * Globals.CharSize;
            
            // Top-left corner
            gr.DrawLine(yellowPen, screenPixelX, screenPixelY, screenPixelX + cornerSizeX, screenPixelY);
            gr.DrawLine(yellowPen, screenPixelX, screenPixelY, screenPixelX, screenPixelY + cornerSizeY);
            
            // Top-right corner
            gr.DrawLine(yellowPen, screenPixelX + screenPixelWidth - cornerSizeX, screenPixelY, screenPixelX + screenPixelWidth, screenPixelY);
            gr.DrawLine(yellowPen, screenPixelX + screenPixelWidth, screenPixelY, screenPixelX + screenPixelWidth, screenPixelY + cornerSizeY);
            
            // Bottom-left corner
            gr.DrawLine(yellowPen, screenPixelX, screenPixelY + screenPixelHeight - cornerSizeY, screenPixelX, screenPixelY + screenPixelHeight);
            gr.DrawLine(yellowPen, screenPixelX, screenPixelY + screenPixelHeight, screenPixelX + cornerSizeX, screenPixelY + screenPixelHeight);
            
            // Bottom-right corner
            gr.DrawLine(yellowPen, screenPixelX + screenPixelWidth - cornerSizeX, screenPixelY + screenPixelHeight, screenPixelX + screenPixelWidth, screenPixelY + screenPixelHeight);
            gr.DrawLine(yellowPen, screenPixelX + screenPixelWidth, screenPixelY + screenPixelHeight - cornerSizeY, screenPixelX + screenPixelWidth, screenPixelY + screenPixelHeight);
        }
        
        private static void DrawLockedText(Graphics gr, AtariMap myMap, Point lockedScreen)
        {
            Font textFont = new Font("Arial", 12, FontStyle.Bold);
            Brush yellowBrush = new SolidBrush(Color.Yellow);
            
            // For tilemaps, ScreenSize is in tiles, so convert to character units
            int screenCharWidth = myMap.ScreenSize.Width;
            int screenCharHeight = myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            
            // Calculate screen position in pixels - need to account for scrolling offset
            int screenStartX = lockedScreen.X * screenCharWidth;
            int screenStartY = lockedScreen.Y * screenCharHeight;
            int screenPixelX = (screenStartX - myMap.OffsetX) * Globals.CharSize;
            int screenPixelY = (screenStartY - myMap.OffsetY) * Globals.CharSize;
            int screenPixelHeight = screenCharHeight * Globals.CharSize;
            
            string lockedText = "Locked";
            SizeF textSize = gr.MeasureString(lockedText, textFont);
            float textX = screenPixelX + (screenCharWidth * Globals.CharSize - textSize.Width) / 2;
            float textY;
            // When vertical coord is 0, show "Locked" below the screen; otherwise above
            if (lockedScreen.Y == 0)
                textY = screenPixelY + screenPixelHeight + 5;
            else
                textY = screenPixelY - textSize.Height - 5;
            
            gr.DrawString(lockedText, textFont, yellowBrush, textX, textY);
        }
        /// <summary>
        /// Performs scroll of a window.
        /// </summary>
        /// <param name="newMouseLoc"></param>
        /// <param name="window"></param>
        /// <returns>True if scrolled.</returns>
        public static bool Scroll(Point newMouseLoc, Globals.WindowType window)
        {
            AtariMap myMap = windows[window].map;
            myMap.Offset = previousOffset;
            
            // For tilemaps, scroll by tiles; for normal maps, scroll by characters
            int deltaX, deltaY;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                // Scroll by tile size for tilemaps
                int tilePixelWidth = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                int tilePixelHeight = myMap.TilemapInfo.TileHeight * Globals.CharSize;
                int tileDeltaX = (newMouseLoc.X - prevMouseLoc.X) / tilePixelWidth;
                int tileDeltaY = (newMouseLoc.Y - prevMouseLoc.Y) / tilePixelHeight;
                
                // Convert tile deltas to character deltas (since OffsetX/OffsetY are in character units)
                deltaX = tileDeltaX * myMap.TilemapInfo.TileWidth;
                deltaY = tileDeltaY * myMap.TilemapInfo.TileHeight;
            }
            else
            {
                // Scroll by character size for normal maps
                deltaX = (newMouseLoc.X - prevMouseLoc.X) / Globals.CharSize;
                deltaY = (newMouseLoc.Y - prevMouseLoc.Y) / Globals.CharSize;
            }
            
            bool newOffset = AtariFontRenderer.CalculateOffset(deltaX, deltaY, myMap);

            if (newOffset)
                Redraw(window);

            return newOffset;
        }

        public static void DrawClipBoard(Point location, Bitmap pictureBoxImage)
        {
            AtariMap myMap = windows[Globals.WindowType.Editor].map;
            Graphics gr = windows[Globals.WindowType.Editor].pictureBoxGraphics;
            
            // Ensure PixelOffsetMode matches the one used for grid drawing
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            
            // Calculate alignment based on map type
            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            
            // For tilemaps, align to tile size instead of char size
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            
            // Align location to grid using truncation
            int alignedX = (location.X / alignSizeX) * alignSizeX;
            int alignedY = (location.Y / alignSizeY) * alignSizeY;
            
            //store contents editor window contents under the current clipboard position -> underimage
            // pictureBoxImage is the zoomed image, so sourceRect should use pixel coordinates (alignedX, alignedY)
            // The UnderClipBoardImage is the same size as ClipboardImage (unzoomed), so we draw at 0,0
            Rectangle sourceRect = new Rectangle(alignedX, alignedY, AtariClipboard.ClipboardImage.Width, AtariClipboard.ClipboardImage.Height);
            AtariClipboard.UnderImageGraphics.DrawImage(pictureBoxImage, 0, 0, sourceRect, GraphicsUnit.Pixel);
            
            //draw clipboard -> location inside editor window (original size, not scaled, aligned to grid)
            // If SkipZero is enabled, create a mask for 0 chars/tiles based on clipboard data
            if (AtariClipboard.SkipZero)
            {
                // Get clipboard data to check for 0 chars/tiles
                byte[,] clipboardData = AtariClipboard.GetData();
                if (clipboardData != null)
                {
                    // Create a 32-bit ARGB bitmap with transparency for 0 chars/tiles
                    Bitmap transparentClipboard = new Bitmap(AtariClipboard.ClipboardImage.Width, AtariClipboard.ClipboardImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    
                    // Lock bits with appropriate format based on clipboard image format
                    System.Drawing.Imaging.PixelFormat srcFormat = AtariClipboard.ClipboardImage.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb
                        ? System.Drawing.Imaging.PixelFormat.Format32bppArgb
                        : System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    
                    BitmapData srcData = AtariClipboard.ClipboardImage.LockBits(
                        new Rectangle(0, 0, AtariClipboard.ClipboardImage.Width, AtariClipboard.ClipboardImage.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        srcFormat);
                    BitmapData dstData = transparentClipboard.LockBits(
                        new Rectangle(0, 0, transparentClipboard.Width, transparentClipboard.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    
                    // For tilemaps, one "cell" is the whole tile (tileWidth*tileHeight chars); for chars, one cell is 8x8
                    int cellWidthPx = 8 * Globals.Zoom;
                    int cellHeightPx = 8 * Globals.Zoom;
                    if (AtariClipboard.IsTileIndexes && myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        cellWidthPx = myMap.TilemapInfo.TileWidth * 8 * Globals.Zoom;
                        cellHeightPx = myMap.TilemapInfo.TileHeight * 8 * Globals.Zoom;
                    }
                    
                    unsafe
                    {
                        int* dstPtr = (int*)dstData.Scan0;
                        Color[] palette = AtariPalette.GetPalette().Entries;
                        
                        bool is32Bit = (srcFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        
                        for (int y = 0; y < AtariClipboard.ClipboardImage.Height; y++)
                        {
                            for (int x = 0; x < AtariClipboard.ClipboardImage.Width; x++)
                            {
                                // Which character/tile this pixel belongs to (tile = whole tile size when in tile mode)
                                int cellX = x / cellWidthPx;
                                int cellY = y / cellHeightPx;
                                
                                bool isZero = false;
                                if (cellX < AtariClipboard.ClipboardWidth && cellY < AtariClipboard.ClipboardHeight)
                                {
                                    byte dataVal = clipboardData[cellX, cellY];
                                    isZero = (dataVal == 0);
                                }
                                
                                if (isZero)
                                {
                                    dstPtr[y * dstData.Stride / 4 + x] = 0;
                                }
                                else
                                {
                                    if (is32Bit)
                                    {
                                        int* src32Ptr = (int*)srcData.Scan0;
                                        dstPtr[y * dstData.Stride / 4 + x] = src32Ptr[y * srcData.Stride / 4 + x];
                                    }
                                    else
                                    {
                                        byte* srcPtr = (byte*)srcData.Scan0;
                                        byte paletteIndex = srcPtr[y * srcData.Stride + x];
                                        Color color = palette[paletteIndex];
                                        dstPtr[y * dstData.Stride / 4 + x] = color.ToArgb();
                                    }
                                }
                            }
                        }
                    }
                    
                    AtariClipboard.ClipboardImage.UnlockBits(srcData);
                    transparentClipboard.UnlockBits(dstData);
                    
                    gr.DrawImage(transparentClipboard, alignedX, alignedY);
                    transparentClipboard.Dispose();
                }
                else
                {
                    // Fallback: draw normally if data is not available
                    gr.DrawImage(AtariClipboard.ClipboardImage, alignedX, alignedY);
                }
            }
            else
            {
                gr.DrawImage(
                    AtariClipboard.ClipboardImage, 
                    alignedX, 
                    alignedY);
            }
        }

        public static void DrawUnderClipBoard(Point location)
        {
            AtariMap myMap = windows[Globals.WindowType.Editor].map;
            Graphics gr = windows[Globals.WindowType.Editor].pictureBoxGraphics;
            
            // Ensure PixelOffsetMode matches the one used for grid drawing
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            
            // Calculate alignment based on map type
            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            
            // For tilemaps, align to tile size instead of char size
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            
            // Align location to grid using truncation (must match DrawClipBoard alignment)
            int alignedX = (location.X / alignSizeX) * alignSizeX;
            int alignedY = (location.Y / alignSizeY) * alignSizeY;
            
            gr.DrawImage(
                AtariClipboard.UnderClipBoardImage, 
                alignedX, 
                alignedY, 
                AtariClipboard.ClipboardImage.Width, 
                AtariClipboard.ClipboardImage.Height);
        }

        /// <summary>Restore map content under the metadata overlay at the given location (aligns to grid).</summary>
        public static void DrawMetadataUnder(Point location, Bitmap underImage)
        {
            if (underImage == null) return;
            AtariMap myMap = windows[Globals.WindowType.Editor].map;
            Graphics gr = windows[Globals.WindowType.Editor].pictureBoxGraphics;
            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            int alignedX = (location.X / alignSizeX) * alignSizeX;
            int alignedY = (location.Y / alignSizeY) * alignSizeY;
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            gr.DrawImage(underImage, alignedX, alignedY, underImage.Width, underImage.Height);
        }

        /// <summary>Capture map under cursor into underImage, draw previewImage at aligned location. Alignment matches character/tile grid.</summary>
        public static void DrawMetadataOverlay(Point location, Bitmap mapImage, Bitmap underImage, Bitmap previewImage)
        {
            if (mapImage == null || underImage == null || previewImage == null) return;
            AtariMap myMap = windows[Globals.WindowType.Editor].map;
            Graphics gr = windows[Globals.WindowType.Editor].pictureBoxGraphics;
            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            int alignedX = (location.X / alignSizeX) * alignSizeX;
            int alignedY = (location.Y / alignSizeY) * alignSizeY;
            int w = previewImage.Width;
            int h = previewImage.Height;
            if (underImage.Width != w || underImage.Height != h)
                return;
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            using (var underGr = Graphics.FromImage(underImage))
                underGr.DrawImage(mapImage, 0, 0, new Rectangle(alignedX, alignedY, w, h), GraphicsUnit.Pixel);
            gr.DrawImage(previewImage, alignedX, alignedY, w, h);
        }
        /*
        public static void SetDestImage(Bitmap _destImage, Graphics _gr)
        {
            destImage = _destImage;
            gr = _gr;
        }*/
    }
}
