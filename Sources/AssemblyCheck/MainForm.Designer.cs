namespace Dot42.AssemblyCheck
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
        /// the Types of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.miSelectAssembly = new System.Windows.Forms.ToolStripMenuItem();
            this.miFrameworkFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.miCurrentFrameworkFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.miChangeFrameworkFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.miCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.expandableSplitter1 = new System.Windows.Forms.Splitter();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.tvList = new System.Windows.Forms.TreeView();
            this.chMissingMembers = new System.Windows.Forms.ColumnHeader();
            this.chScope = new System.Windows.Forms.ColumnHeader();
            this.messageTypesList = new System.Windows.Forms.ImageList(this.components);
            this.rightPanel = new System.Windows.Forms.Panel();
            this.tvUsedIn = new System.Windows.Forms.TreeView();
            this.chUsedInName = new System.Windows.Forms.ColumnHeader();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.tvLog = new System.Windows.Forms.TreeView();
            this.chLog = new System.Windows.Forms.ColumnHeader();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.menuBar.SuspendLayout();
            this.leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tvList)).BeginInit();
            this.rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tvUsedIn)).BeginInit();
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tvLog)).BeginInit();
            this.SuspendLayout();
            // 
            // menuBar
            // 
            this.menuBar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSelectAssembly,
            this.miFrameworkFolder,
            this.miCopy});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(1008, 24);
            this.menuBar.TabIndex = 1;
            this.menuBar.Text = "menuStrip1";
            // 
            // miSelectAssembly
            // 
            this.miSelectAssembly.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miSelectAssembly.Name = "miSelectAssembly";
            this.miSelectAssembly.Size = new System.Drawing.Size(100, 20);
            this.miSelectAssembly.Text = "&Open assembly";
            this.miSelectAssembly.Click += new System.EventHandler(this.OnOpenClick);
            // 
            // miFrameworkFolder
            // 
            this.miFrameworkFolder.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCurrentFrameworkFolder,
            this.miChangeFrameworkFolder});
            this.miFrameworkFolder.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.miFrameworkFolder.Name = "miFrameworkFolder";
            this.miFrameworkFolder.Size = new System.Drawing.Size(112, 20);
            this.miFrameworkFolder.Text = "Framework folder";
            // 
            // miCurrentFrameworkFolder
            // 
            this.miCurrentFrameworkFolder.Enabled = false;
            this.miCurrentFrameworkFolder.Name = "miCurrentFrameworkFolder";
            this.miCurrentFrameworkFolder.Size = new System.Drawing.Size(115, 22);
            this.miCurrentFrameworkFolder.Text = "?";
            // 
            // miChangeFrameworkFolder
            // 
            this.miChangeFrameworkFolder.Name = "miChangeFrameworkFolder";
            this.miChangeFrameworkFolder.Size = new System.Drawing.Size(115, 22);
            this.miChangeFrameworkFolder.Text = "Change";
            this.miChangeFrameworkFolder.Click += new System.EventHandler(this.miChangeFrameworkFolder_Click);
            // 
            // miCopy
            // 
            this.miCopy.Name = "miCopy";
            this.miCopy.Size = new System.Drawing.Size(47, 20);
            this.miCopy.Text = "Copy";
            this.miCopy.Click += new System.EventHandler(this.miCopy_Click);
            // 
            // expandableSplitter1
            // 
            this.expandableSplitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.expandableSplitter1.ForeColor = System.Drawing.Color.Black;
            this.expandableSplitter1.Location = new System.Drawing.Point(568, 24);
            this.expandableSplitter1.Margin = new System.Windows.Forms.Padding(2);
            this.expandableSplitter1.Name = "expandableSplitter1";
            this.expandableSplitter1.Size = new System.Drawing.Size(4, 391);
            this.expandableSplitter1.TabIndex = 5;
            this.expandableSplitter1.TabStop = false;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.tvList);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 24);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(568, 391);
            this.leftPanel.TabIndex = 0;
            // 
            // tvList
            // 
            this.tvList.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.tvList.AllowDrop = true;
            this.tvList.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.tvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvList.ImageList = this.messageTypesList;
            this.tvList.Location = new System.Drawing.Point(0, 0);
            this.tvList.Name = "tvList";
            this.tvList.PathSeparator = ";";
            this.tvList.Size = new System.Drawing.Size(568, 391);
            this.tvList.TabIndex = 0;
            this.tvList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnListAfterNodeSelect);
            this.tvList.DoubleClick += new System.EventHandler(this.OnNodeDoubleClick);
            this.tvList.SizeChanged += new System.EventHandler(this.tvList_SizeChanged);
            // 
            // chMissingMembers
            // 
            this.chMissingMembers.Name = "chMissingMembers";
            this.chMissingMembers.Text = "Missing types and members:";
            this.chMissingMembers.Width = 70;
            // 
            // chScope
            // 
            this.chScope.Name = "chScope";
            this.chScope.Text = "Scope";
            this.chScope.Width = 30;
            // 
            // messageTypesList
            // 
            this.messageTypesList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("messageTypesList.ImageStream")));
            this.messageTypesList.TransparentColor = System.Drawing.Color.Transparent;
            this.messageTypesList.Images.SetKeyName(0, "Class.png");
            this.messageTypesList.Images.SetKeyName(1, "Method.png");
            this.messageTypesList.Images.SetKeyName(2, "Field.png");
            this.messageTypesList.Images.SetKeyName(3, "error.png");
            this.messageTypesList.Images.SetKeyName(4, "information.png");
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.tvUsedIn);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(572, 24);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(436, 391);
            this.rightPanel.TabIndex = 7;
            // 
            // tvUsedIn
            // 
            this.tvUsedIn.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.tvUsedIn.AllowDrop = true;
            this.tvUsedIn.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.tvUsedIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvUsedIn.Location = new System.Drawing.Point(0, 0);
            this.tvUsedIn.Name = "tvUsedIn";
            this.tvUsedIn.PathSeparator = ";";
            this.tvUsedIn.Size = new System.Drawing.Size(436, 391);
            this.tvUsedIn.TabIndex = 0;
            this.tvUsedIn.SizeChanged += new System.EventHandler(this.tvUsedIn_SizeChanged);
            // 
            // chUsedInName
            // 
            this.chUsedInName.Name = "chUsedInName";
            this.chUsedInName.Text = "Used in:";
            this.chUsedInName.Width = 100;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.tvLog);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 415);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1008, 224);
            this.bottomPanel.TabIndex = 8;
            // 
            // tvLog
            // 
            this.tvLog.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.tvLog.AllowDrop = true;
            this.tvLog.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.tvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvLog.ImageList = this.messageTypesList;
            this.tvLog.Location = new System.Drawing.Point(0, 0);
            this.tvLog.Name = "tvLog";
            this.tvLog.PathSeparator = ";";
            this.tvLog.Size = new System.Drawing.Size(1008, 224);
            this.tvLog.TabIndex = 1;
            this.tvLog.DoubleClick += new System.EventHandler(this.OnNodeDoubleClick);
            this.tvLog.SizeChanged += new System.EventHandler(this.tvLog_SizeChanged);
            // 
            // chLog
            // 
            this.chLog.Name = "chLog";
            this.chLog.Text = "Log";
            this.chLog.Width = 100;
            // 
            // progress
            // 
            // 
            // 
            // 
            this.progress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progress.Location = new System.Drawing.Point(0, 639);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(1008, 23);
            this.progress.TabIndex = 1;
            this.progress.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 662);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.expandableSplitter1);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.menuBar);
            this.MainMenuStrip = this.menuBar;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "dot42 Assembly Check";
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.leftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tvList)).EndInit();
            this.rightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tvUsedIn)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tvLog)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem miSelectAssembly;
        private System.Windows.Forms.Splitter expandableSplitter1;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.TreeView tvUsedIn;
        private System.Windows.Forms.ColumnHeader chUsedInName;
        private System.Windows.Forms.TreeView tvList;
        private System.Windows.Forms.ColumnHeader chMissingMembers;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.TreeView tvLog;
        private System.Windows.Forms.ColumnHeader chLog;
        private System.Windows.Forms.ImageList messageTypesList;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.ToolStripMenuItem miFrameworkFolder;
        private System.Windows.Forms.ToolStripMenuItem miCurrentFrameworkFolder;
        private System.Windows.Forms.ToolStripMenuItem miChangeFrameworkFolder;
        private System.Windows.Forms.ColumnHeader chScope;
        private System.Windows.Forms.ToolStripMenuItem miCopy;
    }
}