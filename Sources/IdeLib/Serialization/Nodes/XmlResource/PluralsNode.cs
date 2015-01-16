using System.Collections.Generic;
using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/plurals
    /// </summary>
    [ElementName("plurals")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class PluralsNode : NameNode, ISerializationNodeContainer
    {
        private readonly PluralsItemNodeCollection items = new PluralsItemNodeCollection();

        public PluralsNode()
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
        public PluralsItemNodeCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            items.Add((PluralsItemNode)child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            items.Remove((PluralsItemNode)child);
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
