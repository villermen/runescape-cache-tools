namespace RunescapeCacheToolsGUI
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
			this.components = new System.ComponentModel.Container();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.soundtrackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showMissingTracksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cacheDirectoryTextBox = new System.Windows.Forms.TextBox();
			this.outputDirectoryTextBox = new System.Windows.Forms.TextBox();
			this.changeCacheDirectoryButton = new System.Windows.Forms.Button();
			this.changeOutputDirectoryButton = new System.Windows.Forms.Button();
			this.cacheTreeView = new System.Windows.Forms.TreeView();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.showOutputDirectoryButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.treeViewImageList = new System.Windows.Forms.ImageList(this.components);
			this.extractContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.extractDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.extractOverwriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.jobsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.menuStrip1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.extractContextMenuStrip.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.soundtrackToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(871, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// soundtrackToolStripMenuItem
			// 
			this.soundtrackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showNamesToolStripMenuItem,
            this.showMissingTracksToolStripMenuItem});
			this.soundtrackToolStripMenuItem.Name = "soundtrackToolStripMenuItem";
			this.soundtrackToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
			this.soundtrackToolStripMenuItem.Text = "Soundtrack";
			// 
			// showNamesToolStripMenuItem
			// 
			this.showNamesToolStripMenuItem.Name = "showNamesToolStripMenuItem";
			this.showNamesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.showNamesToolStripMenuItem.Text = "Export Names";
			this.showNamesToolStripMenuItem.Click += new System.EventHandler(this.exportNamesSoundtrackToolStripMenuItem_Click);
			// 
			// showMissingTracksToolStripMenuItem
			// 
			this.showMissingTracksToolStripMenuItem.Name = "showMissingTracksToolStripMenuItem";
			this.showMissingTracksToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.showMissingTracksToolStripMenuItem.Text = "Show Missing Tracks";
			this.showMissingTracksToolStripMenuItem.Click += new System.EventHandler(this.showMissingTracksToolStripMenuItem_Click);
			// 
			// cacheDirectoryTextBox
			// 
			this.cacheDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cacheDirectoryTextBox.Location = new System.Drawing.Point(6, 17);
			this.cacheDirectoryTextBox.Name = "cacheDirectoryTextBox";
			this.cacheDirectoryTextBox.ReadOnly = true;
			this.cacheDirectoryTextBox.Size = new System.Drawing.Size(353, 20);
			this.cacheDirectoryTextBox.TabIndex = 3;
			// 
			// outputDirectoryTextBox
			// 
			this.outputDirectoryTextBox.Location = new System.Drawing.Point(6, 18);
			this.outputDirectoryTextBox.Name = "outputDirectoryTextBox";
			this.outputDirectoryTextBox.ReadOnly = true;
			this.outputDirectoryTextBox.Size = new System.Drawing.Size(295, 20);
			this.outputDirectoryTextBox.TabIndex = 4;
			// 
			// changeCacheDirectoryButton
			// 
			this.changeCacheDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.changeCacheDirectoryButton.Location = new System.Drawing.Point(365, 15);
			this.changeCacheDirectoryButton.Name = "changeCacheDirectoryButton";
			this.changeCacheDirectoryButton.Size = new System.Drawing.Size(65, 23);
			this.changeCacheDirectoryButton.TabIndex = 5;
			this.changeCacheDirectoryButton.Text = "Change...";
			this.changeCacheDirectoryButton.UseVisualStyleBackColor = true;
			this.changeCacheDirectoryButton.Click += new System.EventHandler(this.changeCacheDirectoryButton_Click);
			// 
			// changeOutputDirectoryButton
			// 
			this.changeOutputDirectoryButton.Location = new System.Drawing.Point(307, 17);
			this.changeOutputDirectoryButton.Name = "changeOutputDirectoryButton";
			this.changeOutputDirectoryButton.Size = new System.Drawing.Size(75, 23);
			this.changeOutputDirectoryButton.TabIndex = 6;
			this.changeOutputDirectoryButton.Text = "Change...";
			this.changeOutputDirectoryButton.UseVisualStyleBackColor = true;
			this.changeOutputDirectoryButton.Click += new System.EventHandler(this.changeOutputDirectoryButton_Click);
			// 
			// cacheTreeView
			// 
			this.cacheTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cacheTreeView.Location = new System.Drawing.Point(12, 72);
			this.cacheTreeView.Name = "cacheTreeView";
			this.cacheTreeView.PathSeparator = "/";
			this.cacheTreeView.Size = new System.Drawing.Size(424, 223);
			this.cacheTreeView.TabIndex = 9;
			this.cacheTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.cacheTreeView_NodeMouseClick);
			this.cacheTreeView.DoubleClick += new System.EventHandler(this.cacheTreeView_DoubleClick);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.groupBox1.AutoSize = true;
			this.groupBox1.Controls.Add(this.groupBox3);
			this.groupBox1.Controls.Add(this.cacheDirectoryTextBox);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.cacheTreeView);
			this.groupBox1.Controls.Add(this.changeCacheDirectoryButton);
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(436, 307);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Cache Directory";
			// 
			// showOutputDirectoryButton
			// 
			this.showOutputDirectoryButton.Location = new System.Drawing.Point(307, 45);
			this.showOutputDirectoryButton.Name = "showOutputDirectoryButton";
			this.showOutputDirectoryButton.Size = new System.Drawing.Size(75, 23);
			this.showOutputDirectoryButton.TabIndex = 11;
			this.showOutputDirectoryButton.Text = "Show";
			this.showOutputDirectoryButton.UseVisualStyleBackColor = true;
			this.showOutputDirectoryButton.Click += new System.EventHandler(this.showOutputDirectoryButton_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.outputDirectoryTextBox);
			this.groupBox2.Controls.Add(this.showOutputDirectoryButton);
			this.groupBox2.Controls.Add(this.changeOutputDirectoryButton);
			this.groupBox2.Location = new System.Drawing.Point(0, 0);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(388, 77);
			this.groupBox2.TabIndex = 12;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Output Directory";
			// 
			// treeViewImageList
			// 
			this.treeViewImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.treeViewImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.treeViewImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// extractContextMenuStrip
			// 
			this.extractContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.extractDefaultToolStripMenuItem,
            this.extractOverwriteToolStripMenuItem});
			this.extractContextMenuStrip.Name = "extractContextMenuStrip";
			this.extractContextMenuStrip.Size = new System.Drawing.Size(215, 70);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.openToolStripMenuItem.Text = "Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// extractDefaultToolStripMenuItem
			// 
			this.extractDefaultToolStripMenuItem.Name = "extractDefaultToolStripMenuItem";
			this.extractDefaultToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.extractDefaultToolStripMenuItem.Text = "Extract";
			this.extractDefaultToolStripMenuItem.Click += new System.EventHandler(this.extractDefaultToolStripMenuItem_Click);
			// 
			// extractOverwriteToolStripMenuItem
			// 
			this.extractOverwriteToolStripMenuItem.Name = "extractOverwriteToolStripMenuItem";
			this.extractOverwriteToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.extractOverwriteToolStripMenuItem.Text = "Extract (Overwrite existing)";
			this.extractOverwriteToolStripMenuItem.Click += new System.EventHandler(this.extractOverwriteToolStripMenuItem_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.jobsTableLayoutPanel);
			this.groupBox3.Location = new System.Drawing.Point(28, 92);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(388, 209);
			this.groupBox3.TabIndex = 13;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Current Jobs";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(365, 43);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(65, 23);
			this.button1.TabIndex = 10;
			this.button1.Text = "Default";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// jobsTableLayoutPanel
			// 
			this.jobsTableLayoutPanel.AutoScroll = true;
			this.jobsTableLayoutPanel.ColumnCount = 1;
			this.jobsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.jobsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobsTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
			this.jobsTableLayoutPanel.Name = "jobsTableLayoutPanel";
			this.jobsTableLayoutPanel.RowCount = 1;
			this.jobsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.jobsTableLayoutPanel.Size = new System.Drawing.Size(382, 190);
			this.jobsTableLayoutPanel.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(871, 310);
			this.tableLayoutPanel1.TabIndex = 14;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(871, 334);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.menuStrip1);
			this.Name = "MainForm";
			this.Text = "MainForm";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.extractContextMenuStrip.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem soundtrackToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showNamesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showMissingTracksToolStripMenuItem;
		private System.Windows.Forms.TextBox cacheDirectoryTextBox;
		private System.Windows.Forms.TextBox outputDirectoryTextBox;
		private System.Windows.Forms.Button changeCacheDirectoryButton;
		private System.Windows.Forms.Button changeOutputDirectoryButton;
		private System.Windows.Forms.TreeView cacheTreeView;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button showOutputDirectoryButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ImageList treeViewImageList;
		private System.Windows.Forms.ContextMenuStrip extractContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem extractDefaultToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem extractOverwriteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TableLayoutPanel jobsTableLayoutPanel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}