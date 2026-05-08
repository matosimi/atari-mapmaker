using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AtariMapMaker
{
    public partial class MainForm : Form
    {
        private AtariColorPicker colorPickerForm;
        private AtariMap myMap;
        private string mouseStatus = "";
        //private Bitmap dataImage;                                   //picture with map (data only, no zoom)
        //private Graphics gr;
        private FontCharPicker myCharPicker;
        private TilePicker tilePicker;
        private ElementLibraryDialog elementLibraryDialog;
        private DliForm dliForm;
        private Point currentScreen = new Point(0, 0);
        private Point previousScreen = new Point(-1, -1);  // Track previous screen to optimize redraws
        private bool ScreenSelectionShown = false;
        private bool isScreenLocked = false;
        private Point lockedScreen = new Point(0, 0);
        private bool isContinuousPasteMode = false;  // Track if CTRL+Left mouse is held for continuous paste
        private Point? lastContinuousPasteCell = null;  // Track last grid cell where we pasted in continuous mode
        // Metadata paste-mode overlay: show copied metadata cell under cursor
        private Bitmap metadataUnderImage = null;
        private Bitmap metadataPreviewImage = null;
        private Point? previousMetadataOverlayLocation = null;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "AtariMapMaker v" + version + " by Martin Simecek";
            labelAbout2.Text = "Version " + version + "\n" + Properties.Resources.BuildDate;
            toolTip1.SetToolTip(buttonRefreshFont, "Reload font");

            AtariPalette.Load(Properties.Resources.altirraPAL);
            colorPickerForm = new AtariColorPicker();
            AtariFontRenderer.SetFontData(Properties.Resources.Default, Globals.FontType.Screen);

            myMap = new AtariMap(new Size(4, 4), new Size(32, 20));
            undoManager = new UndoManager(myMap);
            numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;

            this.FillFontColorList();
            pictureBoxMap.Image = new Bitmap(pictureBoxMap.Width, pictureBoxMap.Height)
            {
                Palette = AtariPalette.GetPalette()
            };
            UpdateEditorWindowSizeInChars();
            //dataImage = new Bitmap(Globals.editorWindowSizeInChars.Width * 8, Globals.editorWindowSizeInChars.Height * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        
            /*
            AtariFontRenderer.RenderMapData(myMap, 0, dataImage);
            gr = Graphics.FromImage(pictureBoxMap.Image);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            */
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            AtariPictureTools.Redraw(Globals.WindowType.Editor);

            myCharPicker = new FontCharPicker(pictureBoxClipboard, myMap);

            comboOperation.Items.AddRange(new String[8] { "Export", "Import", "Column Export", "Column Import", "Export DLI", "Import DLI", "Export screen by screen", "Import screen by screen" });
            comboOperation.SelectedIndex = 0;

            dliForm = new DliForm(myMap, pictureBoxMap);
            dliForm.RenderData();

            // Set initial state for V2 UI (controls are in Designer)
            checkBoxMultiFont.Checked = myMap.MultiFontEnabled;
            Globals.MetadataLayerShowText = checkBoxMetadataShowText.Checked;
            Globals.MetadataLayerShowColorLinks = checkBoxMetaDataShowColorLinks != null && checkBoxMetaDataShowColorLinks.Checked;
            Globals.MetadataLayerShowValueLinks = checkBoxMetaDataShowValueLinks != null && checkBoxMetaDataShowValueLinks.Checked;
            UpdateFontMappingReferenceUI();
            UpdateMultiFontUI();
            UpdateMetadataLayerUI();

            // Set up clipboard-related event handlers
            pictureBoxClipboard.Click += PictureBoxClipboard_Click;
            if (buttonClipboardInverse != null)
            {
                buttonClipboardInverse.Click += ButtonClipboardInverse_Click;
            }
            if (checkBoxClipboardSkip0 != null)
            {
                checkBoxClipboardSkip0.CheckedChanged += CheckBoxClipboardSkip0_CheckedChanged;
                // Initialize SkipZero from checkbox state
                AtariClipboard.SkipZero = checkBoxClipboardSkip0.Checked;
            }
            this.KeyDown += MainForm_KeyDown;
            this.KeyPreview = true;  // Enable key preview so form receives key events
            
            // Update button state based on map type
            UpdateClipboardInverseButtonState();
        }

        private UndoManager undoManager;

        private void MenuItemUndo_Click(object sender, EventArgs e)
        {
            if (undoManager != null && undoManager.CanUndo())
            {
                undoManager.Undo();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }

        private void MenuItemRedo_Click(object sender, EventArgs e)
        {
            if (undoManager != null && undoManager.CanRedo())
            {
                undoManager.Redo();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }

        private void MenuItemLinkScreen_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (ScreenLinkDialog dialog = new ScreenLinkDialog(myMap, currentScreen))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
            }
        }

        private void MenuItemScreenMetadata_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            Point screen = isScreenLocked ? lockedScreen : currentScreen;
            using (ScreenMetadataDialog dialog = new ScreenMetadataDialog(myMap, screen))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    RedrawEditorWindow();
                    pictureBoxMap.Refresh();
                }
                UpdateMetadataLayerUI();
            }
        }

        private void MenuItemExportMetadata_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            Point screen = isScreenLocked ? lockedScreen : currentScreen;
            string key = $"{screen.X},{screen.Y}";
            if (myMap.ScreenMetadata == null || !myMap.ScreenMetadata.ContainsKey(key))
            {
                MessageBox.Show("No metadata for this screen.", "Export metadata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var meta = myMap.ScreenMetadata[key];
            if (meta?.ParsedItems == null || meta.ParsedItems.Count == 0)
            {
                MessageBox.Show("No metadata items for this screen.", "Export metadata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var exportForm = new MetadataExportForm(myMap, screen, meta.ParsedItems))
            {
                exportForm.ShowDialog();
            }
        }

        private void CheckBoxMetadataLayer_CheckedChanged(object sender, EventArgs e)
        {
            Globals.MetadataLayerVisible = checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked;
            UpdateMetadataLayerUI();
            RedrawEditorWindow();
            pictureBoxMap.Refresh();
        }

        private int GetMetadataItemCount()
        {
            if (myMap?.ScreenMetadata == null) return 0;
            int count = 0;
            foreach (var meta in myMap.ScreenMetadata.Values)
                count += meta?.ParsedItems?.Count ?? 0;
            return count;
        }

        private void UpdateMetadataLayerUI()
        {
            int itemCount = GetMetadataItemCount();
            if (groupBoxMetadata != null)
                groupBoxMetadata.Text = $"Metadata {itemCount}";
            bool metadataChecked = checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked;
            if (groupBoxDli != null)
                groupBoxDli.Enabled = !metadataChecked;
            if (groupBoxFont != null)
                groupBoxFont.Enabled = !metadataChecked;
            if (buttonMassChangeMetadata != null)
                buttonMassChangeMetadata.Enabled = metadataChecked;
            if (checkBoxMetaDataShowColorLinks != null)
                checkBoxMetaDataShowColorLinks.Enabled = metadataChecked;
            if (checkBoxMetaDataShowValueLinks != null)
                checkBoxMetaDataShowValueLinks.Enabled = metadataChecked;
            if (metadataChecked)
            {
                Globals.MetadataLayerShowColorLinks = checkBoxMetaDataShowColorLinks != null && checkBoxMetaDataShowColorLinks.Checked;
                Globals.MetadataLayerShowValueLinks = checkBoxMetaDataShowValueLinks != null && checkBoxMetaDataShowValueLinks.Checked;
            }
        }

        private void CheckBoxMetadataShowText_CheckedChanged(object sender, EventArgs e)
        {
            Globals.MetadataLayerShowText = checkBoxMetadataShowText != null && checkBoxMetadataShowText.Checked;
            RedrawEditorWindow();
            pictureBoxMap.Refresh();
        }

        private void CheckBoxMetaDataShowColorLinks_CheckedChanged(object sender, EventArgs e)
        {
            Globals.MetadataLayerShowColorLinks = checkBoxMetaDataShowColorLinks != null && checkBoxMetaDataShowColorLinks.Checked;
            if (checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked)
            {
                RedrawEditorWindow();
                pictureBoxMap.Refresh();
            }
        }

        private void CheckBoxMetaDataShowValueLinks_CheckedChanged(object sender, EventArgs e)
        {
            Globals.MetadataLayerShowValueLinks = checkBoxMetaDataShowValueLinks != null && checkBoxMetaDataShowValueLinks.Checked;
            if (checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked)
            {
                RedrawEditorWindow();
                pictureBoxMap.Refresh();
            }
        }

        private void ButtonMassChangeMetadata_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            var form = new MetadataMassChangeForm(
                myMap,
                () => isScreenLocked ? lockedScreen : currentScreen,
                () => { RedrawEditorWindow(); pictureBoxMap.Refresh(); });
            form.FormClosed += (s, ev) => UpdateMetadataLayerUI();
            form.Show(this);
        }

        private void MenuItemScreenDescription_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (ScreenDescriptionDialog dialog = new ScreenDescriptionDialog(myMap, currentScreen))
            {
                dialog.ShowDialog();
            }
        }

        private void ContextMenuStripScreen_Opening(object sender, CancelEventArgs e)
        {
            // Update enabled state of Apply Font Template menu item
            foreach (ToolStripItem item in contextMenuStripScreen.Items)
            {
                if (item.Text == "Apply Font Template...")
                {
                    bool enabled = myMap != null && myMap.MultiFontEnabled;
                    if (enabled)
                    {
                        // Check if current/locked screen references another screen
                        Point targetScreen = isScreenLocked ? lockedScreen : currentScreen;
                        int refScreenX, refScreenY;
                        bool isReferencing = myMap.GetFontMappingReference(targetScreen.X, targetScreen.Y, out refScreenX, out refScreenY);
                        if (isReferencing && (refScreenX != targetScreen.X || refScreenY != targetScreen.Y))
                        {
                            enabled = false; // Disable if referencing another screen
                        }
                    }
                    item.Enabled = enabled;
                    break;
                }
            }
        }

        private void MenuItemApplyFontTemplate_Click(object sender, EventArgs e)
        {
            if (myMap == null || !myMap.MultiFontEnabled) return;
            
            // Use locked screen if locked, otherwise use current screen
            Point targetScreen = isScreenLocked ? lockedScreen : currentScreen;
            
            // Check if target screen references another screen
            int refScreenX, refScreenY;
            bool isReferencing = myMap.GetFontMappingReference(targetScreen.X, targetScreen.Y, out refScreenX, out refScreenY);
            if (isReferencing && (refScreenX != targetScreen.X || refScreenY != targetScreen.Y))
            {
                // Screen references another - template cannot be applied
                MessageBox.Show("Cannot apply font template to a screen that references another screen's font mapping.", "Template Application", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (FontSelectorDialog dialog = new FontSelectorDialog(myMap, 0, targetScreen.X, targetScreen.Y, true))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AtariFontRenderer.ClearFontCache();
                    RedrawEditorWindow();
                }
            }
        }

        private void UpdateEditorWindowSizeInChars()
        {
            Globals.editorWindowSizeInChars = new Size(pictureBoxMap.Width / Globals.CharSize, pictureBoxMap.Height / Globals.CharSize);
        }

        private void ButtonShowFont_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            // Ensure default Screen font is loaded (e.g. after new map) so char picker has valid font
            if (!AtariFontRenderer.fonts.ContainsKey(Globals.FontType.Screen) || AtariFontRenderer.fonts[Globals.FontType.Screen].bitmap == null)
            {
                byte[] defaultFontData = DefaultFontResource.GetDefaultFontDataFromResources();
                AtariFontRenderer.SetFontData(defaultFontData, Globals.FontType.Screen);
            }
            myCharPicker.SetMainMap(myMap);
            myCharPicker.SetZoom();
            myCharPicker.Refresh();
            myCharPicker.RedrawFontWindow();
            myCharPicker.Show();
            myCharPicker.BringToFront();
        }

        private void FillFontColorList()
        {
            listViewColors.Items.Clear();
            listViewColors.LargeImageList = GetFontColorImageList(AtariFontRenderer.Color5);
            //listViewColors.Columns.Add("Color");
            //listViewColors.Columns.Add("Value");
            // listViewColors.Columns.Add("Address");
            listViewColors.SmallImageList = listViewColors.LargeImageList;
            for (int i = 0; i < 5; i++)
            {
                ListViewItem lvi = new ListViewItem
                {
                    ImageIndex = i,
                    Text = i == 4 ? "colbak" : $"colpf{i}"
                };
                lvi.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[i]));
                //lvi.SubItems.Add("$" + String.Format("{0:X4}", 0xd016 + i));
                listViewColors.Items.Add(lvi);
            }
            //add scanline alter colors only when possible
            if (AtariFontRenderer.Color5.Length > 5)
            {
                ListViewItem lviAlter = new ListViewItem
                {
                    ImageIndex = 5,
                    Text = "PF3 alter"
                };
                lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[5]));
                listViewColors.Items.Add(lviAlter);

                lviAlter = new ListViewItem
                {
                    ImageIndex = 6,
                    Text = "PF0 alter"
                };
                lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[6]));
                listViewColors.Items.Add(lviAlter);

                lviAlter = new ListViewItem
                {
                    ImageIndex = 7,
                    Text = "PF2 alter"
                };
                lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[7]));
                listViewColors.Items.Add(lviAlter);

                listViewColors.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewColors.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        private ImageList GetFontColorImageList(byte[] color5)
        {
            ImageList il = new ImageList
            {
                ColorDepth = ColorDepth.Depth24Bit,
                ImageSize = new Size(32, 16)
            };
            for (int i = 0; i < color5.Length; i++)
            {
                Bitmap bmp = new Bitmap(il.ImageSize.Width, il.ImageSize.Height);
                Graphics gr = Graphics.FromImage(bmp);
                gr.Clear(AtariPalette.GetColor(color5[i]));
                il.Images.Add(bmp);

            }
            return il;
        }

        private void ListView1_MouseLeave(object sender, EventArgs e)
        {
            for (int a = 0; a < listViewColors.Items.Count; a++)
            {
                listViewColors.Items[a].Selected = false;
            }
        }

        /// <summary>
        /// Restore map content under clipboard/metadata overlay and clear overlay state.
        /// Call when mouse leaves the map or when scroll starts so no trails or wrong data remain.
        /// </summary>
        private void HideClipboardFromMap()
        {
            if (myMap == null || pictureBoxMap?.Image == null)
                return;
            bool needRefresh = false;
            if (MetadataItemClipboard.HasItem && previousMetadataOverlayLocation.HasValue && metadataUnderImage != null)
            {
                AtariPictureTools.DrawMetadataUnder(previousMetadataOverlayLocation.Value, metadataUnderImage);
                previousMetadataOverlayLocation = null;
                needRefresh = true;
            }
            if (AtariClipboard.IsValid && AtariPictureTools.PreviousClipboardLocation.HasValue)
            {
                AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);
                AtariPictureTools.PreviousClipboardLocation = null;
                AtariPictureTools.PreviousClipboardGridCell = null;
                needRefresh = true;
            }
            if (needRefresh)
                pictureBoxMap.Refresh();
        }

        private void PictureBoxMap_MouseLeave(object sender, EventArgs e)
        {
            HideClipboardFromMap();
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewColors.SelectedItems.Count == 1)
            {
                int colorIndex = listViewColors.SelectedItems[0].Index;
                byte index = AtariFontRenderer.Color5[colorIndex];
                colorPickerForm.Owner = this;
                colorPickerForm.Pick(index);

                AtariFontRenderer.Color5[colorIndex] = colorPickerForm.PickedColorIndex;
                FillFontColorList();
                AtariFontRenderer.RedrawFontImage(Globals.FontType.Screen);
                AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
                if (myCharPicker != null)
                {
                    myCharPicker.Refresh();
                    myCharPicker.GetPictureBox().Refresh();
                }
                // Refresh tile picker if it's open
                if (tilePicker != null && tilePicker.Visible)
                {
                    tilePicker.Refresh();
                }
                //pictureBoxClipboard.Image = AtariPictureTools.windows[Globals.WindowType.Editor].destinationImage;

                //AtariFontRenderer.RedrawFont();
                //pictureBoxMap.Invalidate();
                //myCharPicker.GetRenderer().RedrawFont();
                //myCharPicker.RedrawFontWindow();
                //RedrawEditorWindow();
            }
        }

        private void PictureBoxMap_MouseMove(object sender, MouseEventArgs e)
        {
            int xx = myMap.OffsetX + e.X / Globals.CharSize;
            int yy = myMap.OffsetY + e.Y / Globals.CharSize;
            
            // For tilemaps, ScreenSize is in tiles, so convert to character units
            int screenCharWidth = myMap.ScreenSize.Width;
            int screenCharHeight = myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            
            int scrx = xx / screenCharWidth;
            int scry = yy / screenCharHeight;
            int posx = xx % screenCharWidth;
            int posy = yy % screenCharHeight;
            
            // Clamp screen coordinates to valid range
            if (scrx < 0) scrx = 0;
            if (scry < 0) scry = 0;
            if (scrx >= myMap.MapSize.Width) scrx = myMap.MapSize.Width - 1;
            if (scry >= myMap.MapSize.Height) scry = myMap.MapSize.Height - 1;
            
            // If screen is locked, only update if mouse is within the locked screen
            if (isScreenLocked)
            {
                // Check if mouse is within the locked screen boundaries
                int lockedScreenStartX = lockedScreen.X * screenCharWidth;
                int lockedScreenEndX = (lockedScreen.X + 1) * screenCharWidth;
                int lockedScreenStartY = lockedScreen.Y * screenCharHeight;
                int lockedScreenEndY = (lockedScreen.Y + 1) * screenCharHeight;
                
                if (xx >= lockedScreenStartX && xx < lockedScreenEndX && 
                    yy >= lockedScreenStartY && yy < lockedScreenEndY)
                {
                    // Mouse is within locked screen, update normally
                    currentScreen.X = scrx;
                    currentScreen.Y = scry;
                    UpdateFontMappingReferenceUI();
                }
                else
                {
                    // Mouse is outside locked screen, don't update currentScreen or DLI form
                    // Keep currentScreen as locked screen
                    return;
                }
            }
            else
            {
                // Screen not locked, update normally
                // If DLI form is visible, keep the current screen as the one the DLI form is showing
                // Don't update currentScreen if mouse is in the DLI form area (to the right of the screen)
                if (dliForm != null && dliForm.Visible)
                {
                    // Get the screen that the DLI form is showing
                    int dliScreenNumber = dliForm.screenNumber;
                    int dliScreenX = dliScreenNumber % myMap.MapSize.Width;
                    int dliScreenY = dliScreenNumber / myMap.MapSize.Width;
                    
                    // Check if mouse is in the DLI form area (to the right of the screen)
                    int dliFormStartX = (dliScreenX + 1) * screenCharWidth - 1;
                    int dliFormEndX = dliFormStartX + 6; // DLI form is 6 characters wide (5 colors + 1 font)
                    int dliFormStartY = dliScreenY * screenCharHeight;
                    int dliFormEndY = (dliScreenY + 1) * screenCharHeight;
                    
                    // If mouse is in DLI form area, keep current screen as the DLI form's screen
                    if (xx >= dliFormStartX && xx < dliFormEndX && yy >= dliFormStartY && yy < dliFormEndY)
                    {
                        currentScreen.X = dliScreenX;
                        currentScreen.Y = dliScreenY;
                    }
                    else
                    {
                        // Mouse is not in DLI form area, update normally
                        currentScreen.X = scrx;
                        currentScreen.Y = scry;
                    }
                }
                else
                {
                    // DLI form not visible, update normally
                    currentScreen.X = scrx;
                    currentScreen.Y = scry;
                }
                UpdateFontMappingReferenceUI();
            }
            
            // Only redraw editor window if currentScreen actually changed
            if (currentScreen.X != previousScreen.X || currentScreen.Y != previousScreen.Y)
            {
                RedrawEditorWindow();
                previousScreen = currentScreen;
            }

            if (e.Button == MouseButtons.Right)     //SCROLL
            {
                HideClipboardFromMap();
                if (AtariPictureTools.Scroll(e.Location, Globals.WindowType.Editor))
                {
                    AtariPictureTools.PreviousMouseLocation = e.Location;
                    AtariPictureTools.PreviousOffset = myMap.Offset;
                    if (tabControl1.SelectedTab == tabPage2 && checkBoxShowScreenSelection.Checked)
                    {
                        ShowScreenSelection(false);
                    }
                    else 
                        pictureBoxMap.Refresh();
                }

                if (checkBoxEditDli.Checked)
                {
                    if (isScreenLocked)
                    {
                        // Keep DLI form showing the locked screen
                        UpdateAndShowDliForm(lockedScreen.X, lockedScreen.Y, true);
                    }
                    else
                    {
                        UpdateAndShowDliForm(scrx, scry, true);
                    }
                }
            }


            if (e.Button == MouseButtons.Left)      //SELECT or CONTINUOUS PASTE
            {
                if (mouseStatus == "SELECTION")
                {
                    AtariPictureTools.SelectionChange(e.Location, Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
                else if (AtariClipboard.IsValid && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    // Continuous paste mode: CTRL + Left mouse button
                    isContinuousPasteMode = true;
                    PerformPasteAtLocation(e.Location);
                }
            }

            if (e.Button == MouseButtons.None)  //nothing is pressed
            {
                // Check for continuous paste mode on mouse move (CTRL still held)
                if (isContinuousPasteMode && (Control.ModifierKeys & Keys.Control) == Keys.Control && AtariClipboard.IsValid)
                {
                    // Continue pasting as mouse moves in continuous paste mode
                    PerformPasteAtLocation(e.Location);
                }
                else if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                {
                    // Exit continuous paste mode when CTRL is released
                    isContinuousPasteMode = false;
                    lastContinuousPasteCell = null;
                }

              
              
                if (AtariClipboard.IsValid)  //copy mode (shows alpha blended clipBoard)
                {
                    // Calculate current grid cell based on map type
                    int alignSizeX = Globals.CharSize;
                    int alignSizeY = Globals.CharSize;
                    if (myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                        alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
                    }
                    
                    int currentGridX = e.Location.X / alignSizeX;
                    int currentGridY = e.Location.Y / alignSizeY;
                    Point currentGridCell = new Point(currentGridX, currentGridY);
                    
                    // Only redraw if mouse moved to a different grid cell
                    if (!AtariPictureTools.PreviousClipboardGridCell.HasValue || 
                        AtariPictureTools.PreviousClipboardGridCell.Value != currentGridCell)
                    {
                        // Restore previous clipboard position first (if it exists)
                        if (AtariPictureTools.PreviousClipboardLocation.HasValue)
                        {
                            AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);
                        }
                        
                        // Draw clipboard at new position
                        AtariPictureTools.DrawClipBoard(e.Location, (Bitmap)pictureBoxMap.Image);
                        pictureBoxUnderClipBoard.Image = AtariClipboard.UnderClipBoardImage;
                        pictureBoxUnderClipBoard.Refresh();
                        
                        // Update previous location and grid cell
                        AtariPictureTools.PreviousClipboardLocation = e.Location;
                        AtariPictureTools.PreviousClipboardGridCell = currentGridCell;
                        
                        // Refresh to show changes
                        pictureBoxMap.Refresh();
                    }
                }
                else if (MetadataItemClipboard.HasItem)
                {
                    // Show metadata paste preview under cursor (same as pictureBoxClipboard: tile-sized with text when tilemap)
                    int zoom = Math.Max(1, Globals.Zoom);
                    int alignSizeX = Globals.CharSize;
                    int alignSizeY = Globals.CharSize;
                    int cellPxW = 8 * zoom;
                    int cellPxH = 8 * zoom;
                    bool tileSized = false;
                    if (myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                        alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
                        cellPxW = myMap.TilemapInfo.TileWidth * 8 * zoom;
                        cellPxH = myMap.TilemapInfo.TileHeight * 8 * zoom;
                        tileSized = true;
                    }
                    int alignedX = (e.Location.X / alignSizeX) * alignSizeX;
                    int alignedY = (e.Location.Y / alignSizeY) * alignSizeY;
                    bool cellChanged = !previousMetadataOverlayLocation.HasValue ||
                        previousMetadataOverlayLocation.Value.X != alignedX || previousMetadataOverlayLocation.Value.Y != alignedY;
                    if (cellChanged)
                    {
                        if (previousMetadataOverlayLocation.HasValue && metadataUnderImage != null)
                        {
                            AtariPictureTools.DrawMetadataUnder(previousMetadataOverlayLocation.Value, metadataUnderImage);
                        }
                        metadataPreviewImage?.Dispose();
                        metadataPreviewImage = tileSized
                            ? MetadataLayerRenderer.CreateMetadataItemPreviewBitmap(MetadataItemClipboard.CopiedItem, 8 * zoom, cellOnly: false, cellWidthPixels: cellPxW, cellHeightPixels: cellPxH)
                            : MetadataLayerRenderer.CreateMetadataItemPreviewBitmap(MetadataItemClipboard.CopiedItem, 8 * zoom, cellOnly: true);
                        if (metadataPreviewImage != null && (Bitmap)pictureBoxMap.Image != null)
                        {
                            int w = metadataPreviewImage.Width;
                            int h = metadataPreviewImage.Height;
                            if (metadataUnderImage == null || metadataUnderImage.Width != w || metadataUnderImage.Height != h)
                            {
                                metadataUnderImage?.Dispose();
                                metadataUnderImage = new Bitmap(w, h);
                            }
                            AtariPictureTools.DrawMetadataOverlay(e.Location, (Bitmap)pictureBoxMap.Image, metadataUnderImage, metadataPreviewImage);
                            previousMetadataOverlayLocation = new Point(alignedX, alignedY);
                        }
                        pictureBoxMap.Refresh();
                    }
                }
            }
            

            // For tilemaps, use CharStride and calculate max height in characters
            int maxCharStride = myMap.Stride;
            int maxCharHeight = myMap.MapSize.Height * myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                maxCharStride = myMap.CharStride;
                maxCharHeight = myMap.MapSize.Height * myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            
            if (xx < maxCharStride && yy < maxCharHeight)
            {
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    int tileWidth = myMap.TilemapInfo.TileWidth;
                    int tileHeight = myMap.TilemapInfo.TileHeight;
                    int posTx = posx / tileWidth;
                    int posTy = posy / tileHeight;
                    toolStripStatusLabel1.Text = $"Scr {currentScreen.X}:{currentScreen.Y} Tile {posTx}:{posTy} (char {posx}:{posy}) Glo {xx}:{yy}";
                }
                else
                {
                    toolStripStatusLabel1.Text = $"Scr {currentScreen.X}:{currentScreen.Y} Pos {posx}:{posy} (${(posx + posy * screenCharWidth).ToString("X2")}) Glo {xx}:{yy}";
                }
                // For tilemaps, use CharData; for normal maps, use Data
                byte charVal;
                if (myMap.IsTilemap && myMap.CharData != null)
                {
                    charVal = myMap.CharData[xx + yy * myMap.CharStride];
                }
                else
                {
                    charVal = myMap.Data[xx + yy * myMap.Stride];
                }
                
                // For tilemaps, also show tile index
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    int tileWidth = myMap.TilemapInfo.TileWidth;
                    int tileHeight = myMap.TilemapInfo.TileHeight;
                    int tileX = xx / tileWidth;
                    int tileY = yy / tileHeight;
                    int tileIndex = tileY * myMap.Stride + tileX;
                    byte tileIdx = 0;
                    if (tileIndex >= 0 && tileIndex < myMap.Data.Length)
                    {
                        tileIdx = myMap.Data[tileIndex];
                    }
                    // Compact format: Char $XX (N) | Tile N @ (X,Y)
                    toolStripStatusLabel2.Text = $"Tile ${tileIdx:X2} ({tileIdx}) @ ({tileX},{tileY}) | Char ${charVal:X2} ({charVal})";
                }
                else
                {
                    toolStripStatusLabel2.Text = $"Char: ${charVal:X2} ({charVal})";
                }
                
                //calculate the occurence
                (int idx, int amnt) = myMap.CharOccurence(new Point(currentScreen.X,currentScreen.Y), posx, posy, charVal);
                
                if (myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    int tileWidth = myMap.TilemapInfo.TileWidth;
                    int tileHeight = myMap.TilemapInfo.TileHeight;
                    int posTx = posx / tileWidth;
                    int posTy = posy / tileHeight;
                    int tileOffset = myMap.Stride * myMap.ScreenSize.Height * currentScreen.Y + myMap.ScreenSize.Width * currentScreen.X + posTy * myMap.Stride + posTx;
                    byte tileIdx = (tileOffset >= 0 && tileOffset < myMap.Data.Length) ? myMap.Data[tileOffset] : (byte)0;
                    (int tidx, int tamnt) = myMap.TileOccurence(new Point(currentScreen.X, currentScreen.Y), posTx, posTy, tileIdx);
                    toolStripStatusLabel3.Text = $"Tile {tidx} of {tamnt}";
                }
                else
                {
                    toolStripStatusLabel3.Text = $"Char {idx} of {amnt}";
                }
            }
        }

        private void UpdateAndShowDliForm(int scrx,int scry, bool justUpdatePosition = false)
        {
            // If screen is locked, only update DLI form if the requested screen is the locked screen
            if (isScreenLocked)
            {
                if (scrx != lockedScreen.X || scry != lockedScreen.Y)
                {
                    // Don't update DLI form if mouse is outside locked screen
                    return;
                }
            }
            
            //check for out of bounds screens
            if (scrx >= myMap.MapSize.Width || scry >= myMap.MapSize.Height)
            {
                dliForm.Hide();
                return;
            }
            
            Point dliPoint = DliFormOrigin(scrx, scry);
            if (dliPoint.X != -1)
            {
                Rectangle r = this.RectangleToScreen(this.ClientRectangle);
                dliForm.Left = r.Left + dliPoint.X * Globals.CharSize;
                dliForm.Top = r.Top + dliPoint.Y * Globals.CharSize;
                dliForm.Owner = this;
                if (!justUpdatePosition)
                {
                    myMap.CopyDliColorsFullScreen(scrx + scry * myMap.MapSize.Width, dliForm.DliMap, 0);  //copy screen colors to DLI color editor
                    dliForm.RenderData();
                }
                else
                    dliForm.RenderData(true);
                dliForm.Show(scrx + scry * myMap.MapSize.Width);

            }
            else
            {
                dliForm.Hide();
            }
        }

        private Point DliFormOrigin(int scrx, int scry)
        {
            // For tilemaps, ScreenSize is in tiles, so convert to character units
            int screenCharWidth = myMap.ScreenSize.Width;
            int screenCharHeight = myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            
            int xmin = (scrx + 1) * screenCharWidth - 1;
            int xmax = (scrx + 1) * screenCharWidth + 4;
            int ymin = scry * screenCharHeight;
            int ymax = (scry + 1) * screenCharHeight;

            Rectangle visibleArea = new Rectangle(myMap.OffsetX, myMap.OffsetY, Globals.editorWindowSizeInChars.Width, Globals.editorWindowSizeInChars.Height);
            if (visibleArea.Contains(new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin)))
                return new Point(xmin + 1 - myMap.OffsetX, ymin - myMap.OffsetY);
            else
                return new Point(-1, -1);
        }

        private void PictureBoxMap_MouseDown(object sender, MouseEventArgs e)
        {
            AtariPictureTools.PreviousMouseLocation = e.Location;
            if (e.Button == MouseButtons.Left)
            {

                if (AtariClipboard.IsValid)
                {
                    AtariClipboard.SetDataSource(myMap);     //to copy always to map (not to char selector)
                    int charX = e.X / Globals.CharSize;
                    int charY = e.Y / Globals.CharSize;
                    
                    // If tilemap is enabled, snap to tile grid
                    if (myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        int tileWidth = myMap.TilemapInfo.TileWidth;
                        int tileHeight = myMap.TilemapInfo.TileHeight;
                        charX = (charX / tileWidth) * tileWidth;
                        charY = (charY / tileHeight) * tileHeight;
                        
                        // For tilemaps, check if clipboard contains tile indexes
                        if (AtariClipboard.IsTileIndexes)
                        {
                            // Convert character coordinates to tile coordinates
                            // charX and charY are relative to the visible area, need to add offset
                            int absoluteCharX = myMap.OffsetX + charX;
                            int absoluteCharY = myMap.OffsetY + charY;
                            int tileX = absoluteCharX / tileWidth;
                            int tileY = absoluteCharY / tileHeight;
                            
                            // Calculate character offset for Paste
                            // For tilemaps, use CharStride (character stride)
                            int charStride = myMap.CharStride;
                            int charOffset = absoluteCharX + charStride * absoluteCharY;
                            AtariClipboard.Paste(myMap.Offset + charOffset);
                        }
                        else
                        {
                            // Paste characters (normal mode, but snapped to tile grid)
                            // For tilemaps, we need to use character stride, not tile stride
                            int charStride = myMap.CharStride;
                            if (myMap.OffsetX + charX + AtariClipboard.ClipboardWidth <= charStride)
                            {
                                // Need to add the offset from the map
                                int absoluteCharX = myMap.OffsetX + charX;
                                int absoluteCharY = myMap.OffsetY + charY;
                                int absoluteCharOffset = absoluteCharX + charStride * absoluteCharY;
                                AtariClipboard.Paste(myMap.Offset + absoluteCharOffset);
                            }
                        }
                    }
                    else
                    {
                        // Normal map - paste characters
                        if (myMap.OffsetX + charX + AtariClipboard.ClipboardWidth <= myMap.Stride)
                        {
                            int absoluteCharX = myMap.OffsetX + charX;
                            int absoluteCharY = myMap.OffsetY + charY;
                            int addoffset = absoluteCharX + myMap.Stride * absoluteCharY;
                            AtariClipboard.Paste(myMap.Offset + addoffset);
                        }
                    }
                    
                    // For tilemaps, ensure CharData is up to date after pasting
                    if (myMap.IsTilemap && AtariClipboard.IsTileIndexes)
                    {
                        // CharData is already updated by ExpandTileToCharData in Paste method
                        // But we may need to clear font cache if fonts changed
                        AtariFontRenderer.ClearFontCache();
                    }
                    
                    // Redraw the editor window to show the pasted data immediately
                    // Force a complete redraw by calling Redraw with all parameters
                    // This ensures RenderMapData reads the freshly pasted data
                    AtariPictureTools.Redraw(Globals.WindowType.Editor, true, true, true, currentScreen, isScreenLocked, lockedScreen);
                    
                    // After redrawing, update the UnderClipBoardImage with the NEW content under the clipboard position
                    // This is critical - otherwise the old UnderClipBoardImage will overwrite the pasted data on mouse move
                    if (AtariPictureTools.PreviousClipboardLocation.HasValue)
                    {
                        // Update UnderClipBoardImage with the current content at the clipboard position
                        AtariPictureTools.DrawClipBoard(AtariPictureTools.PreviousClipboardLocation.Value, (Bitmap)pictureBoxMap.Image);
                        pictureBoxUnderClipBoard.Image = AtariClipboard.UnderClipBoardImage;
                        pictureBoxUnderClipBoard.Refresh();
                    }
                    
                    pictureBoxMap.Refresh();
                }
                else if (Globals.MetadataLayerVisible)
                {
                    // Metadata layer: add or edit metadata at this position (tile position for tilemap, char position otherwise)
                    int xx = myMap.OffsetX + e.X / Globals.CharSize;
                    int yy = myMap.OffsetY + e.Y / Globals.CharSize;
                    int screenCharWidth = myMap.ScreenSize.Width;
                    int screenCharHeight = myMap.ScreenSize.Height;
                    if (myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                        screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                    }
                    int scrx = xx / screenCharWidth;
                    int scry = yy / screenCharHeight;
                    if (scrx < 0) scrx = 0;
                    if (scry < 0) scry = 0;
                    if (scrx >= myMap.MapSize.Width) scrx = myMap.MapSize.Width - 1;
                    if (scry >= myMap.MapSize.Height) scry = myMap.MapSize.Height - 1;
                    int charX = xx - scrx * screenCharWidth;
                    int charY = yy - scry * screenCharHeight;
                    int cellX, cellY;
                    int cellWidth, cellHeight;
                    if (myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        int tw = myMap.TilemapInfo.TileWidth;
                        int th = myMap.TilemapInfo.TileHeight;
                        cellX = charX / tw;
                        cellY = charY / th;
                        cellWidth = myMap.ScreenSize.Width;
                        cellHeight = myMap.ScreenSize.Height;
                    }
                    else
                    {
                        cellX = charX;
                        cellY = charY;
                        cellWidth = screenCharWidth;
                        cellHeight = screenCharHeight;
                    }
                    if (cellX < 0 || cellX >= cellWidth || cellY < 0 || cellY >= cellHeight)
                        return;
                    var key = $"{scrx},{scry}";
                    if (myMap.ScreenMetadata == null)
                        myMap.ScreenMetadata = new Dictionary<string, ScreenMetadata>();
                    if (!myMap.ScreenMetadata.ContainsKey(key))
                        myMap.ScreenMetadata[key] = new ScreenMetadata();
                    var meta = myMap.ScreenMetadata[key];
                    var existing = meta.ParsedItems?.Find(i => i.X == cellX && i.Y == cellY);
                    bool ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    bool alt = (Control.ModifierKeys & Keys.Alt) == Keys.Alt;
                    if (ctrl && existing != null)
                    {
                        MetadataItemClipboard.Copy(existing);
                        UpdateMetadataClipboardPreview();
                        RedrawEditorWindow();
                        pictureBoxMap.Refresh();
                        return;
                    }
                    if (alt && existing != null)
                    {
                        MetadataItemClipboard.BeginMove(existing, scrx, scry, cellX, cellY);
                        UpdateMetadataClipboardPreview();
                        RedrawEditorWindow();
                        pictureBoxMap.Refresh();
                        return;
                    }
                    if (!ctrl && MetadataItemClipboard.HasItem)
                    {
                        var copied = MetadataItemClipboard.CopiedItem;
                        if (MetadataItemClipboard.IsMovePending)
                        {
                            if (scrx == MetadataItemClipboard.MoveSourceScreenX && scry == MetadataItemClipboard.MoveSourceScreenY
                                && cellX == MetadataItemClipboard.MoveSourceCellX && cellY == MetadataItemClipboard.MoveSourceCellY)
                            {
                                MetadataItemClipboard.Clear();
                                previousMetadataOverlayLocation = null;
                                UpdateMetadataClipboardPreview();
                                UpdateMetadataLayerUI();
                                RedrawEditorWindow();
                                pictureBoxMap.Refresh();
                                return;
                            }
                            string srcKey = $"{MetadataItemClipboard.MoveSourceScreenX},{MetadataItemClipboard.MoveSourceScreenY}";
                            if (myMap.ScreenMetadata.TryGetValue(srcKey, out var srcMeta) && srcMeta?.ParsedItems != null)
                            {
                                var srcItem = srcMeta.ParsedItems.Find(i => i.X == MetadataItemClipboard.MoveSourceCellX && i.Y == MetadataItemClipboard.MoveSourceCellY);
                                if (srcItem != null)
                                    srcMeta.ParsedItems.Remove(srcItem);
                            }
                        }
                        meta.ParsedItems.RemoveAll(i => i.X == cellX && i.Y == cellY);
                        meta.ParsedItems.Add(new MetadataLayerItem { X = cellX, Y = cellY, Text = copied.Text ?? "", Type = copied.Type, Value = copied.Value, Color = copied.Color });
                        MetadataItemClipboard.Clear();
                        previousMetadataOverlayLocation = null;
                        UpdateMetadataLayerUI();
                        RedrawEditorWindow();
                        pictureBoxMap.Refresh();
                        return;
                    }
                    if (existing != null)
                    {
                        using (var edit = new MetadataItemEditDialog(myMap, existing, "Edit metadata item", myMap.IsTilemap))
                        {
                            if (edit.ShowDialog() == DialogResult.OK && edit.RemoveRequested)
                                meta.ParsedItems.Remove(existing);
                        }
                    }
                    else
                    {
                        var item = new MetadataLayerItem { X = cellX, Y = cellY, Text = "", Type = 0, Value = 0, Color = 0 };
                        using (var edit = new MetadataItemEditDialog(myMap, item, "Add metadata item", myMap.IsTilemap))
                        {
                            if (edit.ShowDialog() == DialogResult.OK && !edit.RemoveRequested)
                                meta.ParsedItems.Add(item);
                        }
                    }
                    UpdateMetadataLayerUI();
                    RedrawEditorWindow();
                    pictureBoxMap.Refresh();
                }
                else
                {
                    if (mouseStatus == "")
                    {
                        AtariPictureTools.SelectionStart(e.Location, Globals.WindowType.Editor);
                        mouseStatus = "SELECTION";

                    }
                }

            }

            if (e.Button == MouseButtons.Right)
            {
                if (mouseStatus == "SELECTION")
                    mouseStatus = "";
                AtariPictureTools.PreviousOffset = myMap.Offset;

                // Check for ALT+Right-click to toggle lock mode
                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    // Toggle lock mode
                    int xx = myMap.OffsetX + e.X / Globals.CharSize;
                    int yy = myMap.OffsetY + e.Y / Globals.CharSize;
                    
                    // For tilemaps, ScreenSize is in tiles, so convert to character units
                    int screenCharWidth = myMap.ScreenSize.Width;
                    int screenCharHeight = myMap.ScreenSize.Height;
                    if (myMap.IsTilemap && myMap.TilemapInfo != null)
                    {
                        screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                        screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                    }
                    
                    int scrx = xx / screenCharWidth;
                    int scry = yy / screenCharHeight;
                    
                    // Clamp screen coordinates to valid range (fixes screen 0,0 issue)
                    if (scrx < 0) scrx = 0;
                    if (scry < 0) scry = 0;
                    if (scrx >= myMap.MapSize.Width) scrx = myMap.MapSize.Width - 1;
                    if (scry >= myMap.MapSize.Height) scry = myMap.MapSize.Height - 1;
                    
                    if (isScreenLocked && lockedScreen.X == scrx && lockedScreen.Y == scry)
                    {
                        // Unlock if clicking on the same locked screen
                        isScreenLocked = false;
                    }
                    else
                    {
                        // Lock the current screen
                        isScreenLocked = true;
                        lockedScreen.X = scrx;
                        lockedScreen.Y = scry;
                        currentScreen.X = scrx;
                        currentScreen.Y = scry;
                        previousScreen = currentScreen;  // Update previous screen to prevent unnecessary redraw
                        UpdateFontMappingReferenceUI();
                    }
                    RedrawEditorWindow();
                }
                else
                {
                    // Right-click without ALT - unlock if locked
                    if (isScreenLocked)
                    {
                        isScreenLocked = false;
                        RedrawEditorWindow();
                    }
                    
                    if (checkBoxEditDli.Checked)
                    {
                        int xx = myMap.OffsetX + e.X / Globals.CharSize;
                        int yy = myMap.OffsetY + e.Y / Globals.CharSize;
                        
                        // For tilemaps, ScreenSize is in tiles, so convert to character units
                        int screenCharWidth = myMap.ScreenSize.Width;
                        int screenCharHeight = myMap.ScreenSize.Height;
                        if (myMap.IsTilemap && myMap.TilemapInfo != null)
                        {
                            screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                            screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                        }
                        
                        int scrx = xx / screenCharWidth;
                        int scry = yy / screenCharHeight;
                        UpdateAndShowDliForm(scrx, scry);
                    }
                    else
                        dliForm.Hide();
                }
            }
        }



        private void PictureBoxMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mouseStatus == "SELECTION")
                {
                    if (AtariPictureTools.SelectionEnd(Globals.WindowType.Editor))
                    {
                        AtariClipboard.IsValid = true;
                        pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                        pictureBoxMap.Refresh();
                    }
                    mouseStatus = "";   //reset mouse status no matter if selection end is valid or not
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                contextMenuStripScreen.Show(pictureBoxMap, e.Location);
            }

        }

        private void PictureBoxMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (MetadataItemClipboard.HasItem)
                {
                    if (previousMetadataOverlayLocation.HasValue && metadataUnderImage != null)
                    {
                        AtariPictureTools.DrawMetadataUnder(previousMetadataOverlayLocation.Value, metadataUnderImage);
                        pictureBoxMap.Refresh();
                    }
                    previousMetadataOverlayLocation = null;
                    metadataUnderImage?.Dispose();
                    metadataUnderImage = null;
                    metadataPreviewImage?.Dispose();
                    metadataPreviewImage = null;
                    MetadataItemClipboard.Clear();
                    UpdateMetadataClipboardPreview();
                }
                // Restore previous clipboard position before clearing
                if (AtariPictureTools.PreviousClipboardLocation.HasValue)
                {
                    AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);
                }
                AtariClipboard.IsValid = false;
                AtariPictureTools.PreviousClipboardLocation = null;
                AtariPictureTools.PreviousClipboardGridCell = null;
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }
        
        /// <summary>
        /// Helper method to perform paste at a given location (used for continuous paste mode)
        /// </summary>
        private void PerformPasteAtLocation(Point location)
        {
            if (!AtariClipboard.IsValid)
                return;
                
            // Calculate current grid cell based on map type
            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            
            int currentGridX = location.X / alignSizeX;
            int currentGridY = location.Y / alignSizeY;
            Point currentGridCell = new Point(currentGridX, currentGridY);
            
            // Only paste if we moved to a different grid cell
            if (lastContinuousPasteCell.HasValue && lastContinuousPasteCell.Value == currentGridCell)
                return;
                
            lastContinuousPasteCell = currentGridCell;
            
            // Perform the paste (reuse the logic from MouseDown)
            AtariClipboard.SetDataSource(myMap);
            int charX = location.X / Globals.CharSize;
            int charY = location.Y / Globals.CharSize;
            
            // If tilemap is enabled, snap to tile grid
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                int tileWidth = myMap.TilemapInfo.TileWidth;
                int tileHeight = myMap.TilemapInfo.TileHeight;
                charX = (charX / tileWidth) * tileWidth;
                charY = (charY / tileHeight) * tileHeight;
                
                if (AtariClipboard.IsTileIndexes)
                {
                    int absoluteCharX = myMap.OffsetX + charX;
                    int absoluteCharY = myMap.OffsetY + charY;
                    int charStride = myMap.CharStride;
                    int charOffset = absoluteCharX + charStride * absoluteCharY;
                    AtariClipboard.Paste(myMap.Offset + charOffset);
                }
                else
                {
                    int charStride = myMap.CharStride;
                    if (myMap.OffsetX + charX + AtariClipboard.ClipboardWidth <= charStride)
                    {
                        int absoluteCharX = myMap.OffsetX + charX;
                        int absoluteCharY = myMap.OffsetY + charY;
                        int absoluteCharOffset = absoluteCharX + charStride * absoluteCharY;
                        AtariClipboard.Paste(myMap.Offset + absoluteCharOffset);
                    }
                }
            }
            else
            {
                if (myMap.OffsetX + charX + AtariClipboard.ClipboardWidth <= myMap.Stride)
                {
                    int absoluteCharX = myMap.OffsetX + charX;
                    int absoluteCharY = myMap.OffsetY + charY;
                    int addoffset = absoluteCharX + myMap.Stride * absoluteCharY;
                    AtariClipboard.Paste(myMap.Offset + addoffset);
                }
            }
            
            // Redraw after paste
            if (myMap.IsTilemap && AtariClipboard.IsTileIndexes)
            {
                AtariFontRenderer.ClearFontCache();
            }
            AtariPictureTools.Redraw(Globals.WindowType.Editor, true, true, true, currentScreen, isScreenLocked, lockedScreen);
            pictureBoxMap.Refresh();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            //this.Text = "debug: " + mouseStatus + " " + drawNo.ToString();

        }

        private void PictureBoxMap_Resize(object sender, EventArgs e)
        {
            /*pictureBox1.Width = splitContainer1.Panel1.Width;

            pictureBox1.Height = splitContainer1.Panel1.Height - toolStrip1.Height;
            */
            /*if (dataImage != null)
                dataImage.Dispose();
            dataImage = new Bitmap(pictureBox1.Width / zoomMultiplier[zoom], pictureBox1.Height / zoomMultiplier[zoom], System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();
            gr.Dispose();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            gr = Graphics.FromImage(pictureBox1.Image);
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            myRenderer.RenderData(myMap, myRenderer.offset, dataImage);
            pictureBox1.Invalidate();
            */
        }

        private void PictureBoxMap_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateEditorWindowSizeInChars();

            if (myMap == null)
                return;
            int ignoredWidth = pictureBoxMap.Width % (8 * Globals.Zoom);
            int ignoredHeight = pictureBoxMap.Height % (8 * Globals.Zoom);
            try
            {
                pictureBoxMap.Image = new Bitmap(pictureBoxMap.Width - ignoredWidth, pictureBoxMap.Height - ignoredHeight);
            }
            catch (ArgumentException)   //fix crash when window shrinked to 0 width or height
            { }
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    SaveMap(saveFileDialog1.FileName); //test.dat
                    break;
            }
            //saveFileDialog1.Filter = "AtariMap (*.amp)|*.amp";
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //    Save(saveFileDialog1.FileName);

        }

        private void SaveMap(string filename)
        {
            AtariJson.Atrmap atrmap = new AtariJson.Atrmap
            {
                MapData = myMap.Data.Select(i => (int)i).ToArray(),
                MapSize = myMap.MapSize,
                MapScreenSize = myMap.ScreenSize,
                FontData = AtariFontRenderer.fonts[Globals.FontType.Screen].data.Select(i => (int)i).ToArray(),
                Color5 = AtariFontRenderer.Color5.Select(i => (int)i).ToArray(),
                DliData = myMap.ColorData.Select(i => (int)i).ToArray(),
                // v2.0 fields
                FontDataArray = myMap.FontDataArray?.Select(font => font?.Select(i => (int)i).ToArray()).ToArray(),
                FontFileNames = myMap.FontFileNames,
                FontLineMappingPerScreen = myMap.FontLineMappingPerScreen?.Select(i => (int)i).ToArray(),
                FontLineMappingReferences = myMap.FontLineMappingReferences,
                FontTemplateLocked = myMap.FontTemplateLocked,
                FontTemplatePattern = myMap.FontTemplatePattern,
                MultiFontEnabled = myMap.MultiFontEnabled,
                MapDescription = myMap.MapDescription,
                ScreenDescriptions = myMap.ScreenDescriptions,
                ScreenMetadataDict = myMap.ScreenMetadata,
                MetadataTypeLabels = myMap.MetadataTypeLabels,
                SubmapPath = myMap.SubmapPath,
                IsTilemap = myMap.IsTilemap,
                TilemapInfo = myMap.TilemapInfo,
                BitmapTileset = myMap.BitmapTileset,
                ElementLibrary = myMap.ElementLibrary,
                ScreenLinks = myMap.ScreenLinks
            };
            AtariJson.SaveAtrMap(atrmap, filename);
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    LoadMap(openFileDialog1.FileName);
                    
                    AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
                    AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
                  
                    checkBoxShowDli.Checked = true;
                    this.FillFontColorList();
                    
                    // Update numeric up/down controls for new map size
                    numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                    numericUpDownScreenFromX.Maximum = myMap.MapSize.Width - 1;
                    numericUpDownScreenToX.Maximum = myMap.MapSize.Width - 1;
                    numericUpDownScreenFromY.Maximum = myMap.MapSize.Height - 1;
                    numericUpDownScreenToY.Maximum = myMap.MapSize.Height - 1;
                    
                    // Update multifont checkbox
                    if (checkBoxMultiFont != null)
                    {
                        checkBoxMultiFont.Checked = myMap.MultiFontEnabled;
                        checkBoxMultiFont.Enabled = true;
                    }
                    
                    currentScreen = new Point(0, 0);
                    previousScreen = new Point(-1, -1);  // Reset to force redraw
                    isScreenLocked = false;
                    UpdateFontMappingReferenceUI();
                    UpdateMultiFontUI();
                    UpdateMetadataLayerUI();
                    UpdateClipboardInverseButtonState();  // Update button state when map type changes
                    RedrawEditorWindow();
                    //myCharPicker.GetRenderer().FontData = AtariFontRenderer.FontData;
                    //myCharPicker.GetRenderer().Color5 = AtariFontRenderer.Color5;
                    if (myCharPicker != null)
                        myCharPicker.RedrawFontWindow();
                    dliForm.Dispose();
                    dliForm = new DliForm(myMap, pictureBoxMap);
                    dliForm.RenderData();
                    dliForm.ZoomResize();
                    numericUpDownScreenFromX.Maximum = myMap.MapSize.Width;
                    numericUpDownScreenFromY.Maximum = myMap.MapSize.Height;
                    numericUpDownScreenToX.Maximum = myMap.MapSize.Width;
                    numericUpDownScreenToY.Maximum = myMap.MapSize.Height;
                    numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                    break;
            }
        }

        private void ClosePopupWindows()
        {
            // Hide popups only; do not dispose (forms stay alive until app exit)
            myCharPicker?.Hide();
            tilePicker?.Hide();
            elementLibraryDialog?.Hide();
        }

        private void ClearClipboard()
        {
            HideClipboardFromMap();
            AtariClipboard.IsValid = false;
            AtariPictureTools.PreviousClipboardLocation = null;
            AtariPictureTools.PreviousClipboardGridCell = null;
            if (pictureBoxClipboard != null)
            {
                pictureBoxClipboard.Image = null;
                pictureBoxClipboard.Refresh();
            }
        }

        private void LoadMap(string fileName)
        {
            ClosePopupWindows();
            ClearClipboard();
            AtariJson.ParseAtrmap(fileName);
            myMap = new AtariMap(AtariJson.ParsedData.MapSize, AtariJson.ParsedData.MapScreenSize)
            {
                Data = AtariJson.ParsedData.MapData.Select(i => (byte)i).ToArray()
            };
            
            undoManager = new UndoManager(myMap);
            AtariFontRenderer.Color5 = AtariJson.ParsedData.Color5.Select(i => (byte)i).ToArray();
            
            // Load fonts - handle v1.2 and v2.0 formats
            if (AtariJson.ParsedData.FontDataArray != null && AtariJson.ParsedData.FontDataArray.Length > 0)
            {
                // v2.0 format with multiple fonts
                for (int i = 0; i < AtariJson.ParsedData.FontDataArray.Length && i < 8; i++)
                {
                    if (AtariJson.ParsedData.FontDataArray[i] != null)
                    {
                        string fileNameFont = (AtariJson.ParsedData.FontFileNames != null && i < AtariJson.ParsedData.FontFileNames.Length) 
                            ? AtariJson.ParsedData.FontFileNames[i] : null;
                        myMap.SetFontData(AtariJson.ParsedData.FontDataArray[i].Select(j => (byte)j).ToArray(), i, fileNameFont);
                    }
                }
                // Set the first font as the active screen font
                if (AtariJson.ParsedData.FontDataArray[0] != null)
                {
                    AtariFontRenderer.SetFontData(AtariJson.ParsedData.FontDataArray[0].Select(i => (byte)i).ToArray(), Globals.FontType.Screen);
                }
            }
            else if (AtariJson.ParsedData.FontData != null)
            {
                // v1.2 format - migrate to v2.0
                myMap.SetFontData(AtariJson.ParsedData.FontData.Select(i => (byte)i).ToArray(), 0);
                AtariFontRenderer.SetFontData(AtariJson.ParsedData.FontData.Select(i => (byte)i).ToArray(), Globals.FontType.Screen);
            }
            
            // Load font line mapping (always per-screen now)
            if (AtariJson.ParsedData.FontLineMappingPerScreen != null)
            {
                myMap.FontLineMappingPerScreen = AtariJson.ParsedData.FontLineMappingPerScreen.Select(i => (byte)i).ToArray();
            }
            else
            {
                // Initialize to all font 0 for all screens
                myMap.SetFontForAllLines(0);
            }
            
            // Load font mapping references
            if (AtariJson.ParsedData.FontLineMappingReferences != null && AtariJson.ParsedData.FontLineMappingReferences.Count > 0)
            {
                // Convert to ScreenReference dictionary (in case old files had Point)
                myMap.FontLineMappingReferences = new Dictionary<string, ScreenReference>();
                foreach (var kvp in AtariJson.ParsedData.FontLineMappingReferences)
                {
                    if (kvp.Value != null)
                        myMap.FontLineMappingReferences[kvp.Key] = new ScreenReference(kvp.Value.X, kvp.Value.Y);
                }
            }
            else
            {
                // Initialize all screens to reference screen 0,0 by default
                myMap.FontLineMappingReferences = new Dictionary<string, ScreenReference>();
                for (int sy = 0; sy < myMap.MapSize.Height; sy++)
                {
                    for (int sx = 0; sx < myMap.MapSize.Width; sx++)
                    {
                        string key = $"{sx},{sy}";
                        myMap.FontLineMappingReferences[key] = new ScreenReference(0, 0);
                    }
                }
            }
            
            // Note: Old files with FontLineMapping (shared mode) are no longer supported
            // They would need to be migrated manually or through a converter
            
            // Load font template settings
            if (AtariJson.ParsedData.FontTemplateLocked.HasValue)
                myMap.FontTemplateLocked = AtariJson.ParsedData.FontTemplateLocked.Value;
            if (!string.IsNullOrEmpty(AtariJson.ParsedData.FontTemplatePattern))
                myMap.FontTemplatePattern = AtariJson.ParsedData.FontTemplatePattern;
            if (AtariJson.ParsedData.MultiFontEnabled.HasValue)
                myMap.MultiFontEnabled = AtariJson.ParsedData.MultiFontEnabled.Value;
            
            // Load DLI data
            if (AtariJson.ParsedData.DliData == null)
                myMap.InitDliColorFullMap();
            else
                myMap.ColorData = AtariJson.ParsedData.DliData.Select(i => (byte)i).ToArray();

            // Load v2.0 fields
            if (!string.IsNullOrEmpty(AtariJson.ParsedData.MapDescription))
                myMap.MapDescription = AtariJson.ParsedData.MapDescription;
            if (AtariJson.ParsedData.ScreenDescriptions != null)
                myMap.ScreenDescriptions = AtariJson.ParsedData.ScreenDescriptions;
            if (AtariJson.ParsedData.ScreenMetadataDict != null)
                myMap.ScreenMetadata = AtariJson.ParsedData.ScreenMetadataDict;
            if (AtariJson.ParsedData.MetadataTypeLabels != null)
                myMap.MetadataTypeLabels = AtariJson.ParsedData.MetadataTypeLabels;
            else
                myMap.MetadataTypeLabels = new Dictionary<byte, string>();
            MetadataTypeRegistry.SyncAfterLoad(myMap);
            if (!string.IsNullOrEmpty(AtariJson.ParsedData.SubmapPath))
            {
                myMap.SubmapPath = AtariJson.ParsedData.SubmapPath;
                myMap.ClearSubmapCache(); // Clear cache when loading from file
            }
            if (AtariJson.ParsedData.IsTilemap.HasValue)
                myMap.IsTilemap = AtariJson.ParsedData.IsTilemap.Value;
            if (AtariJson.ParsedData.TilemapInfo != null)
                myMap.TilemapInfo = AtariJson.ParsedData.TilemapInfo;
            if (AtariJson.ParsedData.BitmapTileset != null)
                myMap.BitmapTileset = AtariJson.ParsedData.BitmapTileset;
            if (AtariJson.ParsedData.ElementLibrary != null)
                myMap.ElementLibrary = AtariJson.ParsedData.ElementLibrary;
            if (AtariJson.ParsedData.ScreenLinks != null)
                myMap.ScreenLinks = AtariJson.ParsedData.ScreenLinks;
            
            // For tilemaps, initialize and regenerate CharData from tile indexes
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                // Initialize CharData array
                myMap.InitializeCharData();
                // Regenerate CharData from loaded tile indexes
                myMap.RegenerateCharDataFromTiles();
            }
            
            // Update clipboard inverse button state based on map type
            UpdateClipboardInverseButtonState();
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.Export((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, (int)numericUpDown5.Value, saveFileDialog1.FileName);
                    int width = (int)((numericUpDownScreenToX.Value - numericUpDownScreenFromX.Value + 1) * myMap.ScreenSize.Width + numericUpDown5.Value);
                    string unit = myMap.IsTilemap ? "tiles" : "characters";
                    MessageBox.Show($"Export dataline width: {width} {unit}");
                    break;
            }
        }

        private void Export(int x1, int y1, int x2, int y2, int extraCharsOnLine, string filename)
        {
            // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = (x2 + 1) * myMap.ScreenSize.Width;
            int yf = (y2 + 1) * myMap.ScreenSize.Height;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);


            byte myData;
            for (int y = ys; y < yf; y++)
                for (int x = xs; x < xf + extraCharsOnLine; x++)
                {

                    if (x < xf)
                        myData = myMap.Data[x + y * myMap.Stride];
                    else
                        myData = 0;

                    fs.WriteByte(myData);
                }
            fs.Close();
            fs.Dispose();
        }

        private void ExportScreens(int x1, int y1, int x2, int y2, string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);

            byte myData;
            for (int ymap = y1; ymap <= y2; ymap++)
                for (int xmap = x1; xmap <= x2; xmap++)
                {
                    // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
                    int xs = xmap * myMap.ScreenSize.Width;
                    int ys = ymap * myMap.ScreenSize.Height;
                    int xf = (xmap + 1) * myMap.ScreenSize.Width;
                    int yf = (ymap + 1) * myMap.ScreenSize.Height;

                    for (int y = ys; y < yf; y++)
                        for (int x = xs; x < xf; x++)
                        {

                            if (x < xf)
                                myData = myMap.Data[x + y * myMap.Stride];
                            else
                                myData = 0;

                            fs.WriteByte(myData);
                        }
                }
            fs.Close();
            fs.Dispose();
        }

        private void ExportScreenByScreen()
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ExportScreens((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, saveFileDialog1.FileName);
                    int width = myMap.ScreenSize.Width;
                    string unit = myMap.IsTilemap ? "tiles" : "characters";
                    MessageBox.Show($"Export dataline width: {width} {unit}");
                    break;
            }
        }

        private void ImportScreenByScreen()
        {
            openFileDialog1.Filter = "Map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ImportScreens((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void ExportColumns(int x1, int y1, int x2, int y2, string filename)
        {
            // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = (x2 + 1) * myMap.ScreenSize.Width;
            int yf = (y2 + 1) * myMap.ScreenSize.Height;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);


            byte myData;

            for (int x = xs; x < xf; x++)
                for (int y = ys; y < yf; y++)
                {


                    myData = myMap.Data[x + y * myMap.Stride];

                    fs.WriteByte(myData);
                }
            fs.Close();
            fs.Dispose();
        }

        private void ImportColumns(int x1, int y1, string filename)
        {
            // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int yf = ys + myMap.ScreenSize.Height;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);


            byte myData;

            int x = xs;
            while (fs.Position <= (fs.Length - myMap.ScreenSize.Height))
            {

                for (int y = ys; y < yf; y++)
                {
                    myData = (byte)fs.ReadByte();

                    myMap.Data[x + y * myMap.Stride] = myData;

                }
                x++;
                if (x == myMap.Stride)
                {
                    MessageBox.Show("Reading aborted! Reached right edge of map.");
                    break;
                }
            }
            fs.Close();
            fs.Dispose();
            
            // For tilemaps, regenerate CharData from imported tile indexes
            if (myMap.IsTilemap)
            {
                myMap.RegenerateCharDataFromTiles();
            }

        }

        // this imports screens 1 by 1 from the ScreenFrom screen rows and columns
        private void ImportScreens(int xmap, int ymap, string filename)
        {
            /*
             * for (int ymap = y1; ymap <= y2; ymap++)
                for (int xmap = x1; xmap <= x2; xmap++)
                {
                    int xs = xmap * myMap.ScreenSize.Width;
                    int ys = ymap * myMap.ScreenSize.Height;
                    int xf = (xmap + 1) * myMap.ScreenSize.Width;
                    int yf = (ymap + 1) * myMap.ScreenSize.Height;
            */

            // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
            int screenSize = myMap.ScreenSize.Width * myMap.ScreenSize.Height;
            int importedScreens = 0;
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);

            while (fs.Position <= (fs.Length - screenSize))
            {

                byte myData;

                int xs = xmap * myMap.ScreenSize.Width;
                int ys = ymap * myMap.ScreenSize.Height;
                int xf = (xmap + 1) * myMap.ScreenSize.Width;
                int yf = (ymap + 1) * myMap.ScreenSize.Height;

                //Import single screen
                for (int y = ys; y < yf; y++)
                    for (int x = xs; x < xf; x++)
                    {
                        myData = (byte)fs.ReadByte();
                        myMap.Data[x + y * myMap.Stride] = myData;
                    }
                importedScreens++;

                //go to next screen
                xmap++;
                if (xmap == myMap.MapSize.Width)
                {
                    xmap = 0;
                    ymap++;
                }
                if (ymap == myMap.MapSize.Height)
                {
                    MessageBox.Show($"Reading aborted! Reached bottom edge of map.\nImported screens: {importedScreens}\nSkipped screens (beyond map bounds): {(fs.Length - importedScreens * screenSize) / screenSize}");
                    break;
                }
            }
            MessageBox.Show($"Imported screens: {importedScreens}\n");
            fs.Close();
            fs.Dispose();
            
            // For tilemaps, regenerate CharData from imported tile indexes
            if (myMap.IsTilemap)
            {
                myMap.RegenerateCharDataFromTiles();
            }
        }

        private void Import(int x1, int y1, int width, string filename)
        {
            // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = xs + width;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);


            byte myData;

            int y = ys;
            while (fs.Position <= (fs.Length - width))
            {
                for (int x = xs; x < xf; x++)
                {
                    myData = (byte)fs.ReadByte();
                    myMap.Data[x + y * myMap.Stride] = myData;

                }
                y++;
                if (y == myMap.MapSize.Height * myMap.ScreenSize.Height)
                {
                    MessageBox.Show("Reading aborted! Reached bottom edge of map.");
                    break;
                }
            }
    
            fs.Close();
            fs.Dispose();
            
            // For tilemaps, regenerate CharData from imported tile indexes
            if (myMap.IsTilemap)
            {
                myMap.RegenerateCharDataFromTiles();
            }

        }

        private void ButtonLoadFont_Click(object sender, EventArgs e)
        {
            // Don't allow loading fonts for tilemaps (fonts come from submap)
            if (myMap != null && myMap.IsTilemap)
            {
                MessageBox.Show("Fonts for tilemaps are inherited from the submap. Use Tilemap Config to update the submap.", 
                    "Tilemap Fonts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            openFileDialog1.Filter = "Atari Font (*.fnt)|*.fnt";
            switch (openFileDialog1.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.OK:
                    LoadFontToSlot(openFileDialog1.FileName, 0);
                    // Also update the multifont checkbox state if needed
                    if (checkBoxMultiFont != null && !checkBoxMultiFont.Checked)
                    {
                        // When loading font in single font mode, ensure font 0 is set
                        // This is already done in LoadFontToSlot, but we make sure it's the active font
                    }
                    break;
            }
        }

        private void LoadFontToSlot(string fileName, int fontSlot)
        {
            byte[] fontData = new byte[1024 * 2];
            FileStream fs = new FileStream(fileName, FileMode.Open);
            fs.Read(fontData, 0, 1024);
            fs.Close();
            for (int a = 0; a < 1024; a++)
            {
                fontData[a + 1024] = (byte)(fontData[a] ^ 0x80);
            }

            myMap.SetFontData(fontData, fontSlot, fileName);
            AtariFontRenderer.ClearFontCache();

            // If loading to slot 0, also set it as the active screen font
            if (fontSlot == 0)
            {
                AtariFontRenderer.SetFontData(fontData, Globals.FontType.Screen);
            }

            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
            pictureBoxMap.Refresh();
            if (myCharPicker != null)
                myCharPicker.Refresh();
            // Refresh tile picker if it's open
            if (tilePicker != null && tilePicker.Visible)
            {
                tilePicker.Refresh();
            }
        }

        private void CheckBoxMultiFont_CheckedChanged(object sender, EventArgs e)
        {
            if (myMap != null)
            {
                myMap.MultiFontEnabled = checkBoxMultiFont.Checked;
                // Enable/disable reference controls based on MultiFont
                UpdateFontMappingReferenceUI();
                AtariFontRenderer.ClearFontCache();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
                UpdateMultiFontUI();
                
                // Update DLI form to show/hide font column
                if (dliForm != null && dliForm.Visible)
                {
                    dliForm.ZoomResize();
                    dliForm.Show(dliForm.screenNumber); // Refresh the form with current screen
                }
            }
        }

        private void CheckBoxFontMappingReference_CheckedChanged(object sender, EventArgs e)
        {
            if (myMap != null)
            {
                bool useReference = checkBoxFontMappingReference.Checked;
                int refX = (int)numericUpDownRefScreenX.Value;
                int refY = (int)numericUpDownRefScreenY.Value;
                
                myMap.SetFontMappingReference(currentScreen.X, currentScreen.Y, useReference, refX, refY);
                
                // Enable/disable numeric updowns
                numericUpDownRefScreenX.Enabled = useReference && myMap.MultiFontEnabled;
                numericUpDownRefScreenY.Enabled = useReference && myMap.MultiFontEnabled;
                labelRefScreen.Enabled = useReference && myMap.MultiFontEnabled;
                
                AtariFontRenderer.ClearFontCache();
                // Use RedrawEditorWindow to preserve locked marker visibility
                RedrawEditorWindow();
                
                // Update DLI form
                if (dliForm != null && dliForm.Visible)
                {
                    dliForm.ZoomResize();
                    dliForm.Show(dliForm.screenNumber);
                }
            }
        }

        private void NumericUpDownRefScreen_ValueChanged(object sender, EventArgs e)
        {
            if (myMap != null && checkBoxFontMappingReference.Checked)
            {
                int refX = (int)numericUpDownRefScreenX.Value;
                int refY = (int)numericUpDownRefScreenY.Value;
                
                myMap.SetFontMappingReference(currentScreen.X, currentScreen.Y, true, refX, refY);
                
                AtariFontRenderer.ClearFontCache();
                // Use RedrawEditorWindow to preserve locked marker visibility
                RedrawEditorWindow();
                
                // Update DLI form
                if (dliForm != null && dliForm.Visible)
                {
                    dliForm.ZoomResize();
                    dliForm.Show(dliForm.screenNumber);
                }
            }
        }

        private void UpdateFontMappingReferenceUI()
        {
            if (myMap == null || checkBoxFontMappingReference == null)
                return;
            
            // Update max values FIRST before trying to set Value
            numericUpDownRefScreenX.Maximum = Math.Max(0, myMap.MapSize.Width - 1);
            numericUpDownRefScreenY.Maximum = Math.Max(0, myMap.MapSize.Height - 1);
            
            // Get current screen reference settings
            bool useReference;
            int refX, refY;
            useReference = myMap.GetFontMappingReference(currentScreen.X, currentScreen.Y, out refX, out refY);
            
            // Clamp reference values to valid range
            refX = Math.Max(0, Math.Min(myMap.MapSize.Width - 1, refX));
            refY = Math.Max(0, Math.Min(myMap.MapSize.Height - 1, refY));
            
            checkBoxFontMappingReference.Checked = useReference;
            numericUpDownRefScreenX.Value = refX;
            numericUpDownRefScreenY.Value = refY;
            
            bool enabled = myMap.MultiFontEnabled;
            checkBoxFontMappingReference.Enabled = enabled;
            numericUpDownRefScreenX.Enabled = enabled && useReference;
            numericUpDownRefScreenY.Enabled = enabled && useReference;
            labelRefScreen.Enabled = enabled && useReference;
            labelRefScreenComma.Enabled = enabled && useReference;
        }

        private void UpdateMultiFontUI()
        {
            bool enabled = checkBoxMultiFont != null && checkBoxMultiFont.Checked;
            bool tilemapEnabled = myMap != null && myMap.IsTilemap;
            
            // For tilemaps, hide/disable multi-font related controls (they don't make sense for tilemaps)
            if (tilemapEnabled)
            {
                if (checkBoxMultiFont != null)
                {
                    checkBoxMultiFont.Visible = false;
                    checkBoxMultiFont.Enabled = false;
                }
                if (checkBoxFontMappingReference != null)
                {
                    checkBoxFontMappingReference.Visible = false;
                    checkBoxFontMappingReference.Enabled = false;
                }
                if (labelRefScreen != null)
                {
                    labelRefScreen.Visible = false;
                    labelRefScreen.Enabled = false;
                }
                if (numericUpDownRefScreenX != null)
                {
                    numericUpDownRefScreenX.Visible = false;
                    numericUpDownRefScreenX.Enabled = false;
                }
                if (numericUpDownRefScreenY != null)
                {
                    numericUpDownRefScreenY.Visible = false;
                    numericUpDownRefScreenY.Enabled = false;
                }
                if (labelRefScreenComma != null)
                {
                    labelRefScreenComma.Visible = false;
                    labelRefScreenComma.Enabled = false;
                }
            }
            else
            {
                // Show controls for regular maps
                if (checkBoxMultiFont != null)
                {
                    checkBoxMultiFont.Visible = true;
                    checkBoxMultiFont.Enabled = true;
                }
                if (checkBoxFontMappingReference != null)
                {
                    checkBoxFontMappingReference.Visible = true;
                    checkBoxFontMappingReference.Enabled = enabled;
                }
                if (labelRefScreen != null)
                {
                    labelRefScreen.Visible = true;
                }
                if (numericUpDownRefScreenX != null)
                {
                    numericUpDownRefScreenX.Visible = true;
                }
                if (numericUpDownRefScreenY != null)
                {
                    numericUpDownRefScreenY.Visible = true;
                }
                if (labelRefScreenComma != null)
                {
                    labelRefScreenComma.Visible = true;
                }
            }
            
            // Font Templates: enabled only when multifont is checked (and not tilemap)
            if (buttonFontTemplate != null)
                buttonFontTemplate.Enabled = !tilemapEnabled && enabled;
            
            // Handle groupBoxFont: groupbox stays enabled; when multifont checked only disable Load/Export/Refresh buttons
            if (groupBoxFont != null)
            {
                groupBoxFont.Enabled = true; // may be overridden by UpdateMetadataLayerUI
                
                foreach (Control ctrl in groupBoxFont.Controls)
                {
                    if (ctrl.Name == "buttonShowTiles")
                    {
                        ctrl.Visible = tilemapEnabled;
                        ctrl.Enabled = tilemapEnabled;
                    }
                    else
                    {
                        ctrl.Visible = !tilemapEnabled;
                        // When multifont is checked, disable only Load font, Export font, Refresh; keep Show font enabled
                        bool isLoadExportOrRefresh = (ctrl == buttonLoadFont || ctrl == buttonExportFont || ctrl == buttonRefreshFont);
                        ctrl.Enabled = !tilemapEnabled && (isLoadExportOrRefresh ? !enabled : true);
                    }
                }
            }
            
            UpdateMetadataLayerUI();
        }

        private void ButtonFontTemplate_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (FontTemplateDialog dialog = new FontTemplateDialog(myMap))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AtariFontRenderer.ClearFontCache();
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
            }
        }

        private void ButtonElementLibrary_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            if (elementLibraryDialog == null)
            {
                elementLibraryDialog = new ElementLibraryDialog(myMap,
                    onClipboardSet: () =>
                    {
                        if (AtariClipboard.IsValid && pictureBoxClipboard != null)
                            RegenerateClipboardImage();
                    },
                    onInvertRequested: () => InvertClipboard());
            }
            elementLibraryDialog.SetMap(myMap);
            elementLibraryDialog.Show();
            elementLibraryDialog.BringToFront();
        }

        private void ButtonMapDescription_Click(object sender, EventArgs e)
        {
            if (myMap == null) return;
            using (MapDescriptionDialog dialog = new MapDescriptionDialog(myMap))
            {
                dialog.ShowDialog();
            }
        }

        private void ButtonTilemapConfig_Click(object sender, EventArgs e)
        {
            if (myMap == null || !myMap.IsTilemap) 
            {
                MessageBox.Show("This dialog is only available for tilemap maps.", "Tilemap Only", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (TilemapConfigDialog dialog = new TilemapConfigDialog(myMap))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Update UI based on tilemap state
                    UpdateMultiFontUI();
                    // Refresh the editor window to show tilemap rendering
                    RedrawEditorWindow();
                }
            }
        }

        private void ButtonShowTiles_Click(object sender, EventArgs e)
        {
            if (myMap == null || !myMap.IsTilemap || string.IsNullOrEmpty(myMap.SubmapPath)) return;
            
            try
            {
                if (tilePicker == null)
                {
                    tilePicker = new TilePicker(myMap, pictureBoxClipboard);
                }
                // Refresh tilepicker to reflect current zoom and color settings
                tilePicker.Refresh();
                tilePicker.Show();
                tilePicker.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tiles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonExportFont_Click(object sender, EventArgs e)
        {
            // Export the single font (font 0 or the active screen font)
            byte[] fontData = null;
            string defaultFileName = "font.fnt";

            // Try to get from font 0 first
            if (myMap != null && myMap.FontDataArray != null && myMap.FontDataArray[0] != null)
            {
                fontData = myMap.FontDataArray[0];
                if (myMap.FontFileNames != null && myMap.FontFileNames[0] != null)
                {
                    defaultFileName = System.IO.Path.GetFileName(myMap.FontFileNames[0]);
                }
            }
            // Otherwise get from AtariFontRenderer (the active screen font)
            else if (AtariFontRenderer.fonts.ContainsKey(Globals.FontType.Screen))
            {
                fontData = AtariFontRenderer.fonts[Globals.FontType.Screen].data;
                if (!string.IsNullOrEmpty(AtariFontRenderer.LastFontFile))
                {
                    defaultFileName = System.IO.Path.GetFileName(AtariFontRenderer.LastFontFile);
                }
            }

            if (fontData == null)
            {
                MessageBox.Show("No font data available to export.", "Export Font", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Atari Font (*.fnt)|*.fnt";
            saveDialog.FileName = defaultFileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Export only first 1024 bytes (standard Atari font format)
                byte[] exportData = new byte[1024];
                Array.Copy(fontData, exportData, Math.Min(1024, fontData.Length));
                File.WriteAllBytes(saveDialog.FileName, exportData);
                MessageBox.Show("Font exported successfully.", "Export Font", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
   

        private void ButtonShiftChars_Click(object sender, EventArgs e)
        {
            //64-79 -> 80-95
            //32-47 -> 64-79
            if (MessageBox.Show("R U sure?", "Shift characters in map", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                for (int i = 0; i < myMap.Data.Length; i++)
                {
                    if (myMap.Data[i] >= 64 && myMap.Data[i] <= 79)
                    {
                        myMap.Data[i] += 16;
                    }
                    else
                    {
                        if (myMap.Data[i] >= 32 && myMap.Data[i] <= 47)
                        {
                            myMap.Data[i] += 32;
                        }
                    }
                }
                MessageBox.Show("Done");
            }
        }

        private void ButtonHoboExport_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "MapData export (*.dat)|*.dat";
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ExportColumns((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, saveFileDialog1.FileName);
                    break;
            }
        }

        private void DliExport()
        {
            saveFileDialog1.Filter = "Dli column export (*.dat)|*.dat";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create);

                byte myData;

                int charOffset = (int)numericUpDownScreenFromX.Value * myMap.ScreenSize.Width + (int)numericUpDownScreenFromY.Value * myMap.Stride * myMap.ScreenSize.Height;
                for (int x = 0; x < 5; x++)
                {
                    if (maskedTextBoxDli.Text[x] == '0') continue;    //skip 0 masks
                    for (int y = 0; y < myMap.ScreenSize.Height; y++)
                    {
                        byte[] color5 = myMap.GetDliColor5(charOffset + y * myMap.Stride);
                        myData = color5[x];
                        fs.WriteByte(myData);
                    }
                }
                fs.Close();
                fs.Dispose();
            }
        }

        private void DliImport()
        {
            openFileDialog1.Filter = "Dli column export (*.dat)|*.dat";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(openFileDialog1.FileName, System.IO.FileMode.Open);
                
                for (int x = 0; x < 5; x++)
                {
                    if (maskedTextBoxDli.Text[x] == '0') continue;    //skip 0 masks
                    for (int y = 0; y < myMap.ScreenSize.Height; y++)
                    {
                        myMap.SetDliColor((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, y, x, (byte)fs.ReadByte());
                    }
                }
                fs.Close();
                fs.Dispose();
            }
        }

        private void ButtonHoboImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Column based map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.ImportColumns((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void ButtonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Map datafile (*.*)|*.*";
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    this.Import((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDown6.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void BtnNewMap_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Create new map? (current mapdata will be deleted!)", "New map", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                bool isTilemap = checkBoxTilemap.Checked;
                string submapPath = null;
                
                // If tilemap mode, require submap selection
                if (isTilemap)
                {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "Atari Map Files (*.atrmap)|*.atrmap|All Files (*.*)|*.*";
                    openDialog.Title = "Select Submap File (Required for Tilemap)";
                    
                    if (openDialog.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("Submap file is required for tilemap mode. Map creation cancelled.", 
                            "Submap Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    submapPath = openDialog.FileName;
                    
                    // Validate submap file exists
                    if (!System.IO.File.Exists(submapPath))
                    {
                        MessageBox.Show("Submap file does not exist. Map creation cancelled.", 
                            "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                
                AtariFontRenderer.SetAlpa(checkBoxAlpa.Checked);
                
                Size mapSize = new Size((int)nudMapW.Value, (int)nudMapH.Value);
                Size screenSize = new Size((int)nudScreenW.Value, (int)nudScreenH.Value);
                
                if (isTilemap)
                {
                    // Load submap first to get tile dimensions
                    try
                    {
                        AtariMap tempSubmap = SubmapManager.LoadSubmap(submapPath);
                        int tileWidth = tempSubmap.ScreenSize.Width;
                        int tileHeight = tempSubmap.ScreenSize.Height;
                        
                        // Copy Color5 from submap (AtariJson.ParsedData contains the parsed submap data)
                        // Note: SubmapManager.LoadSubmap calls AtariJson.ParseAtrmap, so ParsedData is available
                        if (AtariJson.ParsedData.Color5 != null && AtariJson.ParsedData.Color5.Length > 0)
                        {
                            AtariFontRenderer.Color5 = AtariJson.ParsedData.Color5.Select(i => (byte)i).ToArray();
                        }
                        
                        // For tilemaps:
                        // - MapSize: number of screens (e.g., 2x2 screens)
                        // - ScreenSize: size of each screen in TILES (e.g., 5x5 tiles per screen)
                        // - Tile dimensions come from submap (e.g., 2x2 chars per tile)
                        // - Data array stores tile indexes: (MapSize.Width * ScreenSize.Width) x (MapSize.Height * ScreenSize.Height) tiles
                        // - Total map size: 2x2 screens * 5x5 tiles = 10x10 tiles = 20x20 chars (for 2x2 tiles)
                        
                        // Create map with screen size in TILES (not chars)
                        // MapSize: 2x2 screens, ScreenSize: 5x5 tiles per screen
                        // So Data array will be: (2 * 5) x (2 * 5) = 10x10 tiles
                        myMap = new AtariMap(mapSize, screenSize);
                        myMap.IsTilemap = true;
                        myMap.SubmapPath = submapPath;
                        // Clear submap cache when path changes
                        myMap.ClearSubmapCache();
                        
                        // Initialize tilemap info
                        myMap.TilemapInfo = new TilemapData();
                        myMap.TilemapInfo.TileWidth = tileWidth;
                        myMap.TilemapInfo.TileHeight = tileHeight;
                        myMap.TilemapInfo.NumberingPattern = "row-major";
                        myMap.TilemapInfo.Use16BitIndexes = false; // Default to 8-bit, can be changed later
                        
                        // Initialize CharData array for fast rendering
                        // CharData size: (MapSize.Width * ScreenSize.Width * TileWidth) x (MapSize.Height * ScreenSize.Height * TileHeight)
                        // Example: (2 * 5 * 2) x (2 * 5 * 2) = 20x20 chars
                        myMap.InitializeCharData();
                        
                        // Reinitialize ColorData with correct size for tilemaps (character lines, not tile lines)
                        myMap.InitDliColorFullMap();
                        
                        // Reinitialize FontLineMappingPerScreen with correct size for tilemaps (character lines, not tile lines)
                        int screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                        myMap.FontLineMappingPerScreen = new byte[myMap.MapSize.Width * myMap.MapSize.Height * screenCharHeight];
                        
                        // For tilemaps, automatically enable multi-font (fonts come from tiles)
                        myMap.MultiFontEnabled = true;
                        
                        // Update clipboard inverse button state (disable in tile mode)
                        UpdateClipboardInverseButtonState();
                        
                        if (checkBoxMultiFont != null)
                        {
                            checkBoxMultiFont.Checked = true;
                        }
                        
                        // For tilemaps, Data array stores tile indexes, not characters
                        // Size is already correct: (MapSize.Width * ScreenSize.Width) x (MapSize.Height * ScreenSize.Height) tiles
                        // This is set by AtariMap constructor, and represents the tile grid
                        // All values are initialized to 0 (tile index 0)
                        
                        // Inherit all fonts from submap
                        if (tempSubmap.FontDataArray != null)
                        {
                            for (int i = 0; i < tempSubmap.FontDataArray.Length && i < 8; i++)
                            {
                                if (tempSubmap.FontDataArray[i] != null)
                                {
                                    string fontFileName = (tempSubmap.FontFileNames != null && i < tempSubmap.FontFileNames.Length) 
                                        ? tempSubmap.FontFileNames[i] : null;
                                    myMap.SetFontData(tempSubmap.FontDataArray[i], i, fontFileName);
                                }
                            }
                        }
                        
                        // Set first font as active screen font if available
                        if (tempSubmap.FontDataArray != null && tempSubmap.FontDataArray[0] != null)
                        {
                            AtariFontRenderer.SetFontData(tempSubmap.FontDataArray[0], Globals.FontType.Screen);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading submap: {ex.Message}\nMap creation cancelled.", 
                            "Submap Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    // Regular map - use character grid; load default font 0 from resources so Show Font and rendering work
                    myMap = new AtariMap(mapSize, screenSize);
                    myMap.IsTilemap = false;
                    byte[] defaultFontData = DefaultFontResource.GetDefaultFontDataFromResources();
                    myMap.SetFontData(defaultFontData, 0);
                    myMap.SetFontForAllLines(0);
                    AtariFontRenderer.SetFontData(defaultFontData, Globals.FontType.Screen);
                    AtariFontRenderer.ClearFontCache();
                }
                
                ClearClipboard();
                undoManager = new UndoManager(myMap);
                AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
                if (myCharPicker != null)
                    myCharPicker.RedrawFontWindow();
                numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                numericUpDownScreenFromX.Maximum = nudMapW.Value - 1;
                numericUpDownScreenToX.Maximum = nudMapW.Value - 1;
                numericUpDownScreenFromY.Maximum = nudMapH.Value - 1;
                numericUpDownScreenToY.Maximum = nudMapH.Value - 1;

                // Update font mapping reference controls
                if (numericUpDownRefScreenX != null && numericUpDownRefScreenY != null)
                {
                    numericUpDownRefScreenX.Maximum = Math.Max(0, myMap.MapSize.Width - 1);
                    numericUpDownRefScreenY.Maximum = Math.Max(0, myMap.MapSize.Height - 1);
                    numericUpDownRefScreenX.Value = 0;
                    numericUpDownRefScreenY.Value = 0;
                }

                currentScreen = new Point(0, 0);
                previousScreen = new Point(-1, -1);  // Reset to force redraw
                isScreenLocked = false;
                UpdateFontMappingReferenceUI();
                UpdateMultiFontUI();
                UpdateClipboardInverseButtonState();  // Update button state when map type changes
                RedrawEditorWindow();
                dliForm.Dispose();
                dliForm = new DliForm(myMap, pictureBoxMap);
                dliForm.RenderData();
            }

        }

        private void TrackBarZoom_Scroll(object sender, EventArgs e)
        {
            Globals.Zoom = trackBarZoom.Value;
            PictureBoxMap_ClientSizeChanged(null, null);
            if (myCharPicker != null)
                myCharPicker.SetZoom();
            dliForm.ZoomResize();
            dliForm.Hide();
            // Refresh tile picker if it's open
            if (tilePicker != null && tilePicker.Visible)
            {
                tilePicker.Refresh();
            }
        }

        private void ComboBoxDrawGrid_CheckedChanged(object sender, EventArgs e)
        {
            AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
            RedrawEditorWindow();
        }

        private void ComboBoxDrawBorders_CheckedChanged(object sender, EventArgs e)
        {
            AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
            RedrawEditorWindow();
        }

        private void RedrawEditorWindow()
        {
            //AtariFontRenderer.RenderMapData(myMap, AtariFontRenderer.offset, dataImage); //redraw data
            AtariPictureTools.Redraw(Globals.WindowType.Editor, true, true, true, currentScreen, isScreenLocked, lockedScreen); //dataImage);                         //redraw grids
            if (!ScreenSelectionShown)  //refresh only when selection is not supposed to be drawn (prevents flickering)
                pictureBoxMap.Refresh();
            ScreenSelectionShown = false;
        }

        private void ComboBoxOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import","Export DLI"};
            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = true;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = true;
                    numericUpDown5.Enabled = true;
                    numericUpDown6.Enabled = false;
                    break;
                case 1:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = true;
                    break;
                case 2:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = true;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = true;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 3:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 4:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 5:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
            }
            maskedTextBoxDli.Visible = labelDliMask.Visible = comboOperation.SelectedIndex == 4;

        }

        private void ButtonPerform_Click(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import"};
            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    ButtonExport_Click(null, null);
                    break;
                case 1:
                    ButtonImport_Click(null, null);
                    break;
                case 2:
                    ButtonHoboExport_Click(null, null);
                    break;
                case 3:
                    ButtonHoboImport_Click(null, null);
                    break;
                case 4:
                    DliExport();
                    break;
                case 5:
                    DliImport();
                    break;
                case 6:
                    ExportScreenByScreen();
                    break;
                case 7:
                    ImportScreenByScreen();
                    break;
            }
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/matosimi/atari-mapmaker");
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://matosimi.atari.org");
        }

        private void ButtonRefreshFont_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(AtariFontRenderer.LastFontFile))
            {
                AtariFontRenderer.LoadFont(AtariFontRenderer.LastFontFile, Globals.FontType.Screen);
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                AtariPictureTools.Redraw(Globals.WindowType.CharPicker);

                //AtariFontRenderer.RedrawFont();
                //myCharPicker.GetRenderer().LoadFont(myRenderer.LastFontFile);
                //myCharPicker.GetRenderer().RedrawFont();
                //myCharPicker.RedrawFontWindow();
                //RedrawEditorWindow();
            }
        }

        private void CheckBoxEditDli_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEditDli.Checked)
            {
                checkBoxShowDli.Checked = true;
            }
            else
            {
                dliForm.Hide();
            }
            AtariFontRenderer.UseDli = checkBoxShowDli.Checked;
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            pictureBoxMap.Refresh();

        }

        private void CheckBoxShowDli_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxShowDli.Checked) checkBoxEditDli.Checked = false;
            AtariFontRenderer.UseDli = checkBoxShowDli.Checked;
            AtariPictureTools.Redraw(Globals.WindowType.Editor);
            pictureBoxMap.Refresh();
        }

        /// <summary>
        /// Click handler for pictureBoxClipboard - reactivates last used clipboard
        /// </summary>
        private void PictureBoxClipboard_Click(object sender, EventArgs e)
        {
            if (AtariClipboard.ClipboardImage != null)
            {
                AtariClipboard.IsValid = true;
                pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                pictureBoxMap.Refresh();
            }
        }
        
        /// <summary>
        /// Click handler for buttonClipboardInverse - inverts clipboard chars (XOR 0x80)
        /// </summary>
        private void ButtonClipboardInverse_Click(object sender, EventArgs e)
        {
            InvertClipboard();
        }
        
        /// <summary>
        /// CheckedChanged handler for checkBoxClipboardSkip0 - updates SkipZero and regenerates clipboard image so pixel format stays correct (avoids grey when toggling with library content).
        /// </summary>
        private void CheckBoxClipboardSkip0_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxClipboardSkip0 != null)
            {
                AtariClipboard.SkipZero = checkBoxClipboardSkip0.Checked;
                if (AtariClipboard.IsValid && pictureBoxClipboard != null)
                    RegenerateClipboardImage();
            }
        }
        
        /// <summary>
        /// Keyboard handler - 'i' key toggles clipboard inverse; ESC exits metadata paste mode
        /// </summary>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (MetadataItemClipboard.HasItem)
                {
                    if (previousMetadataOverlayLocation.HasValue && metadataUnderImage != null)
                    {
                        AtariPictureTools.DrawMetadataUnder(previousMetadataOverlayLocation.Value, metadataUnderImage);
                        pictureBoxMap.Refresh();
                    }
                    previousMetadataOverlayLocation = null;
                    metadataUnderImage?.Dispose();
                    metadataUnderImage = null;
                    metadataPreviewImage?.Dispose();
                    metadataPreviewImage = null;
                    MetadataItemClipboard.Clear();
                    UpdateMetadataClipboardPreview();
                    e.Handled = true;
                    return;
                }
                // ESC also exits clipboard paste mode (same as right double-click)
                if (AtariClipboard.IsValid)
                {
                    if (AtariPictureTools.PreviousClipboardLocation.HasValue)
                    {
                        AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);
                    }
                    AtariClipboard.IsValid = false;
                    AtariPictureTools.PreviousClipboardLocation = null;
                    AtariPictureTools.PreviousClipboardGridCell = null;
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                    if (pictureBoxClipboard != null)
                    {
                        pictureBoxClipboard.Image = null;
                        pictureBoxClipboard.Refresh();
                    }
                    e.Handled = true;
                    return;
                }
            }
            if (e.KeyCode == Keys.I && buttonClipboardInverse != null && buttonClipboardInverse.Enabled)
            {
                InvertClipboard();
                e.Handled = true;
            }
        }

        /// <summary>Updates pictureBoxClipboard to show metadata item preview when HasItem; otherwise restores char/tile clipboard or clears.</summary>
        private void UpdateMetadataClipboardPreview()
        {
            var current = pictureBoxClipboard.Image;
            bool currentIsClipboardImage = AtariClipboard.IsValid && current == AtariClipboard.ClipboardImage;
            if (MetadataItemClipboard.HasItem)
            {
                int zoom = Math.Max(1, Globals.Zoom);
                int cellSizePixels = 8 * zoom;
                int cellW = cellSizePixels, cellH = cellSizePixels;
                bool tileSized = false;
                if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
                {
                    cellW = myMap.TilemapInfo.TileWidth * 8 * zoom;
                    cellH = myMap.TilemapInfo.TileHeight * 8 * zoom;
                    tileSized = true;
                }
                System.Drawing.Bitmap newBmp;
                if (tileSized)
                    newBmp = MetadataLayerRenderer.CreateMetadataItemPreviewBitmap(MetadataItemClipboard.CopiedItem, cellSizePixels, cellOnly: false, cellWidthPixels: cellW, cellHeightPixels: cellH);
                else
                    newBmp = MetadataLayerRenderer.CreateMetadataItemPreviewBitmap(MetadataItemClipboard.CopiedItem, cellSizePixels, cellOnly: true);
                if (current != null && !currentIsClipboardImage)
                    current.Dispose();
                pictureBoxClipboard.Image = newBmp;
            }
            else
            {
                if (current != null && !currentIsClipboardImage)
                    current.Dispose();
                pictureBoxClipboard.Image = AtariClipboard.IsValid ? AtariClipboard.ClipboardImage : null;
            }
            pictureBoxClipboard.Refresh();
        }
        
        /// <summary>
        /// Inverts all chars in clipboard (XOR 0x80) - only for font mode, not tile mode
        /// </summary>
        private void InvertClipboard()
        {
            if (!AtariClipboard.IsValid || AtariClipboard.IsTileIndexes)
                return;  // Only works in font mode, not tile mode
                
            byte[,] data = AtariClipboard.GetData();
            if (data == null)
                return;
            
            // 1) Hide clipboard from map first: restore under at current position so we don't leave a trail
            if (AtariPictureTools.PreviousClipboardLocation.HasValue)
            {
                AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);
                pictureBoxMap.Refresh();
            }
                
            bool skipZero = checkBoxClipboardSkip0 != null && checkBoxClipboardSkip0.Checked;
            
            // 2) Invert each char (XOR 0x80), skipping 0x00 if skipZero is enabled
            for (int y = 0; y < AtariClipboard.ClipboardHeight; y++)
            {
                for (int x = 0; x < AtariClipboard.ClipboardWidth; x++)
                {
                    byte charVal = data[x, y];
                    if (skipZero && charVal == 0)
                        continue;  // Skip 0x00 if checkbox is checked
                    data[x, y] = (byte)(charVal ^ 0x80);
                }
            }
            
            // 3) Regenerate clipboard image with inverted chars (no map overlay change inside)
            RegenerateClipboardImage();
            
            // 4) Update underclipboard from current map and draw new inverted clipboard on top
            if (AtariPictureTools.PreviousClipboardLocation.HasValue)
            {
                AtariPictureTools.DrawClipBoard(AtariPictureTools.PreviousClipboardLocation.Value, (Bitmap)pictureBoxMap.Image);
                pictureBoxMap.Refresh();
            }
        }
        
        /// <summary>
        /// Regenerates the clipboard image when clipboard contains tile indexes (e.g. from element library). Renders actual tile graphics from submap.
        /// </summary>
        private void RegenerateClipboardImageForTiles()
        {
            if (!AtariClipboard.IsValid || !AtariClipboard.IsTileIndexes || myMap == null || !myMap.IsTilemap || myMap.TilemapInfo == null || string.IsNullOrEmpty(myMap.SubmapPath))
                return;
            byte[,] clipData = AtariClipboard.GetData();
            if (clipData == null) return;
            int w = AtariClipboard.ClipboardWidth;
            int h = AtariClipboard.ClipboardHeight;
            int tileWidth = myMap.TilemapInfo.TileWidth;
            int tileHeight = myMap.TilemapInfo.TileHeight;
            // Use main map's cached submap so we get the same instance used for map rendering (correct fonts/structure)
            AtariMap submap = myMap.GetOrLoadSubmap();
            if (submap == null) return;

            // One "screen" per tile so renderer indexes font per (tx,ty) and line correctly
            AtariMap tempMap = new AtariMap(new Size(w, h), new Size(tileWidth, tileHeight));
            tempMap.MultiFontEnabled = true;
            tempMap.FontLineMappingPerScreen = new byte[tempMap.MapSize.Width * tempMap.MapSize.Height * tempMap.ScreenSize.Height];
            for (int ty = 0; ty < h; ty++)
            {
                for (int tx = 0; tx < w; tx++)
                {
                    byte tileIndex = clipData[tx, ty];
                    int submapScreenX = tileIndex % submap.MapSize.Width;
                    int submapScreenY = tileIndex / submap.MapSize.Width;
                    int screenStartX = submapScreenX * submap.ScreenSize.Width;
                    int screenStartY = submapScreenY * submap.ScreenSize.Height;
                    for (int y = 0; y < tileHeight; y++)
                    {
                        for (int x = 0; x < tileWidth; x++)
                        {
                            int srcIndex = (screenStartX + x) + (screenStartY + y) * submap.Stride;
                            int dstIndex = (tx * tileWidth + x) + (ty * tileHeight + y) * tempMap.Stride;
                            if (srcIndex < submap.Data.Length && dstIndex < tempMap.Data.Length)
                                tempMap.Data[dstIndex] = submap.Data[srcIndex];
                        }
                    }
                    if (submap.FontLineMappingPerScreen != null)
                    {
                        Point actualSubmapScreen = submap.GetReferencedScreen(submapScreenX, submapScreenY);
                        int submapScreenOffset = (actualSubmapScreen.Y * submap.MapSize.Width + actualSubmapScreen.X) * tileHeight;
                        // One screen per tile: (ty*w+tx)*tileHeight + line
                        int tempScreenOffset = (ty * tempMap.MapSize.Width + tx) * tempMap.ScreenSize.Height;
                        for (int line = 0; line < tileHeight; line++)
                        {
                            if (tempScreenOffset + line >= tempMap.FontLineMappingPerScreen.Length) break;
                            int submapIndex = submapScreenOffset + line;
                            if (submapIndex < submap.FontLineMappingPerScreen.Length)
                                tempMap.FontLineMappingPerScreen[tempScreenOffset + line] = submap.FontLineMappingPerScreen[submapIndex];
                            else if (submapScreenOffset < submap.FontLineMappingPerScreen.Length)
                                tempMap.FontLineMappingPerScreen[tempScreenOffset + line] = submap.FontLineMappingPerScreen[submapScreenOffset];
                        }
                    }
                }
            }
            if (submap.FontDataArray != null)
            {
                for (int i = 0; i < submap.FontDataArray.Length; i++)
                    if (submap.FontDataArray[i] != null)
                        tempMap.SetFontData(submap.FontDataArray[i], i);
            }
            int pw = w * tileWidth * 8;
            int ph = h * tileHeight * 8;
            Bitmap baseImage = new Bitmap(pw, ph, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            baseImage.Palette = AtariPalette.GetPalette();// GetIndexedColor5Palette();
            AtariFontRenderer.RenderMapData(tempMap, Globals.FontType.Screen, baseImage);
            
            int zoomedWidth = pw * Globals.Zoom;
            int zoomedHeight = ph * Globals.Zoom;
            Bitmap zoomedImage = new Bitmap(zoomedWidth, zoomedHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            zoomedImage.Palette = AtariPalette.GetPalette(); //GetIndexedColor5Palette();
            BitmapData srcData = baseImage.LockBits(
                new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            BitmapData dstData = zoomedImage.LockBits(
                new Rectangle(0, 0, zoomedWidth, zoomedHeight),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                for (int y = 0; y < zoomedHeight; y++)
                {
                    int sy = y / Globals.Zoom;
                    if (sy >= baseImage.Height) sy = baseImage.Height - 1;
                    for (int x = 0; x < zoomedWidth; x++)
                    {
                        int sx = x / Globals.Zoom;
                        if (sx >= baseImage.Width) sx = baseImage.Width - 1;
                        dstPtr[y * dstData.Stride + x] = srcPtr[sy * srcData.Stride + sx];
                    }
                }
            }
            baseImage.UnlockBits(srcData);
            zoomedImage.UnlockBits(dstData);
            baseImage.Dispose();
            // Convert 8bpp (indices 0-4) to 32bpp so clipboard picturebox displays correct colors regardless of SkipZero
            Color[] palette = AtariPalette.GetPalette().Entries; //GetIndexedColor5Palette().Entries;
            Bitmap displayImage = new Bitmap(zoomedWidth, zoomedHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData srcData8 = zoomedImage.LockBits(new Rectangle(0, 0, zoomedWidth, zoomedHeight), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            BitmapData dstData32 = displayImage.LockBits(new Rectangle(0, 0, zoomedWidth, zoomedHeight), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData8.Scan0;
                int* dstPtr = (int*)dstData32.Scan0;
                for (int y = 0; y < zoomedHeight; y++)
                {
                    for (int x = 0; x < zoomedWidth; x++)
                    {
                        byte idx = srcPtr[y * srcData8.Stride + x];
                        if (idx >= palette.Length) idx = 0;
                        dstPtr[y * (dstData32.Stride / 4) + x] = palette[idx].ToArgb();
                    }
                }
            }
            zoomedImage.UnlockBits(srcData8);
            displayImage.UnlockBits(dstData32);
            zoomedImage.Dispose();
            if (AtariClipboard.ClipboardImage != null)
                AtariClipboard.ClipboardImage.Dispose();
            AtariClipboard.ClipboardImage = displayImage;
            if (AtariClipboard.UnderClipBoardImage != null)
                AtariClipboard.UnderClipBoardImage.Dispose();
            AtariClipboard.UnderClipBoardImage = new Bitmap(AtariClipboard.ClipboardImage);
            if (AtariClipboard.UnderImageGraphics != null)
                AtariClipboard.UnderImageGraphics.Dispose();
            AtariClipboard.UnderImageGraphics = Graphics.FromImage(AtariClipboard.UnderClipBoardImage);
            pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
            pictureBoxClipboard.Refresh();
        }

        

        /// <summary>
        /// Regenerates the clipboard image from current clipboard data
        /// </summary>
        private void RegenerateClipboardImage()
        {
            if (!AtariClipboard.IsValid)
                return;
            if (AtariClipboard.IsTileIndexes)
            {
                RegenerateClipboardImageForTiles();
                return;
            }
            byte[,] data = AtariClipboard.GetData();
            if (data == null)
                return;
            
            // Render clipboard data directly (all lines) using Screen font, no map/DLI.
            // Font bitmap uses GetIndexedColor5Palette() (indices 0-4); we must use the same palette
            // or copied pixels will show wrong colors (e.g. grey).
            Bitmap baseClipboardImage = new Bitmap(AtariClipboard.ClipboardWidth * 8, AtariClipboard.ClipboardHeight * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            baseClipboardImage.Palette = AtariPalette.GetIndexedColor5Palette();
            AtariFontRenderer.RenderClipboardData(data, AtariClipboard.ClipboardWidth, AtariClipboard.ClipboardHeight, baseClipboardImage);
            
            int zoomedWidth = baseClipboardImage.Width * Globals.Zoom;
            int zoomedHeight = baseClipboardImage.Height * Globals.Zoom;
            
            // Scale in 8-bit only (nearest-neighbor) to avoid 8→32→8 round-trip color shifts
            Bitmap newClipboardImage = new Bitmap(zoomedWidth, zoomedHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            newClipboardImage.Palette = AtariPalette.GetIndexedColor5Palette();
            BitmapData srcData = baseClipboardImage.LockBits(
                new Rectangle(0, 0, baseClipboardImage.Width, baseClipboardImage.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            BitmapData dstData = newClipboardImage.LockBits(
                new Rectangle(0, 0, zoomedWidth, zoomedHeight),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                for (int y = 0; y < zoomedHeight; y++)
                {
                    int sy = y / Globals.Zoom;
                    if (sy >= baseClipboardImage.Height) sy = baseClipboardImage.Height - 1;
                    for (int x = 0; x < zoomedWidth; x++)
                    {
                        int sx = x / Globals.Zoom;
                        if (sx >= baseClipboardImage.Width) sx = baseClipboardImage.Width - 1;
                        dstPtr[y * dstData.Stride + x] = srcPtr[sy * srcData.Stride + sx];
                    }
                }
            }
            baseClipboardImage.UnlockBits(srcData);
            newClipboardImage.UnlockBits(dstData);
            baseClipboardImage.Dispose();
            
            // If SkipZero: convert 8bpp→32bpp with exact palette (index → Color), no matching
            if (AtariClipboard.SkipZero)
            {
                Color[] palette = AtariPalette.GetIndexedColor5Palette().Entries;
                Bitmap temp32Bit = new Bitmap(zoomedWidth, zoomedHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData srcData8 = newClipboardImage.LockBits(
                    new Rectangle(0, 0, zoomedWidth, zoomedHeight),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                BitmapData dstData32 = temp32Bit.LockBits(
                    new Rectangle(0, 0, zoomedWidth, zoomedHeight),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* srcPtr = (byte*)srcData8.Scan0;
                    int* dstPtr = (int*)dstData32.Scan0;
                    for (int y = 0; y < zoomedHeight; y++)
                    {
                        for (int x = 0; x < zoomedWidth; x++)
                        {
                            byte idx = srcPtr[y * srcData8.Stride + x];
                            if (idx >= palette.Length) idx = 0;
                            dstPtr[y * (dstData32.Stride / 4) + x] = palette[idx].ToArgb();
                        }
                    }
                }
                newClipboardImage.UnlockBits(srcData8);
                temp32Bit.UnlockBits(dstData32);
                newClipboardImage.Dispose();
                newClipboardImage = temp32Bit;
            }
            
            // Replace old clipboard image
            if (AtariClipboard.ClipboardImage != null)
                AtariClipboard.ClipboardImage.Dispose();
            AtariClipboard.ClipboardImage = newClipboardImage;

            // Keep UnderClipBoardImage in sync with ClipboardImage size (fixes trails when clipboard from element library)
            if (AtariClipboard.UnderClipBoardImage != null)
                AtariClipboard.UnderClipBoardImage.Dispose();
            AtariClipboard.UnderClipBoardImage = new Bitmap(AtariClipboard.ClipboardImage);
            if (AtariClipboard.UnderImageGraphics != null)
                AtariClipboard.UnderImageGraphics.Dispose();
            AtariClipboard.UnderImageGraphics = Graphics.FromImage(AtariClipboard.UnderClipBoardImage);

            // Update UI
            pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
            pictureBoxClipboard.Refresh();
        }
        
        /// <summary>
        /// Updates the enabled state of the inverse button based on map type
        /// </summary>
        private void UpdateClipboardInverseButtonState()
        {
            if (buttonClipboardInverse != null)
            {
                // Disable in tile mode, enable in font mode
                buttonClipboardInverse.Enabled = !(myMap != null && myMap.IsTilemap);
            }
        }

        private void ButtonReplaceCurrentScreen_Click(object sender, EventArgs e)
        {
            if (numericUpDownReplace1.Value != numericUpDownReplace2.Value)
            {
                myMap.SwapChar((byte)numericUpDownReplace1.Value, (byte)numericUpDownReplace2.Value, radioButtonWholeMap.Checked, new Point(currentScreen.X,currentScreen.Y));
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }

        private void ButtonAddScreenRowToMap_Click(object sender, EventArgs e)
        {
            AtariMap newMap = myMap.ExtendWithNewScreenRow();
            myMap = newMap;
            undoManager = new UndoManager(myMap);
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            dliForm?.Dispose();
            dliForm = new DliForm(myMap, pictureBoxMap);
            dliForm.RenderData();
            if (myCharPicker != null)
                myCharPicker.RedrawFontWindow();
            RedrawEditorWindow();
            numericUpDownScreenFromY.Maximum = myMap.MapSize.Height - 1;
            numericUpDownScreenToY.Maximum = myMap.MapSize.Height - 1;
        }

        private void ToolStripMenuItemClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Clear Screen {currentScreen.X}:{currentScreen.Y}?", "Confirmation popup", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                myMap.ClearScreen(currentScreen);
                RedrawEditorWindow();
            }
        }

        private void ToolStripMenuItemHFlip_Click(object sender, EventArgs e)
        {
            myMap.FlipScreen(currentScreen, true);
            RedrawEditorWindow();
        }

        private void ToolStripMenuItemVFlip_Click(object sender, EventArgs e)
        {
            myMap.FlipScreen(currentScreen, false);
            RedrawEditorWindow();
        }

        private void ShowScreenSelection(bool redraw = true)
        {
            if (redraw)
                RedrawEditorWindow();
            // Use character units per screen (for tilemap, ScreenSize is in tiles so multiply by tile size)
            int screenCharWidth = myMap.ScreenSize.Width;
            int screenCharHeight = myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            }
            int left = (int)numericUpDownScreenFromX.Value * screenCharWidth;
            int top = (int)numericUpDownScreenFromY.Value * screenCharHeight;
            int width = (int)(numericUpDownScreenToX.Value - numericUpDownScreenFromX.Value + 1) * screenCharWidth * Globals.CharSize;
            int height = (int)(numericUpDownScreenToY.Value - numericUpDownScreenFromY.Value + 1) * screenCharHeight * Globals.CharSize;
            Graphics g = Graphics.FromImage(pictureBoxMap.Image);
            Brush b = new HatchBrush(HatchStyle.Percent80, Color.FromArgb(96, Color.GreenYellow));
            g.FillRectangle(b, (left - myMap.OffsetX) * Globals.CharSize, (top - myMap.OffsetY) * Globals.CharSize, width, height);
            pictureBoxMap.Refresh();
            this.ScreenSelectionShown = true;
        }

        private void CheckBoxShowScreenSelection_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowScreenSelection.Checked)
            {
                ShowScreenSelection();
            }
            else
            {
                ScreenSelectionShown = false;
                RedrawEditorWindow();
            }
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2 && checkBoxShowScreenSelection.Checked && !ScreenSelectionShown)
            {
                ShowScreenSelection();
            } 
            if (tabControl1.SelectedTab != tabPage2 && ScreenSelectionShown)
            {
                ScreenSelectionShown = false;
                RedrawEditorWindow();
            }
        }

        private void NumericUpDownScreenSelection_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxShowScreenSelection.Checked)
            {
                ShowScreenSelection();
            }
        }

        private void labelPosition_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            flowLayoutPanel1.Refresh();
            flowLayoutPanel1.Invalidate();
        }
    }
}