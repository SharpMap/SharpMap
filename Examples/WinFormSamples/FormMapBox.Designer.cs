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
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.btnTool2 = new System.Windows.Forms.Button();
            this.btnTool = new System.Windows.Forms.Button();
            this.mapBox1 = new SharpMap.Forms.MapBox();
            this.mapDigitizeGeometriesToolStrip1 = new SharpMap.Forms.ToolBar.MapDigitizeGeometriesToolStrip(this.components);
            this.mapVariableLayerToolStrip1 = new SharpMap.Forms.ToolBar.MapVariableLayerToolStrip(this.components);
            this.mapQueryToolStrip1 = new SharpMap.Forms.ToolBar.MapQueryToolStrip(this.components);
            this.mapZoomToolStrip1 = new SharpMap.Forms.ToolBar.MapZoomToolStrip(this.components);
            this.tbAngle = new System.Windows.Forms.TrackBar();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pgMap = new System.Windows.Forms.PropertyGrid();
            this.flowLayoutRight = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.radioButton12 = new System.Windows.Forms.RadioButton();
            this.radioButton10 = new System.Windows.Forms.RadioButton();
            this.radioButton11 = new System.Windows.Forms.RadioButton();
            this.btnCreateTiles = new System.Windows.Forms.Button();
            this.lvwDecorations = new System.Windows.Forms.ListView();
            this.pgMapDecoration = new System.Windows.Forms.PropertyGrid();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.flowLayoutRight.SuspendLayout();
            this.flowLayoutLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // scMain
            // 
            this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMain.Location = new System.Drawing.Point(171, 0);
            this.scMain.Name = "scMain";
            this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel1
            // 
            this.scMain.Panel1.Controls.Add(this.toolStripContainer1);
            this.scMain.Panel1.Controls.Add(this.tbAngle);
            this.scMain.Panel1.SizeChanged += new System.EventHandler(this.mapImage_SizeChanged);
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.dataGridView1);
            this.scMain.Size = new System.Drawing.Size(720, 648);
            this.scMain.SplitterDistance = 525;
            this.scMain.TabIndex = 4;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnTool2);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnTool);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.mapBox1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(720, 430);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(720, 480);
            this.toolStripContainer1.TabIndex = 7;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapDigitizeGeometriesToolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapVariableLayerToolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapQueryToolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mapZoomToolStrip1);
            // 
            // btnTool2
            // 
            this.btnTool2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTool2.Location = new System.Drawing.Point(664, 32);
            this.btnTool2.Name = "btnTool2";
            this.btnTool2.Size = new System.Drawing.Size(53, 23);
            this.btnTool2.TabIndex = 9;
            this.btnTool2.Text = "Magnify";
            this.btnTool2.UseVisualStyleBackColor = true;
            this.btnTool2.Click += new System.EventHandler(this.btnTool_Click);
            // 
            // btnTool
            // 
            this.btnTool.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTool.Location = new System.Drawing.Point(664, 3);
            this.btnTool.Name = "btnTool";
            this.btnTool.Size = new System.Drawing.Size(53, 23);
            this.btnTool.TabIndex = 8;
            this.btnTool.Text = "Hover";
            this.btnTool.UseVisualStyleBackColor = true;
            this.btnTool.Click += new System.EventHandler(this.btnTool_Click);
            // 
            // mapBox1
            // 
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
            this.mapBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.mapBox1.CustomTool = null;
            this.mapBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapBox1.FineZoomFactor = 10D;
            this.mapBox1.Location = new System.Drawing.Point(0, 0);
            this.mapBox1.MapQueryMode = SharpMap.Forms.MapBox.MapQueryType.LayerByIndex;
            this.mapBox1.Margin = new System.Windows.Forms.Padding(0);
            this.mapBox1.Name = "mapBox1";
            this.mapBox1.QueryGrowFactor = 5F;
            this.mapBox1.QueryLayerIndex = 0;
            this.mapBox1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.ShowProgressUpdate = true;
            this.mapBox1.Size = new System.Drawing.Size(720, 430);
            this.mapBox1.TabIndex = 7;
            this.mapBox1.Text = "mapBox1";
            this.mapBox1.WheelZoomMagnitude = 2D;
            this.mapBox1.MapRefreshed += new System.EventHandler(this.mapImage_MapRefreshed);
            this.mapBox1.MapZoomChanged += new SharpMap.Forms.MapBox.MapZoomHandler(this.mapImage_MapZoomChanged);
            this.mapBox1.MapZooming += new SharpMap.Forms.MapBox.MapZoomHandler(this.mapImage_MapZooming);
            this.mapBox1.MapQueried += new SharpMap.Forms.MapBox.MapQueryHandler(this.mapBox1_OnMapQueried);
            this.mapBox1.MapQueryStarted += new System.EventHandler(this.mapBox1_MapQueryStarted);
            this.mapBox1.MapQueryDone += new System.EventHandler(this.mapBox1_MapQueryEnded);
            this.mapBox1.MapCenterChanged += new SharpMap.Forms.MapBox.MapCenterChangedHandler(this.mapImage_MapCenterChanged);
            this.mapBox1.ActiveToolChanged += new SharpMap.Forms.MapBox.ActiveToolChangedHandler(this.mapImage_ActiveToolChanged);
            this.mapBox1.SizeChanged += new System.EventHandler(this.mapImage_SizeChanged);
            // 
            // mapDigitizeGeometriesToolStrip1
            // 
            this.mapDigitizeGeometriesToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapDigitizeGeometriesToolStrip1.Enabled = false;
            this.mapDigitizeGeometriesToolStrip1.Location = new System.Drawing.Point(358, 0);
            this.mapDigitizeGeometriesToolStrip1.MapControl = this.mapBox1;
            this.mapDigitizeGeometriesToolStrip1.Name = "mapDigitizeGeometriesToolStrip1";
            this.mapDigitizeGeometriesToolStrip1.Size = new System.Drawing.Size(110, 25);
            this.mapDigitizeGeometriesToolStrip1.TabIndex = 9;
            // 
            // mapVariableLayerToolStrip1
            // 
            this.mapVariableLayerToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapVariableLayerToolStrip1.Enabled = false;
            this.mapVariableLayerToolStrip1.Location = new System.Drawing.Point(3, 0);
            this.mapVariableLayerToolStrip1.MapControl = this.mapBox1;
            this.mapVariableLayerToolStrip1.Name = "mapVariableLayerToolStrip1";
            this.mapVariableLayerToolStrip1.Size = new System.Drawing.Size(137, 25);
            this.mapVariableLayerToolStrip1.TabIndex = 10;
            // 
            // mapQueryToolStrip1
            // 
            this.mapQueryToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.mapQueryToolStrip1.Enabled = false;
            this.mapQueryToolStrip1.Location = new System.Drawing.Point(140, 0);
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
            this.mapZoomToolStrip1.Location = new System.Drawing.Point(3, 25);
            this.mapZoomToolStrip1.MapControl = this.mapBox1;
            this.mapZoomToolStrip1.Name = "mapZoomToolStrip1";
            this.mapZoomToolStrip1.Size = new System.Drawing.Size(408, 25);
            this.mapZoomToolStrip1.TabIndex = 8;
            this.mapZoomToolStrip1.Text = "mapZoomToolStrip1";
            // 
            // tbAngle
            // 
            this.tbAngle.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tbAngle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbAngle.LargeChange = 45;
            this.tbAngle.Location = new System.Drawing.Point(0, 480);
            this.tbAngle.Maximum = 180;
            this.tbAngle.Minimum = -180;
            this.tbAngle.Name = "tbAngle";
            this.tbAngle.Size = new System.Drawing.Size(720, 45);
            this.tbAngle.SmallChange = 15;
            this.tbAngle.TabIndex = 5;
            this.tbAngle.TickFrequency = 15;
            this.tbAngle.Scroll += new System.EventHandler(this.tbAngle_Scroll);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(720, 119);
            this.dataGridView1.TabIndex = 4;
            // 
            // pgMap
            // 
            this.pgMap.Location = new System.Drawing.Point(3, 3);
            this.pgMap.Name = "pgMap";
            this.pgMap.SelectedObject = this.mapBox1;
            this.pgMap.Size = new System.Drawing.Size(172, 644);
            this.pgMap.TabIndex = 3;
            // 
            // flowLayoutRight
            // 
            this.flowLayoutRight.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flowLayoutRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutRight.Controls.Add(this.pgMap);
            this.flowLayoutRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutRight.Location = new System.Drawing.Point(891, 0);
            this.flowLayoutRight.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutRight.Name = "flowLayoutRight";
            this.flowLayoutRight.Size = new System.Drawing.Size(175, 648);
            this.flowLayoutRight.TabIndex = 5;
            // 
            // flowLayoutLeft
            // 
            this.flowLayoutLeft.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flowLayoutLeft.Controls.Add(this.radioButton2);
            this.flowLayoutLeft.Controls.Add(this.radioButton3);
            this.flowLayoutLeft.Controls.Add(this.radioButton4);
            this.flowLayoutLeft.Controls.Add(this.radioButton1);
            this.flowLayoutLeft.Controls.Add(this.radioButton5);
            this.flowLayoutLeft.Controls.Add(this.radioButton6);
            this.flowLayoutLeft.Controls.Add(this.radioButton7);
            this.flowLayoutLeft.Controls.Add(this.radioButton8);
            this.flowLayoutLeft.Controls.Add(this.radioButton9);
            this.flowLayoutLeft.Controls.Add(this.radioButton12);
            this.flowLayoutLeft.Controls.Add(this.radioButton10);
            this.flowLayoutLeft.Controls.Add(this.radioButton11);
            this.flowLayoutLeft.Controls.Add(this.btnCreateTiles);
            this.flowLayoutLeft.Controls.Add(this.lvwDecorations);
            this.flowLayoutLeft.Controls.Add(this.pgMapDecoration);
            this.flowLayoutLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutLeft.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutLeft.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutLeft.Name = "flowLayoutLeft";
            this.flowLayoutLeft.Padding = new System.Windows.Forms.Padding(10, 4, 4, 4);
            this.flowLayoutLeft.Size = new System.Drawing.Size(171, 648);
            this.flowLayoutLeft.TabIndex = 5;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(13, 7);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(69, 17);
            this.radioButton2.TabIndex = 0;
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
            this.radioButton9.Text = "SpatiaLite";
            this.radioButton9.UseVisualStyleBackColor = true;
            this.radioButton9.Click += new System.EventHandler(this.radioButton_Click);
            this.radioButton9.MouseUp += new System.Windows.Forms.MouseEventHandler(this.radioButton2_MouseUp);
            // 
            // radioButton12
            // 
            this.radioButton12.AutoSize = true;
            this.radioButton12.Location = new System.Drawing.Point(13, 214);
            this.radioButton12.Name = "radioButton12";
            this.radioButton12.Size = new System.Drawing.Size(71, 17);
            this.radioButton12.TabIndex = 13;
            this.radioButton12.Text = "SqlServer";
            this.radioButton12.UseVisualStyleBackColor = true;
            this.radioButton12.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton10
            // 
            this.radioButton10.AutoSize = true;
            this.radioButton10.Location = new System.Drawing.Point(13, 237);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(105, 17);
            this.radioButton10.TabIndex = 9;
            this.radioButton10.Text = "shp_TextOnPath";
            this.radioButton10.UseVisualStyleBackColor = true;
            this.radioButton10.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // radioButton11
            // 
            this.radioButton11.AutoSize = true;
            this.radioButton11.Location = new System.Drawing.Point(13, 260);
            this.radioButton11.Name = "radioButton11";
            this.radioButton11.Size = new System.Drawing.Size(96, 17);
            this.radioButton11.TabIndex = 12;
            this.radioButton11.Text = "GdiImageLayer";
            this.radioButton11.UseVisualStyleBackColor = true;
            this.radioButton11.Click += new System.EventHandler(this.radioButton_Click);
            // 
            // btnCreateTiles
            // 
            this.btnCreateTiles.Location = new System.Drawing.Point(13, 283);
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
            this.lvwDecorations.Location = new System.Drawing.Point(13, 312);
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
            this.pgMapDecoration.Location = new System.Drawing.Point(15, 376);
            this.pgMapDecoration.Name = "pgMapDecoration";
            this.pgMapDecoration.SelectedObject = this.lvwDecorations;
            this.pgMapDecoration.Size = new System.Drawing.Size(143, 262);
            this.pgMapDecoration.TabIndex = 9;
            // 
            // FormMapBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1066, 648);
            this.Controls.Add(this.scMain);
            this.Controls.Add(this.flowLayoutLeft);
            this.Controls.Add(this.flowLayoutRight);
            this.Name = "FormMapBox";
            this.Text = "SharpMap Samples - MapBox";
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel1.PerformLayout();
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.flowLayoutRight.ResumeLayout(false);
            this.flowLayoutLeft.ResumeLayout(false);
            this.flowLayoutLeft.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.DataGridView dataGridView1;
        private WinFormSamples.Samples.FormSqlServerOpts formSqlServerOpts;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutRight;
        private System.Windows.Forms.PropertyGrid pgMap;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutLeft;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.ListView lvwDecorations;
        private System.Windows.Forms.PropertyGrid pgMapDecoration;
        private System.Windows.Forms.Button btnCreateTiles;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private SharpMap.Forms.MapBox mapBox1;
        private SharpMap.Forms.ToolBar.MapQueryToolStrip mapQueryToolStrip1;
        private SharpMap.Forms.ToolBar.MapZoomToolStrip mapZoomToolStrip1;
        private SharpMap.Forms.ToolBar.MapDigitizeGeometriesToolStrip mapDigitizeGeometriesToolStrip1;
        private SharpMap.Forms.ToolBar.MapVariableLayerToolStrip mapVariableLayerToolStrip1;
        private System.Windows.Forms.RadioButton radioButton11;
        private System.Windows.Forms.TrackBar tbAngle;
        private System.Windows.Forms.Button btnTool;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnTool2;
        private System.Windows.Forms.RadioButton radioButton12;
    }
}

