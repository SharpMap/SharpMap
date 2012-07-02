using System;
using System.Collections.Generic;

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
            this.lblError = new System.Windows.Forms.Label();
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
            this.cboWktKeywords.Size = new System.Drawing.Size(516, 21);
            this.cboWktKeywords.TabIndex = 0;
            this.cboWktKeywords.SelectedIndexChanged += OnSelectedIndexChanged;
            // 
            // txtWkt
            // 
            this.txtWkt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWkt.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWkt.Location = new System.Drawing.Point(12, 39);
            this.txtWkt.Multiline = true;
            this.txtWkt.Name = "txtWkt";
            this.txtWkt.Size = new System.Drawing.Size(516, 158);
            this.txtWkt.TabIndex = 1;
            this.txtWkt.TextChanged += new System.EventHandler(this.txtWkt_TextChanged);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(453, 231);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "&OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(9, 200);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(35, 13);
            this.lblError.TabIndex = 3;
            this.lblError.Text = "label1";
            // 
            // WktGeometryCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 266);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtWkt);
            this.Controls.Add(this.cboWktKeywords);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WktGeometryCreator";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WktGeometryCreator";
            this.Load += new System.EventHandler(this.WktGeometryCreator_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void OnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            if (cboWktKeywords.SelectedIndex == -1)
                return;

            txtWkt.Text = ((KeyValuePair<string, string>) cboWktKeywords.SelectedItem).Value;
        }

        #endregion

        private System.Windows.Forms.ComboBox cboWktKeywords;
        private System.Windows.Forms.TextBox txtWkt;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label lblError;
    }
}