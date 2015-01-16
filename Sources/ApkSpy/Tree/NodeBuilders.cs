using System.Collections.Generic;
using System.Linq;

namespace Dot42.ApkSpy.Tree
{
    internal static class NodeBuilders
    {
        /// <summary>
        /// Class ctor
        /// </summary>
        static NodeBuilders()
        {
            Builders = Program.CompositionContainer.GetExportedValues<INodeBuilder>();
        }

        /// <summary>
        /// Gets all node builders
        /// </summary>
        internal static IEnumerable<INodeBuilder> Builders { get; private set; }

        /// <summary>
        /// Create a node for the given file.
        /// </summary>
        internal static Node Create(SourceFile source, string fileName)
        {
            var builder = Builders.FirstOrDefault(x => x.Supports(source, fileName));
            if (builder != null)
                return builder.Create(source, fileName);
            return new UnknownFileNode(source, fileName);
        }
    }
}
