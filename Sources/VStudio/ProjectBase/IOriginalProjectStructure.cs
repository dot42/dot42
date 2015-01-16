namespace Dot42.VStudio.ProjectBase
{
    internal interface IOriginalProjectStructure
    {
        /// <summary>
        /// Gets the first child of the given item (in the original project structure).
        /// </summary>
        uint GetFirstNodeChild(uint itemId);

        /// <summary>
        /// Gets the sibling of the given item (in the original project structure).
        /// </summary>
        uint GetNodeSibling(uint itemId);
    }
}
