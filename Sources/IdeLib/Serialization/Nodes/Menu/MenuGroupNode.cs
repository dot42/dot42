using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Ide.TypeConverters;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// menu/group
    /// </summary>
    [ElementName("group", null, "menu")]
    [Obfuscation(Feature = "@SerializableNode")]
    [Obfuscation(Feature = "@VSNode")]
    public sealed class MenuGroupNode : MenuChildNode, ISerializationNodeContainer
    {
        private readonly MenuItemNodeCollection items;
        private readonly PropertyInfo checkableBehaviorProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MenuGroupNode()
        {
            items = new MenuItemNodeCollection(this);
            items.CollectionChanged += (s, x) => OnPropertyChanged(items.Header, false);
            items.PropertyChanged += (s, x) => OnPropertyChanged(items.Header + "." + x.PropertyName, x);
            checkableBehaviorProperty = ReflectionHelper.PropertyOf(() => CheckableBehavior);
        }

        /// <summary>
        /// Gets the containing for adding child items to.
        /// </summary>
        [Browsable(false)]
        public override IMenuChildNodeContainer ChildContainer { get { return items; } }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets all sub items
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public MenuItemNodeCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// android:checkableBehavior
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("checkableBehavior", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        [TypeConverter(typeof(CheckableBehaviorTypeConverter))]
        public string CheckableBehavior
        {
            get { return Get(checkableBehaviorProperty); }
            set { Set(checkableBehaviorProperty, value); }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            items.Add((MenuItemNode) child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            items.Remove((MenuItemNode) child);
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> ISerializationNodeContainer.Children
        {
            get { return items; }
        }
    }
}
