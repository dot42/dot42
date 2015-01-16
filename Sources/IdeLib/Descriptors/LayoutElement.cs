namespace Dot42.Ide.Descriptors
{
    internal sealed class LayoutElement
    {
        private readonly string name;
        private readonly LayoutType type;
        private readonly bool isAbstract;
        private readonly string superClassName;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal LayoutElement(string name, LayoutType type, bool isAbstract, string superClassName)
        {
            this.name = name;
            this.type = type;
            this.isAbstract = isAbstract;
            this.superClassName = superClassName;
        }

        public LayoutType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Short name of super class
        /// </summary>
        public string SuperClassName
        {
            get { return superClassName; }
        }

        /// <summary>
        /// Is this class abstract?
        /// </summary>
        public bool IsAbstract
        {
            get { return isAbstract; }
        }

        /// <summary>
        /// Short name of class
        /// </summary>
        public string Name
        {
            get { return name; }
        }
    }
}
