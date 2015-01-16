namespace Dot42.JvmClassLib.Structures
{
    /// <summary>
    /// List of constant pool entries
    /// </summary>
    internal sealed class ConstantPool
    {
        private readonly IClassLoader loader;
        private readonly ConstantPoolEntry[] entries;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPool(int count, IClassLoader loader)
        {
            this.loader = loader;
            entries = new ConstantPoolEntry[count-1];
        }

        /// <summary>
        /// Gets/sets an entry
        /// </summary>
        public ConstantPoolEntry this[int index]
        {
            get { return entries[index - 1]; }
            set { entries[index - 1] = value; }
        }

        /// <summary>
        /// Gets the class loader of the containing class.
        /// </summary>
        internal IClassLoader Loader
        {
            get { return loader; }
        }

        /// <summary>
        /// Gets the containing pool
        /// </summary>
        public T GetEntry<T>(int index)
            where T : ConstantPoolEntry
        {
            var entry = this[index];
            if (entry == null)
                return default(T);
            return (T)entry;
        }
    }
}
