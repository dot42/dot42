using System.Text;

namespace Dot42.DexLib
{
    public class FieldReference : IMemberReference
    {
        public FieldReference(ClassReference owner, string name, TypeReference fieldType)
        {
            Owner = owner;
            Type = fieldType;
            Name = name;
        }

        internal FieldReference()
        {
        }

        public ClassReference Owner { get; internal set; }
        public TypeReference Type { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Owner);
            builder.Append("::");
            builder.Append(Name);
            builder.Append(" : ");
            builder.Append(Type);
            return builder.ToString();
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public virtual bool Equals(IMemberReference other)
        {
            return Equals(other as FieldReference);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(FieldReference other)
        {
            return (other != null) && Owner.Equals(other.Owner) && Name.Equals(other.Name) && Type.Equals(other.Type);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public sealed override bool Equals(object other)
        {
            return Equals(other as FieldReference);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                hashCode = (hashCode * 397) ^ Owner.GetHashCode();
                return hashCode;
            }
        }
    }
}