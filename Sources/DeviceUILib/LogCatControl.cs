using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Dot42.AdbLib;
using Dot42.Shared.UI;
using TallComponents.Common.Util;

namespace Dot42.DeviceLib.UI
{
    public partial class LogCatControl : UserControl
    {
        private readonly List<LogEntry> entries = new List<LogEntry>();
        private LogEntryNode lastNode;
        private bool prefsInitialized;
        private IColumnFilter filter;
        private Thread thread;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LogCatControl()
        {
            InitializeComponent();
            var prefs = UserPreferences.Preferences;
            miVerbose.Checked = prefs.LogCatShowVerbose;
            miDebug.Checked = prefs.LogCatShowDebug;
            miInfo.Checked = prefs.LogCatShowInfo;
            miWarning.Checked = prefs.LogCatShowWarning;
            miError.Checked = prefs.LogCatShowError;
            miAssert.Checked = prefs.LogCatShowAssert;
            prefsInitialized = true;
        }

        /// <summary>
        /// Run the log service on the given device.
        /// </summary>
        public void Run(IDevice device)
        {
            // Run a log server.
            if (device != null)
            {
                var newThread = new Thread(x => RunLogServiceAsync(device)) {IsBackground = true};
                thread = newThread;
                newThread.Start();
            }
            else
            {
                // Stop
                thread = null;
            }
        }
        
        /// <summary>
        /// Background thread
        /// </summary>
        private void RunLogServiceAsync(IDevice device)
        {
            var attempt = 0;
            while (attempt < 10)
            {
                attempt++;
                try
                {
                    // Ensure server is running
                    var adb = new Adb();
                    adb.StartServer(Adb.Timeout.StartServer);

                    // Run log messages
                    adb.RunLogService(new LogServiceListener(this, Thread.CurrentThread), device, Adb.LogNames.Main);
                }
                catch (Exception ex)
                {
                    ErrorLog.DumpError(ex);
                }
            }
        }

        /// <summary>
        /// Add received output data.
        /// </summary>
        public void AddEntry(LogEntry entry)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<LogEntry>(AddEntry), entry);
            }
            else
            {
                try
                {
                    entries.Add(entry);
                    if (Show(entry))
                    {
                        var node = new LogEntryNode(entry);
                        lastNode = node;
                        tvLog.Items.Add(node);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.DumpError(ex);
                }
            }
        }

        /// <summary>
        /// Collapse the bottom text view
        /// </summary>
        public void HideTextView()
        {
            // expandableSplitter1.Expanded = false;
        }

        /// <summary>
        /// Ensure that the last added entry is visible.
        /// </summary>
        private void OnTrackVisibleTimerTick(object sender, EventArgs e)
        {
            var node = lastNode;
            lastNode = null;
            if ((node != null) && miAutoScroll.Checked)
            {
                node.EnsureVisible();
            }
        }

        /// <summary>
        /// Should the given entry be made visible?
        /// </summary>
        protected virtual bool Show(LogEntry entry)
        {
            if (filter != null)
            {
                if (!filter.IsVisible(entry))
                    return false;
            }
            switch (entry.Level)
            {
                case LogLevel.Verbose:
                    return miVerbose.Checked;
                case LogLevel.Debug:
                    return miDebug.Checked;
                case LogLevel.Info:
                    return miInfo.Checked;
                case LogLevel.Warning:
                    return miWarning.Checked;
                case LogLevel.Error:
                    return miError.Checked;
                case LogLevel.Assert:
                    return miAssert.Checked;
                default:
                    return true;
            }            
        }

        /// <summary>
        /// Convert a loglevel to a human readable string.
        /// </summary>
        private static string LogLevelToString(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Verbose:
                    return Properties.Resources.LogLevelVerbose;
                case LogLevel.Debug:
                    return Properties.Resources.LogLevelDebug;
                case LogLevel.Info:
                    return Properties.Resources.LogLevelInfo;
                case LogLevel.Warning:
                    return Properties.Resources.LogLevelWarning;
                case LogLevel.Error:
                    return Properties.Resources.LogLevelError;
                case LogLevel.Assert:
                    return Properties.Resources.LogLevelAssert;
                default:
                    return ((int) level).ToString();
            }
        }

        /// <summary>
        /// Selection has changed.
        /// </summary>
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var selection = tvLog.SelectedItems[0] as LogEntryNode;
            var entry = (selection != null) ? selection.Entry : null;
            tbEntry.Text = (entry != null) ? entry.MessageAsString : string.Empty;
        }

        /// <summary>
        /// Log level setting has changed.
        /// </summary>
        private void OnLogLevelChanged(object sender, EventArgs e)
        {
            if (!prefsInitialized)
                return;

            // Update list
            var selection = tvLog.SelectedItems[0] as LogEntryNode;
            var entry = (selection != null) ? selection.Entry : null;
            ApplyFilter(entry);

            // Save preferences
            var prefs = UserPreferences.Preferences;
            prefs.LogCatShowVerbose = miVerbose.Checked;
            prefs.LogCatShowDebug = miDebug.Checked;
            prefs.LogCatShowInfo = miInfo.Checked;
            prefs.LogCatShowWarning = miWarning.Checked;
            prefs.LogCatShowError = miError.Checked;
            prefs.LogCatShowAssert = miAssert.Checked;
            UserPreferences.SaveNow();            
        }

        /// <summary>
        /// Rebuild the list with the new filter.
        /// </summary>
        private void ApplyFilter(LogEntry active)
        {
            tvLog.BeginUpdate();
            var newNodes = entries.Where(Show).Select(x => new LogEntryNode(x)).ToArray();
            tvLog.Items.Clear();
            tvLog.Items.AddRange(newNodes);
            var activeNode = newNodes.FirstOrDefault(x => x.Entry == active);
            lastNode = activeNode ?? newNodes.LastOrDefault();
            tvLog.EndUpdate();      
            if (activeNode != null)
            {
                activeNode.Selected = true;
            }
        }

        /// <summary>
        /// Tree context menu is about to open.
        /// </summary>
        private void treeContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var screenPosition = Control.MousePosition;
            var pt = tvLog.PointToClient(screenPosition);
            var cell = tvLog.GetChildAtPoint(pt);
            if (cell != null)
            {
                var node = cell.Parent;
                // var index = node.Items.IndexOf(cell);
                // var col = tvLog.Columns[index];
                // var newFilter = CreateFilter(index, cell.Text);
                // miFilterOnThisValue.Visible = newFilter != null;
                // miFilterOnThisValue.Text = (newFilter != null) ? newFilter.Title : "-";
                // miFilterOnThisValue.Tag = newFilter;
                //if (newFilter == null) e.Cancel = true;
                miCopy.Visible = true;
                miSepBeforeCopy.Visible = miFilterOnThisValue.Visible;
            }
            else
            {
                miFilterOnThisValue.Visible = false;
                miSepBeforeCopy.Visible = false;
                miCopy.Visible = false;
            }
        }

        /// <summary>
        /// Filter menu is opening
        /// </summary>
        private void miFilter_DropDownOpening(object sender, EventArgs e)
        {
            var hasExtraFilter = (filter != null);
            miFilterExtra.Visible = hasExtraFilter;
            miFilterExtra.Text = (filter != null) ? filter.Title : "?";
            miFilterSeparator.Visible = hasExtraFilter;
        }

        /// <summary>
        /// Create a filter for the given column.
        /// </summary>
        private IColumnFilter CreateFilter(int columnIndex, string cellText)
        {
            switch (columnIndex)
            {
                case 1:
                    return new PidFilter(cellText);
                case 2:
                    return new TidFilter(cellText);
                case 4:
                    return new SourceFilter(cellText);
                default:
                    return null;
            }            
        }

        /// <summary>
        /// Apply filter.
        /// </summary>
        private void miFilterOnThisValue_Click(object sender, EventArgs e)
        {
            filter = (IColumnFilter) miFilterOnThisValue.Tag;
            var selection = tvLog.SelectedItems[0] as LogEntryNode;
            var entry = (selection != null) ? selection.Entry : null;
            ApplyFilter(entry);
        }

        /// <summary>
        /// Copy current entry to clipboard.
        /// </summary>
        private void OnCopyToClipboardClick(object sender, EventArgs e)
        {
            var selection = tvLog.SelectedItems[0] as LogEntryNode;
            if (selection == null)
                return;
            Clipboard.SetText(selection.Entry.MessageAsString);
        }

        /// <summary>
        /// Remove extra filter.
        /// </summary>
        private void miFilterExtraRemove_Click(object sender, EventArgs e)
        {
            filter = null;
            var selection = tvLog.SelectedItems[0] as LogEntryNode;
            var entry = (selection != null) ? selection.Entry : null;
            ApplyFilter(entry);
        }

        /// <summary>
        /// Show tooltip on hover.
        /// </summary>
        private void tvLog_NodeMouseHover(object sender, EventArgs e)
        {
            /*
            var node = tvLog.SelectedItems[0];
            if ((node == null) || string.IsNullOrEmpty(node.ToolTip))
                return;
            //toolTip1.IsBalloon = true;
            toolTip1.InitialDelay = 0;
            toolTip1.Hide(this);
            var pt = new Point(e.X + 20, e.Y + 20);
            pt = tvLog.PointToScreen(pt);
            pt = this.PointToClient(pt);
            toolTip1.Show(node.Tooltip, this, pt, 30 * 1000);
            */
        }

        private sealed class LogServiceListener : ILogListener
        {
            private readonly LogCatControl control;
            private readonly Thread myThread;

            /// <summary>
            /// Default ctor
            /// </summary>
            public LogServiceListener(LogCatControl control, Thread myThread)
            {
                this.control = control;
                this.myThread = myThread;
            }

            /// <summary>
            /// Add received output data.
            /// </summary>
            public void AddEntry(LogEntry entry)
            {
                if (IsCancelled)
                    return;
                control.AddEntry(entry);
            }

            /// <summary>
            /// Should further processing be cancelled?
            /// </summary>
            public bool IsCancelled 
            {
                get { return (control.thread != myThread) || control.IsDisposed; }
            }
        }

        /// <summary>
        /// Node showing a log entry.
        /// </summary>
        private sealed class LogEntryNode : ListViewItem
        {
            private readonly LogEntry entry;

            /// <summary>
            /// Default ctor
            /// </summary>
            public LogEntryNode(LogEntry entry)
            {
                this.entry = entry;
                ToolTipText = entry.MessageAsString;
                Text = entry.Pid.ToString();
                SubItems.Add(entry.Tid.ToString());
                SubItems.Add(entry.Time.ToShortTimeString());
                SubItems.Add(entry.MessageAsString);
                SubItems.Add(LogLevelToString(entry.Level));
                SubItems.Add(entry.Tag);
            }

            /// <summary>
            /// Gets the log entry.
            /// </summary>
            public LogEntry Entry
            {
                get { return entry; }
            }
        }
    }
}
