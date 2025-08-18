namespace DX9ShaderHLSLifier
{
    partial class DependencyGraphWindow
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
            this.graphPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // graphPanel
            // 
            this.graphPanel.AutoScroll = true;
            this.graphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(800, 450);
            this.graphPanel.TabIndex = 0;
            // 
            // DependencyGraphWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.graphPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "DependencyGraphWindow";
            this.Text = "DependencyGraphWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel graphPanel;
    }
}