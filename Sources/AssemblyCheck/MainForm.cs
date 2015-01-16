using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.Graphics;
using Dot42.Shared.UI;
using Mono.Cecil;

namespace Dot42.AssemblyCheck
{
    public partial class MainForm : AppForm
    {
        private readonly Dictionary<string, MemberNode> nodes = new Dictionary<string,MemberNode>();
        private FrameworkInfo framework;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            miSelectAssembly.Image = Icons24.Check;
            miFrameworkFolder.Image = Icons24.Folder;
            miCopy.Image = Icons24.Copy;
            var folder = UserPreferences.Preferences.AssemblyCheckFrameworkFolder;
            framework = (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) ? Frameworks.Instance.GetNewestVersion() : new FrameworkInfo(folder);
            UpdateMenu();
            miCopy.Enabled = false;
        }

        /// <summary>
        /// Verify the assembly for use in Dot42.
        /// </summary>
        private void CheckAssembly(string path)
        {
            if (framework == null)
            {
                MessageBox.Show(this, "No framework folder found", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            miSelectAssembly.Enabled = false;
            miCopy.Enabled = false;
            progress.Visible = true;
            progress.Visible = true;
            tvUsedIn.Nodes.Clear();
            tvList.Nodes.Clear();
            nodes.Clear();
            tvLog.Nodes.Clear();
            
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task = Task.Factory.StartNew(() => new Checker(path, framework, Log, SetProgress).Check());
            task.ContinueWith(x => {
                progress.Visible = false;
                progress.Visible = false;
                miSelectAssembly.Enabled = true;
            }, ui);
        }

        /// <summary>
        /// Set current progress.
        /// </summary>
        private void SetProgress(int current, int max)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int>(SetProgress), current, max);
            }
            else
            {
                progress.Maximum = max;
                progress.Value = current;
            }
        }

        /// <summary>
        /// Add the given message to the log.
        /// </summary>
        private void Log(MessageTypes type, string member, IMetadataScope scope, string msg)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                Invoke(new Action<MessageTypes, string, IMetadataScope, string>(Log), type, member, scope, msg);
            }
            else
            {
                if ((type >= MessageTypes.MissingFirst) && (type <= MessageTypes.MissingLast))
                {
                    MemberNode node;
                    if (!nodes.TryGetValue(member, out node))
                    {
                        node = new MemberNode(member, scope, type);
                        node.ImageIndex = (int) type;
                        nodes.Add(member, node);
                        tvList.Nodes.Add(node);
                    }
                    node.Messages.Add(msg);
                    if (node == tvList.SelectedNode)
                    {
                        tvUsedIn.Nodes.Add(new TreeNode(msg));
                    }
                }
                else
                {
                    tvLog.Nodes.Add(new TreeNode(msg) { ImageIndex = (int) type });
                }
                miCopy.Enabled = (tvList.Nodes.Count > 0);
            }
        }

        /// <summary>
        /// Open a file
        /// </summary>
        private void OnOpenClick(object sender, EventArgs e)
        {
            using(var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".dll";
                dialog.Filter = "Assemblies|*.dll";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    CheckAssembly(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Member node was selected.
        /// </summary>
        private void OnListAfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            var memberNode = tvList.SelectedNode as MemberNode;
            tvUsedIn.BeginUpdate();
            tvUsedIn.Nodes.Clear();
            if (memberNode != null)
            {
                tvUsedIn.Nodes.AddRange(memberNode.Messages.Select(x => new TreeNode(x)).ToArray());
            }
            tvUsedIn.EndUpdate();
        }

        private void tvList_SizeChanged(object sender, EventArgs e)
        {
            tvList.ResumeLayout();
        }

        private void tvUsedIn_SizeChanged(object sender, EventArgs e)
        {
            tvUsedIn.ResumeLayout();
        }

        private void tvLog_SizeChanged(object sender, EventArgs e)
        {
            tvLog.ResumeLayout();
        }

        /// <summary>
        /// Copy node to clipboard
        /// </summary>
        private void OnNodeDoubleClick(object sender, EventArgs e)
        {
            if ((sender as TreeView).SelectedNode != null)
                Clipboard.SetText((sender as TreeView).SelectedNode.Text);
        }

        /// <summary>
        /// Change the current framework folder.
        /// </summary>
        private void miChangeFrameworkFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Frameworks.Root;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                framework = new FrameworkInfo(dialog.SelectedPath);
                UpdateMenu();
                UserPreferences.Preferences.AssemblyCheckFrameworkFolder = dialog.SelectedPath;
                UserPreferences.SaveNow();
            }
        }

        /// <summary>
        /// Copy to clipboard.
        /// </summary>
        private void miCopy_Click(object sender, EventArgs e)
        {
            var doc = new XDocument();
            var root = new XElement("assembly-check");
            root.Add(new XAttribute("version", GetType().Assembly.GetName().Version));
            if (framework != null)
            {
                root.Add(new XAttribute("framework", framework.Name));
            }
            doc.Add(root);

            foreach (var node in tvList.Nodes.OfType<MemberNode>())
            {
                root.Add(node.ToXml());
            }

            Clipboard.SetText(doc.ToString());
        }

        /// <summary>
        /// Update the menu text's
        /// </summary>
        private void UpdateMenu()
        {
            miCurrentFrameworkFolder.Text = (framework != null) ? framework.Folder : "<none>";
        }

        private class MemberNode : TreeNode
        {
            private readonly string member;
            private readonly IMetadataScope scope;
            private readonly MessageTypes type;
            public readonly List<string> Messages = new List<string>();

            public MemberNode(string member, IMetadataScope scope, MessageTypes type)
            {
                this.member = member;
                this.scope = scope;
                this.type = type;
                Text = member;
                // Cells.Add(new Cell((scope != null) ? scope.ToString() : ""));
            }

            public string Member
            {
                get { return member; }
            }

            public XElement ToXml()
            {
                var element = new XElement(GetTypeName());
                if (scope != null)
                {
                    element.Add(new XAttribute("scope", scope.ToString()));
                }
                element.Add(new XText(member));
                return element;
            }

            private string GetTypeName()
            {
                switch (type)
                {
                    case MessageTypes.General:
                        return "general";
                    case MessageTypes.MissingField:
                        return "field";
                    case MessageTypes.MissingMethod:
                        return "method";
                    case MessageTypes.MissingType:
                        return "type";
                    case MessageTypes.UnsupportedFeature:
                        return "unsupported-feature";
                    default:
                        return "unknown" + (int)type;
                }
            }
        }
    }
}
