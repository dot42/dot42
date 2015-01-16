using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("Git: {@FullName}")]
    public sealed class NetGenericInstanceType : NetTypeReference
    {
        private readonly List<NetTypeReference> genericArguments = new List<NetTypeReference>();
        private readonly NetTypeDefinition elementType;
        private readonly NetTypeReference declaringType;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetGenericInstanceType(NetTypeDefinition elementType, NetTypeReference declaringType)
        {
            this.elementType = elementType;
            this.declaringType = declaringType;
        }

        /// <summary>
        /// Underlying element type.
        /// </summary>
        public NetTypeDefinition ElementType
        {
            get { return elementType; }
        }

        /// <summary>
        /// Declaring type (if any)
        /// </summary>
        public NetTypeReference DeclaringType
        {
            get { return declaringType; }
        }

        /// <summary>
        /// Gets all types references in this type.
        /// This includes the element type and any generic arguments.
        /// </summary>
        public override IEnumerable<NetTypeDefinition> GetReferencedTypes()
        {
            yield return elementType;
            if (declaringType != null)
            {
                foreach (var type in declaringType.GetReferencedTypes())
                {
                    yield return type;
                }
            }
            foreach (var type in genericArguments.SelectMany(x => x.GetReferencedTypes()))
            {
                yield return type;
            }
        }

        /// <summary>
        /// Underlying type definition.
        /// </summary>
        public override NetTypeDefinition GetElementType()
        {
            return elementType;
        } 

        /// <summary>
        /// Gets the entire C# name.
        /// </summary>
        public override string FullName
        {
            get
            {
                string prefix;
                if (declaringType != null)
                {
                    prefix = declaringType.FullName + ".";
                }
                else
                {
                    prefix = ElementType.FullName;
                }
                return prefix + "<" + string.Join(", ", genericArguments.Select(x => x.FullName)) + ">";
            }
        }

        /// <summary>
        /// All generic arguments
        /// </summary>
        public IEnumerable<NetTypeReference> GenericArguments { get { return genericArguments;  } }

        /// <summary>
        /// Get the generic argument at the given index
        /// </summary>
        public NetTypeReference GenericArgument(int index) { return genericArguments[index]; }

        /// <summary>
        /// Set the generic argument at the given index
        /// </summary>
        public void SetGenericArgument(int index, NetTypeReference  value) { genericArguments[index] = value; }

        /// <summary>
        /// Add a generic argument to the end of the list.
        /// </summary>
        public void AddGenericArgument(NetTypeReference argument, TypeNameMap typeNameMap)
        {
            if (argument.IsVoid())
            {
                argument = typeNameMap.Object;
            }
            genericArguments.Add(argument);
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override T Accept<T, TData>(INetTypeVisitor<T, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}
