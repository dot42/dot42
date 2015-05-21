
namespace Dot42.ApkSpy
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
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.treeImages = new System.Windows.Forms.ImageList(this.components);
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileExportCode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.miFileRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.miSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.miFindClass = new System.Windows.Forms.ToolStripMenuItem();
            this.miFindNext = new System.Windows.Forms.ToolStripMenuItem();
            this.miFindPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miEmbedSourceCodePositions = new System.Windows.Forms.ToolStripMenuItem();
            this.miEmbedSourceCode = new System.Windows.Forms.ToolStripMenuItem();
            this.miShowControlFlow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.miEnableBaksmali = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.miConfigureBaksmali = new System.Windows.Forms.ToolStripMenuItem();
            this.miDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.miShowAst = new System.Windows.Forms.ToolStripMenuItem();
            this.miFullTypeNames = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.SuspendLayout();
            this.menuBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainContainer
            // 
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.Location = new System.Drawing.Point(0, 24);
            this.mainContainer.Name = "mainContainer";
            // 
            // mainContainer.Panel1
            // 
            this.mainContainer.Panel1.Controls.Add(this.treeView);
            this.mainContainer.Size = new System.Drawing.Size(956, 504);
            this.mainContainer.SplitterDistance = 318;
            this.mainContainer.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.treeView.AllowDrop = true;
            this.treeView.BackColor = System.Drawing.SystemColors.Window;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.PathSeparator = ";";
            this.treeView.Size = new System.Drawing.Size(318, 504);
            this.treeView.TabIndex = 0;
            this.treeView.Text = "advTree1";
            this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeViewBeforeExpand);
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewAfterNodeSelect);
            this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
            this.treeView.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_DragOver);
            // 
            // treeImages
            // 
            this.treeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImages.ImageStream")));
            this.treeImages.TransparentColor = System.Drawing.Color.Transparent;
            this.treeImages.Images.SetKeyName(0, "document_plain.png");
            this.treeImages.Images.SetKeyName(1, "folder_blue.png");
            this.treeImages.Images.SetKeyName(2, "Class.png");
            this.treeImages.Images.SetKeyName(3, "Constructor.png");
            this.treeImages.Images.SetKeyName(4, "Field.png");
            this.treeImages.Images.SetKeyName(5, "Interface.png");
            this.treeImages.Images.SetKeyName(6, "Method.png");
            this.treeImages.Images.SetKeyName(7, "NameSpace.png");
            // 
            // menuBar
            // 
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile,
            this.miSearch,
            this.optionsToolStripMenuItem,
            this.miDebug});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(956, 24);
            this.menuBar.TabIndex = 1;
            this.menuBar.Text = "menuStrip1";
            // 
            // miFile
            // 
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFileOpen,
            this.miFileExportCode,
            this.toolStripSeparator2,
            this.miFileRecent});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(37, 20);
            this.miFile.Text = "&File";
            this.miFile.DropDownOpening += new System.EventHandler(this.miFile_DropDownOpening);
            // 
            // miFileOpen
            // 
            this.miFileOpen.Name = "miFileOpen";
            this.miFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.miFileOpen.Size = new System.Drawing.Size(192, 22);
            this.miFileOpen.Text = "&Open";
            this.miFileOpen.Click += new System.EventHandler(this.miFileOpen_Click);
            // 
            // miFileExportCode
            // 
            this.miFileExportCode.Name = "miFileExportCode";
            this.miFileExportCode.Size = new System.Drawing.Size(192, 22);
            this.miFileExportCode.Text = "&Export with baksmali...";
            this.miFileExportCode.Click += new System.EventHandler(this.miFileExportCode_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(189, 6);
            // 
            // miFileRecent
            // 
            this.miFileRecent.Name = "miFileRecent";
            this.miFileRecent.Size = new System.Drawing.Size(192, 22);
            this.miFileRecent.Text = "Recently used";
            // 
            // miSearch
            // 
            this.miSearch.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFindClass,
            this.miFindNext,
            this.miFindPrevious});
            this.miSearch.Name = "miSearch";
            this.miSearch.Size = new System.Drawing.Size(54, 20);
            this.miSearch.Text = "Search";
            // 
            // miFindClass
            // 
            this.miFindClass.Name = "miFindClass";
            this.miFindClass.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.miFindClass.Size = new System.Drawing.Size(196, 22);
            this.miFindClass.Text = "Find class...";
            this.miFindClass.Click += new System.EventHandler(this.miFindClass_Click);
            // 
            // miFindNext
            // 
            this.miFindNext.Name = "miFindNext";
            this.miFindNext.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.miFindNext.Size = new System.Drawing.Size(196, 22);
            this.miFindNext.Text = "Find next";
            this.miFindNext.Click += new System.EventHandler(this.miFindNext_Click);
            // 
            // miFindPrevious
            // 
            this.miFindPrevious.Name = "miFindPrevious";
            this.miFindPrevious.ShortcutKeyDisplayString = "Shift+F3";
            this.miFindPrevious.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F3)));
            this.miFindPrevious.Size = new System.Drawing.Size(196, 22);
            this.miFindPrevious.Text = "Find previous";
            this.miFindPrevious.Visible = false;
            this.miFindPrevious.Click += new System.EventHandler(this.miFindPrevious_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miEmbedSourceCodePositions,
            this.miEmbedSourceCode,
            this.miShowControlFlow,
            this.miFullTypeNames,
            this.toolStripSeparator3,
            this.miEnableBaksmali,
            this.toolStripSeparator1,
            this.miConfigureBaksmali});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // miEmbedSourceCodePositions
            // 
            this.miEmbedSourceCodePositions.CheckOnClick = true;
            this.miEmbedSourceCodePositions.Name = "miEmbedSourceCodePositions";
            this.miEmbedSourceCodePositions.Size = new System.Drawing.Size(320, 22);
            this.miEmbedSourceCodePositions.Text = "Embed source code &locations into disassembly";
            this.miEmbedSourceCodePositions.Click += new System.EventHandler(this.miEmbedSourceCodePositions_Click);
            // 
            // miEmbedSourceCode
            // 
            this.miEmbedSourceCode.CheckOnClick = true;
            this.miEmbedSourceCode.Name = "miEmbedSourceCode";
            this.miEmbedSourceCode.Size = new System.Drawing.Size(320, 22);
            this.miEmbedSourceCode.Text = "Embed &source code into disassembly";
            this.miEmbedSourceCode.Click += new System.EventHandler(this.miEmbedSourceCode_Click);
            // 
            // miShowControlFlow
            // 
            this.miShowControlFlow.CheckOnClick = true;
            this.miShowControlFlow.Name = "miShowControlFlow";
            this.miShowControlFlow.Size = new System.Drawing.Size(320, 22);
            this.miShowControlFlow.Text = "Show control &flow";
            this.miShowControlFlow.Click += new System.EventHandler(this.miShowControlFlow_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(317, 6);
            // 
            // miEnableBaksmali
            // 
            this.miEnableBaksmali.CheckOnClick = true;
            this.miEnableBaksmali.Name = "miEnableBaksmali";
            this.miEnableBaksmali.Size = new System.Drawing.Size(320, 22);
            this.miEnableBaksmali.Text = "&Use Baksmali to show dex classes";
            this.miEnableBaksmali.Click += new System.EventHandler(this.miEnableBaksmali_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(317, 6);
            // 
            // miConfigureBaksmali
            // 
            this.miConfigureBaksmali.Name = "miConfigureBaksmali";
            this.miConfigureBaksmali.Size = new System.Drawing.Size(320, 22);
            this.miConfigureBaksmali.Text = "&Configure Baksmali...";
            this.miConfigureBaksmali.Click += new System.EventHandler(this.miConfigureBaksmali_Click);
            // 
            // miDebug
            // 
            this.miDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miShowAst});
            this.miDebug.Name = "miDebug";
            this.miDebug.Size = new System.Drawing.Size(54, 20);
            this.miDebug.Text = "Debug";
            // 
            // miShowAst
            // 
            this.miShowAst.CheckOnClick = true;
            this.miShowAst.Name = "miShowAst";
            this.miShowAst.Size = new System.Drawing.Size(123, 22);
            this.miShowAst.Text = "Show Ast";
            // 
            // miFullTypeNames
            // 
            this.miFullTypeNames.CheckOnClick = true;
            this.miFullTypeNames.Name = "miFullTypeNames";
            this.miFullTypeNames.Size = new System.Drawing.Size(320, 22);
            this.miFullTypeNames.Text = "Show full &type names";
            this.miFullTypeNames.Click += new System.EventHandler(this.miFullTypeNames_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 528);
            this.Controls.Add(this.mainContainer);
            this.Controls.Add(this.menuBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuBar;
            this.Name = "MainForm";
            this.Text = "APK Spy";
            this.mainContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
            this.mainContainer.ResumeLayout(false);
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miFileOpen;
        private System.Windows.Forms.ImageList treeImages;
        private System.Windows.Forms.ToolStripMenuItem miFileRecent;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripMenuItem miDebug;
        private System.Windows.Forms.ToolStripMenuItem miShowAst;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miEnableBaksmali;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem miConfigureBaksmali;
        private System.Windows.Forms.ToolStripMenuItem miFileExportCode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem miEmbedSourceCodePositions;
        private System.Windows.Forms.ToolStripMenuItem miEmbedSourceCode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem miShowControlFlow;
        private System.Windows.Forms.ToolStripMenuItem miSearch;
        private System.Windows.Forms.ToolStripMenuItem miFindClass;
        private System.Windows.Forms.ToolStripMenuItem miFindNext;
        private System.Windows.Forms.ToolStripMenuItem miFindPrevious;
        private System.Windows.Forms.ToolStripMenuItem miFullTypeNames;
    }
}