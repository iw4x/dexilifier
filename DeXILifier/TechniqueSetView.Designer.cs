namespace DX9ShaderHLSLifier
{
    partial class TechniqueSetView
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
            this.mappingGroupBox = new System.Windows.Forms.GroupBox();
            this.mappingTable = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.exportAllFromTechsetButton = new System.Windows.Forms.Button();
            this.diffLitShadersButton = new System.Windows.Forms.Button();
            this.mappingGroupBox.SuspendLayout();
            this.mappingTable.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mappingGroupBox
            // 
            this.mappingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mappingGroupBox.Controls.Add(this.mappingTable);
            this.mappingGroupBox.Location = new System.Drawing.Point(3, 3);
            this.mappingGroupBox.Name = "mappingGroupBox";
            this.mappingGroupBox.Size = new System.Drawing.Size(553, 394);
            this.mappingGroupBox.TabIndex = 0;
            this.mappingGroupBox.TabStop = false;
            this.mappingGroupBox.Text = "Mapping";
            // 
            // mappingTable
            // 
            this.mappingTable.ColumnCount = 2;
            this.mappingTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mappingTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mappingTable.Controls.Add(this.label1, 1, 0);
            this.mappingTable.Controls.Add(this.label3, 0, 0);
            this.mappingTable.Controls.Add(this.label4, 0, 1);
            this.mappingTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mappingTable.Location = new System.Drawing.Point(3, 16);
            this.mappingTable.Name = "mappingTable";
            this.mappingTable.RowCount = 2;
            this.mappingTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.mappingTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.mappingTable.Size = new System.Drawing.Size(547, 375);
            this.mappingTable.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(276, 0);
            this.label1.Name = "label1";
            this.mappingTable.SetRowSpan(this.label1, 2);
            this.label1.Size = new System.Drawing.Size(268, 375);
            this.label1.TabIndex = 0;
            this.label1.Text = "lp_sm_omni_r0c0_dtex_dfog_sm3";
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(267, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "BUILD_SHADOW_MAP_COLOR";
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(267, 351);
            this.label4.TabIndex = 3;
            this.label4.Text = "label4";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(562, 332);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(235, 62);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Shaders";
            this.groupBox1.Visible = false;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(3, 16);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(229, 43);
            this.listBox1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.diffLitShadersButton);
            this.groupBox2.Controls.Add(this.exportAllFromTechsetButton);
            this.groupBox2.Location = new System.Drawing.Point(565, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(229, 83);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Actions";
            // 
            // exportAllFromTechsetButton
            // 
            this.exportAllFromTechsetButton.Location = new System.Drawing.Point(6, 19);
            this.exportAllFromTechsetButton.Name = "exportAllFromTechsetButton";
            this.exportAllFromTechsetButton.Size = new System.Drawing.Size(217, 23);
            this.exportAllFromTechsetButton.TabIndex = 0;
            this.exportAllFromTechsetButton.Text = "Export all shaders";
            this.exportAllFromTechsetButton.UseVisualStyleBackColor = true;
            this.exportAllFromTechsetButton.Click += new System.EventHandler(this.exportAllFromTechsetButton_Click);
            // 
            // diffLitShadersButton
            // 
            this.diffLitShadersButton.Location = new System.Drawing.Point(6, 48);
            this.diffLitShadersButton.Name = "diffLitShadersButton";
            this.diffLitShadersButton.Size = new System.Drawing.Size(217, 23);
            this.diffLitShadersButton.TabIndex = 1;
            this.diffLitShadersButton.Text = "Compare lit shaders";
            this.diffLitShadersButton.UseVisualStyleBackColor = true;
            // 
            // TechniqueSetView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mappingGroupBox);
            this.Name = "TechniqueSetView";
            this.Size = new System.Drawing.Size(800, 400);
            this.mappingGroupBox.ResumeLayout(false);
            this.mappingTable.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox mappingGroupBox;
        private System.Windows.Forms.TableLayoutPanel mappingTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button exportAllFromTechsetButton;
        private System.Windows.Forms.Button diffLitShadersButton;
    }
}
