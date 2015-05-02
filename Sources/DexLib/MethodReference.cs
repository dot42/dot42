using System.Text;

namespace Dot42.DexLib
{
    public class MethodReference : IMemberReference
    {
        public MethodReference() : base()
        {
        }

        public MethodReference(ClassReference owner, string name, Prototype prototype) : this()
        {
            Owner = owner;
            Name = name;
            Prototype = prototype;
        }

        public ClassReference Owner { get; set; }
        public Prototype Prototype { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Owner);
            builder.Append("::");
            builder.Append(Name);
            builder.Append(Prototype);
            return builder.ToString();
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public virtual bool Equals(IMemberReference other)
        {
            return Equals(other as MethodReference);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(MethodReference other)
        {
            return (other != null) && Owner.Equals(other.Owner) && Name.Equals(other.Name) && Prototype.Equals(other.Prototype);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public sealed override bool Equals(object other)
        {
            return Equals(other as MethodReference);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode*397) ^ (Prototype != null ? Prototype.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Owner != null ? Owner.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}