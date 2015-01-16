using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal sealed class StartNamespace : NamespaceNode
        {
            /// <summary>
            /// Reading ctor
            /// </summary>
            internal StartNamespace(XmlTree tree, XAttribute attribute)
                : base(tree, ChunkTypes.RES_XML_START_NAMESPACE_TYPE)
            {
                if (!attribute.IsNamespaceDeclaration)
                    throw new ArgumentException("Namespace declaration expected");
                Uri = attribute.Value;
                Prefix = attribute.Name.LocalName;
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            internal StartNamespace(ResReader reader, XmlTree tree)
                : base(reader, tree, ChunkTypes.RES_XML_START_NAMESPACE_TYPE)
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
