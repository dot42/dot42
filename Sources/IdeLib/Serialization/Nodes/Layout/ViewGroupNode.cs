using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Layout
{
    /// <summary>
    /// base class for viewgroup's
    /// </summary>
    [Obfuscation(Feature = "@SerializableNode")]
    public class ViewGroupNode : ViewNode, ISerializationNodeContainer
    {
        private static readonly PropertyInfo addStatesFromChildrenProperty;
        private readonly ViewNodeCollection children;

        /// <summary>
        /// Class ctor
        /// </summary>
        static ViewGroupNode()
        {
            var x = new ViewGroupNode();
            addStatesFromChildrenProperty = ReflectionHelper.PropertyOf(() => x.AddStatesFromChildren);
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ViewGroupNode()
        {            
            children = new ViewNodeCollection(this);
            children.CollectionChanged += (s, x) => OnPropertyChanged(children.Header, false);
            children.PropertyChanged += (s, x) => OnPropertyChanged(children.Header + "." + x.PropertyName, x);
        }

        /// <summary>
        /// Gets all child views
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public ViewNodeCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// android:addStatesFromChildren
        /// </summary>
        [AttributeName("addStatesFromChildren", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string AddStatesFromChildren
        {
            get { return Get(addStatesFromChildrenProperty); }
            set { Set(addStatesFromChildrenProperty, value); }
        }

        /// <summary>
        /// Add the given child.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            children.Add((ViewNode)child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            children.Remove((ViewNode)child);
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> ISerializationNodeContainer.Children
        {
            get { return Children; }
        }
    }
}
