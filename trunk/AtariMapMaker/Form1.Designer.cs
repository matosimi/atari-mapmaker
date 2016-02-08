namespace AtariMapMaker
{
    partial class MainForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelPosition = new System.Windows.Forms.Label();
            this.labelScreen = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnHOBOimport = new System.Windows.Forms.Button();
            this.btn_hoboexport = new System.Windows.Forms.Button();
            this.buttonShiftChars = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.buttonLoadFont = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnNewMap = new System.Windows.Forms.Button();
            this.nudScreenW = new System.Windows.Forms.NumericUpDown();
            this.lblScreenSize = new System.Windows.Forms.Label();
            this.nudScreenH = new System.Windows.Forms.NumericUpDown();
            this.lblMapSize = new System.Windows.Forms.Label();
            this.nudMapH = new System.Windows.Forms.NumericUpDown();
            this.nudMapW = new System.Windows.Forms.NumericUpDown();
            this.tbZoom = new System.Windows.Forms.TrackBar();
            this.cbDrawGrid = new System.Windows.Forms.CheckBox();
            this.cbDrawBorders = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.labelPosition);
            this.splitContainer1.Panel2.Controls.Add(this.labelScreen);
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(914, 542);
            this.splitContainer1.SplitterDistance = 631;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(631, 542);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.ClientSizeChanged += new System.EventHandler(this.pictureBox1_ClientSizeChanged);
            this.pictureBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDoubleClick);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Location = new System.Drawing.Point(9, 508);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(62, 13);
            this.labelPosition.TabIndex = 2;
            this.labelPosition.Text = "Position 0:0";
            // 
            // labelScreen
            // 
            this.labelScreen.AutoSize = true;
            this.labelScreen.Location = new System.Drawing.Point(9, 495);
            this.labelScreen.Name = "labelScreen";
            this.labelScreen.Size = new System.Drawing.Size(59, 13);
            this.labelScreen.TabIndex = 1;
            this.labelScreen.Text = "Screen 0:0";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(2, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(278, 489);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pictureBox2);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(270, 463);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Colors";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(11, 292);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(240, 158);
            this.pictureBox2.TabIndex = 6;
            this.pictureBox2.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listView1);
            this.groupBox2.Location = new System.Drawing.Point(6, 66);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(260, 177);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Font";
            // 
            // listView1
            // 
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(12, 19);
            this.listView1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(234, 146);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            this.listView1.MouseLeave += new System.EventHandler(this.listView1_MouseLeave);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbDrawBorders);
            this.groupBox1.Controls.Add(this.cbDrawGrid);
            this.groupBox1.Controls.Add(this.tbZoom);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 54);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Zoom";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(18, 249);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "show font";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnHOBOimport);
            this.tabPage2.Controls.Add(this.btn_hoboexport);
            this.tabPage2.Controls.Add(this.buttonShiftChars);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.numericUpDown5);
            this.tabPage2.Controls.Add(this.buttonLoadFont);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.numericUpDown4);
            this.tabPage2.Controls.Add(this.numericUpDown3);
            this.tabPage2.Controls.Add(this.numericUpDown2);
            this.tabPage2.Controls.Add(this.numericUpDown1);
            this.tabPage2.Controls.Add(this.buttonExport);
            this.tabPage2.Controls.Add(this.buttonSave);
            this.tabPage2.Controls.Add(this.buttonLoad);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(270, 463);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Save/Load/Export";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnHOBOimport
            // 
            this.btnHOBOimport.Location = new System.Drawing.Point(8, 91);
            this.btnHOBOimport.Margin = new System.Windows.Forms.Padding(2);
            this.btnHOBOimport.Name = "btnHOBOimport";
            this.btnHOBOimport.Size = new System.Drawing.Size(81, 23);
            this.btnHOBOimport.TabIndex = 14;
            this.btnHOBOimport.Text = "HOBOimport";
            this.btnHOBOimport.UseVisualStyleBackColor = true;
            this.btnHOBOimport.Click += new System.EventHandler(this.btnHOBOimport_Click);
            // 
            // btn_hoboexport
            // 
            this.btn_hoboexport.Location = new System.Drawing.Point(8, 203);
            this.btn_hoboexport.Margin = new System.Windows.Forms.Padding(2);
            this.btn_hoboexport.Name = "btn_hoboexport";
            this.btn_hoboexport.Size = new System.Drawing.Size(81, 23);
            this.btn_hoboexport.TabIndex = 13;
            this.btn_hoboexport.Text = "HOBOexport";
            this.btn_hoboexport.UseVisualStyleBackColor = true;
            this.btn_hoboexport.Click += new System.EventHandler(this.btn_hoboexport_Click);
            // 
            // buttonShiftChars
            // 
            this.buttonShiftChars.Location = new System.Drawing.Point(8, 332);
            this.buttonShiftChars.Name = "buttonShiftChars";
            this.buttonShiftChars.Size = new System.Drawing.Size(105, 52);
            this.buttonShiftChars.TabIndex = 12;
            this.buttonShiftChars.Text = "Shift chars\r\n64-79 -> 80-95\r\n32-47 -> 64-79";
            this.buttonShiftChars.UseVisualStyleBackColor = true;
            this.buttonShiftChars.Click += new System.EventHandler(this.buttonShiftChars_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(13, 179);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 44);
            this.label3.TabIndex = 11;
            this.label3.Text = "Extra chars in line";
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(119, 177);
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown5.TabIndex = 10;
            this.numericUpDown5.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // buttonLoadFont
            // 
            this.buttonLoadFont.Location = new System.Drawing.Point(8, 256);
            this.buttonLoadFont.Name = "buttonLoadFont";
            this.buttonLoadFont.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadFont.TabIndex = 8;
            this.buttonLoadFont.Text = "Load Font";
            this.buttonLoadFont.UseVisualStyleBackColor = true;
            this.buttonLoadFont.Click += new System.EventHandler(this.buttonLoadFont_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 44);
            this.label2.TabIndex = 9;
            this.label2.Text = "From X,Y\r\nTo X,Y";
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(194, 151);
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown4.TabIndex = 6;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(194, 125);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown3.TabIndex = 5;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(119, 151);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown2.TabIndex = 4;
            this.numericUpDown2.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(119, 125);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown1.TabIndex = 3;
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(188, 203);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 2;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(94, 26);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save Map";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(6, 24);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 25);
            this.buttonLoad.TabIndex = 0;
            this.buttonLoad.Text = "Load Map";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.nudMapH);
            this.tabPage3.Controls.Add(this.nudMapW);
            this.tabPage3.Controls.Add(this.lblMapSize);
            this.tabPage3.Controls.Add(this.nudScreenH);
            this.tabPage3.Controls.Add(this.lblScreenSize);
            this.tabPage3.Controls.Add(this.nudScreenW);
            this.tabPage3.Controls.Add(this.btnNewMap);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(270, 463);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "New Map";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnNewMap
            // 
            this.btnNewMap.Location = new System.Drawing.Point(6, 149);
            this.btnNewMap.Name = "btnNewMap";
            this.btnNewMap.Size = new System.Drawing.Size(101, 23);
            this.btnNewMap.TabIndex = 0;
            this.btnNewMap.Text = "Create new map";
            this.btnNewMap.UseVisualStyleBackColor = true;
            this.btnNewMap.Click += new System.EventHandler(this.btnNewMap_Click);
            // 
            // nudScreenW
            // 
            this.nudScreenW.Location = new System.Drawing.Point(9, 41);
            this.nudScreenW.Name = "nudScreenW";
            this.nudScreenW.Size = new System.Drawing.Size(79, 20);
            this.nudScreenW.TabIndex = 1;
            this.nudScreenW.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // lblScreenSize
            // 
            this.lblScreenSize.AutoSize = true;
            this.lblScreenSize.Location = new System.Drawing.Point(6, 15);
            this.lblScreenSize.Name = "lblScreenSize";
            this.lblScreenSize.Size = new System.Drawing.Size(176, 13);
            this.lblScreenSize.TabIndex = 2;
            this.lblScreenSize.Text = "Screen size (Width, Height) in chars";
            // 
            // nudScreenH
            // 
            this.nudScreenH.Location = new System.Drawing.Point(103, 41);
            this.nudScreenH.Name = "nudScreenH";
            this.nudScreenH.Size = new System.Drawing.Size(79, 20);
            this.nudScreenH.TabIndex = 3;
            this.nudScreenH.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // lblMapSize
            // 
            this.lblMapSize.AutoSize = true;
            this.lblMapSize.Location = new System.Drawing.Point(6, 80);
            this.lblMapSize.Name = "lblMapSize";
            this.lblMapSize.Size = new System.Drawing.Size(174, 13);
            this.lblMapSize.TabIndex = 4;
            this.lblMapSize.Text = "Map size (Width, Height) in screens";
            // 
            // nudMapH
            // 
            this.nudMapH.Location = new System.Drawing.Point(103, 111);
            this.nudMapH.Name = "nudMapH";
            this.nudMapH.Size = new System.Drawing.Size(79, 20);
            this.nudMapH.TabIndex = 6;
            this.nudMapH.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // nudMapW
            // 
            this.nudMapW.Location = new System.Drawing.Point(9, 111);
            this.nudMapW.Name = "nudMapW";
            this.nudMapW.Size = new System.Drawing.Size(79, 20);
            this.nudMapW.TabIndex = 5;
            this.nudMapW.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // tbZoom
            // 
            this.tbZoom.AutoSize = false;
            this.tbZoom.LargeChange = 1;
            this.tbZoom.Location = new System.Drawing.Point(12, 13);
            this.tbZoom.Maximum = 3;
            this.tbZoom.Name = "tbZoom";
            this.tbZoom.Size = new System.Drawing.Size(130, 35);
            this.tbZoom.TabIndex = 0;
            this.tbZoom.Value = 1;
            this.tbZoom.Scroll += new System.EventHandler(this.tbZoom_Scroll);
            // 
            // cbDrawGrid
            // 
            this.cbDrawGrid.AutoSize = true;
            this.cbDrawGrid.Checked = true;
            this.cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGrid.Location = new System.Drawing.Point(162, 13);
            this.cbDrawGrid.Name = "cbDrawGrid";
            this.cbDrawGrid.Size = new System.Drawing.Size(45, 17);
            this.cbDrawGrid.TabIndex = 1;
            this.cbDrawGrid.Text = "Grid";
            this.cbDrawGrid.UseVisualStyleBackColor = true;
            this.cbDrawGrid.CheckedChanged += new System.EventHandler(this.cbDrawGrid_CheckedChanged);
            // 
            // cbDrawBorders
            // 
            this.cbDrawBorders.AutoSize = true;
            this.cbDrawBorders.Checked = true;
            this.cbDrawBorders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawBorders.Location = new System.Drawing.Point(162, 31);
            this.cbDrawBorders.Name = "cbDrawBorders";
            this.cbDrawBorders.Size = new System.Drawing.Size(98, 17);
            this.cbDrawBorders.TabIndex = 2;
            this.cbDrawBorders.Text = "Screen borders";
            this.cbDrawBorders.UseVisualStyleBackColor = true;
            this.cbDrawBorders.CheckedChanged += new System.EventHandler(this.cbDrawBorders_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 542);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "AtariMapMaker by Martin Simecek";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScreenH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMapW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelScreen;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button buttonLoadFont;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.Button buttonShiftChars;
        private System.Windows.Forms.Button btn_hoboexport;
        private System.Windows.Forms.Button btnHOBOimport;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.NumericUpDown nudMapH;
        private System.Windows.Forms.NumericUpDown nudMapW;
        private System.Windows.Forms.Label lblMapSize;
        private System.Windows.Forms.NumericUpDown nudScreenH;
        private System.Windows.Forms.Label lblScreenSize;
        private System.Windows.Forms.NumericUpDown nudScreenW;
        private System.Windows.Forms.Button btnNewMap;
        private System.Windows.Forms.TrackBar tbZoom;
        private System.Windows.Forms.CheckBox cbDrawBorders;
        private System.Windows.Forms.CheckBox cbDrawGrid;

    }
}

