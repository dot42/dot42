using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("GP: {@Name} [{@Owner}]")]
    public sealed class NetGenericParameter : NetTypeReference
    {
        private readonly string originalName;
        private readonly INetGenericParameterProvider owner;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetGenericParameter(string name, string originalName, INetGenericParameterProvider owner)
        {
            Name = name;
            this.originalName = originalName;
            this.owner = owner;
        }

        /// <summary>
        /// Owner of this parameter
        /// </summary>
        public INetGenericParameterProvider Owner
        {
            get { return owner; }
        }

        /// <summary>
        /// Gets the index of this parameter in the owners generic parameters list.
        /// </summary>
        public int Position { get { return owner.GenericParameters.IndexOf(this); } }

        /// <summary>
        /// Underlying type definition.
        /// </summary>
        public override NetTypeDefinition GetElementType()
        {
            return null;
        }

        /// <summary>
        /// Gets all types references in this type.
        /// This includes the element type and any generic arguments.
        /// </summary>
        public override IEnumerable<NetTypeDefinition> GetReferencedTypes()
        {
            yield break;
        }

        /// <summary>
        /// Name of the type
        /// </summary>
        public override string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(base.Name))
                    return base.Name;
                return base.Name = (owner.IsMethod ? "!!" : "!") + Position;
            }
            set { base.Name = value; }
        }

        /// <summary>
        /// Gets the name as it was in the original java code.
        /// </summary>
        public string OriginalName
        {
            get { return originalName; }
        }

        /// <summary>
        /// Gets the entire C# name.
        /// </summary>
        public override string FullName
        {
            get { return Name; }
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
