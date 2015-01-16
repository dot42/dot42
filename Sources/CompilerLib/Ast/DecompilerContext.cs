using System;
using System.Threading;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast
{
    public sealed class DecompilerContext
    {
        private readonly string name;
        private readonly string declaringTypeName;
        private readonly XTypeDefinition declaringType;
        private readonly XTypeReference returnType;
        private readonly XModule currentModule;
        private readonly DecompilerSettings settings = new DecompilerSettings();

        /// <summary>
        /// .NET ctor
        /// </summary>
        public DecompilerContext(XMethodDefinition currentMethod)
        {
            if (currentMethod == null)
                throw new ArgumentNullException("currentMethod");
            name = currentMethod.Name;
            declaringTypeName = currentMethod.DeclaringType.Name;
            declaringType = currentMethod.DeclaringType;
            returnType = currentMethod.ReturnType;
            currentModule = currentMethod.Module;
        }

        public string CurrentMethodName
        {
            get { return name; }
        }

        public XTypeDefinition DeclaringType
        {
            get { return declaringType; }
        }

        public string DeclaringTypeName
        {
            get { return declaringTypeName; }
        }

        public XTypeReference ReturnType
        {
            get { return returnType; }
        }

        public XModule CurrentModule
        {
            get { return currentModule; }
        }

        public CancellationToken CancellationToken
        {
            get { return CancellationToken.None; }
        }

        public DecompilerSettings Settings { get { return settings; } }

        public XTypeDefinition CurrentType
        {
            get { return declaringType; }
        }

        public class DecompilerSettings
        {
            public bool AlwaysGenerateExceptionVariableForCatchBlocks { get; private set; }
            public bool YieldReturn { get; private set; }
            public bool ObjectOrCollectionInitializers { get; private set; }
            public bool ExpressionTrees { get; private set; }
        }
    }
}
