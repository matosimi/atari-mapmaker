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
        private bool suppressFontMappingUiEvents = false;  // Avoid write-back when syncing font-ref controls from current screen
        private bool isContinuousPasteMode = false;  // Track if CTRL+Left mouse is held for continuous paste
        private Point? lastContinuousPasteCell = null;  // Track last grid cell where we pasted in continuous mode
        // Metadata paste-mode overlay: show copied metadata cell under cursor
        private Bitmap metadataUnderImage = null;
        private Bitmap metadataPreviewImage = null;
        private Point? previousMetadataOverlayLocation = null;
        private string mainFormBaseTitle = "";
        private string currentAtrmapPath = null;
        private string currentDataPath = null;  // last export/import path (separate from atrmap save/load)
        private Point lastMapMouseLocation = Point.Empty;
        /// <summary>When Autopaste is off, Ctrl+V arms one floating-paste session until the user clicks to place it.</summary>
        private bool floatingPasteArmed = false;

        // Free charmap: panelCharsetColors filled in InitFreeCharmapUi from designer panels
        private Panel[] panelCharsetColors;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            mainFormBaseTitle = "AtariMapMaker v" + version + " by Martin Simecek";
            UpdateMainFormCaption();
            labelAbout2.Text = "Version " + version + "\nCommit " + GitCommitInfo.CommitHash + "\n" + GitCommitInfo.CommitDate;
            toolTip1.SetToolTip(buttonRefreshFont, "Reload font");

            AtariPalette.Load(Properties.Resources.altirraPAL);
            colorPickerForm = new AtariColorPicker();
            AtariFontRenderer.SetFontData(Properties.Resources.Default, Globals.FontType.Screen);

            myMap = new AtariMap(new Size(4, 4), new Size(32, 20));
            undoManager = new UndoManager(myMap);
            numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
            UpdateDatalineWidthDefault();

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

            dliForm = CreateDliForm();
            dliForm.RenderData();

            // Set initial state for V2 UI (controls are in Designer)
            checkBoxMultiFont.Checked = myMap.MultiFontEnabled;
            Globals.MetadataLayerShowText = checkBoxMetadataShowText.Checked;
            Globals.MetadataLayerShowColorLinks = checkBoxMetaDataShowColorLinks != null && checkBoxMetaDataShowColorLinks.Checked;
            Globals.MetadataLayerShowValueLinks = checkBoxMetaDataShowValueLinks != null && checkBoxMetaDataShowValueLinks.Checked;
            UpdateFontMappingReferenceUI();
            UpdateMultiFontUI();
            UpdateMetadataLayerUI();
            InitFreeCharmapUi();
            UpdateFreeCharmapUi();

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
            UpdateUndoRedoUI();
            if (checkBoxAutopaste != null)
                toolTip1.SetToolTip(checkBoxAutopaste, "On: select copies clipboard, click/Ctrl+V pastes. Off: Ctrl+C copies, Ctrl+V arms floating paste, click places once.");
        }

        private UndoManager undoManager;

        private void MenuItemUndo_Click(object sender, EventArgs e)
        {
            PerformUndo();
        }

        private void MenuItemRedo_Click(object sender, EventArgs e)
        {
            PerformRedo();
        }

        private void ButtonUndo_Click(object sender, EventArgs e)
        {
            PerformUndo();
        }

        private void ButtonRedo_Click(object sender, EventArgs e)
        {
            PerformRedo();
        }

        private void PerformUndo()
        {
            if (undoManager == null || !undoManager.CanUndo())
                return;

            undoManager.Undo();
            AfterMapDataUndoRedo();
        }

        private void PerformRedo()
        {
            if (undoManager == null || !undoManager.CanRedo())
                return;

            undoManager.Redo();
            AfterMapDataUndoRedo();
        }

        private void AfterMapDataUndoRedo()
        {
            if (myMap != null && myMap.IsTilemap)
                AtariFontRenderer.ClearFontCache();

            RedrawEditorWindow();

            if (IsFloatingPasteActive && AtariClipboard.IsValid && AtariPictureTools.PreviousClipboardLocation.HasValue)
            {
                AtariPictureTools.DrawClipBoard(AtariPictureTools.PreviousClipboardLocation.Value, (Bitmap)pictureBoxMap.Image);
                pictureBoxUnderClipBoard.Image = AtariClipboard.UnderClipBoardImage;
                pictureBoxUnderClipBoard.Refresh();
            }

            pictureBoxMap.Refresh();
            UpdateUndoRedoUI();
        }

        private void UpdateUndoRedoUI()
        {
            bool canUndo = undoManager != null && undoManager.CanUndo();
            bool canRedo = undoManager != null && undoManager.CanRedo();

            if (buttonUndo != null)
                buttonUndo.Enabled = canUndo;
            if (buttonRedo != null)
                buttonRedo.Enabled = canRedo;
            if (menuItemUndo != null)
            {
                menuItemUndo.Enabled = canUndo;
                string undoDesc = canUndo ? undoManager.GetUndoDescription() : "";
                menuItemUndo.Text = string.IsNullOrEmpty(undoDesc) ? "Undo" : "Undo " + undoDesc;
            }
            if (menuItemRedo != null)
            {
                menuItemRedo.Enabled = canRedo;
                string redoDesc = canRedo ? undoManager.GetRedoDescription() : "";
                menuItemRedo.Text = string.IsNullOrEmpty(redoDesc) ? "Redo" : "Redo " + redoDesc;
            }
            if (toolTip1 != null)
            {
                if (buttonUndo != null)
                    toolTip1.SetToolTip(buttonUndo, canUndo ? "Undo " + undoManager.GetUndoDescription() + " (Ctrl+Z)" : "Nothing to undo (Ctrl+Z)");
                if (buttonRedo != null)
                    toolTip1.SetToolTip(buttonRedo, canRedo ? "Redo " + undoManager.GetRedoDescription() + " (Ctrl+Y)" : "Nothing to redo (Ctrl+Y)");
            }
        }

        /// <summary>
        /// Runs a paste action and records a map-data undo step if anything changed.
        /// </summary>
        private void PasteWithUndo(Action pasteAction)
        {
            if (myMap?.Data == null || pasteAction == null)
                return;

            byte[] before = (byte[])myMap.Data.Clone();
            pasteAction();
            var operation = MapDataRegionOperation.CreateFromDiff(before, myMap.Data, "Paste");
            if (operation != null && undoManager != null)
                undoManager.PushOperation(operation);
            UpdateUndoRedoUI();
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
            bool show = checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked;
            Globals.MetadataLayerVisible = show;
            if (!show && checkBoxMetadataEdit != null && checkBoxMetadataEdit.Checked)
                checkBoxMetadataEdit.Checked = false;
            UpdateMetadataLayerUI();
            RedrawEditorWindow();
            pictureBoxMap.Refresh();
        }

        private void CheckBoxMetadataEdit_CheckedChanged(object sender, EventArgs e)
        {
            bool edit = checkBoxMetadataEdit != null && checkBoxMetadataEdit.Checked;
            if (edit && checkBoxMetadataLayer != null && !checkBoxMetadataLayer.Checked)
                checkBoxMetadataLayer.Checked = true;
            Globals.MetadataLayerEditable = edit;
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
            bool showChecked = checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked;
            bool editChecked = checkBoxMetadataEdit != null && checkBoxMetadataEdit.Checked;
            if (groupBoxDli != null)
                groupBoxDli.Enabled = !editChecked;
            if (groupBoxFont != null)
                groupBoxFont.Enabled = !editChecked;
            if (buttonMassChangeMetadata != null)
                buttonMassChangeMetadata.Enabled = editChecked;
            if (checkBoxMetaDataShowColorLinks != null)
                checkBoxMetaDataShowColorLinks.Enabled = showChecked;
            if (checkBoxMetaDataShowValueLinks != null)
                checkBoxMetaDataShowValueLinks.Enabled = showChecked;
            if (trackBarMetadataBlend != null)
                trackBarMetadataBlend.Enabled = showChecked;
            if (labelMetadataBlend != null)
                labelMetadataBlend.Enabled = showChecked;
            if (checkBoxMetadataShowText != null)
                checkBoxMetadataShowText.Enabled = showChecked;
            if (showChecked)
            {
                Globals.MetadataLayerShowColorLinks = checkBoxMetaDataShowColorLinks != null && checkBoxMetaDataShowColorLinks.Checked;
                Globals.MetadataLayerShowValueLinks = checkBoxMetaDataShowValueLinks != null && checkBoxMetaDataShowValueLinks.Checked;
                if (trackBarMetadataBlend != null)
                    Globals.MetadataLayerBlendPercent = trackBarMetadataBlend.Value * 5;
            }
            Globals.MetadataLayerEditable = editChecked;
            Globals.MetadataLayerVisible = showChecked;
        }

        private void CheckBoxMetadataShowText_CheckedChanged(object sender, EventArgs e)
        {
            Globals.MetadataLayerShowText = checkBoxMetadataShowText != null && checkBoxMetadataShowText.Checked;
            RedrawEditorWindow();
            pictureBoxMap.Refresh();
        }

        private void TrackBarMetadataBlend_Scroll(object sender, EventArgs e)
        {
            int pct = trackBarMetadataBlend.Value * 5;
            Globals.MetadataLayerBlendPercent = pct;
            if (labelMetadataBlend != null)
                labelMetadataBlend.Text = $"Map ↔ Meta: {pct}%";
            if (checkBoxMetadataLayer != null && checkBoxMetadataLayer.Checked)
            {
                RedrawEditorWindow();
                pictureBoxMap.Refresh();
            }
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
            Point targetScreen = isScreenLocked ? lockedScreen : currentScreen;
            bool hasLink = myMap?.ScreenLinks != null &&
                myMap.ScreenLinks.Exists(l => l.SourceScreen.X == targetScreen.X && l.SourceScreen.Y == targetScreen.Y);
            bool hasMeta = myMap != null && myMap.ScreenHasMetadata(targetScreen.X, targetScreen.Y);
            bool hasDesc = false;
            if (myMap?.ScreenDescriptions != null)
            {
                string key = $"{targetScreen.X},{targetScreen.Y}";
                if (myMap.ScreenDescriptions.TryGetValue(key, out string desc))
                    hasDesc = !string.IsNullOrWhiteSpace(desc);
            }

            if (menuItemLinkScreen != null)
            {
                menuItemLinkScreen.Checked = hasLink;
                menuItemLinkScreen.Text = "Link to Screen...";
            }
            if (menuItemScreenDescription != null)
            {
                menuItemScreenDescription.Checked = hasDesc;
                menuItemScreenDescription.Text = "Screen Description...";
            }
            if (menuItemScreenMetadata != null)
            {
                menuItemScreenMetadata.Checked = hasMeta;
                menuItemScreenMetadata.Text = "Screen Metadata...";
            }
            if (menuItemExportMetadata != null)
            {
                menuItemExportMetadata.Checked = hasMeta;
                menuItemExportMetadata.Text = "Export metadata...";
            }

            // Update enabled state of Apply Font Template menu item
            foreach (ToolStripItem item in contextMenuStripScreen.Items)
            {
                if (item.Text == "Apply Font Template..." || item.Text.EndsWith("Apply Font Template..."))
                {
                    bool enabled = myMap != null && myMap.MultiFontEnabled && !myMap.FreeCharmapMode;
                    if (enabled)
                    {
                        int refScreenX, refScreenY;
                        bool isReferencing = myMap.GetFontMappingReference(targetScreen.X, targetScreen.Y, out refScreenX, out refScreenY);
                        if (isReferencing && (refScreenX != targetScreen.X || refScreenY != targetScreen.Y))
                            enabled = false;
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
            myCharPicker.NotifyScreenChanged(isScreenLocked ? lockedScreen : currentScreen);
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

                if (AtariFontRenderer.Color5.Length > 8)
                {
                    lviAlter = new ListViewItem
                    {
                        ImageIndex = 8,
                        Text = "PF1 alter"
                    };
                    lviAlter.SubItems.Add("$" + String.Format("{0:X2}", AtariFontRenderer.Color5[8]));
                    listViewColors.Items.Add(lviAlter);
                }

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

        private DliForm CreateDliForm()
        {
            var form = new DliForm(myMap, pictureBoxMap);
            form.OnGlobalColorsChanged = () =>
            {
                FillFontColorList();
                AtariFontRenderer.RedrawFontImage(Globals.FontType.Screen);
                AtariPictureTools.Redraw(Globals.WindowType.CharPicker);
                if (myCharPicker != null)
                {
                    myCharPicker.Refresh();
                    myCharPicker.GetPictureBox()?.Refresh();
                }
                if (tilePicker != null && tilePicker.Visible)
                    tilePicker.Refresh();
            };
            return form;
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewColors.SelectedItems.Count == 1)
            {
                int colorIndex = listViewColors.SelectedItems[0].Index;
                // Dual pick for PF0–PF3 when ALPA is on (not for BAK or dedicated alter rows)
                bool dualPf = AtariFontRenderer.Color5.Length > 5 && colorIndex < 4;
                colorPickerForm.Owner = this;
                if (dualPf)
                {
                    byte primary = AtariFontRenderer.Color5[colorIndex];
                    byte alternate = AtariFontRenderer.GetAlpaAlternateColor(colorIndex);
                    colorPickerForm.Pick(primary, alternate);
                    if (colorPickerForm.PickedNewColor)
                    {
                        if (colorPickerForm.PickedPrimaryChanged)
                            AtariFontRenderer.Color5[colorIndex] = colorPickerForm.PickedColorIndex;
                        if (colorPickerForm.PickedAlternateChanged)
                        {
                            AtariFontRenderer.NormalizeAlpaColors();
                            int altIndex = AtariFontRenderer.GetAlpaAlternateColorIndex(colorIndex);
                            if (altIndex >= 0 && altIndex < AtariFontRenderer.Color5.Length)
                                AtariFontRenderer.Color5[altIndex] = colorPickerForm.PickedAlternateIndex;
                        }
                    }
                }
                else
                {
                    byte index = AtariFontRenderer.Color5[colorIndex];
                    colorPickerForm.Pick(index);
                    AtariFontRenderer.Color5[colorIndex] = colorPickerForm.PickedColorIndex;
                }

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
            }
        }

        private void PictureBoxMap_MouseMove(object sender, MouseEventArgs e)
        {
            lastMapMouseLocation = e.Location;
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
                // Screen not locked: if Edit DLI form is open, keep selection on that screen
                // (hovering elsewhere must not change DLI content or trigger font-ref UI write-back)
                if (checkBoxEditDli.Checked && dliForm != null && dliForm.Visible)
                {
                    int dliScreenNumber = dliForm.screenNumber;
                    currentScreen.X = dliScreenNumber % myMap.MapSize.Width;
                    currentScreen.Y = dliScreenNumber / myMap.MapSize.Width;
                }
                else
                {
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
                if (myCharPicker != null && myCharPicker.Visible)
                    myCharPicker.NotifyScreenChanged(currentScreen);
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

                if (checkBoxEditDli.Checked && dliForm != null && dliForm.Visible)
                {
                    // Reposition only — keep showing the screen selected by right-click (or lock)
                    int dliScrX = isScreenLocked ? lockedScreen.X : (dliForm.screenNumber % myMap.MapSize.Width);
                    int dliScrY = isScreenLocked ? lockedScreen.Y : (dliForm.screenNumber / myMap.MapSize.Width);
                    UpdateAndShowDliForm(dliScrX, dliScrY, true);
                }
            }


            if (e.Button == MouseButtons.Left)      //SELECT or CONTINUOUS PASTE
            {
                if (mouseStatus == "SELECTION")
                {
                    AtariPictureTools.SelectionChange(e.Location, Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                }
                else if (IsAutopasteEnabled && AtariClipboard.IsValid && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    // Continuous paste mode: CTRL + Left mouse button (autopaste only)
                    isContinuousPasteMode = true;
                    PerformPasteAtLocation(e.Location);
                }
            }

            if (e.Button == MouseButtons.None)  //nothing is pressed
            {
                lastMapMouseLocation = e.Location;

                // Check for continuous paste mode on mouse move (CTRL still held)
                if (IsAutopasteEnabled && isContinuousPasteMode && (Control.ModifierKeys & Keys.Control) == Keys.Control && AtariClipboard.IsValid)
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

              
              
                if (IsFloatingPasteActive)  //copy mode (shows alpha blended clipBoard)
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

                if (myMap.MultiFontEnabled || myMap.FreeCharmapMode)
                {
                    byte cs = myMap.GetFontForChar(xx, yy);
                    toolStripStatusLabel2.Text += $" | CS: {cs}";
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
                int newScreenNumber = scrx + scry * myMap.MapSize.Width;
                if (!justUpdatePosition)
                {
                    myMap.CopyDliColorsFullScreen(newScreenNumber, dliForm.DliMap, 0);  //copy screen colors to DLI color editor
                    dliForm.Show(newScreenNumber); // update font column before paint
                    dliForm.RenderData();
                }
                else
                {
                    // Position only — content stays on the already-selected screen
                    dliForm.RenderData(true);
                }
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
            lastMapMouseLocation = e.Location;
            if (e.Button == MouseButtons.Left)
            {

                if (IsFloatingPasteActive)
                {
                    // Autopaste: stay in paste mode. Manual (Ctrl+V armed): place once, then back to selection.
                    PasteClipboardAtLocationAndRedraw(e.Location, exitPasteModeAfter: floatingPasteArmed && !IsAutopasteEnabled);
                }
                else if (Globals.MetadataLayerEditable)
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
                        if (scrx < 0) scrx = 0;
                        if (scry < 0) scry = 0;
                        if (scrx >= myMap.MapSize.Width) scrx = myMap.MapSize.Width - 1;
                        if (scry >= myMap.MapSize.Height) scry = myMap.MapSize.Height - 1;
                        currentScreen.X = scrx;
                        currentScreen.Y = scry;
                        previousScreen = currentScreen;
                        UpdateFontMappingReferenceUI();
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
                    bool copyToClipboard = IsAutopasteEnabled;
                    if (AtariPictureTools.SelectionEnd(Globals.WindowType.Editor, copyToClipboard))
                    {
                        if (copyToClipboard)
                        {
                            AtariClipboard.IsValid = true;
                            pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                        }
                        else
                        {
                            // No-autopaste: keep selection visible until Ctrl+C (or a new selection).
                            AtariPictureTools.RedrawSelection(Globals.WindowType.Editor);
                        }
                        pictureBoxMap.Refresh();
                    }
                    mouseStatus = "";   //reset mouse status no matter if selection end is valid or not
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                contextMenuStripScreen.Show(pictureBoxMap, e.Location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Scroll redraws without hover labels; refresh them for the screen under the cursor
                RedrawEditorWindow();
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
                floatingPasteArmed = false;
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
            }
        }
        
        /// <summary>
        /// Pastes clipboard at the given picture-box location and refreshes the editor view.
        /// </summary>
        private void PasteClipboardAtLocationAndRedraw(Point location, bool exitPasteModeAfter = false)
        {
            if (!AtariClipboard.IsValid)
                return;

            int charX = location.X / Globals.CharSize;
            int charY = location.Y / Globals.CharSize;
            PasteClipboardAtCharCoords(charX, charY);

            if (myMap.IsTilemap && AtariClipboard.IsTileIndexes)
                AtariFontRenderer.ClearFontCache();

            AtariPictureTools.Redraw(Globals.WindowType.Editor, true, comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked, currentScreen, isScreenLocked, lockedScreen);

            if (exitPasteModeAfter)
            {
                // One-shot floating paste (Autopaste off): do not restore under-image (it would hide the paste).
                DisarmFloatingPaste(keepClipboardData: true, restoreUnderImage: false);
            }
            else if (AtariPictureTools.PreviousClipboardLocation.HasValue)
            {
                AtariPictureTools.DrawClipBoard(AtariPictureTools.PreviousClipboardLocation.Value, (Bitmap)pictureBoxMap.Image);
                pictureBoxUnderClipBoard.Image = AtariClipboard.UnderClipBoardImage;
                pictureBoxUnderClipBoard.Refresh();
            }

            pictureBoxMap.Refresh();
        }

        /// <summary>
        /// Shows the floating clipboard overlay at the given picture-box location.
        /// </summary>
        private void ShowFloatingClipboardAt(Point location)
        {
            if (!AtariClipboard.IsValid || pictureBoxMap?.Image == null)
                return;

            if (AtariPictureTools.PreviousClipboardLocation.HasValue)
                AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);

            AtariPictureTools.DrawClipBoard(location, (Bitmap)pictureBoxMap.Image);
            pictureBoxUnderClipBoard.Image = AtariClipboard.UnderClipBoardImage;
            pictureBoxUnderClipBoard.Refresh();

            AtariPictureTools.PreviousClipboardLocation = location;

            int alignSizeX = Globals.CharSize;
            int alignSizeY = Globals.CharSize;
            if (myMap != null && myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                alignSizeX = myMap.TilemapInfo.TileWidth * Globals.CharSize;
                alignSizeY = myMap.TilemapInfo.TileHeight * Globals.CharSize;
            }
            AtariPictureTools.PreviousClipboardGridCell = new Point(location.X / alignSizeX, location.Y / alignSizeY);
            pictureBoxMap.Refresh();
        }

        /// <summary>
        /// Ends floating paste preview. If keepClipboardData, clipboard stays usable for another Ctrl+V.
        /// </summary>
        private void DisarmFloatingPaste(bool keepClipboardData, bool restoreUnderImage = true)
        {
            if (restoreUnderImage && AtariPictureTools.PreviousClipboardLocation.HasValue)
                AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);

            AtariPictureTools.PreviousClipboardLocation = null;
            AtariPictureTools.PreviousClipboardGridCell = null;
            isContinuousPasteMode = false;
            lastContinuousPasteCell = null;
            floatingPasteArmed = false;

            if (!keepClipboardData)
            {
                AtariClipboard.IsValid = false;
                if (pictureBoxClipboard != null)
                {
                    pictureBoxClipboard.Image = null;
                    pictureBoxClipboard.Refresh();
                }
            }

            pictureBoxMap.Refresh();
        }

        /// <summary>
        /// Leaves floating-clipboard paste mode and returns to selection mode.
        /// </summary>
        private void ExitClipboardPasteMode(bool keepClipboardPreview)
        {
            DisarmFloatingPaste(keepClipboardData: keepClipboardPreview);
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
            
            int charX = location.X / Globals.CharSize;
            int charY = location.Y / Globals.CharSize;
            PasteClipboardAtCharCoords(charX, charY);
            
            // Redraw after paste
            if (myMap.IsTilemap && AtariClipboard.IsTileIndexes)
            {
                AtariFontRenderer.ClearFontCache();
            }
            AtariPictureTools.Redraw(Globals.WindowType.Editor, true, comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked, currentScreen, isScreenLocked, lockedScreen);
            pictureBoxMap.Refresh();
        }

        /// <summary>
        /// Pastes clipboard at character coordinates relative to the visible editor area, with undo recording.
        /// </summary>
        private void PasteClipboardAtCharCoords(int charX, int charY)
        {
            AtariClipboard.SetDataSource(myMap);

            PasteWithUndo(() =>
            {
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
            });
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

        private void RememberAtrmapPath(string fileName, bool seedDataPath)
        {
            currentAtrmapPath = fileName;
            if (seedDataPath && !string.IsNullOrEmpty(fileName))
            {
                string dir = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (!string.IsNullOrEmpty(dir))
                    currentDataPath = Path.Combine(dir, baseName + ".dat");
            }
            UpdateMainFormCaption();
        }

        private void RememberDataPath(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
                currentDataPath = fileName;
        }

        private void PrepareAtrmapSaveDialog()
        {
            saveFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            ApplyRememberedPath(saveFileDialog1, currentAtrmapPath, null);
        }

        private void PrepareAtrmapOpenDialog()
        {
            openFileDialog1.Filter = "Atari MapMaker map (*.atrmap)|*.atrmap";
            ApplyRememberedPath(openFileDialog1, currentAtrmapPath, null);
        }

        private void PrepareDataSaveDialog(string filter)
        {
            saveFileDialog1.Filter = filter;
            string fallback = null;
            if (string.IsNullOrEmpty(currentDataPath) && !string.IsNullOrEmpty(currentAtrmapPath))
                fallback = Path.Combine(Path.GetDirectoryName(currentAtrmapPath) ?? "", Path.GetFileNameWithoutExtension(currentAtrmapPath) + ".dat");
            ApplyRememberedPath(saveFileDialog1, currentDataPath, fallback);
        }

        private void PrepareDataOpenDialog(string filter)
        {
            openFileDialog1.Filter = filter;
            string fallback = null;
            if (string.IsNullOrEmpty(currentDataPath) && !string.IsNullOrEmpty(currentAtrmapPath))
                fallback = Path.Combine(Path.GetDirectoryName(currentAtrmapPath) ?? "", Path.GetFileNameWithoutExtension(currentAtrmapPath) + ".dat");
            ApplyRememberedPath(openFileDialog1, currentDataPath, fallback);
        }

        private static void ApplyRememberedPath(FileDialog dialog, string primaryPath, string fallbackPath)
        {
            string path = !string.IsNullOrEmpty(primaryPath) ? primaryPath : fallbackPath;
            if (string.IsNullOrEmpty(path))
                return;
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    dialog.InitialDirectory = dir;
                string name = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(name))
                    dialog.FileName = name;
            }
            catch
            {
            }
        }

        private bool IsAutopasteEnabled
        {
            get { return checkBoxAutopaste == null || checkBoxAutopaste.Checked; }
        }

        /// <summary>True when floating clipboard paste preview/click-to-place is active.</summary>
        private bool IsFloatingPasteActive
        {
            get { return AtariClipboard.IsValid && (IsAutopasteEnabled || floatingPasteArmed); }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            PrepareAtrmapSaveDialog();
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    SaveMap(saveFileDialog1.FileName);
                    break;
            }
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
                FreeCharmapMode = myMap.FreeCharmapMode,
                CharFontData = myMap.CharFontData?.Select(i => (int)i).ToArray(),
                CharsetColors = myMap.CharsetColors?.Select(i => (int)i).ToArray(),
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
            RememberAtrmapPath(filename, seedDataPath: false);
        }

        private void UpdateMainFormCaption()
        {
            string caption = string.IsNullOrEmpty(mainFormBaseTitle)
                ? "AtariMapMaker"
                : mainFormBaseTitle;

            if (!string.IsNullOrEmpty(currentAtrmapPath))
                caption += " - " + Path.GetFileName(currentAtrmapPath);

            string descFirstLine = GetMapDescriptionFirstLine();
            if (!string.IsNullOrEmpty(descFirstLine))
                caption += " - " + descFirstLine;

            this.Text = caption;
        }

        private string GetMapDescriptionFirstLine()
        {
            if (myMap == null || string.IsNullOrWhiteSpace(myMap.MapDescription))
                return null;
            string desc = myMap.MapDescription;
            int nl = desc.IndexOfAny(new[] { '\r', '\n' });
            if (nl >= 0)
                desc = desc.Substring(0, nl);
            desc = desc.Trim();
            return string.IsNullOrEmpty(desc) ? null : desc;
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            PrepareAtrmapOpenDialog();
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    LoadMap(openFileDialog1.FileName);
                    
                    AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
                    AtariPictureTools.SetGridVisibility(comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked);
                  
                    checkBoxShowDli.Checked = true;
                    checkBoxAlpa.Checked = AtariFontRenderer.Color5.Length > 5;
                    UpdateDliMaskControlsVisibility();
                    this.FillFontColorList();
                    
                    // Update numeric up/down controls for new map size
                    numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                    UpdateDatalineWidthDefault();
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
                    UpdateFreeCharmapUi();
                    
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
                    {
                        myCharPicker.SetMainMap(myMap);
                        myCharPicker.RedrawFontWindow();
                    }
                    dliForm.Dispose();
                    dliForm = CreateDliForm();
                    dliForm.RenderData();
                    dliForm.ZoomResize();
                    numericUpDownScreenFromX.Maximum = myMap.MapSize.Width;
                    numericUpDownScreenFromY.Maximum = myMap.MapSize.Height;
                    numericUpDownScreenToX.Maximum = myMap.MapSize.Width;
                    numericUpDownScreenToY.Maximum = myMap.MapSize.Height;
                    numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                    UpdateDatalineWidthDefault();
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
            floatingPasteArmed = false;
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
            UpdateUndoRedoUI();
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

            // Drop multifont bitmaps and char-picker refs from the previously loaded map
            AtariFontRenderer.ClearFontCache();
            AtariFontRenderer.CharPickerFontIndex = null;
            AtariFontRenderer.CharPickerFontSourceMap = null;
            
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

            if (AtariJson.ParsedData.CharsetColors != null && AtariJson.ParsedData.CharsetColors.Length > 0)
            {
                myMap.CharsetColors = AtariMap.CreateDefaultCharsetColors();
                for (int i = 0; i < 8 && i < AtariJson.ParsedData.CharsetColors.Length; i++)
                    myMap.CharsetColors[i] = (byte)AtariJson.ParsedData.CharsetColors[i];
            }
            else
            {
                myMap.EnsureCharsetColors();
            }

            if (AtariJson.ParsedData.FreeCharmapMode.HasValue && AtariJson.ParsedData.FreeCharmapMode.Value
                && !myMap.IsTilemap)
            {
                myMap.EnsureCharFontData();
                if (AtariJson.ParsedData.CharFontData != null)
                {
                    int n = Math.Min(myMap.CharFontData.Length, AtariJson.ParsedData.CharFontData.Length);
                    for (int i = 0; i < n; i++)
                        myMap.CharFontData[i] = (byte)(AtariJson.ParsedData.CharFontData[i] & 0x07);
                }
                myMap.FreeCharmapMode = true;
                if (!myMap.MultiFontEnabled)
                    myMap.MultiFontEnabled = true;
            }
            else
            {
                myMap.FreeCharmapMode = false;
                myMap.CharFontData = null;
            }
            
            // Load DLI data
            if (AtariJson.ParsedData.DliData == null)
                myMap.InitDliColorFullMap();
            else
            {
                myMap.ColorData = AtariJson.ParsedData.DliData.Select(i => (byte)i).ToArray();
                myMap.EnsureDliColorDataLayout(AtariFontRenderer.Color5);
            }

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

            RememberAtrmapPath(fileName, seedDataPath: true);
        }

        private void MapdataExport()
        {
            PrepareDataSaveDialog("MapData export (*.dat)|*.dat");
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    RememberDataPath(saveFileDialog1.FileName);
                    this.Export((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, (int)numericUpDown5.Value, saveFileDialog1.FileName);
                    int width = (int)((numericUpDownScreenToX.Value - numericUpDownScreenFromX.Value + 1) * myMap.ScreenSize.Width + numericUpDown5.Value);
                    string unit = myMap.IsTilemap ? "tiles" : "characters";
                    MessageBox.Show($"Export dataline width: {width} {unit}");
                    break;
            }
        }

        private bool UseCharsetDataOperation()
        {
            return checkBoxCharsetDataOp != null && checkBoxCharsetDataOp.Checked
                && myMap != null && myMap.FreeCharmapMode && !myMap.IsTilemap;
        }

        private byte[] GetDataOperationArray()
        {
            if (UseCharsetDataOperation())
            {
                myMap.EnsureCharFontData();
                return myMap.CharFontData;
            }
            return myMap.Data;
        }

        private void WriteDataOperationByte(byte[] target, int index, byte value)
        {
            if (index < 0 || index >= target.Length)
                return;
            if (UseCharsetDataOperation())
                target[index] = (byte)(value & 0x07);
            else
                target[index] = value;
        }

        private void Export(int x1, int y1, int x2, int y2, int extraCharsOnLine, string filename)
        {
            // For tilemaps, ScreenSize is in tiles; for normal maps, it's in characters
            int xs = x1 * myMap.ScreenSize.Width;
            int ys = y1 * myMap.ScreenSize.Height;
            int xf = (x2 + 1) * myMap.ScreenSize.Width;
            int yf = (y2 + 1) * myMap.ScreenSize.Height;
            byte[] src = GetDataOperationArray();
            int stride = myMap.Stride;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);


            byte myData;
            for (int y = ys; y < yf; y++)
                for (int x = xs; x < xf + extraCharsOnLine; x++)
                {

                    if (x < xf)
                        myData = src[x + y * stride];
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
            byte[] src = GetDataOperationArray();
            int stride = myMap.Stride;

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
                                myData = src[x + y * stride];
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
            PrepareDataSaveDialog("MapData export (*.dat)|*.dat");
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    RememberDataPath(saveFileDialog1.FileName);
                    this.ExportScreens((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, saveFileDialog1.FileName);
                    int width = myMap.ScreenSize.Width;
                    string unit = myMap.IsTilemap ? "tiles" : "characters";
                    MessageBox.Show($"Export dataline width: {width} {unit}");
                    break;
            }
        }

        private void ImportScreenByScreen()
        {
            PrepareDataOpenDialog("Map datafile (*.*)|*.*");
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    RememberDataPath(openFileDialog1.FileName);
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
            byte[] src = GetDataOperationArray();
            int stride = myMap.Stride;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);


            byte myData;

            for (int x = xs; x < xf; x++)
                for (int y = ys; y < yf; y++)
                {


                    myData = src[x + y * stride];

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
            byte[] dest = GetDataOperationArray();
            int stride = myMap.Stride;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);


            byte myData;

            int x = xs;
            while (fs.Position <= (fs.Length - myMap.ScreenSize.Height))
            {

                for (int y = ys; y < yf; y++)
                {
                    myData = (byte)fs.ReadByte();

                    WriteDataOperationByte(dest, x + y * stride, myData);

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
            if (myMap.IsTilemap && !UseCharsetDataOperation())
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
            byte[] dest = GetDataOperationArray();
            int stride = myMap.Stride;

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
                        WriteDataOperationByte(dest, x + y * stride, myData);
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
            if (myMap.IsTilemap && !UseCharsetDataOperation())
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
            byte[] dest = GetDataOperationArray();
            int stride = myMap.Stride;

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);


            byte myData;

            int y = ys;
            while (fs.Position <= (fs.Length - width))
            {
                for (int x = xs; x < xf; x++)
                {
                    myData = (byte)fs.ReadByte();
                    WriteDataOperationByte(dest, x + y * stride, myData);

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
            if (myMap.IsTilemap && !UseCharsetDataOperation())
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
                // Free charmap can only be left via Data manipulation revert — keep Multi-Font on.
                if (!checkBoxMultiFont.Checked && myMap.FreeCharmapMode)
                {
                    checkBoxMultiFont.Checked = true;
                    return;
                }
                myMap.MultiFontEnabled = checkBoxMultiFont.Checked;
                // Enable/disable reference controls based on MultiFont
                UpdateFontMappingReferenceUI();
                AtariFontRenderer.ClearFontCache();
                AtariPictureTools.Redraw(Globals.WindowType.Editor);
                pictureBoxMap.Refresh();
                UpdateMultiFontUI();
                UpdateFreeCharmapUi();
                
                // Update DLI form to show/hide font column
                if (dliForm != null && dliForm.Visible)
                {
                    dliForm.ZoomResize();
                    dliForm.Show(dliForm.screenNumber); // Refresh the form with current screen
                    dliForm.RenderData();
                }
            }
        }

        private void CheckBoxFontMappingReference_CheckedChanged(object sender, EventArgs e)
        {
            if (suppressFontMappingUiEvents || myMap == null)
                return;

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
                dliForm.RenderData();
            }
        }

        private void NumericUpDownRefScreen_ValueChanged(object sender, EventArgs e)
        {
            if (suppressFontMappingUiEvents || myMap == null || !checkBoxFontMappingReference.Checked)
                return;

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
                dliForm.RenderData();
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
            
            // Syncing controls must not fire CheckedChanged/ValueChanged (those write mapping
            // back using stale numeric values and cause a one-frame wrong-font redraw).
            suppressFontMappingUiEvents = true;
            try
            {
                checkBoxFontMappingReference.Checked = useReference;
                numericUpDownRefScreenX.Value = refX;
                numericUpDownRefScreenY.Value = refY;
            }
            finally
            {
                suppressFontMappingUiEvents = false;
            }
            
            bool freeCharmap = myMap.FreeCharmapMode;
            bool enabled = myMap.MultiFontEnabled && !freeCharmap;
            // Hide entire font-mapping reference UI in free charmap (no per-row mapping)
            bool showRef = enabled;
            if (checkBoxFontMappingReference != null)
            {
                checkBoxFontMappingReference.Visible = showRef;
                checkBoxFontMappingReference.Enabled = enabled;
            }
            if (numericUpDownRefScreenX != null)
            {
                numericUpDownRefScreenX.Visible = showRef;
                numericUpDownRefScreenX.Enabled = enabled && useReference;
            }
            if (numericUpDownRefScreenY != null)
            {
                numericUpDownRefScreenY.Visible = showRef;
                numericUpDownRefScreenY.Enabled = enabled && useReference;
            }
            if (labelRefScreen != null)
            {
                labelRefScreen.Visible = showRef;
                labelRefScreen.Enabled = enabled && useReference;
            }
            if (labelRefScreenComma != null)
            {
                labelRefScreenComma.Visible = showRef;
                labelRefScreenComma.Enabled = enabled && useReference;
            }
        }

        private void UpdateMultiFontUI()
        {
            bool enabled = checkBoxMultiFont != null && checkBoxMultiFont.Checked;
            bool tilemapEnabled = myMap != null && myMap.IsTilemap;
            bool freeCharmap = myMap != null && myMap.FreeCharmapMode;
            // Free charmap: hide Multi-Font checkbox and font-mapping reference controls;
            // exit free mode only via Data manipulation revert.
            bool hideMultiFontControls = tilemapEnabled || freeCharmap;
            
            if (hideMultiFontControls)
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
                // Show controls for regular maps (multifont, not free charmap)
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
            
            // Font Templates: enabled only when multifont is checked (and not tilemap / free charmap)
            if (buttonFontTemplate != null)
                buttonFontTemplate.Enabled = !tilemapEnabled && enabled && (myMap == null || !myMap.FreeCharmapMode);
            
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
            UpdateFreeCharmapUi();
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
                if (dialog.ShowDialog() == DialogResult.OK)
                    UpdateMainFormCaption();
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
   
        private void MapdataColumnExport()
        {
            PrepareDataSaveDialog("MapData export (*.dat)|*.dat");
            switch (saveFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    RememberDataPath(saveFileDialog1.FileName);
                    this.ExportColumns((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, (int)numericUpDownScreenToX.Value, (int)numericUpDownScreenToY.Value, saveFileDialog1.FileName);
                    break;
            }
        }

        private void DliExport()
        {
            PrepareDataSaveDialog("Dli column export (*.dat)|*.dat");
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                RememberDataPath(saveFileDialog1.FileName);
                System.IO.FileStream fs = new System.IO.FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create);

                int lineCount = GetDliExportLineCount();
                int charOffset = (int)numericUpDownScreenFromX.Value * myMap.ScreenSize.Width
                    + (int)numericUpDownScreenFromY.Value * myMap.Stride * myMap.ScreenSize.Height;

                // Color-register by color-register: for each enabled register, all lines
                string mask = (maskedTextBoxDli.Text ?? "").PadRight(5, '0');
                for (int x = 0; x < 5; x++)
                {
                    if (mask[x] == '0') continue;
                    for (int y = 0; y < lineCount; y++)
                    {
                        byte[] color5 = myMap.GetDliColor5(charOffset + y * myMap.Stride);
                        fs.WriteByte(color5[x]);
                    }
                }

                if (IsAlpaEnabledForDliMask())
                {
                    string alpaMask = (maskedTextBoxAlpaDli.Text ?? "").PadRight(4, '0');
                    // PF0, PF1, PF2, PF3 alternate indices (no BAK alternate)
                    int[] alpaIndices = { 6, 8, 7, 5 };
                    for (int i = 0; i < 4; i++)
                    {
                        if (alpaMask[i] == '0') continue;
                        int colorIndex = alpaIndices[i];
                        for (int y = 0; y < lineCount; y++)
                        {
                            byte[] color5 = myMap.GetDliColor5(charOffset + y * myMap.Stride);
                            byte value = colorIndex < color5.Length ? color5[colorIndex] : (byte)0;
                            fs.WriteByte(value);
                        }
                    }
                }

                fs.Close();
                fs.Dispose();
            }
        }

        private void DliImport()
        {
            PrepareDataOpenDialog("Dli column export (*.dat)|*.dat");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                RememberDataPath(openFileDialog1.FileName);
                System.IO.FileStream fs = new System.IO.FileStream(openFileDialog1.FileName, System.IO.FileMode.Open);

                int screenX = (int)numericUpDownScreenFromX.Value;
                int screenY = (int)numericUpDownScreenFromY.Value;
                int lineCount = GetDliExportLineCount();

                string mask = (maskedTextBoxDli.Text ?? "").PadRight(5, '0');
                for (int x = 0; x < 5; x++)
                {
                    if (mask[x] == '0') continue;
                    for (int y = 0; y < lineCount; y++)
                    {
                        if (fs.Position >= fs.Length) break;
                        myMap.SetDliColor(screenX, screenY, y, x, (byte)fs.ReadByte());
                    }
                }

                if (IsAlpaEnabledForDliMask())
                {
                    string alpaMask = (maskedTextBoxAlpaDli.Text ?? "").PadRight(4, '0');
                    int[] alpaIndices = { 6, 8, 7, 5 };
                    for (int i = 0; i < 4; i++)
                    {
                        if (alpaMask[i] == '0') continue;
                        int colorIndex = alpaIndices[i];
                        for (int y = 0; y < lineCount; y++)
                        {
                            if (fs.Position >= fs.Length) break;
                            myMap.SetDliColor(screenX, screenY, y, colorIndex, (byte)fs.ReadByte());
                        }
                    }
                }

                fs.Close();
                fs.Dispose();
                RedrawEditorWindow();
            }
        }

        private int GetDliExportLineCount()
        {
            int h = myMap.ScreenSize.Height;
            if (myMap.IsTilemap && myMap.TilemapInfo != null && myMap.TilemapInfo.TileHeight > 0)
                h = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
            return h;
        }

        private bool IsAlpaEnabledForDliMask()
        {
            return AtariFontRenderer.Color5 != null && AtariFontRenderer.Color5.Length > 5;
        }

        private void MapdataColumnImport()
        {
            PrepareDataOpenDialog("Column based map datafile (*.*)|*.*");
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    RememberDataPath(openFileDialog1.FileName);
                    this.ImportColumns((int)numericUpDownScreenFromX.Value, (int)numericUpDownScreenFromY.Value, openFileDialog1.FileName);
                    RedrawEditorWindow();
                    break;
            }
        }

        private void MapdataImport()
        {
            PrepareDataOpenDialog("Map datafile (*.*)|*.*");
            switch (openFileDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    RememberDataPath(openFileDialog1.FileName);
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
                UpdateDliMaskControlsVisibility();
                
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
                        AtariFontRenderer.ClearFontCache();
                        AtariFontRenderer.CharPickerFontIndex = null;
                        AtariFontRenderer.CharPickerFontSourceMap = null;
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
                UpdateUndoRedoUI();
                AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
                if (myCharPicker != null)
                    myCharPicker.RedrawFontWindow();
                numericUpDown6.Maximum = myMap.ScreenSize.Width * myMap.MapSize.Width;
                UpdateDatalineWidthDefault();
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
                currentAtrmapPath = null;
                UpdateMainFormCaption();
                RedrawEditorWindow();
                dliForm.Dispose();
                dliForm = CreateDliForm();
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
            AtariPictureTools.Redraw(Globals.WindowType.Editor, true, comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked, currentScreen, isScreenLocked, lockedScreen);
            if (ShouldShowScreenSelectionOverlay())
                DrawScreenSelectionOverlay();
            else
                ScreenSelectionShown = false;
            pictureBoxMap.Refresh();
        }

        private bool ShouldShowScreenSelectionOverlay()
        {
            return tabControl1.SelectedTab == tabPage2 && checkBoxShowScreenSelection.Checked;
        }

        private void ComboBoxOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import","Export DLI"};
            if (checkBoxExportSingleScreen != null)
                checkBoxExportSingleScreen.Visible = IsSingleScreenExportOperation();

            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDown5.Enabled = true;
                    numericUpDown6.Enabled = false;
                    ApplyExportSingleScreenState();
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
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    ApplyExportSingleScreenState();
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
                case 6: // Export screen by screen — range
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = true;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = true;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
                case 7: // Import screen by screen — single From screen
                    numericUpDownScreenFromX.Enabled = true;
                    numericUpDownScreenToX.Enabled = false;
                    numericUpDownScreenFromY.Enabled = true;
                    numericUpDownScreenToY.Enabled = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    break;
            }
            UpdateDliMaskControlsVisibility();
            if (checkBoxShowScreenSelection.Checked)
                ShowScreenSelection();

        }

        private bool IsSingleScreenExportOperation()
        {
            // Export, Column Export
            return comboOperation.SelectedIndex == 0 || comboOperation.SelectedIndex == 2;
        }

        private void UpdateDliMaskControlsVisibility()
        {
            bool dliOp = comboOperation.SelectedIndex == 4 || comboOperation.SelectedIndex == 5;
            bool alpa = IsAlpaEnabledForDliMask();

            if (labelDliOrderHint != null)
                labelDliOrderHint.Visible = dliOp;
            if (labelDliMask != null)
                labelDliMask.Visible = dliOp;
            if (maskedTextBoxDli != null)
                maskedTextBoxDli.Visible = dliOp;
            if (labelAlpaDliMask != null)
                labelAlpaDliMask.Visible = dliOp && alpa;
            if (maskedTextBoxAlpaDli != null)
                maskedTextBoxAlpaDli.Visible = dliOp && alpa;
        }

        private void UpdateDatalineWidthDefault()
        {
            if (myMap == null || numericUpDown6 == null)
                return;
            int width = myMap.ScreenSize.Width;
            if (width < numericUpDown6.Minimum)
                width = (int)numericUpDown6.Minimum;
            if (width > numericUpDown6.Maximum)
                width = (int)numericUpDown6.Maximum;
            numericUpDown6.Value = width;
        }

        private void ApplyExportSingleScreenState()
        {
            if (!IsSingleScreenExportOperation())
                return;
            bool single = checkBoxExportSingleScreen != null && checkBoxExportSingleScreen.Checked;
            numericUpDownScreenToX.Enabled = !single;
            numericUpDownScreenToY.Enabled = !single;
            if (single)
                SyncExportToScreenFromFrom();
        }

        private void SyncExportToScreenFromFrom()
        {
            if (numericUpDownScreenToX.Value != numericUpDownScreenFromX.Value)
                numericUpDownScreenToX.Value = numericUpDownScreenFromX.Value;
            if (numericUpDownScreenToY.Value != numericUpDownScreenFromY.Value)
                numericUpDownScreenToY.Value = numericUpDownScreenFromY.Value;
        }

        private void CheckBoxExportSingleScreen_CheckedChanged(object sender, EventArgs e)
        {
            ApplyExportSingleScreenState();
            if (checkBoxShowScreenSelection.Checked)
                ShowScreenSelection();
        }

        private void ButtonPerform_Click(object sender, EventArgs e)
        {
            //{"Export","Import","Column Export","Column Import"};
            switch (comboOperation.SelectedIndex)
            {
                case 0:
                    MapdataExport();
                    break;
                case 1:
                    MapdataImport();
                    break;
                case 2:
                    MapdataColumnExport();
                    break;
                case 3:
                    MapdataColumnImport();
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
                if (AtariClipboard.IsValid || floatingPasteArmed)
                {
                    if (AtariPictureTools.PreviousClipboardLocation.HasValue)
                    {
                        AtariPictureTools.DrawUnderClipBoard(AtariPictureTools.PreviousClipboardLocation.Value);
                    }
                    AtariClipboard.IsValid = false;
                    AtariPictureTools.PreviousClipboardLocation = null;
                    AtariPictureTools.PreviousClipboardGridCell = null;
                    floatingPasteArmed = false;
                    AtariPictureTools.Redraw(Globals.WindowType.Editor);
                    pictureBoxMap.Refresh();
                    // Keep clipboard preview visible so Ctrl+V / picturebox click can reactivate paste.
                    if (pictureBoxClipboard != null && AtariClipboard.ClipboardImage != null)
                    {
                        pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                        pictureBoxClipboard.Refresh();
                    }
                    e.Handled = true;
                    return;
                }
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                PerformUndo();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.Y)
            {
                PerformRedo();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                // Ctrl+C only used when Autopaste is off (selection copies automatically when Autopaste is on).
                if (IsAutopasteEnabled)
                    return;

                if (AtariPictureTools.CopyCurrentSelectionToClipboard(Globals.WindowType.Editor))
                {
                    AtariClipboard.IsValid = true;
                    pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                    floatingPasteArmed = false;
                    AtariPictureTools.Redraw(Globals.WindowType.Editor, true, comboBoxDrawBorders.Checked, comboBoxDrawGrid.Checked, currentScreen, isScreenLocked, lockedScreen);
                    pictureBoxMap.Refresh();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Same as clicking the clipboard picturebox: reactivate clipboard if we still have an image.
                if (AtariClipboard.ClipboardImage != null)
                {
                    AtariClipboard.IsValid = true;
                    if (pictureBoxClipboard != null)
                        pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                }
                if (!AtariClipboard.IsValid)
                    return;

                if (!IsAutopasteEnabled)
                    floatingPasteArmed = true;

                ShowFloatingClipboardAt(lastMapMouseLocation);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
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

            byte[,] fontIndices = AtariClipboard.GetFontData();
            byte[] screenColors = GetActiveScreenColorsForClipboard();
            bool useMapFonts = myMap != null && myMap.MultiFontEnabled
                && (myMap.FreeCharmapMode || fontIndices != null);

            // Font bitmap uses indices 0-4; palette comes from active screen colors (or global Color5).
            Bitmap baseClipboardImage = new Bitmap(AtariClipboard.ClipboardWidth * 8, AtariClipboard.ClipboardHeight * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            AtariFontRenderer.RenderClipboardData(
                data,
                AtariClipboard.ClipboardWidth,
                AtariClipboard.ClipboardHeight,
                baseClipboardImage,
                useMapFonts ? myMap : null,
                useMapFonts ? fontIndices : null,
                screenColors);
            
            int zoomedWidth = baseClipboardImage.Width * Globals.Zoom;
            int zoomedHeight = baseClipboardImage.Height * Globals.Zoom;
            
            // Scale in 8-bit only (nearest-neighbor) to avoid 8→32→8 round-trip color shifts
            Bitmap newClipboardImage = new Bitmap(zoomedWidth, zoomedHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            // Copy palette from rendered base (already set to screen or Color5 colors)
            newClipboardImage.Palette = baseClipboardImage.Palette;
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
                Color[] palette = newClipboardImage.Palette.Entries;
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
                            Color c = (idx < palette.Length) ? palette[idx] : Color.Black;
                            dstPtr[y * (dstData32.Stride / 4) + x] = c.ToArgb();
                        }
                    }
                }
                newClipboardImage.UnlockBits(srcData8);
                temp32Bit.UnlockBits(dstData32);
                newClipboardImage.Dispose();

                // Apply skip-zero transparency mask
                int cellPxW = Globals.CharSize;
                int cellPxH = Globals.CharSize;
                AtariClipboard.ApplySkipZeroTransparencyMask(temp32Bit, data,
                    AtariClipboard.ClipboardWidth, AtariClipboard.ClipboardHeight, cellPxW, cellPxH);
                if (AtariClipboard.ClipboardImage != null)
                    AtariClipboard.ClipboardImage.Dispose();
                AtariClipboard.ClipboardImage = temp32Bit;
            }
            else
            {
                if (AtariClipboard.ClipboardImage != null)
                    AtariClipboard.ClipboardImage.Dispose();
                AtariClipboard.ClipboardImage = newClipboardImage;
            }

            if (AtariClipboard.UnderClipBoardImage != null)
                AtariClipboard.UnderClipBoardImage.Dispose();
            if (AtariClipboard.UnderImageGraphics != null)
                AtariClipboard.UnderImageGraphics.Dispose();
            AtariClipboard.UnderClipBoardImage = new Bitmap(AtariClipboard.ClipboardImage);
            AtariClipboard.UnderImageGraphics = Graphics.FromImage(AtariClipboard.UnderClipBoardImage);

            if (pictureBoxClipboard != null)
            {
                pictureBoxClipboard.Image = AtariClipboard.ClipboardImage;
                pictureBoxClipboard.Refresh();
            }
        }

        /// <summary>Colors of the active (current) screen, line 0 — for clipboard / library preview.</summary>
        private byte[] GetActiveScreenColorsForClipboard()
        {
            if (myMap == null)
                return AtariFontRenderer.Color5;

            Point scr = isScreenLocked ? lockedScreen : currentScreen;
            int screenCharWidth = myMap.ScreenSize.Width;
            int screenCharHeight = myMap.ScreenSize.Height;
            int stride = myMap.Stride;
            if (myMap.IsTilemap && myMap.TilemapInfo != null)
            {
                screenCharWidth = myMap.ScreenSize.Width * myMap.TilemapInfo.TileWidth;
                screenCharHeight = myMap.ScreenSize.Height * myMap.TilemapInfo.TileHeight;
                stride = myMap.CharStride;
            }
            int charOffset = scr.Y * screenCharHeight * stride + scr.X * screenCharWidth;
            byte[] colors = myMap.GetDliColor5(charOffset);
            if (colors == null || colors.Length < 5 || colors[0] == Globals.DEFAULT_COLOR)
                return AtariFontRenderer.Color5;
            return colors;
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
            UpdateUndoRedoUI();
            AtariPictureTools.AssignWindow(Globals.WindowType.Editor, (Bitmap)pictureBoxMap.Image, myMap);
            dliForm?.Dispose();
            dliForm = CreateDliForm();
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

        private void DrawScreenSelectionOverlay()
        {
            if (pictureBoxMap?.Image == null || myMap == null)
                return;
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
            // Import-style ops disable To X/Y — highlight only the From screen
            bool singleScreen = !numericUpDownScreenToX.Enabled || !numericUpDownScreenToY.Enabled;
            int screensWide = singleScreen ? 1 : (int)(numericUpDownScreenToX.Value - numericUpDownScreenFromX.Value + 1);
            int screensHigh = singleScreen ? 1 : (int)(numericUpDownScreenToY.Value - numericUpDownScreenFromY.Value + 1);
            if (screensWide < 1) screensWide = 1;
            if (screensHigh < 1) screensHigh = 1;
            int width = screensWide * screenCharWidth * Globals.CharSize;
            int height = screensHigh * screenCharHeight * Globals.CharSize;
            int x = (left - myMap.OffsetX) * Globals.CharSize;
            int y = (top - myMap.OffsetY) * Globals.CharSize;
            int penWidth = 2 * Globals.CharSize;
            Graphics g = Graphics.FromImage(pictureBoxMap.Image);
            using (Pen p = new Pen(Color.FromArgb(96, Color.GreenYellow), penWidth))
            {
                // Pen is centered on the path; inset by half width so the stroke stays inside the selection
                float inset = penWidth / 2f;
                g.DrawRectangle(p, x + inset, y + inset, width - penWidth, height - penWidth);
            }
            ScreenSelectionShown = true;
        }

        private void ShowScreenSelection(bool redraw = true)
        {
            if (redraw)
                RedrawEditorWindow();
            else
            {
                DrawScreenSelectionOverlay();
                pictureBoxMap.Refresh();
            }
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
            if (IsSingleScreenExportOperation()
                && checkBoxExportSingleScreen != null
                && checkBoxExportSingleScreen.Checked
                && (sender == numericUpDownScreenFromX || sender == numericUpDownScreenFromY))
            {
                SyncExportToScreenFromFrom();
            }
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

        /// <summary>
        /// Designer owns the Free Charmap controls; this only wires tooltips, charset panel clicks, and the panel array.
        /// </summary>
        private void InitFreeCharmapUi()
        {
            if (toolTip1 != null && checkBoxCharsetDataOp != null)
            {
                toolTip1.SetToolTip(checkBoxCharsetDataOp,
                    "When checked, Export/Import operate on per-cell charset indexes (0–7) instead of character codes. Available in Free Charmap mode.");
            }

            panelCharsetColors = new Panel[]
            {
                panelCharsetColor0, panelCharsetColor1, panelCharsetColor2, panelCharsetColor3,
                panelCharsetColor4, panelCharsetColor5, panelCharsetColor6, panelCharsetColor7
            };
            for (int i = 0; i < panelCharsetColors.Length; i++)
            {
                if (panelCharsetColors[i] == null)
                    continue;
                panelCharsetColors[i].Tag = i;
                panelCharsetColors[i].Cursor = Cursors.Hand;
                panelCharsetColors[i].Click += PanelCharsetColor_Click;
                foreach (Control child in panelCharsetColors[i].Controls)
                {
                    child.Tag = i;
                    child.Click += PanelCharsetColor_Click;
                }
            }

            RefreshCharsetColorPanels();
        }

        private void UpdateFreeCharmapUi()
        {
            if (groupBoxFreeCharmap == null || myMap == null)
                return;

            bool tilemap = myMap.IsTilemap;
            bool free = myMap.FreeCharmapMode;
            bool multi = myMap.MultiFontEnabled;

            groupBoxFreeCharmap.Enabled = !tilemap;
            if (labelFreeCharmapStatus != null)
            {
                if (tilemap)
                    labelFreeCharmapStatus.Text = "Status: N/A (tilemap)";
                else if (free)
                    labelFreeCharmapStatus.Text = "Status: Free Charmap Mode (per-cell charset)";
                else if (multi)
                    labelFreeCharmapStatus.Text = "Status: Multifont (charset per row)";
                else
                    labelFreeCharmapStatus.Text = "Status: Single font";
            }

            if (buttonConvertToFreeCharmap != null)
                buttonConvertToFreeCharmap.Enabled = !tilemap && !free && multi;
            if (buttonRevertFreeSingleFont != null)
                buttonRevertFreeSingleFont.Enabled = !tilemap && free;
            if (buttonRevertFreeMajority != null)
                buttonRevertFreeMajority.Enabled = !tilemap && free;

            // Colors-tab settings group: only visible in free charmap mode
            if (groupBoxFreeCharmapSettings != null)
            {
                groupBoxFreeCharmapSettings.Visible = free && !tilemap;
                if (groupBoxFreeCharmapSettings.Visible)
                {
                    if (checkBoxShowCharsetOverlay != null) checkBoxShowCharsetOverlay.Enabled = true;
                    if (radioCharsetOverlayNumbers != null) radioCharsetOverlayNumbers.Enabled = true;
                    if (radioCharsetOverlayColors != null) radioCharsetOverlayColors.Enabled = true;
                    if (trackBarCharsetOverlayBlend != null) trackBarCharsetOverlayBlend.Enabled = true;
                    if (labelCharsetOverlayBlend != null) labelCharsetOverlayBlend.Enabled = true;
                    if (labelCharsetColors != null) labelCharsetColors.Enabled = true;
                    if (panelCharsetColors != null)
                    {
                        foreach (var p in panelCharsetColors)
                            if (p != null) p.Enabled = true;
                    }
                }
            }

            if (checkBoxCharsetDataOp != null)
            {
                checkBoxCharsetDataOp.Visible = !tilemap;
                checkBoxCharsetDataOp.Enabled = free;
                if (!free)
                    checkBoxCharsetDataOp.Checked = false;
            }

            if (checkBoxShowCharsetOverlay != null)
                checkBoxShowCharsetOverlay.Checked = free && Globals.FreeCharmapOverlayVisible;

            RefreshCharsetColorPanels();
            UpdateFontMappingReferenceUI();
        }

        private void RefreshCharsetColorPanels()
        {
            if (panelCharsetColors == null || myMap == null)
                return;
            myMap.EnsureCharsetColors();
            for (int i = 0; i < panelCharsetColors.Length; i++)
            {
                if (panelCharsetColors[i] == null)
                    continue;
                Color c = AtariPalette.GetColor(myMap.CharsetColors[i]);
                panelCharsetColors[i].BackColor = c;
                int lum = (c.R * 299 + c.G * 587 + c.B * 114) / 1000;
                foreach (Control child in panelCharsetColors[i].Controls)
                    child.ForeColor = lum > 128 ? Color.Black : Color.White;
            }
        }

        private void PanelCharsetColor_Click(object sender, EventArgs e)
        {
            int charsetIndex = -1;
            if (sender is Control ctrl && ctrl.Tag is int tagInt)
                charsetIndex = tagInt;
            else if (sender is Control ctrl2 && ctrl2.Tag != null && int.TryParse(ctrl2.Tag.ToString(), out int parsed))
                charsetIndex = parsed;
            if (charsetIndex < 0 || charsetIndex > 7)
                return;
            PanelCharsetColor_Click(charsetIndex);
        }

        private void PanelCharsetColor_Click(int charsetIndex)
        {
            if (myMap == null || !myMap.MultiFontEnabled || myMap.IsTilemap || !myMap.FreeCharmapMode)
                return;
            myMap.EnsureCharsetColors();
            colorPickerForm.Pick(myMap.CharsetColors[charsetIndex]);
            if (colorPickerForm.PickedNewColor)
            {
                myMap.CharsetColors[charsetIndex] = colorPickerForm.PickedColorIndex;
                RefreshCharsetColorPanels();
                if (myMap.FreeCharmapMode && Globals.FreeCharmapOverlayVisible)
                    RedrawEditorWindow();
            }
        }

        private void ButtonConvertToFreeCharmap_Click(object sender, EventArgs e)
        {
            if (myMap == null || myMap.IsTilemap)
                return;
            if (!myMap.MultiFontEnabled)
            {
                MessageBox.Show("Enable Multi-Font first.", "Free Charmap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (myMap.FreeCharmapMode)
                return;

            var r = MessageBox.Show(
                "Convert this map to Free Charmap Mode?\n\n" +
                "Each character cell will store which charset (0–7) it uses.\n" +
                "The per-row charset restriction will no longer apply.\n" +
                "DLI font-per-line editing will be disabled while in this mode.\n\n" +
                "You can revert later (with confirmation).",
                "Convert to Free Charmap Mode",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (r != DialogResult.Yes)
                return;

            myMap.ConvertToFreeCharmapMode();
            AtariFontRenderer.ClearFontCache();
            UpdateFreeCharmapUi();
            UpdateMultiFontUI();
            if (dliForm != null && dliForm.Visible)
            {
                dliForm.ZoomResize();
                dliForm.Show(dliForm.screenNumber);
                dliForm.RenderData();
            }
            if (myCharPicker != null && myCharPicker.Visible)
                myCharPicker.Refresh();
            RedrawEditorWindow();
        }

        private void ButtonRevertFreeSingleFont_Click(object sender, EventArgs e)
        {
            if (myMap == null || !myMap.FreeCharmapMode)
                return;

            using (var dlg = new InputDialog("Font index for all rows (0–7):", "Revert Free Charmap", "0"))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                if (!byte.TryParse(dlg.InputText?.Trim(), out byte fontIndex) || fontIndex > 7)
                {
                    MessageBox.Show("Please enter a font index from 0 to 7.", "Revert Free Charmap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var r = MessageBox.Show(
                    $"Revert to normal multifont mode?\n\nAll screen rows will use font {fontIndex}.\n" +
                    "Per-cell charset data will be discarded.\n" +
                    "Characters that used other fonts may display incorrectly.\n\nContinue?",
                    "Revert Free Charmap",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                    return;

                myMap.RevertFreeCharmapToSingleFont(fontIndex);
                FinishFreeCharmapRevert();
            }
        }

        private void ButtonRevertFreeMajority_Click(object sender, EventArgs e)
        {
            if (myMap == null || !myMap.FreeCharmapMode)
                return;

            var r = MessageBox.Show(
                "Revert to normal multifont mode?\n\n" +
                "For each screen row, the charset used most often on that row will be selected.\n" +
                "Per-cell charset data will be discarded.\n" +
                "Characters that used a minority charset on their row may display incorrectly.\n\nContinue?",
                "Revert Free Charmap (majority)",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (r != DialogResult.Yes)
                return;

            myMap.RevertFreeCharmapMajorityPerRow();
            FinishFreeCharmapRevert();
        }

        private void FinishFreeCharmapRevert()
        {
            Globals.FreeCharmapOverlayVisible = false;
            AtariFontRenderer.ClearFontCache();
            UpdateFreeCharmapUi();
            UpdateMultiFontUI();
            if (dliForm != null && dliForm.Visible)
            {
                dliForm.ZoomResize();
                dliForm.Show(dliForm.screenNumber);
                dliForm.RenderData();
            }
            RedrawEditorWindow();
        }

        private void CheckBoxShowCharsetOverlay_CheckedChanged(object sender, EventArgs e)
        {
            Globals.FreeCharmapOverlayVisible = checkBoxShowCharsetOverlay.Checked && myMap != null && myMap.FreeCharmapMode;
            RedrawEditorWindow();
        }

        private void RadioCharsetOverlayMode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioCharsetOverlayNumbers != null && radioCharsetOverlayNumbers.Checked)
                Globals.FreeCharmapOverlayShowNumbers = true;
            else if (radioCharsetOverlayColors != null && radioCharsetOverlayColors.Checked)
                Globals.FreeCharmapOverlayShowNumbers = false;
            if (Globals.FreeCharmapOverlayVisible)
                RedrawEditorWindow();
        }

        private void TrackBarCharsetOverlayBlend_Scroll(object sender, EventArgs e)
        {
            int pct = trackBarCharsetOverlayBlend.Value * 5;
            Globals.FreeCharmapOverlayBlendPercent = pct;
            labelCharsetOverlayBlend.Text = $"Overlay transparency: {pct}%";
            if (Globals.FreeCharmapOverlayVisible)
                RedrawEditorWindow();
        }

    }
}