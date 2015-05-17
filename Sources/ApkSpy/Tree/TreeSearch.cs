using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dot42.ApkSpy.Tree
{
    public static class TreeSearch
    {
        public static TreeNode FindNextNode(this TreeView treeView, string searchString)
        {
            var node = treeView.SelectedNode ?? treeView.Nodes[0];

            var nextNode = FindNextNode(node, searchString);

            if (nextNode != null)
            {
                return nextNode;
            }

            if (treeView.SelectedNode != treeView.Nodes[0])
            {
                // try again from the start.
                return FindNextNode(treeView.Nodes[0], searchString);
            }

            return null;
        }


        private static TreeNode FindChildNode(TreeNode node, string searchString)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                if (IsMatch(searchString, childNode))
                    return childNode;
                var nestedChildNode = FindChildNode(childNode, searchString);
                if (nestedChildNode != null)
                    return nestedChildNode;
            }
            return null;
        }

        private static TreeNode FindNextNode(TreeNode node, string searchString)
        {
            // first, try to match child nodes.
            var childNode = FindChildNode(node, searchString);
            if (childNode != null)
                return childNode;

            // now, search siblings.
            while (node.NextNode != null)
            {
                node = node.NextNode;

                if (IsMatch(searchString, node))
                    return node;
                childNode = FindChildNode(node, searchString);
                if (childNode != null)
                    return childNode;
            }

            // finally, search parent siblings.
            while ((node = node.Parent) != null)
            {
                if (node.NextNode != null)
                    break;
            }

            if (node == null)
                return null;

            node = node.NextNode;

            if (IsMatch(searchString, node))
                return node;

            return FindNextNode(node, searchString);
        }

        private static bool IsMatch(string searchString, TreeNode childNode)
        {
            return childNode.Text.Contains(searchString);
        }
    }
}
