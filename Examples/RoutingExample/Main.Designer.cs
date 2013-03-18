namespace RoutingExample
{
    partial class Main
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
            this.mapBox1 = new SharpMap.Forms.MapBox();
            this.btnOpenShapeFile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mapBox1
            // 
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.None;
            this.mapBox1.BackColor = System.Drawing.Color.White;
            this.mapBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.mapBox1.FineZoomFactor = 10D;
            this.mapBox1.Location = new System.Drawing.Point(12, 12);
            this.mapBox1.Name = "mapBox1";
            this.mapBox1.QueryGrowFactor = 5F;
            this.mapBox1.QueryLayerIndex = 0;
            this.mapBox1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.ShowProgressUpdate = false;
            this.mapBox1.Size = new System.Drawing.Size(601, 400);
            this.mapBox1.TabIndex = 0;
            this.mapBox1.Text = "mapBox1";
            this.mapBox1.WheelZoomMagnitude = 2D;
            this.mapBox1.MouseDown += new SharpMap.Forms.MapBox.MouseEventHandler(this.mapBox1_MouseDown);
            // 
            // btnOpenShapeFile
            // 
            this.btnOpenShapeFile.Location = new System.Drawing.Point(619, 12);
            this.btnOpenShapeFile.Name = "btnOpenShapeFile";
            this.btnOpenShapeFile.Size = new System.Drawing.Size(297, 23);
            this.btnOpenShapeFile.TabIndex = 1;
            this.btnOpenShapeFile.Text = "Open A Shape File";
            this.btnOpenShapeFile.UseVisualStyleBackColor = true;
            this.btnOpenShapeFile.Click += new System.EventHandler(this.btnOpenShapeFile_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(619, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(297, 53);
            this.label1.TabIndex = 2;
            this.label1.Text = "Left clicking on a point in the map box will set the source, right clicking will " +
                "set the destination.  Only works if layer has been loaded";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(622, 104);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(294, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Perform Shortest Path Analysis";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(619, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(297, 42);
            this.label3.TabIndex = 6;
            this.label3.Text = "Clicking This Button Will Perform the shortest path analysis, assuming source and" +
                " destination poiints have been selected.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(619, 172);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Shortest Path Length:";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 424);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOpenShapeFile);
            this.Controls.Add(this.mapBox1);
            this.Name = "Main";
            this.Text = "SharpMap and Quickgraph - PROOF OF CONCEPT";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SharpMap.Forms.MapBox mapBox1;
        private System.Windows.Forms.Button btnOpenShapeFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;

        
    }
}

