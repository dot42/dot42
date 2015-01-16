using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal sealed class CData : Node
        {
            /// <summary>
            /// Creation ctor
            /// </summary>
            internal CData(XmlTree tree, XText text)
                : base(tree, ChunkTypes.RES_XML_CDATA_TYPE)
            {
                Data = text.Value;
                TypedData = new Value(Value.Types.TYPE_NULL, 0);
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            internal CData(ResReader reader, XmlTree tree)
                : base(reader, tree, ChunkTypes.RES_XML_CDATA_TYPE)
            {
                Data = StringPoolRef.Read(reader, tree.StringPool);
                TypedData = new Value(reader);
            }

            public string Data { get; set; }
            public Value TypedData { get; set; }

            /// <summary>
            /// Build an XML document part for this node.
            /// </summary>
            internal override void BuildTree(Stack<XContainer> documentStack)
            {
                var text = new XText(Data);
                documentStack.Peek().Add(text);
            }

            /// <summary>
            /// Assign resource ID's to attributes.
            /// </summary>
            internal override void AssignResourceIds(ResourceIdMap resourceIdMap)
            {
                // Do nothing
            }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            protected internal override void PrepareForWrite()
            {
                base.PrepareForWrite();
                StringPoolRef.Prepare(Tree.StringPool, Data);
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                base.WriteData(writer);
                StringPoolRef.Write(writer, Tree.StringPool, Data);
                TypedData.Write(writer);
            }
        }
    }
}
