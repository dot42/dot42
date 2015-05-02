using System;
using System.Linq;
using System.Text;
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
                var cfg = new ControlFlowGraph(body);
                sb.AppendFormat("#Registers:  {0}{1}", body.Registers.Count, nl);
                sb.AppendFormat("#Incoming:   {0}{1}", body.IncomingArguments, nl);
                sb.AppendFormat("#Outgoing:   {0}{1}", body.OutgoingArguments, nl);
                sb.AppendLine("Code:");
#if DEBUG
                var firstBlock = true;
#endif
                foreach (var block in cfg)
                {
#if DEBUG
                    if (!firstBlock)
                    {
                        sb.AppendLine("\t----------------- ");
                    }
                    sb.AppendFormat("\tEntry [{0}], Exit [{1}]{2}",
                              string.Join(", ", block.EntryBlocks.Select(x => x.Entry.Offset.ToString("x4"))),
                              string.Join(", ", block.ExitBlocks.Select(x => x.Entry.Offset.ToString("x4"))),
                              nl);
                    firstBlock = false;
#endif
                    foreach (var i in block.Instructions)
                    {
                        sb.AppendFormat("\t{0:x4} {1} {2}   {3}{4}", i.Offset, Format(i.OpCode), Register(i), FormatOperand(i.Operand), nl);
                    }
                }
                sb.AppendLine();

                if (body.Exceptions.Any())
                {
                    sb.AppendLine("Exception handlers:");
                    foreach (var handler in body.Exceptions)
                    {
                        sb.AppendFormat("\t{0:x4}-{1:x4}{2}", handler.TryStart.Offset, handler.TryEnd.Offset, nl);
                        foreach (var c in handler.Catches)
                        {
                            sb.AppendFormat("\t\t{0} => {1:x4}{2}", c.Type, c.Instruction.Offset, nl);                            
                        }
                        if (handler.CatchAll != null)
                        {
                            sb.AppendFormat("\t\t{0} => {1:x4}{2}", "<any>", handler.CatchAll.Offset, nl);                                                        
                        }
                    }
                }

                var mapFile = settings.MapFile;
                if (mapFile != null)
                {
                    var typeEntry = mapFile.GetTypeByNewName(methodDef.Owner.Fullname);
                    if (typeEntry != null)
                    {
                        var methodEntry = typeEntry.FindDexMethod(methodDef.Name, methodDef.Prototype.ToSignature());
                        if (methodEntry != null)
                        {
                            var validParameters = methodEntry.Parameters.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
                            if (validParameters.Any())
                            {
                                sb.AppendLine("Parameters:");
                                foreach (var p in validParameters)
                                {
                                    sb.AppendFormat("\tr{0} -> {1}{2}", p.Register, p.Name, nl);
                                }
                                sb.AppendLine();
                            }

                            var validVariables = methodEntry.Variables.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
                            if (validVariables.Any())
                            {
                                sb.AppendLine("Variables:");
                                foreach (var p in validVariables)
                                {
                                    sb.AppendFormat("\tr{0} -> {1}{2}", p.Register, p.Name, nl);
                                }
                                sb.AppendLine();
                            }

                            sb.AppendLine("Source code:");
                            Document lastDocument = null;
                            foreach (var row in mapFile.GetSourceCodePositions(methodEntry))
                            {
                                if (row.Document != lastDocument)
                                {
                                    sb.AppendFormat("\t{0}{1}", row.Document.Path, nl);
                                    lastDocument = row.Document;
                                }
                                var pos = row.Position;
                                sb.AppendFormat("\t{0:x4}\t({1},{2}) - ({3},{4}){5}", pos.MethodOffset, pos.Start.Line, pos.Start.Column, pos.End.Line, pos.End.Column, nl);
                            }
                        }
                    }
                }

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

        private static string Format(OpCodes opcode)
        {
            return OpCodesNames.GetName(opcode).PadRight(15);
        }

        private static string FormatOperand(object operand)
        {
            if (operand == null)
                return string.Empty;
            var ins = operand as Instruction;
            if (ins != null)
            {
                return string.Format("{0:x4}", ins.Offset);
            }
            return operand.ToString();
        }

        private static string Register(Instruction i)
        {
            return string.Join(", ", i.Registers.Select(x => string.Format("r{0}", x.Index)).ToArray());
        }
    }
}
