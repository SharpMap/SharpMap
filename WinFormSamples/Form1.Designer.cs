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
        this.mapImage = new SharpMap.Forms.MapImage();
        this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
        this.radioButton2 = new System.Windows.Forms.RadioButton();
        this.radioButton3 = new System.Windows.Forms.RadioButton();
        this.radioButton4 = new System.Windows.Forms.RadioButton();
        this.radioButton1 = new System.Windows.Forms.RadioButton();
        this.radioButton5 = new System.Windows.Forms.RadioButton();
        this.radioButton6 = new System.Windows.Forms.RadioButton();
        this.radioButton7 = new System.Windows.Forms.RadioButton();
        this.radioButton8 = new System.Windows.Forms.RadioButton();
        ((System.ComponentModel.ISupportInitialize)(this.mapImage)).BeginInit();
        this.flowLayoutPanel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // mapImage
        // 
        this.mapImage.ActiveTool = SharpMap.Forms.MapImage.Tools.None;
        this.mapImage.BackColor = System.Drawing.Color.White;
        this.mapImage.Cursor = System.Windows.Forms.Cursors.Cross;
        this.mapImage.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mapImage.FineZoomFactor = 10;
        this.mapImage.Location = new System.Drawing.Point(219, 0);
        this.mapImage.Name = "mapImage";
        this.mapImage.QueryLayerIndex = 0;
        this.mapImage.Size = new System.Drawing.Size(470, 446);
        this.mapImage.TabIndex = 0;
        this.mapImage.TabStop = false;
        this.mapImage.WheelZoomMagnitude = 2;
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
        this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
        this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
        this.flowLayoutPanel1.Name = "flowLayoutPanel1";
        this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(10, 4, 4, 4);
        this.flowLayoutPanel1.Size = new System.Drawing.Size(219, 446);
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
        this.radioButton7.Size = new System.Drawing.Size(78, 17);
        this.radioButton7.TabIndex = 5;
        this.radioButton7.TabStop = true;
        this.radioButton7.Text = "Tiled WMS";
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
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(689, 446);
        this.Controls.Add(this.mapImage);
        this.Controls.Add(this.flowLayoutPanel1);
        this.Name = "MainForm";
        this.Text = "SharpMap Samples";
        ((System.ComponentModel.ISupportInitialize)(this.mapImage)).EndInit();
        this.flowLayoutPanel1.ResumeLayout(false);
        this.flowLayoutPanel1.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private SharpMap.Forms.MapImage mapImage;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton radioButton4;
    private System.Windows.Forms.RadioButton radioButton5;
    private System.Windows.Forms.RadioButton radioButton6;
    private System.Windows.Forms.RadioButton radioButton7;
    private System.Windows.Forms.RadioButton radioButton8;
  }
}

