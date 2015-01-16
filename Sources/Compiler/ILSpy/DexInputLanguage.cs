using System.ComponentModel.Composition;
using Dot42.Compiler.Code;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    /// <summary>
    /// Shows the ILAst that is used to compile to dex.
    /// </summary>
    [Export(typeof(Language))]
    public class DexInputLanguage : Language
    {
        public override string Name
        {
            get { return "Dex Input"; }
        }

        public override string FileExtension
        {
            get { return ".il"; }
        }

        public override void DecompileMethod(Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ITextOutput output, DecompilationOptions options)
        {
            var node = MethodBodyCompiler.CreateOptimizedAst(method);
            node.WriteTo(new TextOutputBridge(output));
        }
    }
}
