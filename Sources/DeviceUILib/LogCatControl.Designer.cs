namespace Dot42.DeviceLib.UI
{
    partial class LogCatControl
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
            this.components = new System.ComponentModel.Container();
            this.tvLog = new System.Windows.Forms.ListView();
            this.chPid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chTid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chTag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.treeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miFilterOnThisValue = new System.Windows.Forms.ToolStripMenuItem();
            this.miSepBeforeCopy = new System.Windows.Forms.ToolStripSeparator();
            this.miCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tbEntry = new System.Windows.Forms.TextBox();
            this.expandableSplitter1 = new System.Windows.Forms.Splitter();
            this.trackVisibleTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.miFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.miVerbose = new System.Windows.Forms.ToolStripMenuItem();
            this.miDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.miInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.miWarning = new System.Windows.Forms.ToolStripMenuItem();
            this.miError = new System.Windows.Forms.ToolStripMenuItem();
            this.miAssert = new System.Windows.Forms.ToolStripMenuItem();
            this.miFilterSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.miFilterExtra = new System.Windows.Forms.ToolStripMenuItem();
            this.miFilterExtraRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.miView = new System.Windows.Forms.ToolStripMenuItem();
            this.miAutoScroll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.treeContextMenuStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvLog
            // 
            this.tvLog.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.tvLog.AllowDrop = true;
            this.tvLog.BackColor = System.Drawing.SystemColors.Window;
            this.tvLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chPid,
            this.chTid,
            this.chTime,
            this.chMessage,
            this.chLevel,
            this.chTag});
            this.tvLog.ContextMenuStrip = this.treeContextMenuStrip;
            this.tvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvLog.FullRowSelect = true;
            this.tvLog.Location = new System.Drawing.Point(0, 24);
            this.tvLog.MultiSelect = false;
            this.tvLog.Name = "tvLog";
            this.tvLog.Size = new System.Drawing.Size(854, 194);
            this.tvLog.TabIndex = 1;
            this.tvLog.UseCompatibleStateImageBehavior = false;
            this.tvLog.View = System.Windows.Forms.View.Details;
            this.tvLog.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // chPid
            // 
            this.chPid.Text = "PID";
            this.chPid.Width = 75;
            // 
            // chTid
            // 
            this.chTid.Text = "TID";
            this.chTid.Width = 75;
            // 
            // chTime
            // 
            this.chTime.Text = "Time";
            this.chTime.Width = 75;
            // 
            // chMessage
            // 
            this.chMessage.Text = "Message";
            this.chMessage.Width = 75;
            // 
            // chLevel
            // 
            this.chLevel.Text = "Level";
            this.chLevel.Width = 75;
            // 
            // chTag
            // 
            this.chTag.Text = "Source";
            this.chTag.Width = 75;
            // 
            // treeContextMenuStrip
            // 
            this.treeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFilterOnThisValue,
            this.miSepBeforeCopy,
            this.miCopy});
            this.treeContextMenuStrip.Name = "treeContextMenuStrip";
            this.treeContextMenuStrip.Size = new System.Drawing.Size(171, 54);
            this.treeContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.treeContextMenuStrip_Opening);
            // 
            // miFilterOnThisValue
            // 
            this.miFilterOnThisValue.Name = "miFilterOnThisValue";
            this.miFilterOnThisValue.Size = new System.Drawing.Size(170, 22);
            this.miFilterOnThisValue.Text = "Filter on this value";
            this.miFilterOnThisValue.Click += new System.EventHandler(this.miFilterOnThisValue_Click);
            // 
            // miSepBeforeCopy
            // 
            this.miSepBeforeCopy.Name = "miSepBeforeCopy";
            this.miSepBeforeCopy.Size = new System.Drawing.Size(167, 6);
            // 
            // miCopy
            // 
            this.miCopy.Name = "miCopy";
            this.miCopy.Size = new System.Drawing.Size(170, 22);
            this.miCopy.Text = "Copy";
            this.miCopy.Click += new System.EventHandler(this.OnCopyToClipboardClick);
            // 
            // tbEntry
            // 
            this.tbEntry.BackColor = System.Drawing.Color.White;
            this.tbEntry.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbEntry.ForeColor = System.Drawing.Color.Black;
            this.tbEntry.Location = new System.Drawing.Point(0, 224);
            this.tbEntry.MaxLength = 2000000;
            this.tbEntry.Multiline = true;
            this.tbEntry.Name = "tbEntry";
            this.tbEntry.ReadOnly = true;
            this.tbEntry.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbEntry.Size = new System.Drawing.Size(854, 196);
            this.tbEntry.TabIndex = 2;
            // 
            // expandableSplitter1
            // 
            this.expandableSplitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(239)))), ((int)(((byte)(255)))));
            this.expandableSplitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.expandableSplitter1.ForeColor = System.Drawing.Color.Black;
            this.expandableSplitter1.Location = new System.Drawing.Point(0, 218);
            this.expandableSplitter1.Name = "expandableSplitter1";
            this.expandableSplitter1.Size = new System.Drawing.Size(854, 6);
            this.expandableSplitter1.TabIndex = 3;
            this.expandableSplitter1.TabStop = false;
            // 
            // trackVisibleTimer
            // 
            this.trackVisibleTimer.Enabled = true;
            this.trackVisibleTimer.Interval = 300;
            this.trackVisibleTimer.Tick += new System.EventHandler(this.OnTrackVisibleTimerTick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFilter,
            this.miView});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(854, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // miFilter
            // 
            this.miFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miVerbose,
            this.miDebug,
            this.miInfo,
            this.miWarning,
            this.miError,
            this.miAssert,
            this.miFilterSeparator,
            this.miFilterExtra});
            this.miFilter.Name = "miFilter";
            this.miFilter.Size = new System.Drawing.Size(45, 20);
            this.miFilter.Text = "&Filter";
            this.miFilter.DropDownOpening += new System.EventHandler(this.miFilter_DropDownOpening);
            // 
            // miVerbose
            // 
            this.miVerbose.CheckOnClick = true;
            this.miVerbose.Name = "miVerbose";
            this.miVerbose.Size = new System.Drawing.Size(119, 22);
            this.miVerbose.Text = "Verbose";
            this.miVerbose.CheckedChanged += new System.EventHandler(this.OnLogLevelChanged);
            // 
            // miDebug
            // 
            this.miDebug.CheckOnClick = true;
            this.miDebug.Name = "miDebug";
            this.miDebug.Size = new System.Drawing.Size(119, 22);
            this.miDebug.Text = "Debug";
            this.miDebug.CheckedChanged += new System.EventHandler(this.OnLogLevelChanged);
            // 
            // miInfo
            // 
            this.miInfo.CheckOnClick = true;
            this.miInfo.Name = "miInfo";
            this.miInfo.Size = new System.Drawing.Size(119, 22);
            this.miInfo.Text = "Info";
            this.miInfo.CheckedChanged += new System.EventHandler(this.OnLogLevelChanged);
            // 
            // miWarning
            // 
            this.miWarning.CheckOnClick = true;
            this.miWarning.Name = "miWarning";
            this.miWarning.Size = new System.Drawing.Size(119, 22);
            this.miWarning.Text = "Warning";
            this.miWarning.CheckedChanged += new System.EventHandler(this.OnLogLevelChanged);
            // 
            // miError
            // 
            this.miError.CheckOnClick = true;
            this.miError.Name = "miError";
            this.miError.Size = new System.Drawing.Size(119, 22);
            this.miError.Text = "Error";
            this.miError.CheckedChanged += new System.EventHandler(this.OnLogLevelChanged);
            // 
            // miAssert
            // 
            this.miAssert.CheckOnClick = true;
            this.miAssert.Name = "miAssert";
            this.miAssert.Size = new System.Drawing.Size(119, 22);
            this.miAssert.Text = "Assert";
            this.miAssert.CheckedChanged += new System.EventHandler(this.OnLogLevelChanged);
            // 
            // miFilterSeparator
            // 
            this.miFilterSeparator.Name = "miFilterSeparator";
            this.miFilterSeparator.Size = new System.Drawing.Size(116, 6);
            // 
            // miFilterExtra
            // 
            this.miFilterExtra.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFilterExtraRemove});
            this.miFilterExtra.Name = "miFilterExtra";
            this.miFilterExtra.Size = new System.Drawing.Size(119, 22);
            this.miFilterExtra.Text = "tmp";
            // 
            // miFilterExtraRemove
            // 
            this.miFilterExtraRemove.Name = "miFilterExtraRemove";
            this.miFilterExtraRemove.Size = new System.Drawing.Size(117, 22);
            this.miFilterExtraRemove.Text = "Remove";
            this.miFilterExtraRemove.Click += new System.EventHandler(this.miFilterExtraRemove_Click);
            // 
            // miView
            // 
            this.miView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAutoScroll});
            this.miView.Name = "miView";
            this.miView.Size = new System.Drawing.Size(44, 20);
            this.miView.Text = "View";
            // 
            // miAutoScroll
            // 
            this.miAutoScroll.Checked = true;
            this.miAutoScroll.CheckOnClick = true;
            this.miAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miAutoScroll.Name = "miAutoScroll";
            this.miAutoScroll.Size = new System.Drawing.Size(131, 22);
            this.miAutoScroll.Text = "Auto scroll";
            // 
            // LogCatControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvLog);
            this.Controls.Add(this.expandableSplitter1);
            this.Controls.Add(this.tbEntry);
            this.Controls.Add(this.menuStrip1);
            this.Name = "LogCatControl";
            this.Size = new System.Drawing.Size(854, 420);
            this.treeContextMenuStrip.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView tvLog;
        private System.Windows.Forms.TextBox tbEntry;
        private System.Windows.Forms.Splitter expandableSplitter1;
        private System.Windows.Forms.Timer trackVisibleTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem miFilter;
        private System.Windows.Forms.ToolStripMenuItem miVerbose;
        private System.Windows.Forms.ToolStripMenuItem miDebug;
        private System.Windows.Forms.ToolStripMenuItem miInfo;
        private System.Windows.Forms.ToolStripMenuItem miWarning;
        private System.Windows.Forms.ToolStripMenuItem miError;
        private System.Windows.Forms.ToolStripMenuItem miAssert;
        private System.Windows.Forms.ToolStripMenuItem miView;
        private System.Windows.Forms.ToolStripMenuItem miAutoScroll;
        private System.Windows.Forms.ContextMenuStrip treeContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem miFilterOnThisValue;
        private System.Windows.Forms.ToolStripSeparator miFilterSeparator;
        private System.Windows.Forms.ToolStripMenuItem miFilterExtra;
        private System.Windows.Forms.ToolStripMenuItem miFilterExtraRemove;
        private System.Windows.Forms.ToolStripMenuItem miCopy;
        private System.Windows.Forms.ToolStripSeparator miSepBeforeCopy;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader chPid;
        private System.Windows.Forms.ColumnHeader chTid;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ColumnHeader chMessage;
        private System.Windows.Forms.ColumnHeader chLevel;
        private System.Windows.Forms.ColumnHeader chTag;
    }
}
