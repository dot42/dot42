using System.Collections.Generic;
using System.Reflection;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/style
    /// </summary>
    [ElementName("style")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class StyleNode : NameNode, ISerializationNodeContainer
    {
        private readonly StyleItemNodeCollection items = new StyleItemNodeCollection();
        private readonly PropertyInfo parentProperty;

        public StyleNode()
        {
            parentProperty = ReflectionHelper.PropertyOf(() => Parent);
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

        [AttributeName("parent")]
        [Obfuscation(Feature = "@Xaml")]
        public string Parent
        {
            get { return Get(parentProperty); }
            set
            {
                Set(parentProperty, value);
            }
        }

        /// <summary>
        /// Gets all items
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public StyleItemNodeCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            items.Add((StyleItemNode) child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            items.Remove((StyleItemNode)child);
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
