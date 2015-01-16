namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal abstract class NamespaceNode : Node
        {
            /// <summary>
            /// Reading ctor
            /// </summary>
            protected NamespaceNode(XmlTree tree, ChunkTypes expectedType)
                : base(tree, expectedType)
            {
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            protected NamespaceNode(ResReader reader, XmlTree tree, ChunkTypes expectedType)
                : base(reader, tree, expectedType)
            {
                Prefix = StringPoolRef.Read(reader, tree.StringPool);
                Uri = StringPoolRef.Read(reader, tree.StringPool);
            }

            /// <summary>
            /// Namespace prefix
            /// </summary>
            public string Prefix { get; set; }

            /// <summary>
            /// Namespace URI
            /// </summary>
            public string Uri { get; set; }

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
                StringPoolRef.Prepare(Tree.StringPool, Prefix);
                StringPoolRef.Prepare(Tree.StringPool, Uri);
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                base.WriteData(writer);
                StringPoolRef.Write(writer, Tree.StringPool, Prefix);
                StringPoolRef.Write(writer, Tree.StringPool, Uri);
            }
        }
    }
}
