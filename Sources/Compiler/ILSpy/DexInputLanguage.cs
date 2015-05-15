extern alias ilspy;
using System;
using System.ComponentModel.Composition;
using Dot42.CompilerLib;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel.DotNet;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    /// <summary>
    /// Shows the ILAst that is used to compile to dex.
    /// </summary>
    [Export(typeof(Language))]
    public class DexInputLanguage : CompiledLanguage
    {
        internal static StopAstConversion StopConversion { get; set; }

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
            
            var xMethod = GetXMethodDefinition(method);
            var ilMethod = xMethod as XBuilder.ILMethodDefinition;
            if (ilMethod == null || !ilMethod.OriginalMethod.HasBody)
            {
                output.Write("not an il method or method without body.");
                return;
            }

            var methodSource = new MethodSource(xMethod, ilMethod.OriginalMethod);
            var node = MethodBodyCompiler.CreateOptimizedAst(AssemblyCompiler, methodSource, false, StopConversion);

            if (StopConversion != StopAstConversion.None)
            {
                output.Write("// Stop " + StopConversion);
                output.WriteLine();
                output.WriteLine();
            }


            node.WriteTo(new TextOutputBridge(output));
        }
    }
}
