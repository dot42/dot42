using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// menu
    /// </summary>
    [ElementName("menu")]
    [AddNamespace("android", AndroidConstants.AndroidNamespace)]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class MenuNode : SerializationNode, ISerializationNodeContainer
    {
        private readonly MenuChildNodeCollection children;
        private readonly PropertyInfo flattenedChildrenProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MenuNode()
            : this(null)
        {            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public MenuNode(MenuChildNode parent)
        {
            children = new MenuChildNodeCollection(parent);
            flattenedChildrenProperty = ReflectionHelper.PropertyOf(() => FlattenedChildren);
            children.CollectionChanged += (s, x) => { OnPropertyChanged(children.Header, false); OnPropertyChanged(flattenedChildrenProperty.Name, false); };
            children.PropertyChanged += (s, x) => { OnPropertyChanged(children.Header + "." + x.PropertyName, x); OnPropertyChanged(flattenedChildrenProperty.Name, false); };
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets all items and groups
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public MenuChildNodeCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// Gets all items and items within groups
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public IEnumerable<MenuChildNode> FlattenedChildren
        {
            get
            {
                foreach (var child in children)
                {
                    if (child is MenuItemNode) yield return child;
                    if (child is MenuGroupNode)
                    {
                        foreach (var groupItem in ((MenuGroupNode) child).Items)
                        {
                            yield return groupItem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            children.Add((MenuChildNode) child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            children.Remove((MenuChildNode)child);
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> ISerializationNodeContainer.Children
        {
            get { return children; }
        }
    }
}
