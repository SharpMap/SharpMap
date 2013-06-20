using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SharpMap.Data;
using SharpMap.Rendering.Decoration;
using SharpMap.Forms;
using WinFormSamples.Samples;

namespace WinFormSamples
{
    partial class FormMapBox
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
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tbAngle = new System.Windows.Forms.TrackBar();
            this.scMapProp = new System.Windows.Forms.SplitContainer();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.mapBox1 = new SharpMap.Forms.MapBox();
            this.mapQueryToolStrip1 = new SharpMap.Forms.ToolBar.MapQueryToolStrip();
            this.mapZoomToolStrip1 = new SharpMap.Forms.ToolBar.MapZoomToolStrip(this.components);
            this.mapDigitizeGeometriesToolStrip1 = new SharpMap.Forms.ToolBar.MapDigitizeGeometriesToolStrip();
            this.mapVariableLayerToolStrip1 = new SharpMap.Forms.ToolBar.MapVariableLayerToolStrip();
            this.pgMap = new System.Windows.Forms.PropertyGrid();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.radioButton10 = new System.Windows.Forms.RadioButton();
            this.btnCreateTiles = new System.Windows.Forms.Button();
            this.lvwDecorations = new System.Windows.Forms.ListView();
            this.pgMapDecoration = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scMapProp)).BeginInit();
            this.scMapProp.Panel1.SuspendLayout();
            this.scMapProp.Panel2.SuspendLayout();
            this.scMapProp.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scMain
            // 
            this.scMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scMain.Location = new System.Drawing.Point(174, 0);
            this.scMain.Name = "scMain";
            this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.dataGridView1);
            this.scMain.Size = new System.Drawing.Size(822, 635);
            this.scMain.SplitterDistance = 449;
            this.scMain.TabIndex = 4;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(822, 182);
            this.dataGridView1.TabIndex = 4;
            // 
            // tbAngle
            // 
            this.tbAngle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbAngle.Location = new System.Drawing.Point(0, 404);
            this.tbAngle.Maximum = 180;
            this.tbAngle.Minimum = -180;
            this.tbAngle.Name = "tbAngle";
            this.tbAngle.Size = new System.Drawing.Size(600, 45);
            this.tbAngle.TabIndex = 5;
            this.tbAngle.TickFrequency = 15;
            this.tbAngle.Scroll += new System.EventHandler(this.tbAngle_Scroll);
            // 
            // scMapProp
            // 
            this.scMapProp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scMapProp.Location = new System.Drawing.Point(174, 3);
            this.scMapProp.Name = "scMapProp";
            // 
            // scMapProp.Panel1
            // 
            this.scMapProp.Panel1.Controls.Add(this.tbAngle);
            this.scMapProp.Panel1.Controls.Add(this.toolStripContainer1);
            this.scMapProp.Panel1.SizeChanged += new System.EventHandler(this.mapImage_SizeChanged);
            // 
            // scMapProp.Panel2
            // 
            this.scMapProp.Panel2.Controls.Add(this.pgMap);
            this.scMapProp.Size = new System.Drawing.Size(822, 449);
            this.scMapProp.SplitterDistance = 600;
            this.scMapProp.TabIndex = 2;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.mapBox1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(594, 345);
            this.toolStripContainer1.Location = new System.Drawing.Point(3, 3);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(594, 395);
            this.toolStripContainer1.TabIndex = 7;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapZoomToolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapQueryToolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapDigitizeGeometriesToolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapVariableLayerToolStrip1);
            // 
            // mapBox1
            // 
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
            this.mapBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.mapBox1.FineZoomFactor = 10D;
            this.mapBox1.Location = new System.Drawing.Point(3, 3);
            this.mapBox1.MapQueryMode = SharpMap.Forms.MapBox.MapQueryType.LayerByIndex;
            this.mapBox1.Name = "mapBox1";
            this.mapBox1.PreviewMode = SharpMap.Forms.MapBox.PreviewModes.Fast;
            this.mapBox1.QueryGrowFactor = 5F;
            this.mapBox1.QueryLayerIndex = 0;
            this.mapBox1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.ShowProgressUpdate = true;
            this.mapBox1.Size = new System.Drawing.Size(588, 339);
            this.mapBox1.TabIndex = 7;
            this.mapBox1.Text = "mapBox1";
            this.mapBox1.WheelZoomMagnitude = 2D;
            this.mapBox1.MapRefreshed += new System.EventHandler(this.mapImage_MapRefreshed);
            this.mapBox1.MapZoomChanged += new SharpMap.Forms.MapBox.MapZoomHandler(this.mapImage_MapZoomChanged);
            this.mapBox1.MapZooming += new SharpMap.Forms.MapBox.MapZoomHandler(this.mapImage_MapZooming);
            this.mapBox1.MapQueried += new SharpMap.Forms.MapBox.MapQueryHandler(this.mapImage_MapQueried);
            this.mapBox1.MapQueryStarted += new System.EventHandler(this.mapBox1_MapQueryStarted);
            this.mapBox1.MapQueryDone += new System.EventHandler(this.mapBox1_MapQueryEnded);
            this.mapBox1.MapCenterChanged += new SharpMap.Forms.MapBox.MapCenterChangedHandler(this.mapImage_MapCenterChanged);
            this.mapBox1.ActiveToolChanged += new SharpMap.Forms.MapBox.ActiveToolChangedHandler(this.mapImage_ActiveToolChanged);
            this.mapBox1.SizeChanged += new System.EventHandler(this.mapImage_SizeChanged);
            // 
            // mapQueryToolStrip1
            // 
            this.mapQueryToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapQueryToolStrip1.Enabled = false;
            this.mapQueryToolStrip1.Location = new System.Drawing.Point(113, 0);
            this.mapQueryToolStrip1.MapControl = this.mapBox1;
            this.mapQueryToolStrip1.Name = "mapQueryToolStrip1";
            this.mapQueryToolStrip1.Size = new System.Drawing.Size(216, 25);
            this.mapQueryToolStrip1.TabIndex = 8;
            this.mapQueryToolStrip1.Text = "mapQueryToolStrip1";
            // 
            // mapZoomToolStrip1
            // 
            this.mapZoomToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapZoomToolStrip1.Enabled = false;
            this.mapZoomToolStrip1.Location = new System.Drawing.Point(140, 25);
            this.mapZoomToolStrip1.MapControl = this.mapBox1;
            this.mapZoomToolStrip1.Name = "mapZoomToolStrip1";
            this.mapZoomToolStrip1.Size = new System.Drawing.Size(262, 25);
            this.mapZoomToolStrip1.TabIndex = 8;
            this.mapZoomToolStrip1.Text = "mapZoomToolStrip1";
            // 
            // mapDigitizeGeometriesToolStrip1
            // 
            this.mapDigitizeGeometriesToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapDigitizeGeometriesToolStrip1.Enabled = false;
            this.mapDigitizeGeometriesToolStrip1.Location = new System.Drawing.Point(3, 0);
            this.mapDigitizeGeometriesToolStrip1.MapControl = this.mapBox1;
            this.mapDigitizeGeometriesToolStrip1.Name = "mapDigitizeGeometriesToolStrip1";
            this.mapDigitizeGeometriesToolStrip1.Size = new System.Drawing.Size(110, 25);
            this.mapDigitizeGeometriesToolStrip1.TabIndex = 9;
            // 
            // mapVariableLayerToolStrip1
            // 
            this.mapVariableLayerToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapVariableLayerToolStrip1.Enabled = false;
            this.mapVariableLayerToolStrip1.Location = new System.Drawing.Point(3, 25);
            this.mapVariableLayerToolStrip1.MapControl = this.mapBox1;
            this.mapVariableLayerToolStrip1.Name = "mapVariableLayerToolStrip1";
            this.mapVariableLayerToolStrip1.Size = new System.Drawing.Size(137, 25);
            this.mapVariableLayerToolStrip1.TabIndex = 10;
            // 
            // pgMap
            // 
            this.pgMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgMap.Location = new System.Drawing.Point(0, 0);
            this.pgMap.Name = "pgMap";
            this.pgMap.SelectedObject = this.mapBox1;
            this.pgMap.Size = new System.Drawing.Size(218, 449);
            this.pgMap.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.radioButton2);
            this.flowLayoutPanel1.Controls.Add(this.radioButton3);
            this.flowLayoutPanel1.Controls.Add(this.radioButton4);
            this.flowLayoutPanel1.Controls.Add(this.radioButton1);
            this.flowLayoutPanel1.Controls.Add(this.radioButton5);
            this.flowLayoutPanel1.Controls.Add(this.radioButton6);
            this.flowLayoutPanel1.Controls.Add(this.radioButton7);
            this.flowLayoutPanel1.Controls.Add(this.radioButton8);
            this.flowLayoutPanel1.Controls.Add(this.radioButton9);
            this.flowLayoutPanel1.Controls.Add(this.radioButton10);
            this.flowLayoutPanel1.Controls.Add(this.btnCreateTiles);
            this.flowLayoutPanel1.Controls.Add(this.lvwDecorations);
            this.flowLayoutPanel1.Controls.Add(this.pgMapDecoration);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(10, 4, 4, 4);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(171, 635);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(13, 7);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(69, 17);
            this.radioButton2.TabIndex = 0;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Shapefile";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Click += new System.EventHandler(this.radioButton_Click);
            this.radioButton2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.radioButton2_MouseUp);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(13, 30);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(98, 17);
            this.radioButton3.TabIndex = 1;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "GradientTheme";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(13, 53);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(81, 17);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "WMS Client";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(13, 76);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(78, 17);
            this.radioButton1.TabIndex = 2;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "WFS Client";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(13, 99);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(97, 17);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "OGR - MapInfo";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.Click += new System.EventHandler(this.radioButton_Click);
            this.radioButton5.MouseUp += new System.Windows.Forms.MouseEventHandler(this.radioButton2_MouseUp);
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(13, 122);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(98, 17);
            this.radioButton6.TabIndex = 4;
            this.radioButton6.TabStop = true;
            this.radioButton6.Text = "GDAL - GeoTiff";
            this.radioButton6.UseVisualStyleBackColor = true;
            this.radioButton6.Click += new System.EventHandler(this.radioButton_Click);
            this.radioButton6.MouseUp += new System.Windows.Forms.MouseEventHandler(this.radioButton2_MouseUp);
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Location = new System.Drawing.Point(13, 145);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(101, 17);
            this.radioButton7.TabIndex = 5;
            this.radioButton7.TabStop = true;
            this.radioButton7.Text = "TileLayer - OSM";
            this.radioButton7.UseVisualStyleBackColor = true;
            this.radioButton7.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton8
            // 
            this.radioButton8.AutoSize = true;
            this.radioButton8.Location = new System.Drawing.Point(13, 168);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(61, 17);
            this.radioButton8.TabIndex = 6;
            this.radioButton8.TabStop = true;
            this.radioButton8.Text = "PostGis";
            this.radioButton8.UseVisualStyleBackColor = true;
            this.radioButton8.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton9
            // 
            this.radioButton9.AutoSize = true;
            this.radioButton9.Location = new System.Drawing.Point(13, 191);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(72, 17);
            this.radioButton9.TabIndex = 7;
            this.radioButton9.TabStop = true;
            this.radioButton9.Text = "SpatiaLite";
            this.radioButton9.UseVisualStyleBackColor = true;
            this.radioButton9.Click += new System.EventHandler(this.radioButton_Click);
            this.radioButton9.MouseUp += new System.Windows.Forms.MouseEventHandler(this.radioButton2_MouseUp);
            // 
            // radioButton10
            // 
            this.radioButton10.AutoSize = true;
            this.radioButton10.Location = new System.Drawing.Point(13, 214);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(105, 17);
            this.radioButton10.TabIndex = 9;
            this.radioButton10.TabStop = true;
            this.radioButton10.Text = "shp_TextOnPath";
            this.radioButton10.UseVisualStyleBackColor = true;
            this.radioButton10.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // btnCreateTiles
            // 
            this.btnCreateTiles.Location = new System.Drawing.Point(13, 237);
            this.btnCreateTiles.Name = "btnCreateTiles";
            this.btnCreateTiles.Size = new System.Drawing.Size(147, 23);
            this.btnCreateTiles.TabIndex = 11;
            this.btnCreateTiles.Text = "Create tiles";
            this.btnCreateTiles.UseVisualStyleBackColor = true;
            this.btnCreateTiles.Click += new System.EventHandler(this.btnCreateTiles_Click);
            // 
            // lvwDecorations
            // 
            this.lvwDecorations.CheckBoxes = true;
            this.lvwDecorations.Location = new System.Drawing.Point(13, 266);
            this.lvwDecorations.MultiSelect = false;
            this.lvwDecorations.Name = "lvwDecorations";
            this.lvwDecorations.Size = new System.Drawing.Size(147, 58);
            this.lvwDecorations.TabIndex = 4;
            this.lvwDecorations.UseCompatibleStateImageBehavior = false;
            this.lvwDecorations.View = System.Windows.Forms.View.List;
            this.lvwDecorations.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvwDecorations_ItemChecked);
            this.lvwDecorations.SelectedIndexChanged += new System.EventHandler(this.lvwDecorations_SelectedIndexChanged);
            // 
            // pgMapDecoration
            // 
            this.pgMapDecoration.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pgMapDecoration.Location = new System.Drawing.Point(15, 330);
            this.pgMapDecoration.Name = "pgMapDecoration";
            this.pgMapDecoration.SelectedObject = this.lvwDecorations;
            this.pgMapDecoration.Size = new System.Drawing.Size(143, 291);
            this.pgMapDecoration.TabIndex = 9;
            // 
            // FormMapBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 635);
            this.Controls.Add(this.scMapProp);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.scMain);
            this.Name = "FormMapBox";
            this.Text = "SharpMap Samples - MapBox";
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAngle)).EndInit();
            this.scMapProp.Panel1.ResumeLayout(false);
            this.scMapProp.Panel1.PerformLayout();
            this.scMapProp.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMapProp)).EndInit();
            this.scMapProp.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        void mapBox1_MapQueryStarted(object sender, EventArgs e)
        {
            dataGridView1.DataSource = _queriedData = null;
        }

        void mapBox1_MapQueryEnded(object sender, EventArgs e)
        {
            dataGridView1.DataSource = _queriedData;
        }

        private FeatureDataSet _queriedData;
        private void mapBox1_OnMapQueried(FeatureDataTable data)
        {
            if (_queriedData == null)
                _queriedData = new FeatureDataSet();
            _queriedData.Tables.Add(data);
        }

        #endregion

    private System.Windows.Forms.SplitContainer scMain;
    private System.Windows.Forms.DataGridView dataGridView1;

        private void radioButton_Click(object sender, EventArgs e)
        {
            Cursor mic = mapBox1.Cursor;
            mapBox1.Cursor = Cursors.WaitCursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                //mapImage.ActiveTool = MapImage.Tools.None;
                string text = ((RadioButton)sender).Text;

                switch (text)
                {
                    case "Shapefile":
                        mapBox1.Map = ShapefileSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GradientTheme":
                        mapBox1.Map = GradiantThemeSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WFS Client":
                        mapBox1.Map = WfsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "WMS Client":
                        mapBox1.Map = TiledWmsSample.InitializeMap();
                        //mapBox1.Map = WmsSample.InitializeMap(tbAngle.Value);
                        break;
                    case "OGR - MapInfo":
                    case "OGR - S-57":
                        mapBox1.Map = OgrSample.InitializeMap(tbAngle.Value);
                        break;
                    case "GDAL - GeoTiff":
                    case "GDAL - '.DEM'":
                    case "GDAL - '.ASC'":
                    case "GDAL - '.VRT'":
                        mapBox1.Map = GdalSample.InitializeMap(tbAngle.Value);
                        mapBox1.ActiveTool = MapBox.Tools.Pan;
                        break;
                    case "TileLayer - OSM":
                    case "TileLayer - OSM with XLS":
                    case "TileLayer - Bing Roads":
                    case "TileLayer - Bing Aerial":
                    case "TileLayer - Bing Hybrid":
                    case "TileLayer - GoogleMap":
                    case "TileLayer - GoogleSatellite":
                    case "TileLayer - GoogleTerrain":
                    case "TileLayer - GoogleLabels":
                    case "Eniro":
                        /*
                        tbAngle.Visible = text.Equals("TileLayer - GoogleLabels");
                        if (!tbAngle.Visible) tbAngle.Value = 0;
                         */
                        mapBox1.Map = TileLayerSample.InitializeMap(tbAngle.Value);
                        ((RadioButton)sender).Text = (mapBox1.Map.BackgroundLayer.Count > 0)
                            ?((RadioButton)sender).Text = mapBox1.Map.BackgroundLayer[0].LayerName
                            : ((RadioButton)sender).Text = mapBox1.Map.Layers[0].LayerName;
                        break;
                    case "PostGis":
                        mapBox1.Map = PostGisSample.InitializeMap(tbAngle.Value);
                        break;
                    case "SpatiaLite":
                        mapBox1.Map = SpatiaLiteSample.InitializeMap(tbAngle.Value);
                        break;
                    case "Oracle":
                        mapBox1.Map = OracleSample.InitializeMap(tbAngle.Value);
                        break;
                    case "shp_TextOnPath":
                        mapBox1.Map=TextOnPathSample.InitializeMapOrig(tbAngle.Value);
                        break;
                    default:
                        break;
                }

                //Add checked Map decorations
                foreach (ListViewItem checkedItem in lvwDecorations.CheckedItems)
                {
                    Type mdType;
                    if (MapDecorationTypes.TryGetValue(checkedItem.Text, out mdType))
                    {
                        IMapDecoration md = Activator.CreateInstance(mdType) as IMapDecoration;
                        mapBox1.Map.Decorations.Add(md);
                    }
                }
                
                mapBox1.Map.Size = Size;
                mapBox1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }
            Cursor = Cursors.Default;
            mapBox1.Cursor = mic;
        }

        private void mapImage_MapQueried(SharpMap.Data.FeatureDataTable data)
        {
            dataGridView1.DataSource = data as System.Data.DataTable;
        }

        private void mapImage_MapQueriedDataSet(SharpMap.Data.FeatureDataSet data)
        {
            dataGridView1.DataSource = data as System.Data.DataSet;
        }

        private void mapImage_ActiveToolChanged(MapBox.Tools tool)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapCenterChanged(GeoAPI.Geometries.Coordinate center)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapRefreshed(object sender, EventArgs e)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_MapZoomChanged(double zoom)
        {
            UpdatePropertyGrid();
            Console.WriteLine("Current Extents: {0}", mapBox1.Map.Envelope);
        }

        private void mapImage_MapZooming(double zoom)
        {
            UpdatePropertyGrid();
        }

        private void mapImage_SizeChanged(object sender, EventArgs e)
        {
            mapBox1.Refresh();
        }

        private void tbAngle_Scroll(object sender, EventArgs e)
        {
            System.Drawing.Drawing2D.Matrix matrix = new Matrix(); 
            if (tbAngle.Value != 0f)
                matrix.RotateAt(tbAngle.Value, new PointF(mapBox1.Width * 0.5f, mapBox1.Height * 0.5f));

            mapBox1.Map.MapTransform = matrix;
            AdjustRotation(mapBox1.Map.Layers, tbAngle.Value);
            AdjustRotation(mapBox1.Map.VariableLayers, tbAngle.Value);

            mapBox1.Refresh();
        }

        private void radioButton2_MouseUp(object sender, MouseEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb== null) return;

            if (e.Button != MouseButtons.Right) return;

            SharpMap.Map map = null;
            switch (rb.Name)
            {
                case "radioButton2": // ShapeFile
                    map = Samples.ShapefileSample.InitializeMap(tbAngle.Value, GetOpenFileName("Shapefile|*.shp"));
                    break;
                case "radioButton5": // Ogr
                    map = Samples.OgrSample.InitializeMap(tbAngle.Value, GetOpenFileName("Ogr Datasource|*.*"));
                    break;
                case "radioButton6": // Gdal
                    map = Samples.GdalSample.InitializeMap(tbAngle.Value, GetOpenFileName("Gdal Datasource|*.*"));
                    break;
                case "radioButton9": // spatialite
                    map = Samples.SpatiaLiteSample.InitializeMap(tbAngle.Value, GetOpenFileName("SpatiaLite 2|*.db;*.sqlite"));
                    break;
            }

            if (map == null)
                return;

            
            //Add checked Map decorations
            foreach (ListViewItem checkedItem in lvwDecorations.CheckedItems)
            {
                Type mdType;
                if (MapDecorationTypes.TryGetValue(checkedItem.Text, out mdType))
                {
                    IMapDecoration md = Activator.CreateInstance(mdType) as IMapDecoration;
                    map.Decorations.Add(md);
                }
            }

            mapBox1.Map = map;
        }

        private void lvwDecorations_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Type mdType;
            if (!MapDecorationTypes.TryGetValue(e.Item.Text, out mdType))
                return;

            if (e.Item.Checked)
            {
                IMapDecoration ins = Activator.CreateInstance(mdType) as IMapDecoration;
                if (ins != null)
                {
                       mapBox1.Map.Decorations.Add(ins);
                }
            }
            else
            {
                foreach (var item in mapBox1.Map.Decorations)
                {
                    var mdTmpType = item.GetType();
                    if (mdType.Equals(mdTmpType))
                    {
                        mapBox1.Map.Decorations.Remove(item);
                        break;
                    }
                }
            }
            e.Item.Selected = true;
            mapBox1.Refresh();
        }

        private void lvwDecorations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvwDecorations.SelectedItems.Count == 0)
            {    
                pgMapDecoration.SelectedObject = null;
                return;
            }

            var lvwi = (ListViewItem) lvwDecorations.SelectedItems[0];
            if (!lvwi.Checked)
            {
                pgMapDecoration.SelectedObject = null;
                pgMapDecoration.Visible = false;
                return;
            }

            Type mdType;
            if (MapDecorationTypes.TryGetValue(lvwi.Text, out mdType))
            {
                foreach (IMapDecoration mapDecoration in mapBox1.Map.Decorations)
                {
                    if (mapDecoration.GetType().Equals(mdType))
                    {
                        pgMapDecoration.SelectedObject = mapDecoration;
                        pgMapDecoration.Visible = true;
                        return;
                    }
                }
            }
            pgMapDecoration.SelectedObject = null;
            pgMapDecoration.Visible = false;

        }

        private SplitContainer scMapProp;
        private TrackBar tbAngle;
        private PropertyGrid pgMap;
        private FlowLayoutPanel flowLayoutPanel1;
        private RadioButton radioButton2;
        private RadioButton radioButton3;
        private RadioButton radioButton4;
        private RadioButton radioButton1;
        private RadioButton radioButton5;
        private RadioButton radioButton6;
        private RadioButton radioButton7;
        private RadioButton radioButton8;
        private RadioButton radioButton9;
        private RadioButton radioButton10;
        private ListView lvwDecorations;
        private PropertyGrid pgMapDecoration;
        private Button btnCreateTiles;
        private ToolStripContainer toolStripContainer1;
        private MapBox mapBox1;
        private SharpMap.Forms.ToolBar.MapQueryToolStrip mapQueryToolStrip1;
        private SharpMap.Forms.ToolBar.MapZoomToolStrip mapZoomToolStrip1;
        private SharpMap.Forms.ToolBar.MapDigitizeGeometriesToolStrip mapDigitizeGeometriesToolStrip1;
        private SharpMap.Forms.ToolBar.MapVariableLayerToolStrip mapVariableLayerToolStrip1;

        private void btnCreateTiles_Click(object sender, EventArgs e)
        {
            if (mapBox1.Map == null)
                return;

            if (mapBox1.Map.Layers.Count == 0)
                return;

            using (var f = new FormCreateTilesSample())
            {
                f.Map = mapBox1.Map;
                f.ShowDialog();
            }
        }
  }
}

