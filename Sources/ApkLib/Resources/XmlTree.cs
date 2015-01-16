using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    public partial class XmlTree : Chunk
    {
        private readonly StringPool strings;
        private readonly ResourceMap resourceMap;
        private readonly List<Node>  nodes = new List<Node>();

        /// <summary>
        /// Stream ctor
        /// </summary>
        public XmlTree(Stream stream)
            : this(new ResReader(stream))
        {            
        }

        /// <summary>
        /// Reading ctor
        /// </summary>
        public XmlTree(ResReader reader)
            : base(reader, ChunkTypes.RES_XML_TYPE)
        {
            strings = new StringPool(reader);
            resourceMap = new ResourceMap(reader);

            while (true)
            {
                var tag = reader.PeekChunkType();
                Node node;
                switch (tag)
                {
                    case ChunkTypes.RES_XML_START_NAMESPACE_TYPE:
                        node = new StartNamespace(reader, this);
                        break;
                    case ChunkTypes.RES_XML_START_ELEMENT_TYPE:
                        node = new StartElement(reader, this);
                        break;
                    case ChunkTypes.RES_XML_CDATA_TYPE:
                        node = new CData(reader, this);
                        break;
                    case ChunkTypes.RES_XML_END_ELEMENT_TYPE:
                        node = new EndElement(reader, this);
                        break;
                    case ChunkTypes.RES_XML_END_NAMESPACE_TYPE:
                        node = new EndNamespace(reader, this);
                        break;
                    default:
                        throw new IOException(string.Format("Unexpected tag: 0x{0:X}", (int)tag));
                }

                nodes.Add(node);
                if (tag == ChunkTypes.RES_XML_END_NAMESPACE_TYPE)
                    break;
            }
        }

        /// <summary>
        /// Create a tree for the given document
        /// </summary>
        public XmlTree(XDocument doc)
            : base(ChunkTypes.RES_XML_TYPE)
        {
            strings = new StringPool();
            resourceMap = new ResourceMap();

            CreateNodes(doc.Root);
        }

        /// <summary>
        /// Add nodes for the given element and its children
        /// </summary>
        private void CreateNodes(XElement element)
        {
            // Start namespace
            var namespaceNodes = element.Attributes().Where(x => x.IsNamespaceDeclaration).Select(x => new StartNamespace(this, x)).ToList();
            namespaceNodes.ForEach(x => nodes.Add(x));

            // Start element
            var start = new StartElement(this, element);
            nodes.Add(start);

            // Element children
            foreach (var child in element.Nodes())
            {
                var childElement = child as XElement;
                if (childElement != null)
                    CreateNodes(childElement);
                var childText = child as XText;
                if (childText != null)
                    nodes.Add(new CData(this, childText));
            }

            // End element
            nodes.Add(new EndElement(this, start));

            // End namespaces
            var endNamespaceNodes = namespaceNodes.Select(x => new EndNamespace(this, x)).Reverse().ToList();
            endNamespaceNodes.ForEach(x => nodes.Add(x));
        }

        /// <summary>
        /// Gets my string pool
        /// </summary>
        protected internal StringPool StringPool
        {
            get { return strings; }
        }

        /// <summary>
        /// Convert to XML
        /// </summary>
        public XDocument AsXml()
        {
            var doc = new XDocument();
            var stack = new Stack<XContainer>();
            stack.Push(doc);
            foreach (var node in nodes)
            {
                node.BuildTree(stack);
            }
            return doc;
        }

        /// <summary>
        /// Call the given action for each attribute of each start element.
        /// </summary>
        internal void VisitAttributes(Action<Attribute> action)
        {
            foreach (var x in nodes.OfType<StartElement>())
            {
                x.Attributes.ForEach(action);
            }
        }

        /// <summary>
        /// Assign resource ID's to attributes.
        /// </summary>
        public void AssignResourceIds(ResourceIdMap resourceIdMap)
        {
            nodes.ForEach(x => x.AssignResourceIds(resourceIdMap));
        }

        /// <summary>
        /// Prepare this chunk for writing
        /// </summary>
        protected internal override void PrepareForWrite()
        {
            base.PrepareForWrite();
            nodes.ForEach(x => x.PrepareForWrite());

            // Prepare strings and resource map
            strings.Sort();
            resourceMap.Clear();
            var resourceIdEntries = strings.Entries.Where(x => x.HasResourceId).ToList();
            var index = 0;
            foreach (var entry in resourceIdEntries)
            {
                resourceMap.Set(index++, entry.ResourceId);
            }
        }

        /// <summary>
        /// Write the data of this chunk.
        /// </summary>
        protected override void WriteData(ResWriter writer)
        {
            base.WriteData(writer);
            strings.Write(writer);
            resourceMap.Write(writer);
            nodes.ForEach(x => x.Write(writer));
        }
    }
}
