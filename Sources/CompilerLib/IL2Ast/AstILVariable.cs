using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.IL2Ast
{
    internal sealed class AstILVariable : AstVariable 
    {
        private readonly VariableDefinition  originalVariable;
        private readonly string              originalVariableName;
        private readonly ParameterDefinition originalParameter;

        /// <summary>
        /// Variable ctor
        /// </summary>
        public AstILVariable(string name, XTypeReference type, VariableDefinition originalVariable,string originalVariableName)
        {
            Name = name;
            Type = type;
            this.originalVariable = originalVariable;
            this.originalVariableName = originalVariableName;
        }

        /// <summary>
        /// Parameter ctor
        /// </summary>
        public AstILVariable(string name, XTypeReference type, ParameterDefinition originalParameter)
        {
            Name = name;
            Type = type;
            this.originalParameter = originalParameter;
        }

        /// <summary>
        /// Is this variable pinnen?
        /// </summary>
        public override bool IsPinned
        {
            get { return (originalVariable != null) && originalVariable.IsPinned; }
        }

        /// <summary>
        /// Is this variable a parameter of the current method?
        /// </summary>
        public override bool IsParameter
        {
            get { return (originalParameter != null); }
        }

        /// <summary>
        /// Gets the original variable object (source specific).
        /// </summary>
        public override object OriginalVariable { get { return originalVariable; } }

        /// <summary>
        /// Gets the original parameter object (source specific).
        /// </summary>
        public override object OriginalParameter { get { return originalParameter; } }

        /// <summary>
        /// Gets the name of the variable as used in the original code (IL/Java).
        /// Can be null
        /// </summary>
        public override string OriginalName
        {
            get
            {
                if (originalVariableName != null)
                    return originalVariableName;
                if (originalParameter != null)
                    return originalParameter.Name;
                return Name;
            }
        }

        /// <summary>
        /// Is this variable "this"?
        /// </summary>
        public override bool IsThis
        {
            get { return IsParameter && (originalParameter.Index == -1); }
        }

        /// <summary>
        /// Gets the type of this variable.
        /// </summary>
        protected override TTypeRef GetType<TTypeRef>(ITypeResolver<TTypeRef> typeResolver)
        {
            if ((originalVariable == null) || (originalVariable.VariableType == null))
                return default(TTypeRef);
            var xVarType = XBuilder.AsTypeReference(Type.Module, originalVariable.VariableType);
            return typeResolver.GetTypeReference(xVarType);
        }
    }
}