using System.ComponentModel.Composition;
using System.Linq;
using Dot42.Compiler.Code;
using Dot42.DexLib.Instructions;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    [Export(typeof(Language))]
    public class DexLanguage : Language
    {
        private AssemblyCompiler compiler;

        public override string Name
        {
            get { return "Dex Output"; }
        }

        public override string FileExtension
        {
            get { return ".dexasm"; }
        }

        public override void DecompileMethod(Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ITextOutput output, DecompilationOptions options)
        {
            var declaringType = method.DeclaringType;
            var assembly = declaringType.Module.Assembly;

            if ((compiler == null) || (compiler.Assembly != assembly))
            {
                compiler = null;
                var c = new AssemblyCompiler(assembly, new NamespaceConverter("pkg.name", ""));
                c.Compile();
                compiler = c;
            }

            var cmethod = compiler.GetMethod(method);
            if ((cmethod != null) && (cmethod.DexMethod != null))
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
            else
            {
                output.Write("Method not found in dex");
                output.WriteLine();
            }
        }
    }
}
