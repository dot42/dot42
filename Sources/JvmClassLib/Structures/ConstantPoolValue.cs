namespace Dot42.JvmClassLib.Structures
{
    public abstract class ConstantPoolValue<T> : ConstantPoolEntry, IConstantPoolValue
    {
        private readonly T value;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolValue(ConstantPool constantPool, T value)
            : base(constantPool)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the actual value
        /// </summary>
        public T Value { get { return value; } }

        /// <summary>
        /// Gets the actual value
        /// </summary>
        object IConstantPoolValue.Value { get { return value; } }

        /// <summary>
        /// Gets human readable string
        /// </summary>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
