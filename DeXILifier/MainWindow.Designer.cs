namespace DX9ShaderHLSLifier
{
    partial class MainWindow
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.GroupBox groupBox3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            System.Windows.Forms.GroupBox rebuiltHlslGroup;
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node1");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node2");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("❌ Node0", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.outputWindow = new System.Windows.Forms.RichTextBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.copyToClipboard = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.showRelationalTreeButton = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lineNumbersTextBox = new System.Windows.Forms.RichTextBox();
            this.decompilationResultTextBox = new System.Windows.Forms.RichTextBox();
            this.inputTextBox = new System.Windows.Forms.RichTextBox();
            this.decompilationProgressBar = new System.Windows.Forms.ProgressBar();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.alwaysCreateVariablesBox = new System.Windows.Forms.CheckBox();
            this.reduceInstructionsBox = new System.Windows.Forms.CheckBox();
            this.renameVariablesBox = new System.Windows.Forms.CheckBox();
            this.inlineConstantsCheckBox = new System.Windows.Forms.CheckBox();
            this.optimizeCheckBox = new System.Windows.Forms.CheckBox();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.openFolderButton = new System.Windows.Forms.ToolStripButton();
            this.tabView = new System.Windows.Forms.TabControl();
            this.explorerTab = new System.Windows.Forms.TabPage();
            this.explorerTreeView = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.compiledShaderTab = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolOutputTabs = new System.Windows.Forms.TabControl();
            this.shaderTab = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.techniqueSetTab = new System.Windows.Forms.TabPage();
            this.techSetView = new DX9ShaderHLSLifier.TechniqueSetView();
            groupBox3 = new System.Windows.Forms.GroupBox();
            rebuiltHlslGroup = new System.Windows.Forms.GroupBox();
            groupBox3.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            rebuiltHlslGroup.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.settingsGroupBox.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabView.SuspendLayout();
            this.explorerTab.SuspendLayout();
            this.compiledShaderTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolOutputTabs.SuspendLayout();
            this.shaderTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.techniqueSetTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(this.toolStrip3);
            groupBox3.Controls.Add(this.outputWindow);
            groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox3.Location = new System.Drawing.Point(0, 0);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(326, 757);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Recompiled shader";
            // 
            // toolStrip3
            // 
            this.toolStrip3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip3.AutoSize = false;
            this.toolStrip3.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip3.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton6});
            this.toolStrip3.Location = new System.Drawing.Point(6, 19);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(314, 23);
            this.toolStrip3.TabIndex = 7;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton6.Text = "toolStripButton1";
            // 
            // outputWindow
            // 
            this.outputWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputWindow.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.outputWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.outputWindow.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputWindow.ForeColor = System.Drawing.SystemColors.Window;
            this.outputWindow.Location = new System.Drawing.Point(6, 45);
            this.outputWindow.Name = "outputWindow";
            this.outputWindow.ReadOnly = true;
            this.outputWindow.Size = new System.Drawing.Size(314, 706);
            this.outputWindow.TabIndex = 1;
            this.outputWindow.Text = "";
            // 
            // rebuiltHlslGroup
            // 
            rebuiltHlslGroup.BackColor = System.Drawing.SystemColors.Control;
            rebuiltHlslGroup.Controls.Add(this.toolStrip2);
            rebuiltHlslGroup.Controls.Add(this.panel1);
            rebuiltHlslGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            rebuiltHlslGroup.Location = new System.Drawing.Point(0, 0);
            rebuiltHlslGroup.Name = "rebuiltHlslGroup";
            rebuiltHlslGroup.Size = new System.Drawing.Size(704, 757);
            rebuiltHlslGroup.TabIndex = 3;
            rebuiltHlslGroup.TabStop = false;
            rebuiltHlslGroup.Text = "Rebuilt HLSL";
            // 
            // toolStrip2
            // 
            this.toolStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip2.AutoSize = false;
            this.toolStrip2.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToClipboard,
            this.toolStripButton3,
            this.toolStripButton5,
            this.showRelationalTreeButton});
            this.toolStrip2.Location = new System.Drawing.Point(6, 19);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(692, 23);
            this.toolStrip2.TabIndex = 7;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // copyToClipboard
            // 
            this.copyToClipboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyToClipboard.Image = ((System.Drawing.Image)(resources.GetObject("copyToClipboard.Image")));
            this.copyToClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToClipboard.Name = "copyToClipboard";
            this.copyToClipboard.Size = new System.Drawing.Size(23, 20);
            this.copyToClipboard.Text = "Copy output to clipboard";
            this.copyToClipboard.Click += new System.EventHandler(this.copyToClipboard_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton3.Text = "toolStripButton1";
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton5.Text = "toolStripButton5";
            // 
            // showRelationalTreeButton
            // 
            this.showRelationalTreeButton.CheckOnClick = true;
            this.showRelationalTreeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showRelationalTreeButton.Image = ((System.Drawing.Image)(resources.GetObject("showRelationalTreeButton.Image")));
            this.showRelationalTreeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showRelationalTreeButton.Name = "showRelationalTreeButton";
            this.showRelationalTreeButton.Size = new System.Drawing.Size(23, 20);
            this.showRelationalTreeButton.Text = "Relational view";
            this.showRelationalTreeButton.Click += new System.EventHandler(this.showRelationalTreeButton_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.lineNumbersTextBox);
            this.panel1.Controls.Add(this.decompilationResultTextBox);
            this.panel1.Location = new System.Drawing.Point(6, 45);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(692, 706);
            this.panel1.TabIndex = 1;
            // 
            // lineNumbersTextBox
            // 
            this.lineNumbersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lineNumbersTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lineNumbersTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lineNumbersTextBox.CausesValidation = false;
            this.lineNumbersTextBox.DetectUrls = false;
            this.lineNumbersTextBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lineNumbersTextBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lineNumbersTextBox.Location = new System.Drawing.Point(8, 8);
            this.lineNumbersTextBox.Name = "lineNumbersTextBox";
            this.lineNumbersTextBox.ReadOnly = true;
            this.lineNumbersTextBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lineNumbersTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.lineNumbersTextBox.Size = new System.Drawing.Size(26, 665);
            this.lineNumbersTextBox.TabIndex = 1;
            this.lineNumbersTextBox.Text = "001";
            this.lineNumbersTextBox.WordWrap = false;
            // 
            // decompilationResultTextBox
            // 
            this.decompilationResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.decompilationResultTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.decompilationResultTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.decompilationResultTextBox.CausesValidation = false;
            this.decompilationResultTextBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.decompilationResultTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.decompilationResultTextBox.Location = new System.Drawing.Point(40, 8);
            this.decompilationResultTextBox.Name = "decompilationResultTextBox";
            this.decompilationResultTextBox.ReadOnly = true;
            this.decompilationResultTextBox.Size = new System.Drawing.Size(644, 665);
            this.decompilationResultTextBox.TabIndex = 0;
            this.decompilationResultTextBox.Text = "Waiting for input...";
            this.decompilationResultTextBox.WordWrap = false;
            // 
            // inputTextBox
            // 
            this.inputTextBox.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.inputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.inputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTextBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.inputTextBox.Location = new System.Drawing.Point(3, 3);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(385, 698);
            this.inputTextBox.TabIndex = 1;
            this.inputTextBox.Text = "";
            this.inputTextBox.WordWrap = false;
            // 
            // decompilationProgressBar
            // 
            this.decompilationProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.decompilationProgressBar.Location = new System.Drawing.Point(410, 742);
            this.decompilationProgressBar.Name = "decompilationProgressBar";
            this.decompilationProgressBar.Size = new System.Drawing.Size(117, 23);
            this.decompilationProgressBar.TabIndex = 4;
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsGroupBox.Controls.Add(this.alwaysCreateVariablesBox);
            this.settingsGroupBox.Controls.Add(this.reduceInstructionsBox);
            this.settingsGroupBox.Controls.Add(this.renameVariablesBox);
            this.settingsGroupBox.Controls.Add(this.inlineConstantsCheckBox);
            this.settingsGroupBox.Controls.Add(this.optimizeCheckBox);
            this.settingsGroupBox.Location = new System.Drawing.Point(410, 51);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(117, 178);
            this.settingsGroupBox.TabIndex = 5;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "Settings";
            // 
            // alwaysCreateVariablesBox
            // 
            this.alwaysCreateVariablesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.alwaysCreateVariablesBox.Location = new System.Drawing.Point(6, 155);
            this.alwaysCreateVariablesBox.Name = "alwaysCreateVariablesBox";
            this.alwaysCreateVariablesBox.Size = new System.Drawing.Size(105, 17);
            this.alwaysCreateVariablesBox.TabIndex = 4;
            this.alwaysCreateVariablesBox.Text = "Immutable vars";
            this.alwaysCreateVariablesBox.UseVisualStyleBackColor = true;
            // 
            // reduceInstructionsBox
            // 
            this.reduceInstructionsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reduceInstructionsBox.Checked = true;
            this.reduceInstructionsBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.reduceInstructionsBox.Location = new System.Drawing.Point(6, 111);
            this.reduceInstructionsBox.Name = "reduceInstructionsBox";
            this.reduceInstructionsBox.Size = new System.Drawing.Size(105, 17);
            this.reduceInstructionsBox.TabIndex = 3;
            this.reduceInstructionsBox.Text = "Reduce ops";
            this.reduceInstructionsBox.UseVisualStyleBackColor = true;
            // 
            // renameVariablesBox
            // 
            this.renameVariablesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.renameVariablesBox.Checked = true;
            this.renameVariablesBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.renameVariablesBox.Location = new System.Drawing.Point(6, 82);
            this.renameVariablesBox.Name = "renameVariablesBox";
            this.renameVariablesBox.Size = new System.Drawing.Size(105, 23);
            this.renameVariablesBox.TabIndex = 2;
            this.renameVariablesBox.Text = "Rename vars";
            this.renameVariablesBox.UseVisualStyleBackColor = true;
            // 
            // inlineConstantsCheckBox
            // 
            this.inlineConstantsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inlineConstantsCheckBox.Checked = true;
            this.inlineConstantsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.inlineConstantsCheckBox.Location = new System.Drawing.Point(6, 53);
            this.inlineConstantsCheckBox.Name = "inlineConstantsCheckBox";
            this.inlineConstantsCheckBox.Size = new System.Drawing.Size(105, 23);
            this.inlineConstantsCheckBox.TabIndex = 1;
            this.inlineConstantsCheckBox.Text = "Inline const.";
            this.inlineConstantsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optimizeCheckBox
            // 
            this.optimizeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optimizeCheckBox.Checked = true;
            this.optimizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optimizeCheckBox.Location = new System.Drawing.Point(6, 24);
            this.optimizeCheckBox.Name = "optimizeCheckBox";
            this.optimizeCheckBox.Size = new System.Drawing.Size(105, 23);
            this.optimizeCheckBox.TabIndex = 0;
            this.optimizeCheckBox.Text = "Humanize";
            this.optimizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(776, 722);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.openFolderButton});
            this.toolStrip1.Location = new System.Drawing.Point(9, 9);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(518, 23);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.openFileButton_Click);
            // 
            // openFolderButton
            // 
            this.openFolderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openFolderButton.Image = ((System.Drawing.Image)(resources.GetObject("openFolderButton.Image")));
            this.openFolderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openFolderButton.Name = "openFolderButton";
            this.openFolderButton.Size = new System.Drawing.Size(23, 20);
            this.openFolderButton.Text = "toolStripButton2";
            this.openFolderButton.Click += new System.EventHandler(this.openFolderButton_Click);
            // 
            // tabView
            // 
            this.tabView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabView.Controls.Add(this.explorerTab);
            this.tabView.Controls.Add(this.compiledShaderTab);
            this.tabView.Location = new System.Drawing.Point(9, 35);
            this.tabView.Name = "tabView";
            this.tabView.SelectedIndex = 0;
            this.tabView.Size = new System.Drawing.Size(399, 730);
            this.tabView.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tabView.TabIndex = 2;
            // 
            // explorerTab
            // 
            this.explorerTab.Controls.Add(this.explorerTreeView);
            this.explorerTab.Location = new System.Drawing.Point(4, 22);
            this.explorerTab.Name = "explorerTab";
            this.explorerTab.Padding = new System.Windows.Forms.Padding(3);
            this.explorerTab.Size = new System.Drawing.Size(391, 704);
            this.explorerTab.TabIndex = 0;
            this.explorerTab.Text = "Explorer";
            this.explorerTab.UseVisualStyleBackColor = true;
            // 
            // explorerTreeView
            // 
            this.explorerTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explorerTreeView.ImageIndex = 0;
            this.explorerTreeView.ImageList = this.imageList1;
            this.explorerTreeView.ItemHeight = 32;
            this.explorerTreeView.Location = new System.Drawing.Point(3, 3);
            this.explorerTreeView.Name = "explorerTreeView";
            treeNode1.Name = "Node1";
            treeNode1.Text = "Node1";
            treeNode2.Name = "Node2";
            treeNode2.Text = "Node2";
            treeNode3.Name = "Node0";
            treeNode3.Text = "❌ Node0";
            this.explorerTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3});
            this.explorerTreeView.SelectedImageIndex = 0;
            this.explorerTreeView.Size = new System.Drawing.Size(385, 698);
            this.explorerTreeView.TabIndex = 0;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "TECHNIQUESET");
            this.imageList1.Images.SetKeyName(1, "TECHNIQUE");
            this.imageList1.Images.SetKeyName(2, "VERTEXSHADER");
            this.imageList1.Images.SetKeyName(3, "PIXELSHADER");
            this.imageList1.Images.SetKeyName(4, "UNKNOWN");
            // 
            // compiledShaderTab
            // 
            this.compiledShaderTab.Controls.Add(this.inputTextBox);
            this.compiledShaderTab.Location = new System.Drawing.Point(4, 22);
            this.compiledShaderTab.Name = "compiledShaderTab";
            this.compiledShaderTab.Padding = new System.Windows.Forms.Padding(3);
            this.compiledShaderTab.Size = new System.Drawing.Size(391, 704);
            this.compiledShaderTab.TabIndex = 1;
            this.compiledShaderTab.Text = "Decompiled bytecode";
            this.compiledShaderTab.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabView);
            this.splitContainer1.Panel1.Controls.Add(this.decompilationProgressBar);
            this.splitContainer1.Panel1.Controls.Add(this.settingsGroupBox);
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.toolOutputTabs);
            this.splitContainer1.Size = new System.Drawing.Size(1606, 771);
            this.splitContainer1.SplitterDistance = 535;
            this.splitContainer1.TabIndex = 7;
            // 
            // toolOutputTabs
            // 
            this.toolOutputTabs.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.toolOutputTabs.Controls.Add(this.shaderTab);
            this.toolOutputTabs.Controls.Add(this.techniqueSetTab);
            this.toolOutputTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolOutputTabs.Location = new System.Drawing.Point(0, 0);
            this.toolOutputTabs.Multiline = true;
            this.toolOutputTabs.Name = "toolOutputTabs";
            this.toolOutputTabs.SelectedIndex = 0;
            this.toolOutputTabs.Size = new System.Drawing.Size(1067, 771);
            this.toolOutputTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.toolOutputTabs.TabIndex = 2;
            // 
            // shaderTab
            // 
            this.shaderTab.Controls.Add(this.splitContainer2);
            this.shaderTab.Location = new System.Drawing.Point(23, 4);
            this.shaderTab.Name = "shaderTab";
            this.shaderTab.Padding = new System.Windows.Forms.Padding(3);
            this.shaderTab.Size = new System.Drawing.Size(1040, 763);
            this.shaderTab.TabIndex = 0;
            this.shaderTab.Text = "Shader HLSL";
            this.shaderTab.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(rebuiltHlslGroup);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(groupBox3);
            this.splitContainer2.Size = new System.Drawing.Size(1034, 757);
            this.splitContainer2.SplitterDistance = 704;
            this.splitContainer2.TabIndex = 0;
            // 
            // techniqueSetTab
            // 
            this.techniqueSetTab.Controls.Add(this.techSetView);
            this.techniqueSetTab.Location = new System.Drawing.Point(23, 4);
            this.techniqueSetTab.Name = "techniqueSetTab";
            this.techniqueSetTab.Padding = new System.Windows.Forms.Padding(3);
            this.techniqueSetTab.Size = new System.Drawing.Size(1040, 763);
            this.techniqueSetTab.TabIndex = 1;
            this.techniqueSetTab.Text = "Technique set";
            // 
            // techSetView
            // 
            this.techSetView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.techSetView.Location = new System.Drawing.Point(3, 3);
            this.techSetView.Name = "techSetView";
            this.techSetView.Size = new System.Drawing.Size(1034, 757);
            this.techSetView.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1606, 771);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "DeXilifier";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            groupBox3.ResumeLayout(false);
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            rebuiltHlslGroup.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.settingsGroupBox.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabView.ResumeLayout(false);
            this.explorerTab.ResumeLayout(false);
            this.compiledShaderTab.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolOutputTabs.ResumeLayout(false);
            this.shaderTab.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.techniqueSetTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox inputTextBox;
        private System.Windows.Forms.ProgressBar decompilationProgressBar;
        private System.Windows.Forms.RichTextBox outputWindow;
        private System.Windows.Forms.GroupBox settingsGroupBox;
        private System.Windows.Forms.CheckBox reduceInstructionsBox;
        private System.Windows.Forms.CheckBox renameVariablesBox;
        private System.Windows.Forms.CheckBox inlineConstantsCheckBox;
        private System.Windows.Forms.CheckBox optimizeCheckBox;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox lineNumbersTextBox;
        private System.Windows.Forms.RichTextBox decompilationResultTextBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton openFolderButton;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton copyToClipboard;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        public System.Windows.Forms.TabPage explorerTab;
        public System.Windows.Forms.TabPage compiledShaderTab;
        public System.Windows.Forms.TreeView explorerTreeView;
        public System.Windows.Forms.TabControl tabView;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripButton showRelationalTreeButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl toolOutputTabs;
        private System.Windows.Forms.TabPage shaderTab;
        private System.Windows.Forms.TabPage techniqueSetTab;
        internal TechniqueSetView techSetView;
        private System.Windows.Forms.CheckBox alwaysCreateVariablesBox;
    }
}