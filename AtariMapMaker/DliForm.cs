using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AtariMapMaker
{
    public partial class DliForm : Form
    {
        private readonly AtariMap dliMap;
        private readonly AtariMap screenMap;
        public const Globals.WindowType window = Globals.WindowType.Dli;
        private readonly AtariColorPicker colorPicker;
        public int screenNumber { get; private set; }
        private readonly PictureBox pictureBoxScreen;
        private readonly byte[,] clipBoard;
        private Globals.ClipBoardEnum clipBoardDataType;
        private Point clickedChar;

        public DliForm(AtariMap screenMap, PictureBox pictureBoxScreen)
        {
            this.screenMap = screenMap;
            this.pictureBoxScreen = pictureBoxScreen;
            // For tilemaps, ScreenSize.Height is in tiles, so convert to character lines
            int lines = GetScreenCharHeight(screenMap);
            clipBoard = new byte[lines, 6];
            dliMap = new AtariMap(new Size(1, 1), new Size(6, lines));
            byte[] dliFormFontData = new byte[1024];

            // Initialize columns 0-4 for colors (as before)
            for (int a = 0; a < lines * 6; a++)
            {
                int col = a % 6;
                if (col < 5)
                    dliMap.Data[a] = (col == 3) ? (byte)0x82 : (byte)col;
                else
                    // Column 5: font numbers will be set from FontLineMapping
                    dliMap.Data[a] = 0x30; // Default to '0'
            }

            // Create DLI font with digit characters (0-7) at positions 0x30-0x37
            for (int i = 0; i < 8; i++)
            {
                dliFormFontData[i] = 0b01010101;
                dliFormFontData[i + 8] = 0b10101010;
                dliFormFontData[i + 16] = 0b11111111;
            }
            // Add digit characters 0-7 at positions 0x30-0x37
            // Each digit is 8 bytes (8 rows)
            for (int digit = 0; digit < 8; digit++)
            {
                int baseOffset = 0x30 * 8 + digit * 8;
                // Create a simple digit pattern (vertical bars for digits)
                for (int row = 0; row < 8; row++)
                {
                    // Simple pattern: show digit value as binary pattern
                    byte pattern = (byte)((digit == row % 8) ? 0xFF : 0x00);
                    // Better: create actual digit shapes
                    if (row == 0 || row == 7)
                        pattern = 0x7E; // Top and bottom: 01111110
                    else if (row == 3 || row == 4)
                        pattern = 0x42; // Middle: 01000010
                    else
                        pattern = (byte)(0x42 | (digit << 1)); // Sides with digit indicator
                    dliFormFontData[baseOffset + row] = pattern;
                }
            }
            // Better approach: use standard digit patterns
            // Digits 0-7: create recognizable patterns
            CreateDigitFont(dliFormFontData, 0x30, 0, 0, 16, 68, 68, 68, 68, 16, 0); // 0
            CreateDigitFont(dliFormFontData, 0x31, 1, 0, 16, 16, 16, 16, 16, 16, 0); // 1
            CreateDigitFont(dliFormFontData, 0x32, 2, 0, 16, 68, 4, 16, 64, 84, 0); // 2
            CreateDigitFont(dliFormFontData, 0x33, 3, 0, 84, 4, 16, 4, 68, 16, 0); // 3
            CreateDigitFont(dliFormFontData, 0x34, 4, 0, 64, 64, 64, 68, 84, 4, 0); // 4
            CreateDigitFont(dliFormFontData, 0x35, 5, 0, 84, 64, 80, 4, 68, 16, 0); // 5
            CreateDigitFont(dliFormFontData, 0x36, 6, 0, 20, 64, 80, 68, 68, 16, 0); // 6
            CreateDigitFont(dliFormFontData, 0x37, 7, 0, 84, 4, 4, 16, 16, 16, 0); // 7

            AtariFontRenderer.SetFontData(dliFormFontData, Globals.FontType.Dli);
            InitializeComponent();
            this.Font = new Font("Segoe UI", 8F);
            // Initial width will be set in Show() method based on MultiFont state
            bool showFontColumn = screenMap.MultiFontEnabled;
            int numColumns = showFontColumn ? 6 : 5;
            pictureBoxDli.Width = numColumns * Globals.CharSize;
            pictureBoxDli.Height = dliMap.ScreenSize.Height * Globals.CharSize;
            pictureBoxDli.Image = new Bitmap(numColumns * Globals.CharSize, lines * Globals.CharSize);
            this.Width = numColumns * Globals.CharSize + 2; // +2 for border
            this.Refresh();
            AtariPictureTools.AssignWindow(Globals.WindowType.Dli, (Bitmap)pictureBoxDli.Image, dliMap);
            colorPicker = new AtariColorPicker();
        }

        private void CreateDigitFont(byte[] fontData, int charCode, int digit, byte r0, byte r1, byte r2, byte r3, byte r4, byte r5, byte r6, byte r7)
        {
            int offset = charCode * 8;
            fontData[offset + 0] = r0;
            fontData[offset + 1] = r1;
            fontData[offset + 2] = r2;
            fontData[offset + 3] = r3;
            fontData[offset + 4] = r4;
            fontData[offset + 5] = r5;
            fontData[offset + 6] = r6;
            fontData[offset + 7] = r7;
        }

        /// <summary>
        /// Get screen height in character lines (for tilemaps, converts from tiles to characters)
        /// </summary>
        private int GetScreenCharHeight(AtariMap map)
        {
            if (map.IsTilemap && map.TilemapInfo != null)
            {
                return map.ScreenSize.Height * map.TilemapInfo.TileHeight;
            }
            return map.ScreenSize.Height;
        }

        public AtariMap DliMap { get { return dliMap; } }
        public void Show(int screenNumber)
        {
            this.screenNumber = screenNumber;
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;
            int screenCharHeight = GetScreenCharHeight(screenMap);
            
            // Copy colors from screenMap to dliMap (this was done in UpdateAndShowDliForm, but we need to ensure it's done here too)
            // The CopyDliColorsFullScreen is called in UpdateAndShowDliForm before Show(), so dliMap should already have the colors
            
            // Update font numbers in column 5 from FontLineMapping (only if MultiFont is enabled)
            bool showFontColumn = screenMap.MultiFontEnabled;
            if (showFontColumn)
            {
                for (int line = 0; line < screenCharHeight; line++)
                {
                    byte fontIndex = screenMap.GetFontForLine(screenX, screenY, line);
                    int charOffset = 5 + line * dliMap.Stride;
                    dliMap.Data[charOffset] = (byte)(0x30 + fontIndex); // 0x30-0x37 for digits 0-7
                }
            }
            else if (!showFontColumn)
            {
                // Hide font column by setting it to space or background
                for (int line = 0; line < screenCharHeight; line++)
                {
                    int charOffset = 5 + line * dliMap.Stride;
                    dliMap.Data[charOffset] = 0x20; // Space character
                }
            }
            
            // Update form width based on MultiFont state
            int numColumns = showFontColumn ? 6 : 5;
            pictureBoxDli.Width = numColumns * Globals.CharSize;
            this.Width = numColumns * Globals.CharSize + 2; // +2 for border
            
            this.Show();
        }

        public void RenderData(bool refreshPictureBoxOnly = false)
        {
            if (!refreshPictureBoxOnly)
                AtariPictureTools.Redraw(Globals.WindowType.Dli);
            pictureBoxDli.Refresh();
        }
        public void ZoomResize()
        {
            bool showFontColumn = screenMap.MultiFontEnabled;
            int numColumns = showFontColumn ? 6 : 5;
            int screenCharHeight = GetScreenCharHeight(screenMap);
            pictureBoxDli.Width = numColumns * Globals.CharSize;
            pictureBoxDli.Height = screenCharHeight * Globals.CharSize;
            pictureBoxDli.Image = new Bitmap(pictureBoxDli.Width, pictureBoxDli.Height);
            this.Width = numColumns * Globals.CharSize + 2; // +2 for border
            AtariPictureTools.AssignWindow(window, (Bitmap)pictureBoxDli.Image, dliMap);
            AtariPictureTools.Redraw(Globals.WindowType.Dli, true, false, true);
        }
        public void Initialize()
        {
       
            //AtariFontRenderer.FontData = dliFormFontData;
        }

        private void PictureBoxDli_MouseDown(object sender, MouseEventArgs e)
        {
            int xchar = e.X / Globals.CharSize;
            int ychar = e.Y / Globals.CharSize;
            clickedChar = new Point(xchar, ychar);

            // Handle font number column (column 5) - only if MultiFont is enabled
            if (xchar == 5 && screenMap.MultiFontEnabled)
            {
                int screenCharHeight = GetScreenCharHeight(screenMap);
                if (ychar < 0 || ychar >= screenCharHeight)
                    return;

                int screenX = screenNumber % screenMap.MapSize.Width;
                int screenY = screenNumber / screenMap.MapSize.Width;
                
                // Check if this screen references another screen - if so, editing is not allowed
                int refScreenX, refScreenY;
                bool isReferencing = screenMap.GetFontMappingReference(screenX, screenY, out refScreenX, out refScreenY);
                if (isReferencing && (refScreenX != screenX || refScreenY != screenY))
                {
                    // This screen references another screen - editing is read-only
                    // Just show context menu on right click, but don't allow editing
                    if (e.Button == MouseButtons.Right)
                    {
                        FillContextMenu();
                        contextMenuStripDli.Show(pictureBoxDli, e.X, e.Y);
                    }
                    // Left click does nothing when referencing
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    // In tilemap mode, font numbers are read-only (inherited from tiles)
                    if (screenMap.IsTilemap)
                    {
                        return; // Cannot edit font numbers in tilemap mode
                    }
                    
                    // Left click: increase font number (0-7, wrapping)
                    // Shift+Left click: decrease font number (0-7, wrapping)
                    byte currentFont = screenMap.GetFontForLine(screenX, screenY, ychar);
                    byte newFont;
                    
                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    {
                        // Shift+Left click: decrease font number
                        newFont = (byte)((currentFont + 7) % 8); // +7 is same as -1 mod 8
                    }
                    else
                    {
                        // Left click: increase font number
                        newFont = (byte)((currentFont + 1) % 8);
                    }
                    
                    screenMap.SetFontForLine(screenX, screenY, ychar, newFont);
                    // Update the displayed character in column 5
                    int charOffset = 5 + ychar * dliMap.Stride;
                    dliMap.Data[charOffset] = (byte)(0x30 + newFont); // 0x30-0x37 for digits 0-7
                    AtariFontRenderer.ClearFontCache();
                    RedrawDliAndMap();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Right click: show context menu (for fill down, copy/paste, etc.)
                    FillContextMenu();
                    contextMenuStripDli.Show(pictureBoxDli, e.X, e.Y);
                }
                return;
            }

            // Handle color columns (0-4)
            if (xchar < 5)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        byte[] color5 = dliMap.GetDliColor5(xchar + ychar * dliMap.Stride);
                        colorPicker.Pick(color5[0] == Globals.DEFAULT_COLOR ? AtariFontRenderer.Color5[xchar] : color5[xchar]);
                        if (colorPicker.PickedNewColor)
                        {
                            if (color5[0] == Globals.DEFAULT_COLOR)
                                InitializeDliDataForSelectedScreen();   //initialize DLI data

                            dliMap.SetDliColor(0, 0, ychar, xchar, colorPicker.PickedColorIndex);
                            screenMap.SetDliColor(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, ychar, xchar, colorPicker.PickedColorIndex);
                            RedrawDliAndMap();
                        }
                        break;
                    case MouseButtons.Right:
                        FillContextMenu();
                        contextMenuStripDli.Show(pictureBoxDli, e.X, e.Y);
                        break;
                }
            }
        }

        private void ShowFontSelector(int line)
        {
            // Show font selection dialog for this line
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;
            using (FontSelectorDialog dialog = new FontSelectorDialog(screenMap, line, screenX, screenY))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    RedrawDliAndMap();
                }
            }
        }
        /// <summary>
        /// Fills up DLI data of selected screen with current common colors (AtariFontRenderer.Color5)
        /// </summary>
        private void InitializeDliDataForSelectedScreen()
        {
            dliMap.SetDliColor5Multiple(0, 0, 0, -1, AtariFontRenderer.Color5);
            screenMap.SetDliColor5Multiple(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, 0, -1, AtariFontRenderer.Color5);
        }

        private ToolStripMenuItem fontsForWholeScreenToolStripMenuItem;
        private ToolStripMenuItem copyAllFontsToClipboardToolStripMenuItem;
        private ToolStripMenuItem pasteAllFontsFromClipboardToolStripMenuItem;

        private void FillContextMenu()
        {
            fillDown5ToolStripMenuItem.DropDownItems.Clear();
            fillDown1ToolStripMenuItem.DropDownItems.Clear();
            int screenCharHeight = GetScreenCharHeight(screenMap);
            for (int i = 1; i < screenCharHeight - clickedChar.Y; i++)
            {
                ToolStripItem tsi = fillDown5ToolStripMenuItem.DropDownItems.Add(i.ToString());
                tsi.Click += Number_Click;
                tsi = fillDown1ToolStripMenuItem.DropDownItems.Add(i.ToString());
                tsi.Click += Number_Click;
            }
            paste1FromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.color;
            paste5FromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.color5;
            pasteAllFromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.colorAll;

            // Add "Fonts for whole screen" menu item if MultiFont is enabled
            if (screenMap.MultiFontEnabled)
            {
                if (fontsForWholeScreenToolStripMenuItem == null)
                {
                    fontsForWholeScreenToolStripMenuItem = new ToolStripMenuItem("Fonts for whole screen");
                    copyAllFontsToClipboardToolStripMenuItem = new ToolStripMenuItem("Copy to clipboard");
                    copyAllFontsToClipboardToolStripMenuItem.Click += CopyAllFontsToClipboardToolStripMenuItem_Click;
                    pasteAllFontsFromClipboardToolStripMenuItem = new ToolStripMenuItem("Paste from clipboard");
                    pasteAllFontsFromClipboardToolStripMenuItem.Click += PasteAllFontsFromClipboardToolStripMenuItem_Click;
                    
                    fontsForWholeScreenToolStripMenuItem.DropDownItems.Add(copyAllFontsToClipboardToolStripMenuItem);
                    fontsForWholeScreenToolStripMenuItem.DropDownItems.Add(pasteAllFontsFromClipboardToolStripMenuItem);
                    
                    contextMenuStripDli.Items.Add(new ToolStripSeparator());
                    contextMenuStripDli.Items.Add(fontsForWholeScreenToolStripMenuItem);
                }
                else if (!contextMenuStripDli.Items.Contains(fontsForWholeScreenToolStripMenuItem))
                {
                    // Re-add if it was removed
                    contextMenuStripDli.Items.Add(new ToolStripSeparator());
                    contextMenuStripDli.Items.Add(fontsForWholeScreenToolStripMenuItem);
                }
                
                // Enable/disable paste menu item
                pasteAllFontsFromClipboardToolStripMenuItem.Enabled = clipBoardDataType == Globals.ClipBoardEnum.fontAll;
            }
            else
            {
                // Remove font menu if MultiFont is disabled
                if (fontsForWholeScreenToolStripMenuItem != null && contextMenuStripDli.Items.Contains(fontsForWholeScreenToolStripMenuItem))
                {
                    contextMenuStripDli.Items.Remove(fontsForWholeScreenToolStripMenuItem);
                    // Note: We don't set to null so it can be re-added when MultiFont is enabled again
                }
            }
        }

        private void Number_Click(object sender, EventArgs e)
        {
            var tsi = (ToolStripItem)sender;
            bool wholeLine = tsi.OwnerItem.OwnerItem.Name == colorsToolStripMenuItem.Name;
            int lines = int.Parse(tsi.Text) + 1;
            FillDown(wholeLine, lines);
            RedrawDliAndMap();
        }

        private void PickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBoxDli_MouseDown(sender, new MouseEventArgs(MouseButtons.Left, 1, clickedChar.X*Globals.CharSize, clickedChar.Y*Globals.CharSize, 0));
        }

        private void FillDown(bool wholeLine, int lines)
        {
            int startingLine = clickedChar.Y;
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;

            if (clickedChar.X == 5 && screenMap.MultiFontEnabled)
            {
                // Check if this screen references another screen - if so, editing is not allowed
                int refScreenX, refScreenY;
                bool isReferencing = screenMap.GetFontMappingReference(screenX, screenY, out refScreenX, out refScreenY);
                if (isReferencing && (refScreenX != screenX || refScreenY != screenY))
                {
                    // This screen references another screen - editing is read-only
                    return;
                }
                
                // Fill down font numbers
                byte fontIndex = screenMap.GetFontForLine(screenX, screenY, clickedChar.Y);
                int screenCharHeight = GetScreenCharHeight(screenMap);
                for (int j = 0; j < lines && (startingLine + j) < screenCharHeight; j++)
                {
                    screenMap.SetFontForLine(screenX, screenY, startingLine + j, fontIndex);
                    int charOffset = 5 + (startingLine + j) * dliMap.Stride;
                    dliMap.Data[charOffset] = (byte)(0x30 + fontIndex);
                }
                AtariFontRenderer.ClearFontCache();
            }
            else if (clickedChar.X < 5)
            {
                // Fill down colors
                byte[] color5 = dliMap.GetDliColor5(clickedChar.X + 6 * clickedChar.Y);
                if (wholeLine)
                {
                    dliMap.SetDliColor5Multiple(0, 0, startingLine, lines, color5);
                    screenMap.SetDliColor5Multiple(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, startingLine, lines, color5);
                }
                else
                {
                    dliMap.SetDliColorMultiple(0, 0, startingLine, lines, clickedChar.X, color5[clickedChar.X]);
                    screenMap.SetDliColorMultiple(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, startingLine, lines, clickedChar.X, color5[clickedChar.X]);
                }
            }
            RedrawDliAndMap();
        }

        private void RedrawDliAndMap()
        {
            AtariPictureTools.Redraw(window);
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            pictureBoxDli.Refresh();
            pictureBoxScreen.Refresh();
        }

        private void CopyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickedChar.X == 5 && screenMap.MultiFontEnabled)
            {
                // Copy font number
                int screenX = screenNumber % screenMap.MapSize.Width;
                int screenY = screenNumber / screenMap.MapSize.Width;
                byte fontIndex = screenMap.GetFontForLine(screenX, screenY, clickedChar.Y);
                clipBoard[0, 5] = fontIndex;
                clipBoardDataType = Globals.ClipBoardEnum.color; // Reuse enum for single value
            }
            else if (clickedChar.X < 5)
            {
                byte[] color5 = dliMap.GetDliColor5(clickedChar);
                clipBoard[0, clickedChar.X] = color5[clickedChar.X];
                clipBoardDataType = Globals.ClipBoardEnum.color;
            }
        }

        private void Copy5ToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = dliMap.GetDliColor5(clickedChar);
            for (int i = 0; i < color5.Length; i++)
                clipBoard[0, i] = color5[i];
            // Only copy colors, not fonts
            clipBoardDataType = Globals.ClipBoardEnum.color5;
        }

        private void CopyAllToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;
            int screenCharHeight = GetScreenCharHeight(screenMap);
            for (int i = 0; i < screenCharHeight; i++)
            {
                byte[] color5 = dliMap.GetDliColor5(i * 6);
                for (int j = 0; j < 5; j++)
                    clipBoard[i, j] = color5[j];
                // Only copy colors, not fonts
            }
            clipBoardDataType = Globals.ClipBoardEnum.colorAll;
        }

        private void Paste1FromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickedChar.X == 5 && screenMap.MultiFontEnabled)
            {
                // Paste font number
                int screenX = screenNumber % screenMap.MapSize.Width;
                int screenY = screenNumber / screenMap.MapSize.Width;
                byte fontIndex = clipBoard[0, 5];
                screenMap.SetFontForLine(screenX, screenY, clickedChar.Y, fontIndex);
                int charOffset = 5 + clickedChar.Y * dliMap.Stride;
                dliMap.Data[charOffset] = (byte)(0x30 + fontIndex);
                AtariFontRenderer.ClearFontCache();
            }
            else if (clickedChar.X < 5)
            {
                byte[] color5 = dliMap.GetDliColor5(clickedChar);
                if (color5[0] == Globals.DEFAULT_COLOR)
                    InitializeDliDataForSelectedScreen();   //initialize DLI data

                dliMap.SetDliColor(0, 0, clickedChar.Y, clickedChar.X, clipBoard[0, 0]);
                screenMap.SetDliColor(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, clickedChar.Y, clickedChar.X, clipBoard[0, 0]);
            }
            RedrawDliAndMap();
        }

        private void Paste5FromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = dliMap.GetDliColor5(clickedChar);
            if (color5[0] == Globals.DEFAULT_COLOR)
                InitializeDliDataForSelectedScreen();   //initialize DLI data

            for (int i = 0; i < 5; i++)
                color5[i] = clipBoard[0, i];
            dliMap.SetDliColor5Multiple(0, 0, clickedChar.Y, 1, color5);
            screenMap.SetDliColor5Multiple(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, clickedChar.Y, 1, color5);
            
            // Only paste colors, not fonts
            RedrawDliAndMap();
        }

        private void PasteAllFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = new byte[5];
            int screenCharHeight = GetScreenCharHeight(screenMap);
            for (int j = 0; j < screenCharHeight; j++)
            {
                for (int i = 0; i < 5; i++)
                    color5[i] = clipBoard[j, i];
                dliMap.SetDliColor5Multiple(0, 0, j, 1, color5);
                screenMap.SetDliColor5Multiple(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width, j, 1, color5);
                
                // Only paste colors, not fonts
            }
            RedrawDliAndMap();
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] color5 = new byte[5];
            color5[0] = Globals.DEFAULT_COLOR;
            dliMap.SetDliColor5Multiple(0,0,0,-1,color5);
            screenMap.SetDliColor5Multiple(screenNumber % screenMap.MapSize.Width, screenNumber / screenMap.MapSize.Width,0 , -1, color5);
            RedrawDliAndMap();
        }

        private void CopyAllFontsToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;
            int screenCharHeight = GetScreenCharHeight(screenMap);
            for (int i = 0; i < screenCharHeight; i++)
            {
                clipBoard[i, 5] = screenMap.GetFontForLine(screenX, screenY, i);
            }
            clipBoardDataType = Globals.ClipBoardEnum.fontAll;
        }

        private void PasteAllFontsFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;
            
            // Check if this screen references another screen - if so, editing is not allowed
            int refScreenX, refScreenY;
            bool isReferencing = screenMap.GetFontMappingReference(screenX, screenY, out refScreenX, out refScreenY);
            if (isReferencing && (refScreenX != screenX || refScreenY != screenY))
            {
                // This screen references another screen - editing is read-only
                return;
            }
            
            int screenCharHeight = GetScreenCharHeight(screenMap);
            for (int j = 0; j < screenCharHeight; j++)
            {
                if (screenMap.MultiFontEnabled)
                {
                    byte fontIndex = clipBoard[j, 5];
                    screenMap.SetFontForLine(screenX, screenY, j, fontIndex);
                    int charOffset = 5 + j * dliMap.Stride;
                    dliMap.Data[charOffset] = (byte)(0x30 + fontIndex);
                }
            }
            AtariFontRenderer.ClearFontCache();
            RedrawDliAndMap();
        }

        private void FillDownAllFontsNumber_Click(object sender, EventArgs e)
        {
            int screenX = screenNumber % screenMap.MapSize.Width;
            int screenY = screenNumber / screenMap.MapSize.Width;
            
            // Check if this screen references another screen - if so, editing is not allowed
            int refScreenX, refScreenY;
            bool isReferencing = screenMap.GetFontMappingReference(screenX, screenY, out refScreenX, out refScreenY);
            if (isReferencing && (refScreenX != screenX || refScreenY != screenY))
            {
                // This screen references another screen - editing is read-only
                return;
            }
            
            var tsi = (ToolStripItem)sender;
            int lines = int.Parse(tsi.Text) + 1;
            int startingLine = clickedChar.Y;
            
            // Fill down font numbers
            byte fontIndex = screenMap.GetFontForLine(screenX, screenY, clickedChar.Y);
            int screenCharHeight = GetScreenCharHeight(screenMap);
            for (int j = 0; j < lines && (startingLine + j) < screenCharHeight; j++)
            {
                screenMap.SetFontForLine(screenX, screenY, startingLine + j, fontIndex);
                int charOffset = 5 + (startingLine + j) * dliMap.Stride;
                dliMap.Data[charOffset] = (byte)(0x30 + fontIndex);
            }
            AtariFontRenderer.ClearFontCache();
            RedrawDliAndMap();
        }
    }
}
