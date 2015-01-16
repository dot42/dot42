using System;
using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Layout
{
    /// <summary>
    /// Android.Widget.TextView
    /// </summary>
    [ElementName("TextView")]
    [Obfuscation(Feature = "@SerializableNode")]
    [Description("Android.Widget.TextView")]
    [Obfuscation(Feature = "@VSNode")]
    public class TextViewNode : ViewNode
    {
        private static readonly PropertyInfo textProperty;

        /// <summary>
        /// Class ctor
        /// </summary>
        static TextViewNode()
        {
            var x = new TextViewNode();
            textProperty = ReflectionHelper.PropertyOf(() => x.Text);
        }

        /// <summary>
        /// android:text
        /// </summary>
        [AttributeName("text", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string Text
        {
            get { return Get(textProperty); }
            set { Set(textProperty, value); }
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
