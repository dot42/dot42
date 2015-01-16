using System.CodeDom;
using Dot42.Cecil;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build a proxy method for a specific OperationContract interface method.
    /// </summary>
    internal abstract class ProxyMethodBuilder
    {
        protected readonly ProxyClassBuilder Parent;
        protected readonly MethodDefinition InterfaceMethod;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ProxyMethodBuilder(ProxyClassBuilder parent, MethodDefinition interfaceMethod)
        {
            Parent = parent;
            InterfaceMethod = interfaceMethod;
        }

        /// <summary>
        /// Create all internal structures.
        /// </summary>
        public abstract void Create(ProxySerializationContext context);

        /// <summary>
        /// Create the entire proxy method as C# code.
        /// </summary>
        public abstract void Generate(CodeTypeDeclaration declaringType, CodeGenerator generator);

        /// <summary>
        /// Create a method declaration skeleton.
        /// </summary>
        protected CodeMemberMethod CreateProxyMethodDeclaration()
        {
            var methodDecl = new CodeMemberMethod { Name = InterfaceMethod.Name, Attributes = MemberAttributes.Public | MemberAttributes.Final };
            methodDecl.ReturnType = InterfaceMethod.ReturnType.Accept(CodeTypeVisitor.Instance, null);
            foreach (var parameter in InterfaceMethod.Parameters)
            {
                methodDecl.Parameters.Add(new CodeParameterDeclarationExpression(parameter.ParameterType.Accept(CodeTypeVisitor.Instance, null), parameter.Name));
            }

            return methodDecl;
        }
    }
}
