using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dot42.ApkSpy.Disassembly;
using Dot42.ApkSpy.IPC;
using Dot42.ApkSpy.Tree;
using Dot42.Utility;
using Ookii.Dialogs;
using TallComponents.Common.Util;
using Node = Dot42.ApkSpy.Tree.Node;

namespace Dot42.ApkSpy
{
    public partial class MainForm : Form, ISpySettings
    {
        private SourceFile source;
        private string originalPath;

        private bool initialized;
        private string lastSearchString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
#if !DEBUG && !ENABLE_SHOW_AST
            miDebug.Visible = false;
#elif DEBUG
            Locations.SetTarget(null);
            miShowAst.Checked = true;
#endif
            miEnableBaksmali.Checked = SettingsPersitor.EnableBaksmali;
            miEmbedSourceCode.Checked = SettingsPersitor.EmbedSourceCode;
            miEmbedSourceCodePositions.Checked = SettingsPersitor.EmbedSourceCodePositions;
            miShowControlFlow.Checked = SettingsPersitor.ShowControlFlow;
        }

        /// <summary>
        /// Load last used file
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Size = SettingsPersitor.WindowSize;
            var location = SettingsPersitor.WindowLocation;
            if (Screen.AllScreens.Any(x => x.Bounds.Contains(location)))
                Location = SettingsPersitor.WindowLocation;
            string path = SettingsPersitor.Files.FirstOrDefault();

            string[] arguments = Environment.GetCommandLineArgs();
            if (null != arguments)
                if (arguments.Length > 1)
                    if (!string.IsNullOrEmpty(arguments[1]))
                        if (File.Exists(arguments[1]))
                            path = arguments[1];

            if (path != null)
                Open(path);
            initialized = true;
        }

        /// <summary>
        /// Record size changes
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (initialized)
            {
                SettingsPersitor.WindowSize = Size;
            }
        }

        /// <summary>
        /// Record location changes
        /// </summary>
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (initialized)
            {
                SettingsPersitor.WindowLocation = Location;
            }
        }

        /// <summary>
        /// Open the given file.
        /// </summary>
        internal void Open(string path)
        {
            if (source != null)
            {
                source.Dispose();
            }
            source = null;
            originalPath = path;
            try
            {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();
                mainContainer.Panel2.Controls.Clear();

                source = SourceFile.Open(path, this);
                foreach (var fileName in source.FileNames.OrderBy(x => x))
                {
                    var parent = treeView.Nodes.GetParentForFile(Path.GetDirectoryName(fileName), 1, new[] { '/', '\\' });
                    parent.Add(NodeBuilders.Create(source, fileName));
                }

                foreach (Node node in treeView.Nodes)
                {
                    node.EnsureChildNodesCreated();
                }

                //treeView.Sort();
                Text = string.Format("{0} - [{1}]", Application.ProductName, path);

                // Add to MRU
                SettingsPersitor.Add(path);

                // Try to re-open last known tree path
                TreePath = SettingsPersitor.LastTreePath;
            }
            catch (Exception ex)
            {
                var msg = string.Format("Open file {0} failed: {1}", path, ex.Message);
                MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLog.DumpError(ex);
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Open a file
        /// </summary>
        private void miFileOpen_Click(object sender, EventArgs e)
        {
            using(var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Source Files|*.apk;*.jar;*.dex";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Open(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Node has been selected.
        /// </summary>
        private void TreeViewAfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            var node = treeView.SelectedNode as Node;
            var view = (node != null) ? node.CreateView(source) : null;

            var container = mainContainer.Panel2;
            container.SuspendLayout();
            container.Controls.Clear();
            if (view != null)
            {
                view.Dock = DockStyle.Fill;
                container.Controls.Add(view);
            }
            container.ResumeLayout(true);

            // Check if the node was selected programatically.
            if (e.Action != TreeViewAction.Unknown)
            {
                SettingsPersitor.LastTreePath = TreePath;
            }
        }

        /// <summary>
        /// A node is about to be expanded.
        /// </summary>
        private void TreeViewBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node as Node;
            if (node != null)
            {
                treeView.BeginUpdate();
                node.EnsureChildNodesCreated();
                foreach (var child in node.Nodes.OfType<Node>())
                {
                    child.EnsureChildNodesCreated();
                }
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Update recently used files list
        /// </summary>
        private void miFile_DropDownOpening(object sender, EventArgs e)
        {
            miFileRecent.DropDownItems.Clear();
            foreach (var iterator in SettingsPersitor.Files)
            {
                var path = iterator;
                var item = new ToolStripMenuItem(Path.GetFileName(path));
                item.Click += (s, x) => Open(path);
                item.ToolTipText = path;
                miFileRecent.DropDownItems.Add(item);
            }
            miFileRecent.Enabled = (miFileRecent.DropDownItems.Count > 0);
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if ((files.Length == 1) && (files[0].EndsWith(".apk", StringComparison.OrdinalIgnoreCase)))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if ((files.Length == 1) && (files[0].EndsWith(".apk", StringComparison.OrdinalIgnoreCase)))
                {
                    Open(files[0]);
                }
            }
        }

        /// <summary>
        /// Key identying location of currently selected node
        /// </summary>
        private string TreePath
        {
            get
            {
                return GetTreePath(treeView.SelectedNode);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                var parts = value.Split('|');
                var nodeCol = treeView.Nodes;
                TreeNode toSelect = null;
                try
                {
                    treeView.BeginUpdate();
                    foreach (var part in parts)
                    {
                        var node = nodeCol.Cast<TreeNode>().FirstOrDefault(x => x.Text == part);
                        if (node == null)
                            break;
                        toSelect = node;
                        nodeCol = node.Nodes;
                        node.Expand();
                    }
                }
                finally
                {
                    if (toSelect != null)
                    {
                        treeView.SelectedNode = toSelect;
                        Action ensureVisible = () => toSelect.EnsureVisible();
                        BeginInvoke(ensureVisible);
                    }
                    treeView.EndUpdate();
                }
            }
        }

        private static string GetTreePath(TreeNode node)
        {
            if (node == null)
                return string.Empty;
            var sb = new StringBuilder();
            while (node != null)
            {
                if (sb.Length > 0)
                    sb.Insert(0, '|');
                sb.Insert(0, node.Text);
                node = node.Parent;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Show abstract syntax tree
        /// </summary>
        public bool ShowAst
        {
            get
            {
#if DEBUG || ENABLE_SHOW_AST
                return miShowAst.Checked;
#else
                return false;
#endif
            }
        }

        public bool EnableBaksmali { get { return miEnableBaksmali.Checked; } }
        public string BaksmaliCommand { get { return SettingsPersitor.BaksmaliCommand; } }
        public string BaksmaliParameters { get { return SettingsPersitor.BaksmaliParameters; } }
        public bool ShowControlFlow { get { return miShowControlFlow.Checked; } }
        public bool EmbedSourceCodePositions { get { return miEmbedSourceCodePositions.Checked; } }
        public bool EmbedSourceCode { get { return miEmbedSourceCode.Checked; } }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool bHandled = false;

            if (keyData == Keys.F5)
            {
                if(originalPath != null)
                    Open(originalPath);

                bHandled = true;
            }
            else if (keyData == Keys.F3)
            {
                miFindNext_Click(this, new EventArgs());
                bHandled = true; 
            }
            else if ((keyData & Keys.KeyCode) == Keys.F3 &&  (keyData & Keys.Shift) != 0)
            {
                miFindPrevious_Click(this, new EventArgs());
                bHandled = true;
            }
            else if ((keyData & Keys.KeyCode) == Keys.F && (keyData & Keys.Control) != 0)
            {
                miFindClass_Click(this, new EventArgs());
                bHandled = true;
            }

            return bHandled;
        }

        private void miEnableBaksmali_Click(object sender, EventArgs e)
        {
            SettingsPersitor.EnableBaksmali = EnableBaksmali;

            if (EnableBaksmali && string.IsNullOrEmpty(BaksmaliCommand))
                miConfigureBaksmali_Click(sender,e);
            UpdateView();
        }

        private void miConfigureBaksmali_Click(object sender, EventArgs e)
        {
            using (var dlg = new ConfigureBaksmaliDialog())
            {
                dlg.BaksmaliCommand = SettingsPersitor.BaksmaliCommand;
                dlg.BaksmaliParameter = SettingsPersitor.BaksmaliParameters;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    SettingsPersitor.BaksmaliCommand = dlg.BaksmaliCommand;
                    SettingsPersitor.BaksmaliParameters = dlg.BaksmaliParameter;
                }
            }
        }

        private void miFileExportCode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(BaksmaliCommand))
                miConfigureBaksmali_Click(sender, e);
            if (string.IsNullOrEmpty(BaksmaliCommand))
                return;
            
            VistaFolderBrowserDialog dlg= new VistaFolderBrowserDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var cmd = BaksmaliDisassembler.GetBacksmaliCommand(this, originalPath, dlg.SelectedPath);
            List<string> output = new List<string>();
            int ret = Run.System(output, cmd);

            if (ret != 0 || output.Any())
            {
                StringBuilder msg = new StringBuilder();
                if (ret != 0)
                {
                    msg.Append("Return value was: ");
                    msg.Append(ret);
                    msg.AppendLine();
                    msg.AppendLine();
                }
                msg.Append(string.Join("\n", output));

                MessageBox.Show(this, msg.ToString(), "Results", MessageBoxButtons.OK);
            }
            else
            {
                Process.Start(dlg.SelectedPath);
            }
        }

        private void UpdateView()
        {
            var node = treeView.SelectedNode;
            treeView.SelectedNode = null;
            treeView.SelectedNode = node;
        }
   

        private void miEmbedSourceCodePositions_Click(object sender, EventArgs e)
        {
            SettingsPersitor.EmbedSourceCodePositions = EmbedSourceCodePositions;
            UpdateView();
        }

        private void miEmbedSourceCode_Click(object sender, EventArgs e)
        {
            SettingsPersitor.EmbedSourceCode = EmbedSourceCode;
            UpdateView();
        }

        private void miShowControlFlow_Click(object sender, EventArgs e)
        {
            SettingsPersitor.ShowControlFlow = ShowControlFlow;
            UpdateView();
        }

        private void miFindClass_Click(object sender, EventArgs e)
        {
            using (var dlg = new SearchDialog())
            {
                dlg.SearchText = lastSearchString;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    lastSearchString = dlg.SearchText;

                    if(!string.IsNullOrEmpty(lastSearchString))
                        miFindNext_Click(sender, e);
                }
            }
        }

        private void miFindNext_Click(object sender, EventArgs e)
        {
            if (treeView.Nodes.Count == 0) return;

            var searchString = lastSearchString;

            if (string.IsNullOrEmpty(searchString))
            {
                miFindClass_Click(sender, e);
                return;
            }

            var nextNode = treeView.FindNextNode(searchString);
            if(nextNode != null)
                TreePath = GetTreePath(nextNode);
        }

        private void miFindPrevious_Click(object sender, EventArgs e)
        {
            if (treeView.Nodes.Count == 0) return;
            // TODO...
        }
        
    }
}
 
