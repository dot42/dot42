using System;
using System.Windows.Forms;

namespace Dot42.ApkSpy.Tree
{
    public abstract class Node : TreeNode
    {
        private bool childNodesCreated;

        /// <summary>
        /// Improved image index
        /// </summary>
        internal new int ImageIndex
        {
            get { return base.ImageIndex; }
            set
            {
                base.ImageIndex = value;
                //SelectedImageIndex = value;
            }
        }

        /// <summary>
        /// Create a view to display the Types of this node.
        /// </summary>
        internal virtual Control CreateView(ISpyContext settings)
        {
            return null;
        }

        /// <summary>
        /// Makes sure the child nodes are created
        /// </summary>
        internal void EnsureChildNodesCreated()
        {
            if (!childNodesCreated)
            {
                try
                {
                    childNodesCreated = true;
                    CreateChildNodes();
                } catch (Exception ex)
                {
                    Nodes.Add(new ExceptionNode(ex));
                }
            }
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected virtual void CreateChildNodes()
        {
            // Override me
        }
    }
}
