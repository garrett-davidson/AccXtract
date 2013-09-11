namespace AccXtract
{
    partial class Form1
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
            this.localGroup = new System.Windows.Forms.GroupBox();
            this.chromeLocalPanel = new System.Windows.Forms.Panel();
            this.chromDefaultButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFolderButton = new System.Windows.Forms.Button();
            this.localGroup.SuspendLayout();
            this.chromeLocalPanel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // localGroup
            // 
            this.localGroup.AutoSize = true;
            this.localGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.localGroup.Controls.Add(this.chromeLocalPanel);
            this.localGroup.Controls.Add(this.label1);
            this.localGroup.Location = new System.Drawing.Point(24, 105);
            this.localGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 0);
            this.localGroup.Name = "localGroup";
            this.localGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 0);
            this.localGroup.Size = new System.Drawing.Size(529, 160);
            this.localGroup.TabIndex = 0;
            this.localGroup.TabStop = false;
            this.localGroup.Tag = "";
            this.localGroup.Text = "Local";
            // 
            // chromeLocalPanel
            // 
            this.chromeLocalPanel.AutoScroll = true;
            this.chromeLocalPanel.Controls.Add(this.chromDefaultButton);
            this.chromeLocalPanel.Location = new System.Drawing.Point(20, 74);
            this.chromeLocalPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 0);
            this.chromeLocalPanel.Name = "chromeLocalPanel";
            this.chromeLocalPanel.Size = new System.Drawing.Size(497, 62);
            this.chromeLocalPanel.TabIndex = 1;
            // 
            // chromDefaultButton
            // 
            this.chromDefaultButton.AutoSize = true;
            this.chromDefaultButton.Location = new System.Drawing.Point(8, 8);
            this.chromDefaultButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.chromDefaultButton.Name = "chromDefaultButton";
            this.chromDefaultButton.Size = new System.Drawing.Size(181, 72);
            this.chromDefaultButton.TabIndex = 0;
            this.chromDefaultButton.Text = "Default";
            this.chromDefaultButton.UseVisualStyleBackColor = true;
            this.chromDefaultButton.Click += new System.EventHandler(this.addChromeProfile);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(408, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Google Chrome (Click to add to Chrome)";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.menuStrip1.Size = new System.Drawing.Size(607, 47);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadFolderToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(64, 39);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadFolderToolStripMenuItem
            // 
            this.loadFolderToolStripMenuItem.Name = "loadFolderToolStripMenuItem";
            this.loadFolderToolStripMenuItem.Size = new System.Drawing.Size(349, 40);
            this.loadFolderToolStripMenuItem.Text = "Load AccXtract Folder...";
            this.loadFolderToolStripMenuItem.Click += new System.EventHandler(this.loadFolderButton_Click);
            // 
            // loadFolderButton
            // 
            this.loadFolderButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.loadFolderButton.Location = new System.Drawing.Point(0, 47);
            this.loadFolderButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.loadFolderButton.Name = "loadFolderButton";
            this.loadFolderButton.Size = new System.Drawing.Size(607, 46);
            this.loadFolderButton.TabIndex = 2;
            this.loadFolderButton.Text = "Load AccXtract Folder...";
            this.loadFolderButton.UseVisualStyleBackColor = true;
            this.loadFolderButton.Click += new System.EventHandler(this.loadFolderButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(191F, 191F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(607, 521);
            this.Controls.Add(this.loadFolderButton);
            this.Controls.Add(this.localGroup);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.localGroup.ResumeLayout(false);
            this.localGroup.PerformLayout();
            this.chromeLocalPanel.ResumeLayout(false);
            this.chromeLocalPanel.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox localGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel chromeLocalPanel;
        private System.Windows.Forms.Button chromDefaultButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFolderToolStripMenuItem;
        private System.Windows.Forms.Button loadFolderButton;
    }
}

