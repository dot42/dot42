using System;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Layout
{
    /// <summary>
    /// base class for all view nodes
    /// </summary>
    [Obfuscation(Feature = "@SerializableNode")]
    public class ViewNode : SerializationNode
    {
        private static readonly PropertyInfo idProperty;

        /// <summary>
        /// Class ctor
        /// </summary>
        static ViewNode()
        {
            var x = new ViewNode();
            idProperty = ReflectionHelper.PropertyOf(() => x.Id);
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ViewNode()
        {
        }

        /// <summary>
        /// android:id
        /// </summary>
        [AttributeName("id", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string Id
        {
            get { return Get(idProperty); }
            set { Set(idProperty, value); }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            throw new NotImplementedException();
        }
    }
}
