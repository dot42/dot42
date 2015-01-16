using System.Collections.Generic;

namespace Dot42.ImportJarLib.Model
{
    public sealed class NetCustomAttribute
    {
        private readonly NetTypeReference type;
        private readonly List<object> ctorArguments = new List<object>();
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>(); 

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetCustomAttribute(NetTypeReference type, params object[] ctorArguments)
        {
            this.type = type;
            ConstructorArguments.AddRange(ctorArguments);
        }

        /// <summary>
        /// Type of custom attribute
        /// </summary>
        public NetTypeReference AttributeType { get { return type; } }

        /// <summary>
        /// Arguments passed to the attribute ctor.
        /// </summary>
        public List<object> ConstructorArguments { get { return ctorArguments; } }

        /// <summary>
        /// Custom attribute properties.
        /// </summary>
        public Dictionary<string, object> Properties { get { return properties; } }

        /// <summary>
        /// Copy the properties of the source attribute into this attribute
        /// </summary>
        public void CopyPropertiesFrom(NetCustomAttribute source)
        {
            foreach (var entry in source.properties)
            {
                Properties[entry.Key] = entry.Value;
            }
        }
    }
}
