using System.Collections.Generic;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal abstract class Node : Chunk
        {
            private readonly XmlTree tree;

            /// <summary>
            /// Creation ctor
            /// </summary>
            protected Node(XmlTree tree, ChunkTypes expectedType)
                : base(expectedType)
            {
                this.tree = tree;
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            protected Node(ResReader reader, XmlTree tree, ChunkTypes expectedType)
                : base(reader, expectedType)
            {
                this.tree = tree;
                LineNumber = reader.ReadInt32();
                Comment = StringPoolRef.Read(reader, tree.StringPool);
            }

            /// <summary>
            /// Gets the containing tree.
            /// </summary>
            protected XmlTree Tree { get { return tree; } }

            public int LineNumber { get; set; }
            public string Comment { get; set; }

            /// <summary>
            /// Build an XML document part for this node.
            /// </summary>
            internal abstract void BuildTree(Stack<XContainer> documentStack);

            /// <summary>
            /// Assign resource ID's to attributes.
            /// </summary>
            internal abstract void AssignResourceIds(ResourceIdMap resourceIdMap);

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            protected internal override void PrepareForWrite()
            {
                base.PrepareForWrite();
                StringPoolRef.Prepare(tree.StringPool, Comment);
            }

            /// <summary>
            /// Write the header of this chunk.
            /// Always call the base method first.
            /// </summary>
            protected sealed override void WriteHeader(ResWriter writer)
            {
                base.WriteHeader(writer);
                writer.WriteInt32(LineNumber);
                StringPoolRef.Write(writer, tree.StringPool, Comment);
                // Extended data (in derived types) is not part of the header
            }
        }
    }
}
