namespace SharpMap.Forms
{
    partial class WktGeometryCreator
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
            this.cboWktKeywords = new System.Windows.Forms.ComboBox();
            this.txtWkt = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cboWktKeywords
            // 
            this.cboWktKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboWktKeywords.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWktKeywords.FormattingEnabled = true;
            this.cboWktKeywords.Location = new System.Drawing.Point(12, 12);
            this.cboWktKeywords.Name = "cboWktKeywords";
            this.cboWktKeywords.Size = new System.Drawing.Size(268, 21);
            this.cboWktKeywords.TabIndex = 0;
            // 
            // txtWkt
            // 
            this.txtWkt.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWkt.Location = new System.Drawing.Point(12, 39);
            this.txtWkt.Multiline = true;
            this.txtWkt.Name = "txtWkt";
            this.txtWkt.Size = new System.Drawing.Size(268, 186);
            this.txtWkt.TabIndex = 1;
            this.txtWkt.TextChanged += new System.EventHandler(this.txtWkt_TextChanged);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(205, 231);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "&OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // WktGeometryCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtWkt);
            this.Controls.Add(this.cboWktKeywords);
            this.Name = "WktGeometryCreator";
            this.Text = "WktGeometryCreator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboWktKeywords;
        private System.Windows.Forms.TextBox txtWkt;
        private System.Windows.Forms.Button btnOk;
    }
}