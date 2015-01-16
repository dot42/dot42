using System;
using System.Linq;
using System.Windows.Forms;

namespace Dot42.ApkSpy.Tree
{
    internal static class TreeExtensions
    {
        /// <summary>
        /// Gets the correct parent node for the given file.
        /// </summary>
        internal static TreeNodeCollection GetParentForFile(this TreeNodeCollection root, string dir, int imageIndex, char[] seperateChars)
        {
            if (String.IsNullOrEmpty(dir))
                return root;

            var index = dir.LastIndexOfAny(seperateChars);
            var name = (index < 0) ? dir : dir.Substring(index + 1);
            var parentName = (index < 0) ? null : dir.Substring(0, index);

            var parent = GetParentForFile(root, parentName, imageIndex, seperateChars);
            var node = parent.OfType<DirNode>().FirstOrDefault(x => x.Directory == name);
            if (node != null)
                return node.Nodes;

            // Create new
            node = new DirNode(name) { ImageIndex = imageIndex };
            parent.Add(node);
            return node.Nodes;
        }
    }
}
