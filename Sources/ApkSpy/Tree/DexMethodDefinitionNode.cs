using System;
using System.Linq;
using System.Text;
using Dot42.ApkSpy.Disassembly;
using Dot42.CompilerLib.RL2DexCompiler;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;

namespace Dot42.ApkSpy.Tree
{
    internal class DexMethodDefinitionNode : TextNode
    {
        private readonly MethodDefinition methodDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexMethodDefinitionNode(MethodDefinition methodDef)
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
            sb.AppendFormat("AccessFlags: {0}{1}", methodDef.AccessFlags, nl);
            sb.AppendFormat("Prototype:   {0}{1}", methodDef.Prototype, nl);

            var body = methodDef.Body;
            if (body != null)
            {
                sb.AppendFormat("#Registers:  {0}{1}", body.Registers.Count, nl);
                sb.AppendFormat("#Incoming:   {0}{1}", body.IncomingArguments, nl);
                sb.AppendFormat("#Outgoing:   {0}{1}", body.OutgoingArguments, nl);
                sb.AppendLine();

                var formatter = new MethodBodyDisassemblyFormatter(methodDef, settings.MapFile);
                var code = formatter.Format(settings.EmbedSourceCodePositions, settings.EmbedSourceCode, settings.ShowControlFlow);
                sb.AppendLine(code);

#if DEBUG
                var debugInfo = body.DebugInfo;
                if (debugInfo != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("DEX Debug info:");
                    sb.AppendFormat("\tStart line: {0}{1}", debugInfo.LineStart, nl);
                    int lineNumber = (int) debugInfo.LineStart;
                    var address = 0;
                    var index = 0;
                    foreach (var ins in debugInfo.DebugInstructions)
                    {
                        if (!ins.IsLowLevel)
                            sb.AppendFormat("\t{0:x4}\tline {1}, address {2:x4} -> {3}{4}", index, lineNumber, address, ins, nl);                        
                        ins.UpdateLineAndAddress(ref lineNumber, ref address);
                        index++;
                    }
                }
#endif
            }

            return sb.ToString();
        }

        private string AccessFlagsAsString(AccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(AccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Synchronized) ? "synchronized " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Bridge) ? "bridge " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.VarArgs) ? "varargs " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Native) ? "native " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Abstract) ? "abstract " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.StrictFp) ? "strictfp " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Constructor) ? "constructor " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.DeclaredSynchronized) ? "synchronized " : string.Empty;

            return result.Trim();
        }
    }
}
