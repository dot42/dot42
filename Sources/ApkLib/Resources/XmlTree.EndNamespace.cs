using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal sealed class EndNamespace : NamespaceNode
        {
            /// <summary>
            /// Creation ctor
            /// </summary>
            internal EndNamespace(XmlTree tree, StartNamespace start)
                : base(tree, ChunkTypes.RES_XML_END_NAMESPACE_TYPE)
            {
                Prefix = start.Prefix;
                Uri = start.Uri;
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            internal EndNamespace(ResReader reader, XmlTree tree)
                : base(reader, tree, ChunkTypes.RES_XML_END_NAMESPACE_TYPE)
            {
            }

            /// <summary>
            /// Build an XML document part for this node.
            /// </summary>
            internal override void BuildTree(Stack<XContainer> documentStack)
            {
                // Do nothing
            }
        }
    }
}
