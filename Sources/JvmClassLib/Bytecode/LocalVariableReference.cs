namespace Dot42.JvmClassLib.Bytecode
{
    /// <summary>
    /// Reference to a local variable.
    /// </summary>
    public sealed class LocalVariableReference
    {
        private readonly MethodDefinition method;
        private readonly int index;
        private readonly bool isParameter;
        private readonly bool isThis;
#if DEBUG
        private readonly int id = lastId++;
        private static int lastId = 0;
#endif

        /// <summary>
        /// Default ctor
        /// </summary>
        internal LocalVariableReference(MethodDefinition method, int index)
        {
            this.method = method;
            this.index = index;
            isParameter = (index < method.GetParametersLocalVariableSlots());
            isThis = (index == 0) && method.HasThis;
        }

        /// <summary>
        /// Index into the local variable frame.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Is this reference to a parameter?
        /// </summary>
        public bool IsParameter
        {
            get { return isParameter; }
        }

        /// <summary>
        /// Is this variable "this"?
        /// </summary>
        public bool IsThis
        {
            get { return isThis; }
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
#if DEBUG
            return string.Format("{0}{1}({2})", isParameter ? "PAR" : "VAR", index, id);
#else
            return string.Format("{0}{1}", isParameter ? "PAR" : "VAR", index);
#endif
        }

        /// <summary>
        /// Is this equal to other?
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as LocalVariableReference);
        }

        public bool Equals(LocalVariableReference other)
        {
            return (other != null) && (other.method == method) && (other.index == index);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
