namespace AtariMapMaker
{
    partial class DliForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBoxDli = new System.Windows.Forms.PictureBox();
            this.contextMenuStripDli = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copy1ToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paste1FromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillDown1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copy5ToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paste5FromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillDown5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allColorsforWholeScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAllToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteAllFromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDli)).BeginInit();
            this.contextMenuStripDli.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxDli
            // 
            this.pictureBoxDli.Location = new System.Drawing.Point(1, 0);
            this.pictureBoxDli.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxDli.Name = "pictureBoxDli";
            this.pictureBoxDli.Size = new System.Drawing.Size(80, 320);
            this.pictureBoxDli.TabIndex = 0;
            this.pictureBoxDli.TabStop = false;
            this.pictureBoxDli.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDli_MouseDown);
            // 
            // contextMenuStripDli
            // 
            this.contextMenuStripDli.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorToolStripMenuItem,
            this.colorsToolStripMenuItem,
            this.allColorsforWholeScreenToolStripMenuItem});
            this.contextMenuStripDli.Name = "contextMenuStrip1";
            this.contextMenuStripDli.Size = new System.Drawing.Size(224, 70);
            this.contextMenuStripDli.Text = "DLI colors";
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pickToolStripMenuItem,
            this.copy1ToClipboardToolStripMenuItem,
            this.paste1FromClipboardToolStripMenuItem,
            this.fillDown1ToolStripMenuItem});
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.colorToolStripMenuItem.Text = "Color";
            // 
            // pickToolStripMenuItem
            // 
            this.pickToolStripMenuItem.Name = "pickToolStripMenuItem";
            this.pickToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.pickToolStripMenuItem.Text = "Pick";
            this.pickToolStripMenuItem.Click += new System.EventHandler(this.PickToolStripMenuItem_Click);
            // 
            // copy1ToClipboardToolStripMenuItem
            // 
            this.copy1ToClipboardToolStripMenuItem.Name = "copy1ToClipboardToolStripMenuItem";
            this.copy1ToClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copy1ToClipboardToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.copy1ToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            this.copy1ToClipboardToolStripMenuItem.Click += new System.EventHandler(this.CopyToClipboardToolStripMenuItem_Click);
            // 
            // paste1FromClipboardToolStripMenuItem
            // 
            this.paste1FromClipboardToolStripMenuItem.Name = "paste1FromClipboardToolStripMenuItem";
            this.paste1FromClipboardToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.paste1FromClipboardToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.paste1FromClipboardToolStripMenuItem.Text = "Paste from clipboard";
            this.paste1FromClipboardToolStripMenuItem.Click += new System.EventHandler(this.Paste1FromClipboardToolStripMenuItem_Click);
            // 
            // fillDown1ToolStripMenuItem
            // 
            this.fillDown1ToolStripMenuItem.Name = "fillDown1ToolStripMenuItem";
            this.fillDown1ToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.fillDown1ToolStripMenuItem.Text = "Fill Down (n lines)";
            // 
            // colorsToolStripMenuItem
            // 
            this.colorsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copy5ToClipboardToolStripMenuItem,
            this.paste5FromClipboardToolStripMenuItem,
            this.fillDown5ToolStripMenuItem});
            this.colorsToolStripMenuItem.Name = "colorsToolStripMenuItem";
            this.colorsToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.colorsToolStripMenuItem.Text = "5 Colors";
            // 
            // copy5ToClipboardToolStripMenuItem
            // 
            this.copy5ToClipboardToolStripMenuItem.Name = "copy5ToClipboardToolStripMenuItem";
            this.copy5ToClipboardToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.copy5ToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            this.copy5ToClipboardToolStripMenuItem.Click += new System.EventHandler(this.Copy5ToClipboardToolStripMenuItem_Click);
            // 
            // paste5FromClipboardToolStripMenuItem
            // 
            this.paste5FromClipboardToolStripMenuItem.Name = "paste5FromClipboardToolStripMenuItem";
            this.paste5FromClipboardToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.paste5FromClipboardToolStripMenuItem.Text = "Paste from clipboard";
            this.paste5FromClipboardToolStripMenuItem.Click += new System.EventHandler(this.Paste5FromClipboardToolStripMenuItem_Click);
            // 
            // fillDown5ToolStripMenuItem
            // 
            this.fillDown5ToolStripMenuItem.Name = "fillDown5ToolStripMenuItem";
            this.fillDown5ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.fillDown5ToolStripMenuItem.Text = "Fill Down (n lines)";
            // 
            // allColorsforWholeScreenToolStripMenuItem
            // 
            this.allColorsforWholeScreenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyAllToClipboardToolStripMenuItem,
            this.pasteAllFromClipboardToolStripMenuItem,
            this.resetToolStripMenuItem});
            this.allColorsforWholeScreenToolStripMenuItem.Name = "allColorsforWholeScreenToolStripMenuItem";
            this.allColorsforWholeScreenToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.allColorsforWholeScreenToolStripMenuItem.Text = "All Colors (for whole screen)";
            // 
            // copyAllToClipboardToolStripMenuItem
            // 
            this.copyAllToClipboardToolStripMenuItem.Name = "copyAllToClipboardToolStripMenuItem";
            this.copyAllToClipboardToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.copyAllToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            this.copyAllToClipboardToolStripMenuItem.Click += new System.EventHandler(this.CopyAllToClipboardToolStripMenuItem_Click);
            // 
            // pasteAllFromClipboardToolStripMenuItem
            // 
            this.pasteAllFromClipboardToolStripMenuItem.Name = "pasteAllFromClipboardToolStripMenuItem";
            this.pasteAllFromClipboardToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.pasteAllFromClipboardToolStripMenuItem.Text = "Paste from clipboard";
            this.pasteAllFromClipboardToolStripMenuItem.Click += new System.EventHandler(this.PasteAllFromClipboardToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.ResetToolStripMenuItem_Click);
            // 
            // DliForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(162, 336);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBoxDli);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DliForm";
            this.ShowInTaskbar = false;
            this.Text = "DliForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDli)).EndInit();
            this.contextMenuStripDli.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxDli;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDli;
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copy1ToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paste1FromClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillDown1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copy5ToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paste5FromClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillDown5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allColorsforWholeScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyAllToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteAllFromClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
    }
}