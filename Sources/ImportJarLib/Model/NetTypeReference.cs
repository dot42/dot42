using System.Collections.Generic;

namespace Dot42.ImportJarLib.Model
{
    public abstract class NetTypeReference
    {
        /// <summary>
        /// Name of the type
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Underlying type definition.
        /// </summary>
        public abstract NetTypeDefinition GetElementType();

        /// <summary>
        /// Gets all types references in this type.
        /// This includes the element type and any generic arguments.
        /// </summary>
        public abstract IEnumerable<NetTypeDefinition> GetReferencedTypes();

        /// <summary>
        /// Gets the entire C# name.
        /// </summary>
        public abstract string FullName { get; }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public abstract T Accept<T, TData>(INetTypeVisitor<T, TData> visitor, TData data);

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }
    }
}
