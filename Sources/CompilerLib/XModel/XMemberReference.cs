using System.Diagnostics;

namespace Dot42.CompilerLib.XModel
{
    [DebuggerDisplay("{FullName}")]
    public abstract class XMemberReference : XReference, IXMemberReference 
    {
        private readonly XTypeReference declaringType;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XMemberReference(XModule module, XTypeReference declaringType)
            : base(module)
        {
            this.declaringType = declaringType;
        }

        /// <summary>
        /// Gets the type that contains this member
        /// </summary>
        public XTypeReference DeclaringType { get { return declaringType; } }
    }
}
