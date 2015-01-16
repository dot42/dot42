using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("Arr: {@FullName}")]
    public sealed class NetArrayType : NetTypeReference
    {
        private readonly NetTypeReference elementType;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetArrayType(NetTypeReference elementType)
        {
            this.elementType = elementType;
        }

        /// <summary>
        /// Array element type.
        /// </summary>
        public NetTypeReference ElementType
        {
            get { return elementType; }
        }

        /// <summary>
        /// Gets all types references in this type.
        /// This includes the element type and any generic arguments.
        /// </summary>
        public override IEnumerable<NetTypeDefinition> GetReferencedTypes()
        {
            return elementType.GetReferencedTypes();
        }

        /// <summary>
        /// Underlying type definition.
        /// </summary>
        public override NetTypeDefinition GetElementType()
        {
            return elementType.GetElementType();
        } 

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override T Accept<T, TData>(INetTypeVisitor<T, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the entire C# name.
        /// </summary>
        public override string FullName
        {
            get
            {
                var result = elementType.FullName + "[]";
                if (result == "System.Void[]")
                    result = "System.Object[]";
                return result;
            }
        }
    }
}
