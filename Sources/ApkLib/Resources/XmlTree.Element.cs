namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        internal abstract class Element : Node
        {
            /// <summary>
            /// Creation ctor
            /// </summary>
            protected Element(XmlTree tree, ChunkTypes expectedType)
                : base(tree, expectedType)
            {
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            protected Element(ResReader reader, XmlTree tree, ChunkTypes expectedType)
                : base(reader, tree, expectedType)
            {
                Namespace = StringPoolRef.Read(reader, tree.StringPool);
                Name = StringPoolRef.Read(reader, tree.StringPool);
            }

            public string Namespace { get; set; }
            public string Name { get; set; }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            protected internal override void PrepareForWrite()
            {
                base.PrepareForWrite();
                StringPoolRef.Prepare(Tree.StringPool, Namespace);
                StringPoolRef.Prepare(Tree.StringPool, Name);
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                base.WriteData(writer);
                StringPoolRef.Write(writer, Tree.StringPool, Namespace);
                StringPoolRef.Write(writer, Tree.StringPool, Name);
            }
        }
    }
}
