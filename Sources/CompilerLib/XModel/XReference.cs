using System.Diagnostics;

namespace Dot42.CompilerLib.XModel
{
    [DebuggerDisplay("{FullName}")]
    public abstract class XReference
    {
        private readonly XModule module;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XReference(XModule module)
        {
            this.module = module;
        }

        /// <summary>
        /// Name of the reference
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Name of the reference (including all)
        /// </summary>
        public abstract string FullName { get; }

        /// <summary>
        /// Gets the containing module
        /// </summary>
        public XModule Module { get { return module; } }

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Flush all cached members
        /// </summary>
        protected virtual void Reset()
        {
            // Override me
        }
    }
}
