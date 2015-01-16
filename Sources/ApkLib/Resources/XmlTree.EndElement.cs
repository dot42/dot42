using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal sealed class EndElement : Element
        {
            /// <summary>
            /// Creation ctor
            /// </summary>
            internal EndElement(XmlTree tree, StartElement start)
                : base(tree, ChunkTypes.RES_XML_END_ELEMENT_TYPE)
            {
                Name = start.Name;
                Namespace = start.Namespace;
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            internal EndElement(ResReader reader, XmlTree tree)
                : base(reader, tree, ChunkTypes.RES_XML_END_ELEMENT_TYPE)
            {
            }

            /// <summary>
            /// Build an XML document part for this node.
            /// </summary>
            internal override void BuildTree(Stack<XContainer> documentStack)
            {
                documentStack.Pop();
            }

            /// <summary>
            /// Assign resource ID's to attributes.
            /// </summary>
            internal override void AssignResourceIds(ResourceIdMap resourceIdMap)
            {
                // Do nothing
            }
        }
    }
}
