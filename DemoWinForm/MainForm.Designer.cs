// Copyright 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

namespace DemoWinForm
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
			this.CoordinatesLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.LayersDataGridView = new System.Windows.Forms.DataGridView();
			this.EnabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.LayerTypeColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.LayerNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LayerPropertiesColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.FeaturesDataGridView = new System.Windows.Forms.DataGridView();
			this.MainToolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.splitContainer4 = new System.Windows.Forms.SplitContainer();
			this.LayerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MoveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MoveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LayerContextMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.AddLayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RemoveLayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AddLayerDialog = new System.Windows.Forms.OpenFileDialog();
			this.MainMapImage = new SharpMap.Forms.MapImage();
			this.NewToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.AddNewRandomGeometryLayer = new System.Windows.Forms.ToolStripButton();
			this.OpenToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.SaveToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.PrintToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.CutToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.CopyToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.PasteToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.AddLayerToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.RemoveLayerToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ZoomToExtentsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ZoomOutModeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ZoomInModeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.PanToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.QueryModeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.MainStatusStrip.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LayersDataGridView)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeaturesDataGridView)).BeginInit();
			this.MainToolStrip.SuspendLayout();
			this.splitContainer4.SuspendLayout();
			this.LayerContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainMapImage)).BeginInit();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.MainStatusStrip);
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(686, 452);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(686, 499);
			this.toolStripContainer1.TabIndex = 0;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.MainToolStrip);
			// 
			// MainStatusStrip
			// 
			this.MainStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CoordinatesLabel});
			this.MainStatusStrip.Location = new System.Drawing.Point(0, 0);
			this.MainStatusStrip.Name = "MainStatusStrip";
			this.MainStatusStrip.Size = new System.Drawing.Size(686, 22);
			this.MainStatusStrip.TabIndex = 0;
			// 
			// CoordinatesLabel
			// 
			this.CoordinatesLabel.AutoSize = false;
			this.CoordinatesLabel.Name = "CoordinatesLabel";
			this.CoordinatesLabel.Size = new System.Drawing.Size(671, 17);
			this.CoordinatesLabel.Spring = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.LayersDataGridView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(686, 452);
			this.splitContainer1.SplitterDistance = 187;
			this.splitContainer1.TabIndex = 1;
			// 
			// LayersDataGridView
			// 
			this.LayersDataGridView.AllowUserToAddRows = false;
			this.LayersDataGridView.AllowUserToDeleteRows = false;
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.LayersDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
			this.LayersDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.LayersDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EnabledColumn,
            this.LayerTypeColumn,
            this.LayerNameColumn,
            this.LayerPropertiesColumn});
			this.LayersDataGridView.ContextMenuStrip = this.LayerContextMenu;
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.LayersDataGridView.DefaultCellStyle = dataGridViewCellStyle8;
			this.LayersDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LayersDataGridView.GridColor = System.Drawing.SystemColors.ControlLightLight;
			this.LayersDataGridView.Location = new System.Drawing.Point(0, 0);
			this.LayersDataGridView.MultiSelect = false;
			this.LayersDataGridView.Name = "LayersDataGridView";
			this.LayersDataGridView.ReadOnly = true;
			dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.LayersDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
			this.LayersDataGridView.RowHeadersVisible = false;
			this.LayersDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.LayersDataGridView.Size = new System.Drawing.Size(187, 452);
			this.LayersDataGridView.TabIndex = 0;
			this.LayersDataGridView.SelectionChanged += new System.EventHandler(this.LayersDataGridView_SelectionChanged);
			// 
			// EnabledColumn
			// 
			this.EnabledColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.EnabledColumn.Frozen = true;
			this.EnabledColumn.HeaderText = "";
			this.EnabledColumn.MinimumWidth = 25;
			this.EnabledColumn.Name = "EnabledColumn";
			this.EnabledColumn.ReadOnly = true;
			this.EnabledColumn.Width = 25;
			// 
			// LayerTypeColumn
			// 
			this.LayerTypeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.LayerTypeColumn.HeaderText = "";
			this.LayerTypeColumn.MinimumWidth = 25;
			this.LayerTypeColumn.Name = "LayerTypeColumn";
			this.LayerTypeColumn.ReadOnly = true;
			this.LayerTypeColumn.Width = 25;
			// 
			// LayerNameColumn
			// 
			this.LayerNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.LayerNameColumn.HeaderText = "Layer";
			this.LayerNameColumn.Name = "LayerNameColumn";
			this.LayerNameColumn.ReadOnly = true;
			// 
			// LayerPropertiesColumn
			// 
			this.LayerPropertiesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.LayerPropertiesColumn.HeaderText = "";
			this.LayerPropertiesColumn.MinimumWidth = 25;
			this.LayerPropertiesColumn.Name = "LayerPropertiesColumn";
			this.LayerPropertiesColumn.ReadOnly = true;
			this.LayerPropertiesColumn.Width = 25;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.MainMapImage);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.FeaturesDataGridView);
			this.splitContainer2.Size = new System.Drawing.Size(495, 452);
			this.splitContainer2.SplitterDistance = 277;
			this.splitContainer2.TabIndex = 0;
			// 
			// FeaturesDataGridView
			// 
			this.FeaturesDataGridView.AllowUserToAddRows = false;
			this.FeaturesDataGridView.AllowUserToDeleteRows = false;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.FeaturesDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.FeaturesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.FeaturesDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
			this.FeaturesDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeaturesDataGridView.Location = new System.Drawing.Point(0, 0);
			this.FeaturesDataGridView.Name = "FeaturesDataGridView";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.FeaturesDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.FeaturesDataGridView.Size = new System.Drawing.Size(495, 171);
			this.FeaturesDataGridView.TabIndex = 0;
			// 
			// MainToolStrip
			// 
			this.MainToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewToolStripButton,
            this.AddNewRandomGeometryLayer,
            this.toolStripSeparator4,
            this.OpenToolStripButton,
            this.SaveToolStripButton,
            this.PrintToolStripButton,
            this.toolStripSeparator,
            this.CutToolStripButton,
            this.CopyToolStripButton,
            this.PasteToolStripButton,
            this.toolStripSeparator1,
            this.AddLayerToolStripButton,
            this.RemoveLayerToolStripButton,
            this.toolStripSeparator3,
            this.ZoomToExtentsToolStripButton,
            this.ZoomOutModeToolStripButton,
            this.ZoomInModeToolStripButton,
            this.PanToolStripButton,
            this.QueryModeToolStripButton,
            this.toolStripSeparator2});
			this.MainToolStrip.Location = new System.Drawing.Point(3, 0);
			this.MainToolStrip.Name = "MainToolStrip";
			this.MainToolStrip.Size = new System.Drawing.Size(418, 25);
			this.MainToolStrip.TabIndex = 0;
			// 
			// toolStripSeparator
			// 
			this.toolStripSeparator.Name = "toolStripSeparator";
			this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// splitContainer4
			// 
			this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer4.Location = new System.Drawing.Point(0, 0);
			this.splitContainer4.Name = "splitContainer4";
			this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.splitContainer4.Size = new System.Drawing.Size(481, 358);
			this.splitContainer4.SplitterDistance = 220;
			this.splitContainer4.TabIndex = 0;
			// 
			// LayerContextMenu
			// 
			this.LayerContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoveUpToolStripMenuItem,
            this.MoveDownToolStripMenuItem,
            this.LayerContextMenuSeparator,
            this.AddLayerToolStripMenuItem,
            this.RemoveLayerToolStripMenuItem});
			this.LayerContextMenu.Name = "LayerContextMenu";
			this.LayerContextMenu.Size = new System.Drawing.Size(155, 98);
			// 
			// MoveUpToolStripMenuItem
			// 
			this.MoveUpToolStripMenuItem.Name = "MoveUpToolStripMenuItem";
			this.MoveUpToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.MoveUpToolStripMenuItem.Text = "Move &Up";
			this.MoveUpToolStripMenuItem.Visible = false;
			this.MoveUpToolStripMenuItem.Click += new System.EventHandler(this.MoveUpToolStripMenuItem_Click);
			// 
			// MoveDownToolStripMenuItem
			// 
			this.MoveDownToolStripMenuItem.Name = "MoveDownToolStripMenuItem";
			this.MoveDownToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.MoveDownToolStripMenuItem.Text = "Move &Down";
			this.MoveDownToolStripMenuItem.Visible = false;
			this.MoveDownToolStripMenuItem.Click += new System.EventHandler(this.MoveDownToolStripMenuItem_Click);
			// 
			// LayerContextMenuSeparator
			// 
			this.LayerContextMenuSeparator.Name = "LayerContextMenuSeparator";
			this.LayerContextMenuSeparator.Size = new System.Drawing.Size(151, 6);
			this.LayerContextMenuSeparator.Visible = false;
			// 
			// AddLayerToolStripMenuItem
			// 
			this.AddLayerToolStripMenuItem.Name = "AddLayerToolStripMenuItem";
			this.AddLayerToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.AddLayerToolStripMenuItem.Text = "&Add Layer";
			this.AddLayerToolStripMenuItem.Click += new System.EventHandler(this.AddLayerToolStripMenuItem_Click);
			// 
			// RemoveLayerToolStripMenuItem
			// 
			this.RemoveLayerToolStripMenuItem.Name = "RemoveLayerToolStripMenuItem";
			this.RemoveLayerToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.RemoveLayerToolStripMenuItem.Text = "&Remove Layer";
			this.RemoveLayerToolStripMenuItem.Click += new System.EventHandler(this.RemoveLayerToolStripMenuItem_Click);
			// 
			// AddLayerDialog
			// 
			this.AddLayerDialog.Filter = "Shapefiles|*.shp|All files|*.*";
			this.AddLayerDialog.InitialDirectory = ".";
			this.AddLayerDialog.Multiselect = true;
			this.AddLayerDialog.RestoreDirectory = true;
			this.AddLayerDialog.Title = "Choose Layer Data";
			// 
			// MainMapImage
			// 
			this.MainMapImage.ActiveTool = SharpMap.Forms.MapImage.Tools.None;
			this.MainMapImage.Cursor = System.Windows.Forms.Cursors.Cross;
			this.MainMapImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainMapImage.FineZoomFactor = 10;
			this.MainMapImage.Location = new System.Drawing.Point(0, 0);
			this.MainMapImage.Name = "MainMapImage";
			this.MainMapImage.QueryLayerIndex = 0;
			this.MainMapImage.Size = new System.Drawing.Size(495, 277);
			this.MainMapImage.TabIndex = 0;
			this.MainMapImage.TabStop = false;
			this.MainMapImage.WheelZoomMagnitude = 2;
			this.MainMapImage.MouseMove += new SharpMap.Forms.MapImage.MouseEventHandler(this.MainMapImage_MouseMove);
			// 
			// NewToolStripButton
			// 
			this.NewToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.NewToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("NewToolStripButton.Image")));
			this.NewToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.NewToolStripButton.Name = "NewToolStripButton";
			this.NewToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.NewToolStripButton.Text = "&New";
			// 
			// AddNewRandomGeometryLayer
			// 
			this.AddNewRandomGeometryLayer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.AddNewRandomGeometryLayer.Image = global::DemoWinForm.Properties.Resources.NewRandomGeometry;
			this.AddNewRandomGeometryLayer.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AddNewRandomGeometryLayer.Name = "AddNewRandomGeometryLayer";
			this.AddNewRandomGeometryLayer.Size = new System.Drawing.Size(23, 22);
			this.AddNewRandomGeometryLayer.Text = "toolStripButton1";
			this.AddNewRandomGeometryLayer.Click += new System.EventHandler(this.AddNewRandomGeometryLayer_Click);
			// 
			// OpenToolStripButton
			// 
			this.OpenToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.OpenToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenToolStripButton.Image")));
			this.OpenToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.OpenToolStripButton.Name = "OpenToolStripButton";
			this.OpenToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.OpenToolStripButton.Text = "&Open";
			// 
			// SaveToolStripButton
			// 
			this.SaveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.SaveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveToolStripButton.Image")));
			this.SaveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.SaveToolStripButton.Name = "SaveToolStripButton";
			this.SaveToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.SaveToolStripButton.Text = "&Save";
			// 
			// PrintToolStripButton
			// 
			this.PrintToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.PrintToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("PrintToolStripButton.Image")));
			this.PrintToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PrintToolStripButton.Name = "PrintToolStripButton";
			this.PrintToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.PrintToolStripButton.Text = "&Print";
			// 
			// CutToolStripButton
			// 
			this.CutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.CutToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("CutToolStripButton.Image")));
			this.CutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CutToolStripButton.Name = "CutToolStripButton";
			this.CutToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.CutToolStripButton.Text = "C&ut";
			// 
			// CopyToolStripButton
			// 
			this.CopyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.CopyToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("CopyToolStripButton.Image")));
			this.CopyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CopyToolStripButton.Name = "CopyToolStripButton";
			this.CopyToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.CopyToolStripButton.Text = "&Copy";
			// 
			// PasteToolStripButton
			// 
			this.PasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.PasteToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("PasteToolStripButton.Image")));
			this.PasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PasteToolStripButton.Name = "PasteToolStripButton";
			this.PasteToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.PasteToolStripButton.Text = "&Paste";
			// 
			// AddLayerToolStripButton
			// 
			this.AddLayerToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.AddLayerToolStripButton.Image = global::DemoWinForm.Properties.Resources.AddTableHS;
			this.AddLayerToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AddLayerToolStripButton.Name = "AddLayerToolStripButton";
			this.AddLayerToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.AddLayerToolStripButton.Text = "Add Layer";
			this.AddLayerToolStripButton.Click += new System.EventHandler(this.AddLayerToolStripButton_Click);
			// 
			// RemoveLayerToolStripButton
			// 
			this.RemoveLayerToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.RemoveLayerToolStripButton.Image = global::DemoWinForm.Properties.Resources.RemoveTable;
			this.RemoveLayerToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RemoveLayerToolStripButton.Name = "RemoveLayerToolStripButton";
			this.RemoveLayerToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.RemoveLayerToolStripButton.Text = "Remove Layer";
			this.RemoveLayerToolStripButton.Click += new System.EventHandler(this.RemoveLayerToolStripButton_Click);
			// 
			// ZoomToExtentsToolStripButton
			// 
			this.ZoomToExtentsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomToExtentsToolStripButton.Image = global::DemoWinForm.Properties.Resources.ZoomToExtents;
			this.ZoomToExtentsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ZoomToExtentsToolStripButton.Name = "ZoomToExtentsToolStripButton";
			this.ZoomToExtentsToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ZoomToExtentsToolStripButton.Text = "Zoom to Extents";
			this.ZoomToExtentsToolStripButton.Click += new System.EventHandler(this.ZoomToExtentsToolStripButton_Click);
			// 
			// ZoomOutModeToolStripButton
			// 
			this.ZoomOutModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomOutModeToolStripButton.Image = global::DemoWinForm.Properties.Resources.ZoomOut;
			this.ZoomOutModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ZoomOutModeToolStripButton.Name = "ZoomOutModeToolStripButton";
			this.ZoomOutModeToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ZoomOutModeToolStripButton.Text = "Zoom Out";
			this.ZoomOutModeToolStripButton.Click += new System.EventHandler(this.ZoomOutModeToolStripButton_Click);
			// 
			// ZoomInModeToolStripButton
			// 
			this.ZoomInModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomInModeToolStripButton.Image = global::DemoWinForm.Properties.Resources.ZoomIn;
			this.ZoomInModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ZoomInModeToolStripButton.Name = "ZoomInModeToolStripButton";
			this.ZoomInModeToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ZoomInModeToolStripButton.Text = "Zoom In";
			this.ZoomInModeToolStripButton.Click += new System.EventHandler(this.ZoomInModeToolStripButton_Click);
			// 
			// PanToolStripButton
			// 
			this.PanToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.PanToolStripButton.Image = global::DemoWinForm.Properties.Resources.pan;
			this.PanToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PanToolStripButton.Name = "PanToolStripButton";
			this.PanToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.PanToolStripButton.Text = "Pan";
			this.PanToolStripButton.Click += new System.EventHandler(this.PanToolStripButton_Click);
			// 
			// QueryModeToolStripButton
			// 
			this.QueryModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.QueryModeToolStripButton.Image = global::DemoWinForm.Properties.Resources.query;
			this.QueryModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.QueryModeToolStripButton.Name = "QueryModeToolStripButton";
			this.QueryModeToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.QueryModeToolStripButton.Text = "Query";
			this.QueryModeToolStripButton.Click += new System.EventHandler(this.QueryModeToolStripButton_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(686, 499);
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "MainForm";
			this.Text = "Form1";
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.MainStatusStrip.ResumeLayout(false);
			this.MainStatusStrip.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LayersDataGridView)).EndInit();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FeaturesDataGridView)).EndInit();
			this.MainToolStrip.ResumeLayout(false);
			this.MainToolStrip.PerformLayout();
			this.splitContainer4.ResumeLayout(false);
			this.LayerContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainMapImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.SplitContainer splitContainer4;
		private SharpMap.Forms.MapImage MainMapImage;
		private System.Windows.Forms.ToolStrip MainToolStrip;
		private System.Windows.Forms.ToolStripButton NewToolStripButton;
		private System.Windows.Forms.ToolStripButton OpenToolStripButton;
		private System.Windows.Forms.ToolStripButton SaveToolStripButton;
		private System.Windows.Forms.ToolStripButton PrintToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripButton CutToolStripButton;
		private System.Windows.Forms.ToolStripButton CopyToolStripButton;
		private System.Windows.Forms.ToolStripButton PasteToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.StatusStrip MainStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel CoordinatesLabel;
		private System.Windows.Forms.ToolStripButton ZoomToExtentsToolStripButton;
		private System.Windows.Forms.ToolStripButton ZoomInModeToolStripButton;
		private System.Windows.Forms.ToolStripButton QueryModeToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.DataGridView LayersDataGridView;
		private System.Windows.Forms.DataGridViewCheckBoxColumn EnabledColumn;
		private System.Windows.Forms.DataGridViewImageColumn LayerTypeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn LayerNameColumn;
		private System.Windows.Forms.DataGridViewButtonColumn LayerPropertiesColumn;
		private System.Windows.Forms.DataGridView FeaturesDataGridView;
		private System.Windows.Forms.ToolStripButton AddLayerToolStripButton;
		private System.Windows.Forms.ToolStripButton RemoveLayerToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ContextMenuStrip LayerContextMenu;
		private System.Windows.Forms.ToolStripMenuItem MoveUpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MoveDownToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator LayerContextMenuSeparator;
		private System.Windows.Forms.ToolStripMenuItem AddLayerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RemoveLayerToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog AddLayerDialog;
		private System.Windows.Forms.ToolStripButton ZoomOutModeToolStripButton;
		private System.Windows.Forms.ToolStripButton PanToolStripButton;
		private System.Windows.Forms.ToolStripButton AddNewRandomGeometryLayer;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
	}
}

