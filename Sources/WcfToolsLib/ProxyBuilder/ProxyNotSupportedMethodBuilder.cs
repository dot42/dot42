using System;
using System.CodeDom;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build a proxy method for a specific non-OperationContract interface method.
    /// </summary>
    internal sealed class ProxyNotSupportedMethodBuilder : ProxyMethodBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal ProxyNotSupportedMethodBuilder(ProxyClassBuilder parent, MethodDefinition interfaceMethod)
            : base(parent, interfaceMethod)
        {
        }

        /// <summary>
        /// Create all internal structures.
        /// </summary>
        public override void Create(ProxySerializationContext context)
        {
            // Do nothing
        }

        /// <summary>
        /// Create the entire proxy method as C# code.
        /// </summary>
        public override void Generate(CodeTypeDeclaration declaringType, CodeGenerator generator)
        {
            // Prepare method
            var methodDecl = CreateProxyMethodDeclaration();

            // Add code
            methodDecl.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof (NotSupportedException))));

            // Add method to declaring type
            declaringType.Members.Add(methodDecl);   
        }
    }
}
