namespace WinFormSamples.Samples
{
    partial class FormSqlServerOpts
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSqlServerOpts));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labTable = new System.Windows.Forms.Label();
            this.chkSpatialValidate = new System.Windows.Forms.CheckBox();
            this.optSpatialGeog = new System.Windows.Forms.RadioButton();
            this.optSpatialGeom = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.optExtentsIndividual = new System.Windows.Forms.RadioButton();
            this.labExtentsTime = new System.Windows.Forms.Label();
            this.cmdGetExtents = new System.Windows.Forms.Button();
            this.optExtentsAggregate = new System.Windows.Forms.RadioButton();
            this.optExtentsSpatialIndex = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkHintsSeek = new System.Windows.Forms.CheckBox();
            this.chkHintNoLock = new System.Windows.Forms.CheckBox();
            this.chkHintIndex = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.optDefQueryName = new System.Windows.Forms.RadioButton();
            this.optDefQueryId = new System.Windows.Forms.RadioButton();
            this.optDefQueryNone = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.optDataProviderNative = new System.Windows.Forms.RadioButton();
            this.optDataProviderWKB = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labTable);
            this.groupBox1.Controls.Add(this.chkSpatialValidate);
            this.groupBox1.Controls.Add(this.optSpatialGeog);
            this.groupBox1.Controls.Add(this.optSpatialGeom);
            this.groupBox1.Location = new System.Drawing.Point(12, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(191, 116);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Spatial Type";
            // 
            // labTable
            // 
            this.labTable.AutoSize = true;
            this.labTable.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labTable.Location = new System.Drawing.Point(21, 91);
            this.labTable.Name = "labTable";
            this.labTable.Size = new System.Drawing.Size(37, 13);
            this.labTable.TabIndex = 3;
            this.labTable.Text = "Table:";
            // 
            // chkSpatialValidate
            // 
            this.chkSpatialValidate.AutoSize = true;
            this.chkSpatialValidate.Location = new System.Drawing.Point(21, 69);
            this.chkSpatialValidate.Name = "chkSpatialValidate";
            this.chkSpatialValidate.Size = new System.Drawing.Size(118, 17);
            this.chkSpatialValidate.TabIndex = 2;
            this.chkSpatialValidate.Text = "Validate geometries";
            this.chkSpatialValidate.UseVisualStyleBackColor = true;
            // 
            // optSpatialGeog
            // 
            this.optSpatialGeog.AutoSize = true;
            this.optSpatialGeog.Location = new System.Drawing.Point(21, 43);
            this.optSpatialGeog.Name = "optSpatialGeog";
            this.optSpatialGeog.Size = new System.Drawing.Size(101, 17);
            this.optSpatialGeog.TabIndex = 1;
            this.optSpatialGeog.Text = "SQL Geography";
            this.optSpatialGeog.UseVisualStyleBackColor = true;
            // 
            // optSpatialGeom
            // 
            this.optSpatialGeom.AutoSize = true;
            this.optSpatialGeom.Location = new System.Drawing.Point(21, 19);
            this.optSpatialGeom.Name = "optSpatialGeom";
            this.optSpatialGeom.Size = new System.Drawing.Size(94, 17);
            this.optSpatialGeom.TabIndex = 0;
            this.optSpatialGeom.Text = "SQL Geometry";
            this.optSpatialGeom.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.optExtentsIndividual);
            this.groupBox2.Controls.Add(this.labExtentsTime);
            this.groupBox2.Controls.Add(this.cmdGetExtents);
            this.groupBox2.Controls.Add(this.optExtentsAggregate);
            this.groupBox2.Controls.Add(this.optExtentsSpatialIndex);
            this.groupBox2.Location = new System.Drawing.Point(218, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(191, 117);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Extents Mode";
            // 
            // optExtentsIndividual
            // 
            this.optExtentsIndividual.AutoSize = true;
            this.optExtentsIndividual.Location = new System.Drawing.Point(21, 19);
            this.optExtentsIndividual.Name = "optExtentsIndividual";
            this.optExtentsIndividual.Size = new System.Drawing.Size(155, 17);
            this.optExtentsIndividual.TabIndex = 1;
            this.optExtentsIndividual.Text = "Individual features (slowest)";
            this.optExtentsIndividual.UseVisualStyleBackColor = true;
            // 
            // labExtentsTime
            // 
            this.labExtentsTime.AutoSize = true;
            this.labExtentsTime.Location = new System.Drawing.Point(102, 92);
            this.labExtentsTime.Name = "labExtentsTime";
            this.labExtentsTime.Size = new System.Drawing.Size(0, 13);
            this.labExtentsTime.TabIndex = 6;
            // 
            // cmdGetExtents
            // 
            this.cmdGetExtents.Location = new System.Drawing.Point(21, 87);
            this.cmdGetExtents.Name = "cmdGetExtents";
            this.cmdGetExtents.Size = new System.Drawing.Size(75, 23);
            this.cmdGetExtents.TabIndex = 5;
            this.cmdGetExtents.Text = "Get Extents";
            this.cmdGetExtents.UseVisualStyleBackColor = true;
            // 
            // optExtentsAggregate
            // 
            this.optExtentsAggregate.AutoSize = true;
            this.optExtentsAggregate.Checked = true;
            this.optExtentsAggregate.ForeColor = System.Drawing.Color.SteelBlue;
            this.optExtentsAggregate.Location = new System.Drawing.Point(21, 42);
            this.optExtentsAggregate.Name = "optExtentsAggregate";
            this.optExtentsAggregate.Size = new System.Drawing.Size(119, 17);
            this.optExtentsAggregate.TabIndex = 4;
            this.optExtentsAggregate.TabStop = true;
            this.optExtentsAggregate.Text = "Aggregate (v2012+)";
            this.optExtentsAggregate.UseVisualStyleBackColor = true;
            // 
            // optExtentsSpatialIndex
            // 
            this.optExtentsSpatialIndex.AutoSize = true;
            this.optExtentsSpatialIndex.Location = new System.Drawing.Point(21, 65);
            this.optExtentsSpatialIndex.Name = "optExtentsSpatialIndex";
            this.optExtentsSpatialIndex.Size = new System.Drawing.Size(161, 17);
            this.optExtentsSpatialIndex.TabIndex = 3;
            this.optExtentsSpatialIndex.Text = "Spatial index (Geometry only)";
            this.optExtentsSpatialIndex.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkHintsSeek);
            this.groupBox3.Controls.Add(this.chkHintNoLock);
            this.groupBox3.Controls.Add(this.chkHintIndex);
            this.groupBox3.Location = new System.Drawing.Point(218, 200);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(191, 86);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Table Hints";
            // 
            // chkHintsSeek
            // 
            this.chkHintsSeek.AutoSize = true;
            this.chkHintsSeek.Location = new System.Drawing.Point(21, 65);
            this.chkHintsSeek.Name = "chkHintsSeek";
            this.chkHintsSeek.Size = new System.Drawing.Size(81, 17);
            this.chkHintsSeek.TabIndex = 2;
            this.chkHintsSeek.Text = "Force Seek";
            this.chkHintsSeek.UseVisualStyleBackColor = true;
            // 
            // chkHintNoLock
            // 
            this.chkHintNoLock.AutoSize = true;
            this.chkHintNoLock.Location = new System.Drawing.Point(21, 42);
            this.chkHintNoLock.Name = "chkHintNoLock";
            this.chkHintNoLock.Size = new System.Drawing.Size(67, 17);
            this.chkHintNoLock.TabIndex = 1;
            this.chkHintNoLock.Text = "No Lock";
            this.chkHintNoLock.UseVisualStyleBackColor = true;
            // 
            // chkHintIndex
            // 
            this.chkHintIndex.AutoSize = true;
            this.chkHintIndex.Location = new System.Drawing.Point(21, 20);
            this.chkHintIndex.Name = "chkHintIndex";
            this.chkHintIndex.Size = new System.Drawing.Size(82, 17);
            this.chkHintIndex.TabIndex = 0;
            this.chkHintIndex.Text = "Force Index";
            this.chkHintIndex.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(11, 294);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(398, 241);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.optDefQueryName);
            this.groupBox4.Controls.Add(this.optDefQueryId);
            this.groupBox4.Controls.Add(this.optDefQueryNone);
            this.groupBox4.Location = new System.Drawing.Point(12, 200);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(191, 86);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Definition Query";
            // 
            // optDefQueryName
            // 
            this.optDefQueryName.AutoSize = true;
            this.optDefQueryName.Location = new System.Drawing.Point(21, 62);
            this.optDefQueryName.Name = "optDefQueryName";
            this.optDefQueryName.Size = new System.Drawing.Size(162, 17);
            this.optDefQueryName.TabIndex = 2;
            this.optDefQueryName.Text = "Name IN (\'Taipei\', \'Bangkok\')";
            this.optDefQueryName.UseVisualStyleBackColor = true;
            // 
            // optDefQueryId
            // 
            this.optDefQueryId.AutoSize = true;
            this.optDefQueryId.Location = new System.Drawing.Point(21, 39);
            this.optDefQueryId.Name = "optDefQueryId";
            this.optDefQueryId.Size = new System.Drawing.Size(165, 17);
            this.optDefQueryId.TabIndex = 1;
            this.optDefQueryId.Text = "Id IN (1, 3, 5, 7, 9, 11, 13, 15)";
            this.optDefQueryId.UseVisualStyleBackColor = true;
            // 
            // optDefQueryNone
            // 
            this.optDefQueryNone.AutoSize = true;
            this.optDefQueryNone.Checked = true;
            this.optDefQueryNone.Location = new System.Drawing.Point(21, 19);
            this.optDefQueryNone.Name = "optDefQueryNone";
            this.optDefQueryNone.Size = new System.Drawing.Size(51, 17);
            this.optDefQueryNone.TabIndex = 0;
            this.optDefQueryNone.TabStop = true;
            this.optDefQueryNone.Text = "None";
            this.optDefQueryNone.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.optDataProviderNative);
            this.groupBox5.Controls.Add(this.optDataProviderWKB);
            this.groupBox5.Location = new System.Drawing.Point(14, 7);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(395, 65);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Data Provider";
            // 
            // optDataProviderNative
            // 
            this.optDataProviderNative.AutoSize = true;
            this.optDataProviderNative.Location = new System.Drawing.Point(19, 42);
            this.optDataProviderNative.Name = "optDataProviderNative";
            this.optDataProviderNative.Size = new System.Drawing.Size(340, 17);
            this.optDataProviderNative.TabIndex = 1;
            this.optDataProviderNative.Text = "SharpMap.SqlServerSpatialObjects [queries using Sql native types]";
            this.optDataProviderNative.UseVisualStyleBackColor = true;
            // 
            // optDataProviderWKB
            // 
            this.optDataProviderWKB.AutoSize = true;
            this.optDataProviderWKB.Checked = true;
            this.optDataProviderWKB.Location = new System.Drawing.Point(19, 19);
            this.optDataProviderWKB.Name = "optDataProviderWKB";
            this.optDataProviderWKB.Size = new System.Drawing.Size(331, 17);
            this.optDataProviderWKB.TabIndex = 0;
            this.optDataProviderWKB.TabStop = true;
            this.optDataProviderWKB.Text = "SharpMap.Data.Providers.SqlServer2008(+) [queries using WKB]";
            this.optDataProviderWKB.UseVisualStyleBackColor = true;
            // 
            // FormSqlServerOpts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 545);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSqlServerOpts";
            this.Text = "FormSqlServerOpts";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton optSpatialGeog;
        private System.Windows.Forms.RadioButton optSpatialGeom;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton optExtentsSpatialIndex;
        private System.Windows.Forms.RadioButton optExtentsIndividual;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkHintsSeek;
        private System.Windows.Forms.CheckBox chkHintNoLock;
        private System.Windows.Forms.CheckBox chkHintIndex;
        private System.Windows.Forms.CheckBox chkSpatialValidate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton optExtentsAggregate;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton optDefQueryName;
        private System.Windows.Forms.RadioButton optDefQueryId;
        private System.Windows.Forms.RadioButton optDefQueryNone;
        private System.Windows.Forms.Label labExtentsTime;
        private System.Windows.Forms.Button cmdGetExtents;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton optDataProviderNative;
        private System.Windows.Forms.RadioButton optDataProviderWKB;
        private System.Windows.Forms.Label labTable;
    }
}
