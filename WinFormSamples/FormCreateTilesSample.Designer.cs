namespace WinFormSamples
{
    partial class FormCreateTilesSample
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
            this.btnExit = new System.Windows.Forms.Button();
            this.btnDo = new System.Windows.Forms.Button();
            this.lblFolderHeader = new System.Windows.Forms.Label();
            this.lblZoomLevels = new System.Windows.Forms.Label();
            this.lblFolder = new System.Windows.Forms.Label();
            this.btnFolder = new System.Windows.Forms.Button();
            this.fbdFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.txtZoomLevels = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbOpacity = new System.Windows.Forms.TrackBar();
            this.chkSampleWebPage = new System.Windows.Forms.CheckBox();
            this.txtGoogleMapsApiKey = new System.Windows.Forms.TextBox();
            this.chkMercator = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbOpacity)).BeginInit();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(250, 227);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 0;
            this.btnExit.Text = "&Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnDo_Click);
            // 
            // btnDo
            // 
            this.btnDo.Location = new System.Drawing.Point(331, 227);
            this.btnDo.Name = "btnDo";
            this.btnDo.Size = new System.Drawing.Size(75, 23);
            this.btnDo.TabIndex = 1;
            this.btnDo.Text = "&Do";
            this.btnDo.UseVisualStyleBackColor = true;
            this.btnDo.Click += new System.EventHandler(this.btnDo_Click);
            // 
            // lblFolderHeader
            // 
            this.lblFolderHeader.AutoSize = true;
            this.lblFolderHeader.Location = new System.Drawing.Point(12, 9);
            this.lblFolderHeader.Name = "lblFolderHeader";
            this.lblFolderHeader.Size = new System.Drawing.Size(99, 13);
            this.lblFolderHeader.TabIndex = 2;
            this.lblFolderHeader.Text = "Create tiles in folder";
            // 
            // lblZoomLevels
            // 
            this.lblZoomLevels.AutoSize = true;
            this.lblZoomLevels.Location = new System.Drawing.Point(12, 71);
            this.lblZoomLevels.Name = "lblZoomLevels";
            this.lblZoomLevels.Size = new System.Drawing.Size(163, 13);
            this.lblZoomLevels.TabIndex = 3;
            this.lblZoomLevels.Text = "Zoom levels to create (e.g. 1-3;5)";
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(12, 22);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(35, 13);
            this.lblFolder.TabIndex = 4;
            this.lblFolder.Text = "label2";
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(379, 17);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(28, 23);
            this.btnFolder.TabIndex = 5;
            this.btnFolder.Text = "...";
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // txtZoomLevels
            // 
            this.txtZoomLevels.Location = new System.Drawing.Point(15, 87);
            this.txtZoomLevels.Name = "txtZoomLevels";
            this.txtZoomLevels.Size = new System.Drawing.Size(392, 20);
            this.txtZoomLevels.TabIndex = 6;
            this.txtZoomLevels.Text = "0-4";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Opacity of tiles";
            // 
            // tbOpacity
            // 
            this.tbOpacity.Location = new System.Drawing.Point(15, 130);
            this.tbOpacity.Maximum = 100;
            this.tbOpacity.Name = "tbOpacity";
            this.tbOpacity.Size = new System.Drawing.Size(392, 45);
            this.tbOpacity.TabIndex = 9;
            this.tbOpacity.TickFrequency = 5;
            this.tbOpacity.Value = 30;
            // 
            // chkSampleWebPage
            // 
            this.chkSampleWebPage.AutoSize = true;
            this.chkSampleWebPage.Location = new System.Drawing.Point(15, 174);
            this.chkSampleWebPage.Name = "chkSampleWebPage";
            this.chkSampleWebPage.Size = new System.Drawing.Size(143, 17);
            this.chkSampleWebPage.TabIndex = 10;
            this.chkSampleWebPage.Text = "Create sample web page";
            this.chkSampleWebPage.UseVisualStyleBackColor = true;
            this.chkSampleWebPage.CheckedChanged += new System.EventHandler(this.chkSampleWebPage_CheckedChanged);
            // 
            // txtGoogleMapsApiKey
            // 
            this.txtGoogleMapsApiKey.Enabled = false;
            this.txtGoogleMapsApiKey.Location = new System.Drawing.Point(36, 197);
            this.txtGoogleMapsApiKey.Name = "txtGoogleMapsApiKey";
            this.txtGoogleMapsApiKey.Size = new System.Drawing.Size(371, 20);
            this.txtGoogleMapsApiKey.TabIndex = 11;
            // 
            // chkMercator
            // 
            this.chkMercator.AutoSize = true;
            this.chkMercator.Checked = true;
            this.chkMercator.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMercator.Location = new System.Drawing.Point(12, 47);
            this.chkMercator.Name = "chkMercator";
            this.chkMercator.Size = new System.Drawing.Size(167, 17);
            this.chkMercator.TabIndex = 12;
            this.chkMercator.Text = "Transform to Google Mercator";
            this.chkMercator.UseVisualStyleBackColor = true;
            // 
            // FormCreateTilesSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 262);
            this.Controls.Add(this.chkMercator);
            this.Controls.Add(this.txtGoogleMapsApiKey);
            this.Controls.Add(this.chkSampleWebPage);
            this.Controls.Add(this.tbOpacity);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtZoomLevels);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.lblZoomLevels);
            this.Controls.Add(this.lblFolderHeader);
            this.Controls.Add(this.btnDo);
            this.Controls.Add(this.btnExit);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCreateTilesSample";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "CreateTilesSample";
            ((System.ComponentModel.ISupportInitialize)(this.tbOpacity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnDo;
        private System.Windows.Forms.Label lblFolderHeader;
        private System.Windows.Forms.Label lblZoomLevels;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.FolderBrowserDialog fbdFolder;
        private System.Windows.Forms.TextBox txtZoomLevels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar tbOpacity;
        private System.Windows.Forms.CheckBox chkSampleWebPage;
        private System.Windows.Forms.TextBox txtGoogleMapsApiKey;
        private System.Windows.Forms.CheckBox chkMercator;
    }
}