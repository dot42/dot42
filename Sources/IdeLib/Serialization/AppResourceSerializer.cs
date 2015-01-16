using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Dot42.Ide.Serialization.Nodes.Layout;
using Dot42.Ide.Serialization.Nodes.Menu;
using Dot42.Ide.Serialization.Nodes.XmlResource;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Serialize/deserialize XML resources
    /// </summary>
    public class AppResourceSerializer
    {
        private static readonly NodeSerializer[] Serializers = new NodeSerializer[] {
#if DEBUG // Remove when going live
            // layout
            new StandardNodeSerializer<LinearLayoutNode>(),
            new StandardNodeSerializer<TextViewNode>(),
#endif

            // menu
            new StandardNodeSerializer<MenuNode>(),
            new StandardNodeSerializer<MenuGroupNode>(),
            new StandardNodeSerializer<MenuItemNode>(),

            // values nodes
            new StandardNodeSerializer<BoolNode>(),
            new StandardNodeSerializer<ColorNode>(),
            new StandardNodeSerializer<DimensionNode>(),
            new StandardNodeSerializer<IdNode>(),
            new StandardNodeSerializer<IntegerNode>(),
            new StandardNodeSerializer<IntegerArrayNode>(),
            new StandardNodeSerializer<IntegerArrayItemNode>(),
            new StandardNodeSerializer<PluralsNode>(),
            new StandardNodeSerializer<PluralsItemNode>(),
            new StandardNodeSerializer<ResourcesNode>(),
            new StandardNodeSerializer<StringNode>(),
            new StandardNodeSerializer<StringArrayNode>(),
            new StandardNodeSerializer<StringArrayItemNode>(),
            new StandardNodeSerializer<StyleNode>(),
            new StandardNodeSerializer<StyleItemNode>(),
            new StandardNodeSerializer<TypedArrayNode>(),
            new StandardNodeSerializer<TypedArrayItemNode>(),
        };

        /// <summary>
        /// Gets or sets the document moniker.
        /// </summary>
        public string DocumentMoniker { get; set; }

        /// <summary>
        /// Can the given element be de-serialized?
        /// </summary>
        public bool CanDeserialize(XDocument document)
        {
            try
            {
                return CanDeserialize(document.Root);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Can the given element be de-serialized?
        /// </summary>
        private static bool CanDeserialize(XElement element)
        {
            if (!Serializers.Any(x => x.CanDeserialize(element)))
                return false;
            return element.Elements().All(CanDeserialize);
        }

        /// <summary>
        /// De-serialize the given element into a node.
        /// </summary>
        public SerializationNode Deserialize(XDocument document)
        {
            return Deserialize(document.Root, null);
        }

        /// <summary>
        /// De-serialize the given element into a node.
        /// </summary>
        private static SerializationNode Deserialize(XElement element, ISerializationNodeContainer container)
        {
            var serializer = Serializers.FirstOrDefault(x => x.CanDeserialize(element));
            if (serializer == null)
                throw new SerializationException(string.Format("Cannot deserialize element {0}", element.Name));
            var node = serializer.Deserialize(element, container);
            if (container != null)
            {
                node = container.Add(node);
            }
            foreach (var child in element.Elements())
            {
                var nodeAsContainer = node as ISerializationNodeContainer;
                if (nodeAsContainer == null)
                {
                    throw new SerializationException(string.Format("Node {0} in not a valid container", node));
                }
                Deserialize(child, nodeAsContainer);
            }
            return node;
        }

        /// <summary>
        /// Serialize the given node into an element.
        /// </summary>
        public XDocument Serialize(SerializationNode node)
        {
            var root = Serialize(node, null, null);
            return new XDocument(root);
        }

        /// <summary>
        /// Serialize the given node into an element.
        /// </summary>
        private static XElement Serialize(SerializationNode node, ISerializationNodeContainer parentNode, XElement parent)
        {
            var element = Serializers.First(x => x.CanSerialize(node)).Serialize(node, parentNode);
            if (parent != null)
            {
                parent.Add(element);
            }
            var nodeAsContainer = node as ISerializationNodeContainer;
            if (nodeAsContainer != null)
            {
                foreach (var childNode in nodeAsContainer.Children)
                {
                    Serialize(childNode, nodeAsContainer, element);
                }
            }
            return element;
        }
    }
}
