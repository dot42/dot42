using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dot42.BarDeployLib;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib;
using Dot42.Mapping;

namespace Dot42.Debugger.Forms
{
    public partial class MainForm : Form
    {
        private readonly DebuggerLib.Debugger debugger = new DebuggerLib.Debugger(null);
        private DalvikProcess lastProcess;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            lvProcesses.ItemActivated += OnProcessActivated;
            debugger.ConnectedChanged += OnDebuggerConnectedChanged;
            UpdateStatus();
        }

        /// <summary>
        /// Status notification
        /// </summary>
        private void OnDebuggerConnectedChanged(object sender, System.EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(OnDebuggerConnectedChanged), sender, e);
            }
            else
            {
                if (debugger.Connected && (lastProcess != debugger.Process))
                {
                    lastProcess = debugger.Process;
                    var ui = TaskScheduler.FromCurrentSynchronizationContext();
                    debugger.Process.IsSuspendedChanged += (s, x) => Task.Factory.StartNew(RefreshThreads, CancellationToken.None, TaskCreationOptions.None, ui);
                    
                }
                treeView.Nodes.Clear();                
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            lbStatus.Text = debugger.Connected ? "connected to " + debugger.AppName : "not connected";
        }

        /// <summary>
        /// Time time initialization
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            lvDevices.DeviceMonitorEnabled = true;
        }

        private void OnProcessActivated(object sender, Utility.EventArgs<int> e)
        {
            Attach(e.Data);
        }

        private void OnAttachClick(object sender, EventArgs e)
        {
            using (var dialog = new AttachForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                Attach(dialog.Pid);
            }
        }

        /// <summary>
        /// Attach to the given process ID.
        /// </summary>
        private void Attach(int pid)
        {
            MapFile mapFile;
            string mapFileName;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select map file";
                dialog.DefaultExt = ".d42map";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                mapFileName = dialog.FileName;
                mapFile = new MapFile(mapFileName);
            }

            var device = lvDevices.SelectedDevice.Device;
            var connectTask = Task.Factory.StartNew(() => debugger.Connect(device, pid, mapFile, Path.ChangeExtension(mapFileName, "apk")));
            connectTask.ContinueWith(t => debugger.PrepareAsync());
        }

        /// <summary>
        /// Device selection has changed.
        /// </summary>
        private void OnSelectedDeviceChanged(object sender, EventArgs e)
        {
            var selection = lvDevices.SelectedDevice;
            var device = ((selection != null) && selection.IsConnected) ? selection.Device : null;
            lvProcesses.Device = device;
        }

        /// <summary>
        /// Exit the VM
        /// </summary>
        private void cmdExit_Click(object sender, EventArgs e)
        {
            debugger.Process.ExitAndDisconnect(0);
        }

        /// <summary>
        /// Refresh the list of threads.
        /// </summary>
        private void RefreshThreads()
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            foreach (var thread in debugger.Process.ThreadManager.Threads)
            {
                var node = new TreeNode(thread.Id.ToString());
                node.Tag = thread;
                thread.GetNameAsync().ContinueWith(x => { node.Text = x.Result; }, ui);
                treeView.Nodes.Add(node);
            }
            treeView.EndUpdate();            
        }

        private void treeView_AfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            var node = treeView.SelectedNode;
            if (node != null) tbEntry.Text = node.Text;

            DalvikStackFrame stackFrame;
            ReferenceTypeId typeId;
            MethodId methodId;
            cmdLocals.Enabled = TryGetStackFrame(node, out stackFrame);
            cmdMethods.Enabled = TryGetTypeId(node, out typeId);
            cmdVariableTable.Enabled = TryGetTypeAndMethodId(node, out typeId, out methodId);
        }

        private void cmdSuspend_Click(object sender, EventArgs e)
        {
            debugger.Process.SuspendAsync();
        }

        private void cmdResume_Click(object sender, EventArgs e)
        {
            debugger.Process.ResumeAsync();
        }

        private void cmdHelo_Click(object sender, EventArgs e)
        {
            debugger.SendHelo();
        }

        private void cmdEnableThreadStatus_Click(object sender, EventArgs e)
        {
            debugger.Ddms.SendThreadStatus();
        }

        private void cmdCallStack_Click(object sender, EventArgs e)
        {
            var selection = treeView.SelectedNode;
            var thread = (selection != null) ? selection.Tag as DalvikThread: null;
            if (thread == null)
                return;
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() => thread.GetCallStack()).ContinueWith(x => {
                treeView.BeginUpdate();
                selection.Nodes.Clear();
                foreach (var frame in x.Result)
                {
                    var node = new TreeNode(frame.Location.ToString()) { Tag = frame };
                    selection.Nodes.Add(node);
                }
                selection.Expand();
                treeView.EndUpdate();
            }, ui);
        }

        private void cmdAllClasses_Click(object sender, EventArgs e)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            debugger.VirtualMachine.AllClassesWithGenericAsync().ContinueWith(x => {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();
                foreach (var classInfo in x.Result.OrderBy(c => c.Signature))
                {
                    var node = new TreeNode(classInfo.Signature + " " + classInfo.TypeId) { Tag = classInfo };
                    treeView.Nodes.Add(node);
                }
                treeView.EndUpdate();
            }, ui);
        }

        private void cmdClassBySignature_Click(object sender, EventArgs e)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            debugger.VirtualMachine.ClassBySignatureAsync(tbEntry.Text).ContinueWith(x => {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();
                foreach (var classInfo in x.Result.OrderBy(c => c.Signature))
                {
                    var node = new TreeNode(classInfo.Signature) { Tag = classInfo };
                    treeView.Nodes.Add(node);
                }
                treeView.EndUpdate();
            }, ui);
        }

        /// <summary>
        /// Get the values of local variables in the current stackframe.
        /// </summary>
        private void cmdLocals_Click(object sender, EventArgs e)
        {
            var selection = treeView.SelectedNode;
            DalvikStackFrame frame;
            if (!TryGetStackFrame(selection, out frame))
                return;

            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            frame.GetValuesAsync().ContinueWith(x => {
                treeView.BeginUpdate();
                foreach (var sfValue in x.Result)
                {
                    var node = new TreeNode(sfValue.Name + "=" + sfValue.Value);
                    selection.Nodes.Add(node);
                }
                treeView.EndUpdate();
            }, ui);
        }

        /// <summary>
        /// Gets the local variable table of the current method.
        /// </summary>
        private void getVariableTable_Click(object sender, EventArgs e)
        {
            var selection = treeView.SelectedNode;
            ReferenceTypeId typeId;
            MethodId methodId;
            if (!TryGetTypeAndMethodId(selection, out typeId, out methodId))
                return;

            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            debugger.Method.VariableTableWithGenericAsync(typeId, methodId).ContinueWith(x => {
                treeView.BeginUpdate();
                foreach (var variableInfo in x.Result)
                {
                    var node = new TreeNode(variableInfo.Name) { Tag = variableInfo };
                    selection.Nodes.Add(node);
                }
                treeView.EndUpdate();
            }, ui);
        }

        /// <summary>
        /// Get all methods of the current type.
        /// </summary>
        private void cmdMethods_Click(object sender, EventArgs e)
        {
            var selection = treeView.SelectedNode;
            ReferenceTypeId typeId;
            if (!TryGetTypeId(selection, out typeId))
                return;

            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            debugger.ReferenceType.MethodsAsync(typeId).ContinueWith(x => {
                treeView.BeginUpdate();
                foreach (var methodInfo in x.Result)
                {
                    var node = new TreeNode(methodInfo.Name + " " + methodInfo.Id + " " + ((AccessFlags)methodInfo.AccessFlags)) { Tag = methodInfo };
                    selection.Nodes.Add(node);
                }
                treeView.EndUpdate();
            }, ui);
        }

        /// <summary>
        /// Try to get a type ID from the given node.
        /// </summary>
        private static bool TryGetTypeId(TreeNode node, out ReferenceTypeId typeId)
        {
            typeId = null;
            if (node == null)
                return false;
            var tag = node.Tag;
            if (tag is DalvikStackFrame)
            {
                typeId = ((DalvikStackFrame)tag).Location.Class;
            }
            else if (tag is ClassInfo)
            {
                typeId = ((ClassInfo)tag).TypeId;
            }
            return (typeId != null);
        }

        /// <summary>
        /// Try to get a type and method ID from the given node.
        /// </summary>
        private static bool TryGetTypeAndMethodId(TreeNode node, out ReferenceTypeId typeId, out MethodId methodId)
        {
            typeId = null;
            methodId = null;
            if (node == null)
                return false;
            var tag = node.Tag;
            if (tag is DalvikStackFrame)
            {
                typeId = ((DalvikStackFrame) tag).Location.Class;
                methodId = ((DalvikStackFrame) tag).Location.Method;
            }
            else if (tag is MethodInfo)
            {
                methodId = ((MethodInfo) tag).Id;
                TryGetTypeId(node.Parent, out typeId);
            }
            return (methodId != null) && (typeId != null);
        }

        /// <summary>
        /// Try to get a stackframe from the given node.
        /// </summary>
        private static bool TryGetStackFrame(TreeNode node, out DalvikStackFrame stackFrame)
        {
            stackFrame = (node != null) ? node.Tag as DalvikStackFrame : null;
            return (stackFrame != null);
        }

        private void cmdBlackberryLogin_Click(object sender, EventArgs e)
        {
            RunDeployer(x => x.Login());
        }

        private void cmdBlackBerryInstall_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".bar";
                dialog.Filter = "BAR files|*.bar";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var barPath = dialog.FileName;
                    var launch = cbLaunchAfterInstall.Checked;
                    RunDeployer(x => x.Install(barPath, launch));
                }
            }
        }

        private void cmdBlackBerryUninstall_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".bar";
                dialog.Filter = "BAR files|*.bar";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var barPath = dialog.FileName;
                    RunDeployer(x => x.UninstallFile(barPath));
                }
            }
        }

        /// <summary>
        /// Run the deployer in a background task.
        /// </summary>
        private void RunDeployer(Action<BarDeployLib.BarDeployer> action)
        {            
            treeView.Nodes.Clear();
            var device = BlackBerryDevices.Instance.DefaultDevice;
            var deployer = new BarDeployer(device) { Logger = LogResults };
            Enabled = false;
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() => {
                try
                {
                    action(deployer);
                }
                catch (Exception ex)
                {
                    LogResults("Failure: " + ex.Message);
                }
            }).ContinueWith(t => { Enabled = true; }, ui);
        }

        private void LogResults(string line)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogResults), line);
            }
            else
            {
                treeView.Nodes.Add(new TreeNode(line));
            }
        }
    }
}
