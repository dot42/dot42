using System;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.XModel;
using Dot42.JvmClassLib.Bytecode;

namespace Dot42.CompilerLib.Java2Ast
{
    internal sealed class AstJavaVariable : AstVariable
    {
        private readonly LocalVariableReference varRef;
        private readonly bool isParameter;
        private readonly int index;
        private readonly bool isThis;

        /// <summary>
        /// Variable ctor
        /// </summary>
        public AstJavaVariable(XTypeReference type, LocalVariableReference varRef)
        {
            this.varRef = varRef;
            index = varRef.Index;
            isParameter = varRef.IsParameter;
            isThis = varRef.IsThis;
            Name = (isParameter ? "_P" : "_V") + varRef;
            Type = type;
        }

        /// <summary>
        /// Variable ctor
        /// </summary>
        public AstJavaVariable(XTypeReference type, LocalVariableReference varRef, string name)
            : this(type, varRef)
        {
            Name = name;
        }

        /// <summary>
        /// Index of this variable in the local variables table of the current frame
        /// </summary>
        public int Index { get { return index; } }

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
            get { return isParameter; }
        }

        /// <summary>
        /// Gets the original variable object (source specific).
        /// </summary>
        public override object OriginalVariable { get { return index; } }

        /// <summary>
        /// Gets the original parameter object (source specific).
        /// </summary>
        public override object OriginalParameter { get { return varRef; } }

        /// <summary>
        /// Gets the name of the variable as used in the original code (IL/Java).
        /// Can be null
        /// </summary>
        public override string OriginalName { get { return Name; } }

        public LocalVariableReference LocalVariableReference
        {
            get { return varRef; }
        }

        /// <summary>
        /// Is this variable "this"?
        /// </summary>
        public override bool IsThis
        {
            get { return isThis; }
        }

        /// <summary>
        /// Gets the type of this variable.
        /// </summary>
        protected override TTypeRef GetType<TTypeRef>(ITypeResolver<TTypeRef> typeResolver)
        {
            throw new NotImplementedException();
/*            if ((originalVariable == null) || (originalVariable.VariableType == null))
                return null;
            return originalVariable.VariableType.GetReference(typeResolver);*/
        }
    }
}