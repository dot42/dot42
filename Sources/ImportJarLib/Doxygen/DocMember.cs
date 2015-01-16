namespace Dot42.ImportJarLib.Doxygen
{
    public abstract class DocMember
    {
        private readonly string name;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected DocMember(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Member name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Documentation
        /// </summary>
        public DocDescription Description { get; set; }

        /// <summary>
        /// Link references.
        /// </summary>
        internal virtual void Link(DocModel model)
        {
            // override me
        }
    }

    public abstract class DocMember<TOwner> : DocMember
        where TOwner : DocMember
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected DocMember(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Owner
        /// </summary>
        public TOwner DeclaringClass { get; set; }
    }
}
