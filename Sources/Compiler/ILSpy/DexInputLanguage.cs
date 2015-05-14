extern alias ilspy;

using System.ComponentModel.Composition;
using Dot42.CompilerLib;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel;
using ilspy::Mono.Cecil;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    /// <summary>
    /// Shows the ILAst that is used to compile to dex.
    /// </summary>
    [Export(typeof(Language))]
    public class DexInputLanguage : CompiledLanguage
    {
        public override string Name
        {
            get { return "Dex Input"; }
        }

        public override string FileExtension
        {
            get { return ".il"; }
        }

        public override void DecompileMethod(ilspy::Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ITextOutput output, DecompilationOptions options)
        {
            var cMethod = GetCompiledMethod(method);
            var xMethod = GetXMethodDefinition(method);
            
            var methodSource= new MethodSource(xMethod, cMethod.ILSource);
            
            var node = MethodBodyCompiler.CreateOptimizedAst(AssemblyCompiler, methodSource, false);
            node.WriteTo(new TextOutputBridge(output));
        }
    }
}
