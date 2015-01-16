using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        [DebuggerDisplay("{@Name}")]
        internal sealed class StartElement : Element
        {
            private readonly List<Attribute> attributes = new List<Attribute>();

            /// <summary>
            /// Reading ctor
            /// </summary>
            internal StartElement(XmlTree tree, XElement element)
                : base(tree, ChunkTypes.RES_XML_START_ELEMENT_TYPE)
            {
                Name = element.Name.LocalName;
                Namespace = string.IsNullOrEmpty(element.Name.NamespaceName) ? null : element.Name.NamespaceName;
                foreach (var attr in element.Attributes().Where(x => !x.IsNamespaceDeclaration))
                {
                    attributes.Add(new Attribute(tree, attr));
                }

                IXmlLineInfo lineInfo = element;
                if (lineInfo.HasLineInfo())
                {
                    LineNumber = lineInfo.LineNumber;
                }
            }

            /// <summary>
            /// Reading ctor
            /// </summary>
            internal StartElement(ResReader reader, XmlTree tree)
                : base(reader, tree, ChunkTypes.RES_XML_START_ELEMENT_TYPE)
            {
                var attributeStart = reader.ReadUInt16();
                var attributeSize = reader.ReadUInt16();
                var attributeCount = reader.ReadUInt16();
                var id = reader.ReadUInt16();
                var classIndex = reader.ReadUInt16();
                var styleIndex = reader.ReadUInt16();

                for (var i = 0; i < attributeCount; i++)
                {
                    attributes.Add(new Attribute(reader, tree));
                }
            }

            public List<Attribute> Attributes { get { return attributes; } }

            /// <summary>
            /// Build an XML document part for this node.
            /// </summary>
            internal override void BuildTree(Stack<XContainer> documentStack)
            {
                var element = new XElement(XName.Get(Name, Namespace ?? string.Empty));
                documentStack.Peek().Add(element);
                documentStack.Push(element);

                attributes.ForEach(x => x.BuildTree(documentStack));
            }

            /// <summary>
            /// Assign resource ID's to attributes.
            /// </summary>
            internal override void AssignResourceIds(ResourceIdMap resourceIdMap)
            {
                attributes.ForEach(x => x.AssignResourceIds(resourceIdMap));
            }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            protected internal override void PrepareForWrite()
            {
                base.PrepareForWrite();
                attributes.ForEach(x => x.PrepareForWrite());
            }

            /// <summary>
            /// Write the data of this chunk.
            /// </summary>
            protected override void WriteData(ResWriter writer)
            {
                base.WriteData(writer);
                writer.WriteUInt16(20); // attributeStart
                writer.WriteUInt16(20); // attributeSize
                writer.WriteUInt16(attributes.Count); // attributeCount
                writer.WriteUInt16(IndexOf("id"));
                writer.WriteUInt16(IndexOf("class"));
                writer.WriteUInt16(IndexOf("style"));
                foreach (var attr in attributes)
                {
                    attr.Write(writer);
                }
            }

            /// <summary>
            /// Gets a 1 based index of an attribute with given id.
            /// </summary>
            /// <returns>0 if not found</returns>
            private int IndexOf(string attributeName)
            {
                var attr = attributes.FirstOrDefault(x => x.Name == attributeName);
                if (attr == null)
                    return 0;
                return attributes.IndexOf(attr) + 1;
            }
        }
    }
}
