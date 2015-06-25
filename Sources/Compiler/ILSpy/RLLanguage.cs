extern alias ilspy;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CompilerLib;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Transformations;
using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy;



namespace Dot42.Compiler.ILSpy
{
    [Export(typeof(Language))]
    public class RLLanguage : CompiledLanguage
    {
        internal static bool ShowHasSeqPoint { get; set; }
        public static int StopOptimizationAfter { get; set; }

        public override string Name
        {
            get { return "RL Output"; }
        }

        public override string FileExtension
        {
            get { return ".rl"; }
        }

        static RLLanguage()
        {
            StopOptimizationAfter = -1;
        }

        public override void DecompileMethod(ilspy::Mono.Cecil.MethodDefinition method, ITextOutput output, DecompilationOptions options)
        {
            var xMethod = GetXMethodDefinition(method);
            var ilMethod = xMethod as XBuilder.ILMethodDefinition;

            CompiledMethod cmethod;

            if (ilMethod == null || !ilMethod.OriginalMethod.HasBody)
            {
                output.Write("");
                output.WriteLine("// not an il method or method without body.");
                return;
            }
            
            var methodSource = new MethodSource(xMethod, ilMethod.OriginalMethod);
            var target = (DexTargetPackage) AssemblyCompiler.TargetPackage;
            var dMethod = (MethodDefinition)xMethod.GetReference(target);
            DexMethodBodyCompiler.TranslateToRL(AssemblyCompiler, target, methodSource, dMethod, false, out cmethod);

            var rlBody = cmethod.RLBody;

            // Optimize RL code
            string lastApplied = RLTransformations.Transform(target.DexFile, rlBody, StopOptimizationAfter == -1?int.MaxValue:StopOptimizationAfter);
            if(lastApplied != null)
                output.WriteLine("// Stop after " + lastApplied);

            PrintMethod(cmethod, output, options);
        }

        private void PrintMethod(CompiledMethod cmethod, ITextOutput output, DecompilationOptions options)
        {
            if ((cmethod != null) && (cmethod.RLBody != null))
            {
                var body = cmethod.RLBody;
                var basicBlocks = BasicBlock.Find(body);

                foreach (var block in basicBlocks)
                {
                    output.Write(string.Format("D_{0:X4}:", block.Entry.Index));
                    output.WriteLine();
                    output.Indent();
                    foreach (var ins in block.Instructions)
                    {
                        if (ShowHasSeqPoint)
                        {
                            if (ins.SequencePoint != null)
                                output.Write(ins.SequencePoint.IsSpecial ? "!" : "~");
                        }

                        output.Write(ins.ToString());
                        output.WriteLine();
                    }
                    output.Unindent();
                }

                if (body.Exceptions.Any())
                {
                    output.WriteLine();
                    output.Write("Exception handlers:");
                    output.WriteLine();
                    output.Indent();
                    foreach (var handler in body.Exceptions)
                    {
                        output.Write(string.Format("{0:x4}-{1:x4}", handler.TryStart.Index, handler.TryEnd.Index));
                        output.WriteLine();
                        output.Indent();
                        foreach (var c in handler.Catches)
                        {
                            output.Write(string.Format("{0} => {1:x4}", c.Type, c.Instruction.Index));
                            output.WriteLine();
                        }
                        if (handler.CatchAll != null)
                        {
                            output.Write(string.Format("{0} => {1:x4}", "<any>", handler.CatchAll.Index));
                            output.WriteLine();
                        }
                        output.Unindent();
                    }
                    output.Unindent();
                }
            }
            else
            {
                output.Write("Method not found in dex");
                output.WriteLine();
            }
        }
    }
}
