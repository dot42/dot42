using System.Collections.Generic;
using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/integer-array
    /// </summary>
    [ElementName("integer-array")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class IntegerArrayNode : NameNode, ISerializationNodeContainer
    {
        private readonly IntegerArrayItemNodeCollection items = new IntegerArrayItemNodeCollection();

        public IntegerArrayNode()
        {
            items.CollectionChanged += (s, x) => OnPropertyChanged("Items", false);
            items.PropertyChanged += (s, x) => OnPropertyChanged("Items." + x.PropertyName, x);
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets all items
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public IntegerArrayItemNodeCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            items.Add((IntegerArrayItemNode)child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            items.Remove((IntegerArrayItemNode)child);
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> ISerializationNodeContainer.Children
        {
            get { return items.Children; }
        }
    }
}
