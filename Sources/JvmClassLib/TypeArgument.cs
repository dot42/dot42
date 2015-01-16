using System;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Single argument of type signature.
    /// </summary>
    public sealed class TypeArgument
    {
        private readonly TypeArgumentWildcard wildcard;
        private readonly TypeReference signature;

        public static readonly TypeArgument Any = new TypeArgument();

        /// <summary>
        /// Any ctor
        /// </summary>
        private TypeArgument()
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public TypeArgument(TypeArgumentWildcard wildcard, TypeReference signature)
        {
            if (signature == null)
                throw new ArgumentNullException("signature");
            this.wildcard = wildcard;
            this.signature = signature;
        }

        public TypeReference Signature
        {
            get { return signature; }
        }

        public TypeArgumentWildcard Wildcard
        {
            get { return wildcard; }
        }

        /// <summary>
        /// Is this an Any '*' argument?
        /// </summary>
        public bool IsAny { get { return (signature == null); } }

        public override string ToString()
        {
            if (IsAny)
                return "*";
            string prefix;
            switch (wildcard)
            {
                case TypeArgumentWildcard.Plus:
                    prefix = "+";
                    break;
                case TypeArgumentWildcard.Minus:
                    prefix = "-";
                    break;
                default:
                    prefix = string.Empty;
                    break;
            }
            return prefix + signature;
        }
    }
}
