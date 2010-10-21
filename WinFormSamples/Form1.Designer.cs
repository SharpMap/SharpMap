namespace WinFormSamples
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
        this.scMain = new System.Windows.Forms.SplitContainer();
        this.scMapProp = new System.Windows.Forms.SplitContainer();
        this.tbAngle = new System.Windows.Forms.TrackBar();
        this.mapImage = new SharpMap.Forms.MapImage();
        this.pgMap = new System.Windows.Forms.PropertyGrid();
        this.dataGridView1 = new System.Windows.Forms.DataGridView();
        this.flowLayoutPanel1.SuspendLayout();
        this.scMain.Panel1.SuspendLayout();
        this.scMain.Panel2.SuspendLayout();
        this.scMain.SuspendLayout();
        this.scMapProp.Panel1.SuspendLayout();
        this.scMapProp.Panel2.SuspendLayout();
        this.scMapProp.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.tbAngle)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.mapImage)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
        this.SuspendLayout();
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
        this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
        this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
        this.flowLayoutPanel1.Name = "flowLayoutPanel1";
        this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(10, 4, 4, 4);
        this.flowLayoutPanel1.Size = new System.Drawing.Size(166, 446);
        this.flowLayoutPanel1.TabIndex = 2;
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
        // 
        // radioButton10
        // 
        this.radioButton10.AutoSize = true;
        this.radioButton10.Location = new System.Drawing.Point(13, 214);
        this.radioButton10.Name = "radioButton10";
        this.radioButton10.Size = new System.Drawing.Size(56, 17);
        this.radioButton10.TabIndex = 8;
        this.radioButton10.TabStop = true;
        this.radioButton10.Text = "Oracle";
        this.radioButton10.UseVisualStyleBackColor = true;
        this.radioButton10.Click += new System.EventHandler(this.radioButton_Click);
        // 
        // scMain
        // 
        this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.scMain.Location = new System.Drawing.Point(166, 0);
        this.scMain.Name = "scMain";
        this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // scMain.Panel1
        // 
        this.scMain.Panel1.Controls.Add(this.scMapProp);
        // 
        // scMain.Panel2
        // 
        this.scMain.Panel2.Controls.Add(this.dataGridView1);
        this.scMain.Size = new System.Drawing.Size(523, 446);
        this.scMain.SplitterDistance = 316;
        this.scMain.TabIndex = 4;
        // 
        // scMapProp
        // 
        this.scMapProp.Dock = System.Windows.Forms.DockStyle.Fill;
        this.scMapProp.Location = new System.Drawing.Point(0, 0);
        this.scMapProp.Name = "scMapProp";
        // 
        // scMapProp.Panel1
        // 
        this.scMapProp.Panel1.Controls.Add(this.tbAngle);
        this.scMapProp.Panel1.Controls.Add(this.mapImage);
        // 
        // scMapProp.Panel2
        // 
        this.scMapProp.Panel2.Controls.Add(this.pgMap);
        this.scMapProp.Size = new System.Drawing.Size(523, 316);
        this.scMapProp.SplitterDistance = 383;
        this.scMapProp.TabIndex = 2;
        // 
        // tbAngle
        // 
        this.tbAngle.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.tbAngle.Location = new System.Drawing.Point(0, 271);
        this.tbAngle.Maximum = 360;
        this.tbAngle.Name = "tbAngle";
        this.tbAngle.Size = new System.Drawing.Size(383, 45);
        this.tbAngle.TabIndex = 5;
        this.tbAngle.TickFrequency = 15;
        this.tbAngle.Visible = false;
        this.tbAngle.Scroll += new System.EventHandler(this.tbAngle_Scroll);
        // 
        // mapImage
        // 
        this.mapImage.ActiveTool = SharpMap.Forms.MapImage.Tools.None;
        this.mapImage.BackColor = System.Drawing.Color.White;
        this.mapImage.Cursor = System.Windows.Forms.Cursors.Cross;
        this.mapImage.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mapImage.FineZoomFactor = 10;
        this.mapImage.Location = new System.Drawing.Point(0, 0);
        this.mapImage.Name = "mapImage";
        this.mapImage.QueryLayerIndex = 0;
        this.mapImage.Size = new System.Drawing.Size(383, 316);
        this.mapImage.TabIndex = 1;
        this.mapImage.TabStop = false;
        this.mapImage.WheelZoomMagnitude = 2;
        this.mapImage.MapZooming += new SharpMap.Forms.MapImage.MapZoomHandler(this.mapImage_MapZooming);
        this.mapImage.MapRefreshed += new System.EventHandler(this.mapImage_MapRefreshed);
        this.mapImage.MapZoomChanged += new SharpMap.Forms.MapImage.MapZoomHandler(this.mapImage_MapZoomChanged);
        this.mapImage.ActiveToolChanged += new SharpMap.Forms.MapImage.ActiveToolChangedHandler(this.mapImage_ActiveToolChanged);
        this.mapImage.SizeChanged += new System.EventHandler(this.mapImage_SizeChanged);
        this.mapImage.MapQueried += new SharpMap.Forms.MapImage.MapQueryHandler(this.mapImage_MapQueried);
        this.mapImage.MapCenterChanged += new SharpMap.Forms.MapImage.MapCenterChangedHandler(this.mapImage_MapCenterChanged);
        // 
        // pgMap
        // 
        this.pgMap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pgMap.Location = new System.Drawing.Point(0, 0);
        this.pgMap.Name = "pgMap";
        this.pgMap.SelectedObject = this.mapImage;
        this.pgMap.Size = new System.Drawing.Size(136, 316);
        this.pgMap.TabIndex = 3;
        // 
        // dataGridView1
        // 
        this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dataGridView1.Location = new System.Drawing.Point(0, 0);
        this.dataGridView1.Name = "dataGridView1";
        this.dataGridView1.Size = new System.Drawing.Size(523, 126);
        this.dataGridView1.TabIndex = 4;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(689, 446);
        this.Controls.Add(this.scMain);
        this.Controls.Add(this.flowLayoutPanel1);
        this.Name = "MainForm";
        this.Text = "SharpMap Samples";
        this.flowLayoutPanel1.ResumeLayout(false);
        this.flowLayoutPanel1.PerformLayout();
        this.scMain.Panel1.ResumeLayout(false);
        this.scMain.Panel2.ResumeLayout(false);
        this.scMain.ResumeLayout(false);
        this.scMapProp.Panel1.ResumeLayout(false);
        this.scMapProp.Panel1.PerformLayout();
        this.scMapProp.Panel2.ResumeLayout(false);
        this.scMapProp.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.tbAngle)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.mapImage)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
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
    private System.Windows.Forms.SplitContainer scMain;
    private System.Windows.Forms.SplitContainer scMapProp;
    private SharpMap.Forms.MapImage mapImage;
    private System.Windows.Forms.PropertyGrid pgMap;
    private System.Windows.Forms.DataGridView dataGridView1;
    private System.Windows.Forms.TrackBar tbAngle;
  }
}

