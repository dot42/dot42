using System.ComponentModel.Composition;
using System.Linq;
using Dot42.Compiler.Code;
using Dot42.Compiler.Code.RL;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    [Export(typeof(Language))]
    public class RLLanguage : Language
    {
        private AssemblyCompiler compiler;

        public override string Name
        {
            get { return "RL Output"; }
        }

        public override string FileExtension
        {
            get { return ".rl"; }
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
