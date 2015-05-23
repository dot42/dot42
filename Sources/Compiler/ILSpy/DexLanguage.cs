extern alias ilspy;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.ApkSpy.Disassembly;
using Dot42.CompilerLib;
using Dot42.DebuggerLib;
using Dot42.DexLib.Instructions;
using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    [Export(typeof(Language))]
    public class DexLanguage : CompiledLanguage
    {
        public static bool ShowFullNames { get; set; }
        public static bool DebugOperandTypes { get; set; }

        public override string Name
        {
            get { return "Dex Output"; }
        }

        public override string FileExtension
        {
            get { return ".dexasm"; }
        }


        public override ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition SyntaxHighlighting
        {
            get
            {
                return new CSharpLanguage().SyntaxHighlighting;
            }
        }

        public override void DecompileMethod(ilspy::Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ITextOutput output, DecompilationOptions options)
        {
            var cmethod = GetCompiledMethod(method);

            if ((cmethod != null) && (cmethod.DexMethod != null))
            {
                try
                {
                    var f = new MethodBodyDisassemblyFormatter(cmethod.DexMethod, MapFile);
                    var formatOptions = FormatOptions.EmbedSourceCode | FormatOptions.ShowJumpTargets;
                    if(ShowFullNames) formatOptions |= FormatOptions.FullTypeNames;
                    if(DebugOperandTypes) formatOptions |= FormatOptions.DebugOperandTypes;
                    
                    var s = f.Format(formatOptions);
                    output.Write(s);
                }
                catch (Exception)
                {
                    output.Write("\n\n// Formatting error. Using Fallback.\n\n");
                    FallbackFormatting(output, cmethod);    
                }
                
            }
            else
            {
                output.Write("Method not found in dex");
                output.WriteLine();
            }
        }

        private static void FallbackFormatting(ITextOutput output, CompiledMethod cmethod)
        {
            var body = cmethod.DexMethod.Body;

            body.UpdateInstructionOffsets();
            var targetInstructions = body.Instructions.Select(x => x.Operand).OfType<Instruction>().ToList();
            targetInstructions.AddRange(body.Exceptions.Select(x => x.TryStart));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.TryEnd));
            targetInstructions.AddRange(body.Exceptions.SelectMany(x => x.Catches, (h, y) => y.Instruction));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.CatchAll));

            foreach (var ins in body.Instructions)
            {
                if (targetInstructions.Contains(ins) || (ins.Offset == 0))
                {
                    output.Write(string.Format("D_{0:X4}:", ins.Offset));
                    output.WriteLine();
                }
                output.Indent();
                output.Write(ins.ToString());
                output.WriteLine();
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
                    output.Write(string.Format("{0:x4}-{1:x4}", handler.TryStart.Offset, handler.TryEnd.Offset));
                    output.WriteLine();
                    output.Indent();
                    foreach (var c in handler.Catches)
                    {
                        output.Write(string.Format("{0} => {1:x4}", c.Type, c.Instruction.Offset));
                        output.WriteLine();
                    }
                    if (handler.CatchAll != null)
                    {
                        output.Write(string.Format("{0} => {1:x4}", "<any>", handler.CatchAll.Offset));
                        output.WriteLine();
                    }
                    output.Unindent();
                }
                output.Unindent();
            }
        }
    }
}
