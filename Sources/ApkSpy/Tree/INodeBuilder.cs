namespace Dot42.ApkSpy.Tree
{
    internal interface INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        bool Supports(SourceFile source, string fileName);

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        Node Create(SourceFile source, string fileName);
    }
}
