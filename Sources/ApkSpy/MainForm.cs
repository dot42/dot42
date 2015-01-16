using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dot42.ApkSpy.Tree;
using TallComponents.Common.Util;
using Node = Dot42.ApkSpy.Tree.Node;

namespace Dot42.ApkSpy
{
    public partial class MainForm : Form, ISpySettings
    {
        private SourceFile source;
        private bool initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
#if !DEBUG
            miDebug.Visible = false;
#else
            miShowAst.Checked = true;
#endif
        }

        /// <summary>
        /// Load last used file
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Size = RecentlyUsedFiles.WindowSize;
            var location = RecentlyUsedFiles.WindowLocation;
            if (Screen.AllScreens.Any(x => x.Bounds.Contains(location)))
                Location = RecentlyUsedFiles.WindowLocation;
            string path = RecentlyUsedFiles.Files.FirstOrDefault();

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
                RecentlyUsedFiles.WindowSize = Size;
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
                RecentlyUsedFiles.WindowLocation = Location;
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
                RecentlyUsedFiles.Add(path);

                // Try to re-open last known tree path
                TreePath = RecentlyUsedFiles.LastTreePath;
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
                dialog.Filter = "Source Files|*.apk;*.jar";

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

            // Cehck if the node was selected programatically.
            if (e.Action != TreeViewAction.Unknown)
            {
                RecentlyUsedFiles.LastTreePath = TreePath;
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
            foreach (var iterator in RecentlyUsedFiles.Files)
            {
                var path = iterator;
                var item = new ToolStripMenuItem(Path.GetFileName(path));
                item.Click += (s, x) => Open(path);
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
                var node = treeView.SelectedNode;
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

        /// <summary>
        /// Show abstract syntax tree
        /// </summary>
        public bool ShowAst
        {
            get
            {
#if DEBUG
                return miShowAst.Checked;
#else
                return false;
#endif
            }
        }
    }
}
 