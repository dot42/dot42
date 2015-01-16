namespace Dot42.JvmClassLib.Structures
{
    public abstract class ConstantPoolEntry
    {
        private readonly ConstantPool constantPool;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolEntry(ConstantPool constantPool)
        {
            this.constantPool = constantPool;
        }

        /// <summary>
        /// Gets the class loader of the containing class.
        /// </summary>
        internal IClassLoader Loader
        {
            get { return constantPool.Loader; }
        }

        /// <summary>
        /// Gets the containing pool
        /// </summary>
        protected T GetEntry<T>(int index)
            where T : ConstantPoolEntry
        {
            return constantPool.GetEntry<T>(index);
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public abstract ConstantPoolTags Tag { get; }
    }
}
