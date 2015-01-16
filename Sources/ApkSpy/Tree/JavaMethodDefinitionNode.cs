using System;
using System.IO;
using System.Linq;
using System.Text;

using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Optimizer;
using Dot42.JvmClassLib;
using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Bytecode;
using Dot42.Utility;

using MethodDefinition = Dot42.JvmClassLib.MethodDefinition;

namespace Dot42.ApkSpy.Tree
{
    internal class JavaMethodDefinitionNode : TextNode
    {
        private readonly MethodDefinition methodDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JavaMethodDefinitionNode(MethodDefinition methodDef)
        {
            this.methodDef = methodDef;
            Text = methodDef.Name;
            ImageIndex = 6;
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            var nl = Environment.NewLine;
            var sb = new StringBuilder();
            sb.AppendFormat("AccessFlags: {0} (0x{1:X4}){2}", AccessFlagsAsString(methodDef.AccessFlags), (int)methodDef.AccessFlags, nl);
            sb.AppendFormat("Descriptor:  {0}{1}", methodDef.Descriptor, nl);
            sb.AppendFormat("Signature:   {0}{1}", (methodDef.Signature != null) ? methodDef.Signature.Original : "<none>", nl);
            sb.AppendFormat("Annotations: {0}{1}", TextNode.LoadAnnotations(methodDef), nl);

            var code = methodDef.Attributes.OfType<CodeAttribute>().FirstOrDefault();
            if (code != null)
            {
                sb.AppendFormat("Max locals:  {0}{1}", code.MaxLocals, nl);
                sb.AppendFormat("Max stack:   {0}{1}", code.MaxStack, nl);
                sb.AppendLine("Code:");
                foreach (var i in code.Instructions)
                {
                    sb.AppendFormat("\t{0:x4} {1} {2} {3} {4}", i.Offset, Format(i.Opcode), FormatOperand(i.Operand), FormatOperand(i.Operand2), nl);
                }
                sb.AppendLine();

                if (code.ExceptionHandlers.Any())
                {
                    sb.AppendLine("Exception handlers:");
                    foreach (var handler in code.ExceptionHandlers.OrderBy(x => x.StartPc))
                    {
                        sb.AppendFormat("\t{0:x4}-{1:x4}  => {2:x4} ({3})  {4}", handler.StartPc, handler.EndPc, handler.HandlerPc, handler.CatchType, nl);
                    }
                    sb.AppendLine();
                }

                if (code.Attributes.OfType<LocalVariableTableAttribute>().Any())
                {
                    var locVarAttr = code.Attributes.OfType<LocalVariableTableAttribute>().First();
                    sb.AppendLine("Local variables:");
                    foreach (var locVar in locVarAttr.Variables.OrderBy(x => x.StartPc))
                    {
                        sb.AppendFormat("\t{0:x4}-{1:x4}  => {2} ({3})  {4}", locVar.StartPc, locVar.EndPc, locVar.Name, locVar.Index, nl);
                    }
                    sb.AppendLine();
                }

#if DEBUG
                if (settings.ShowAst)
                {
                    sb.AppendLine("\n\nAST:\n");
                    try
                    {
                        var module = settings.Module;
                        var xMethod = CompilerLib.XModel.Java.XBuilder.AsMethodDefinition(module, methodDef);
                        var astBuilder = new CompilerLib.Java2Ast.AstBuilder(module, methodDef, xMethod.DeclaringType, true);
                        var context = new DecompilerContext(xMethod);
                        var ast = astBuilder.Build();

                        var writer = new PlainTextOutput(new StringWriter(sb));
                        ast.WriteTo(writer);
                        writer.WriteLine();

                        // Optimize AST
                        sb.AppendLine("\n\nOptimized AST:\n");
                        var astOptimizer = new AstOptimizer(context, ast);
                        astOptimizer.Optimize();
                        ast.WriteTo(writer);
                        writer.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        sb.Append(string.Format("Error: {0}\n\n{1}", ex.Message, ex.StackTrace));
                    }
                }
#endif
            }

            return sb.ToString();
        }

        private string AccessFlagsAsString(MethodAccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(MethodAccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Synchronized) ? "synchronized " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Bridge) ? "bridge " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.VarArgs) ? "varargs " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Native) ? "native " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Abstract) ? "abstract " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Strict) ? "strict " : string.Empty;
            result += accessFlags.HasFlag(MethodAccessFlags.Synthetic) ? "synthetic " : string.Empty;

            return result.Trim();
        }

        private static string Format(Code code)
        {
            return CodeNames.GetName(code).PadRight(15);
        }

        private static string FormatOperand(object operand)
        {
            if (operand == null)
                return string.Empty;

            if (operand is Instruction)
            {
                return string.Format("L_{0:X4}", ((Instruction) operand).Offset);
            }

            return operand.ToString();
        }
    }
}
