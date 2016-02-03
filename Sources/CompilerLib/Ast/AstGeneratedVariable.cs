namespace Dot42.CompilerLib.Ast
{
    public sealed class AstGeneratedVariable : AstVariable 
    {
        private readonly string originalName;

        /// <summary>
        /// Generated ctor
        /// </summary>
        public AstGeneratedVariable(string name, string originalName, bool preventOptimization = false)
        {
            this.originalName = originalName ?? name;
            Name = name;
            IsGenerated = true;
            PreventOptimizations = preventOptimization;
        }

        /// <summary>
        /// Is this variable pinnen?
        /// </summary>
        public override bool IsPinned
        {
            get { return false; }
        }

        /// <summary>
        /// Is this variable a parameter of the current method?
        /// </summary>
        public override bool IsParameter
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the original variable object (source specific).
        /// </summary>
        public override object OriginalVariable { get { return null; } }

        /// <summary>
        /// Gets the original parameter object (source specific).
        /// </summary>
        public override object OriginalParameter { get { return null; } }

        /// <summary>
        /// Gets the name of the variable as used in the original code (IL/Java).
        /// Can be null
        /// </summary>
        public override string OriginalName { get { return originalName; } }

        /// <summary>
        /// Is this variable "this"?
        /// </summary>
        public override bool IsThis
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the type of this variable.
        /// </summary>
        protected override TTypeRef GetType<TTypeRef>(ITypeResolver<TTypeRef> typeResolver)
        {
            return default(TTypeRef);
        }
    }
}