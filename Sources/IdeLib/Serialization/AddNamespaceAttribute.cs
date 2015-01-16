using System;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Tells the serializer to an an XMLNS to the serialized XML element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AddNamespaceAttribute : Attribute
    {
        private readonly string @namespace;
        private readonly string prefix;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AddNamespaceAttribute(string prefix, string @namespace)
        {
            this.prefix = prefix;
            this.@namespace = @namespace;
        }

        /// <summary>
        /// Namespace to add
        /// </summary>
        public string Namespace
        {
            get { return @namespace; }
        }

        /// <summary>
        /// Prefix to use for the namespace 
        /// </summary>
        public string Prefix
        {
            get { return prefix; }
        }
    }
}
