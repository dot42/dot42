namespace Dot42.Ide.Serialization.Nodes.Menu
{
    internal static class Extensions
    {
        /// <summary>
        /// Can the given node be added to this container?
        /// </summary>
        public static bool CanAdd(this IMenuChildNodeContainer container, MenuChildNode node)
        {
            if (node == null)
                return false;
            if (node is MenuGroupNode)
                return container.CanAddGroupNodes;
            return true;
        }
    }
}
