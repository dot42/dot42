using System;
using System.Linq;
using System.Windows.Forms;
using Dot42.AdbLib;
using Dot42.Utility;
using TallComponents.Common.Extensions;
using TallComponents.Common.Util;

namespace Dot42.DeviceLib.UI
{
    public partial class JdwpProcessListView : UserControl
    {
        /// <summary>
        /// The current device selection has changed.
        /// </summary>
        public event EventHandler SelectedProcessChanged;

        /// <summary>
        /// Fired when a list item was activated.
        /// </summary>
        public event EventHandler<EventArgs<int>> ItemActivated;

        private IDevice device;
        private JdwpMonitor monitor;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpProcessListView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets/sets the context menu strip of the listview.
        /// </summary>
        public new ContextMenuStrip ContextMenuStrip
        {
            get { return lvProcesses.ContextMenuStrip; }
            set { lvProcesses.ContextMenuStrip = value; }
        }

        /// <summary>
        /// Gets/sets the monitored device.
        /// </summary>
        public IDevice Device
        {
            get { return device; }
            set
            {
                if (device != value)
                {
                    if (monitor != null)
                    {
                        monitor.Dispose();
                        monitor = null;
                    }
                    lvProcesses.Items.Clear();
                    device = value;
                    if (value != null)
                    {
                        monitor = new JdwpMonitor(value);
                        monitor.ProcessAdded += OnProcessAdded;
                        monitor.ProcessRemoved += OnProcessRemoved;
                        monitor.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// A process has been added.
        /// </summary>
        void OnProcessAdded(object sender, EventArgs<int> e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<EventArgs<int>>(OnProcessAdded), sender, e);
            }
            else
            {
                lvProcesses.Items.Add(new ProcessItem(e.Data));
            }
        }

        /// <summary>
        /// A pid has been removed.
        /// </summary>
        void OnProcessRemoved(object sender, EventArgs<int> e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<EventArgs<int>>(OnProcessRemoved), sender, e);
            }
            else
            {
                var toRemove = lvProcesses.Items.Cast<ProcessItem>().Where(x => x.Pid == e.Data).ToList();
                foreach (var item in toRemove)
                {
                    lvProcesses.Items.Remove(item);
                }
            }
        }

        /// <summary>
        /// Selection changed.
        /// </summary>
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProcessChanged.Fire(this);
        }

        /// <summary>
        /// Item was activated
        /// </summary>
        private void OnItemActivate(object sender, EventArgs e)
        {
            try
            {
                var selection = SelectedProcess;
                if (selection >= 0)
                {
                    ItemActivated.Fire(this, new EventArgs<int>(selection));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
        }

        /// <summary>
        /// Gets the selected pid or -1 if there is no selection.
        /// </summary>
        public int SelectedProcess
        {
            get
            {
                var selection = (lvProcesses.SelectedItems.Count > 0) ? lvProcesses.SelectedItems[0] : null;
                return (selection != null) ? ((ProcessItem)selection).Pid : -1;
            }
        }

        private sealed class ProcessItem : ListViewItem
        {
            public readonly int Pid;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ProcessItem(int pid)
            {
                Pid = pid;
                Text = pid.ToString();
            }
        }
    }
}
