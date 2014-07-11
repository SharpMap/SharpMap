namespace SharpMap.Forms
{
    partial class MiniMapControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._resizeTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // _resizeTimer
            // 
            this._resizeTimer.Interval = 500;
            this._resizeTimer.Tick += new System.EventHandler(this._resizeTimer_Tick);
            // 
            // MiniMapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.DoubleBuffered = true;
            this.Name = "MiniMapView";
            this.Size = new System.Drawing.Size(187, 146);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer _resizeTimer;
    }
}
