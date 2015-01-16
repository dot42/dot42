namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Single value in the VM
    /// </summary>
    public abstract class DalvikValue
    {
        private readonly object value;
        public readonly Jdwp.Tag tag;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected DalvikValue(Value value, DalvikProcess process)
        {
            tag = value.Tag;
            this.value = value.IsPrimitive ? value.ValueObject : new DalvikObjectReference((ObjectId)value.ValueObject, process);
        }

        /// <summary>
        /// Gets the value as .NET object.
        /// </summary>
        public object Value
        {
            get { return value; }
        }

        /// <summary>
        /// Is this a primitive value?
        /// If not, it is an object reference (which can be null)
        /// </summary>
        public bool IsPrimitive { get { return (!(value is DalvikObjectReference)); } }

        /// <summary>
        /// Is this value an array?
        /// </summary>
        public bool IsArray { get { return (tag == Jdwp.Tag.Array); } }

        /// <summary>
        /// Is this value a string?
        /// </summary>
        public bool IsString { get { return (tag == Jdwp.Tag.String); } }

        /// <summary>
        /// Is this value a boolean?
        /// </summary>
        public bool IsBoolean { get { return (tag == Jdwp.Tag.Boolean); } }

        /// <summary>
        /// Gets the value as object reference.
        /// Returns null for primitive values.
        /// </summary>
        public DalvikObjectReference ObjectReference
        {
            get { return value as DalvikObjectReference; }
        }

        /// <summary>
        /// Gets the name of the item that has this value
        /// </summary>
        public abstract string Name { get; }
    }
}
