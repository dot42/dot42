namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Parameter of a method
    /// </summary>
    public abstract class XParameter
    {
        private readonly string name;
        private readonly XParameterKind kind;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XParameter(string name, XParameterKind kind = XParameterKind.Input)
        {
            this.name = name;
            this.kind = kind;
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public abstract XTypeReference ParameterType { get; }

        /// <summary>
        /// Is my parameter type the same as the type of the other parameter?
        /// </summary>
        public bool IsSame(XParameter other)
        {
            /*if (Kind != other.Kind)
                return false;*/
            return ParameterType.IsSame(other.ParameterType);
        }

        /// <summary>
        /// Kind of parameter (in/out/byref)
        /// </summary>
        public XParameterKind Kind { get { return kind; } }

        public static XParameter Create(string name, XTypeReference type)
        {
            return new TypedParameter(name, type);
        }

        /// <summary>
        /// PArameter implementation with given type.
        /// </summary>
        private class TypedParameter : XParameter
        {
            private readonly XTypeReference type;

            public TypedParameter(string name, XTypeReference type, XParameterKind kind = XParameterKind.Input)
                : base(name, kind)
            {
                this.type = type;
            }

            /// <summary>
            /// Type of the parameter
            /// </summary>
            public override XTypeReference ParameterType
            {
                get { return type; }
            }
        }
    }
}
