using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace Dot42.WcfTools.ProxyBuilder
{
    public sealed class CodeGenerator
    {
        private readonly CodeCompileUnit codeCompileUnit;
        private readonly CodeNamespace codeNamespace;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal CodeGenerator()
        {
            codeCompileUnit = new CodeCompileUnit();
            codeNamespace = new CodeNamespace("Dot42.Generated.Wcf");
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("Dot42.Internal"));
            codeCompileUnit.Namespaces.Add(codeNamespace);
        }

        /// <summary>
        /// Add a given type declaration
        /// </summary>
        public void Add(CodeTypeDeclaration typeDecl)
        {
            codeNamespace.Types.Add(typeDecl);
        }

        /// <summary>
        /// Generate the actual code to the given writer.
        /// </summary>
        public void Generate(TextWriter writer)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var options = new CodeGeneratorOptions { BracingStyle = "C" };
            provider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, options);
            writer.Flush();
        }
    }
}
