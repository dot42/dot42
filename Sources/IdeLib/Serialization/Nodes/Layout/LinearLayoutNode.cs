using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Layout
{
    /// <summary>
    /// Android.Widget.LinearLayout
    /// </summary>
    [ElementName("LinearLayout")]
    [Obfuscation(Feature = "@SerializableNode")]
    [Description("Android.Widget.LinearLayout")]
    [Obfuscation(Feature = "@VSNode")]
    public class LinearLayoutNode : ViewGroupNode
    {
        private static readonly PropertyInfo orientationProperty;

        /// <summary>
        /// Class ctor
        /// </summary>
        static LinearLayoutNode()
        {
            var x = new LinearLayoutNode();
            orientationProperty = ReflectionHelper.PropertyOf(() => x.Orientation);
        }

        /// <summary>
        /// android:orientation
        /// </summary>
        [AttributeName("orientation", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string Orientation
        {
            get { return Get(orientationProperty); }
            set { Set(orientationProperty, value); }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}
