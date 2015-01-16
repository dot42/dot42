using Dot42.ApkLib;

namespace Dot42.ApkSpy.Tree
{
    internal class UnknownFileNode : FileNode
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public UnknownFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }
    }
}
