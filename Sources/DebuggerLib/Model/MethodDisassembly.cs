using System.Text;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;

namespace Dot42.DebuggerLib.Model
{
    public class MethodDisassembly
    {
        private readonly TypeEntry _typeEntry;
        private readonly MethodEntry _methodEntry;
        private readonly MethodDefinition _methodDef;
        private readonly MapFileLookup _mapFile;

        public ClassDefinition Class { get { return _methodDef.Owner; } }
        public MethodDefinition Method { get { return _methodDef; } }
        public MethodEntry MethodEntry { get { return _methodEntry; } }
        public TypeEntry TypeEntry { get { return _typeEntry; } }

        public MethodDisassembly(TypeEntry typeEntry,  MethodEntry methodEntry, MethodDefinition methodDef, MapFileLookup mapFile)
        {
            _typeEntry = typeEntry;
            _methodEntry = methodEntry;
            _methodDef = methodDef;
            _mapFile = mapFile;
        }

        public string FormatAddress(Instruction ins)
        {
            return ins.Offset.ToString("X3").PadLeft(4);
        }

        public string FormatOpCode(Instruction ins)
        {
            return ins.OpCode.ToString().PadLeft(20) + " ";
        }

        public string FormatOperands(Instruction ins)
        {
            return FormatOperands(ins, _methodDef.Body);
        }

        public string FormatInstruction(Instruction ins)
        {
            return FormatAddress(ins) + " " + FormatOpCode(ins) + " " + FormatOperands(ins);
        }

        public static string FormatRegister(Register r, MethodBody body)
        {
            if (body.IsComing(r))
            {
                int parameterIdx = r.Index - body.Registers.Count + body.IncomingArguments;
                return "p" + parameterIdx;
            }
            return "r" + r.Index;
        }

        public static string FormatOperands(Instruction ins, MethodBody body)
        {
            StringBuilder ops = new StringBuilder();

            foreach (var r in ins.Registers)
            {
                if (ops.Length > 0)
                {
                    ops.Append(",");
                    Align(ops, 4);
                }
                ops.Append(FormatRegister(r, body));
            }
            if (ops.Length == 0)
                ops.Append(" ");

            if (ins.Operand != null)
            {
                Align(ops, 12);

                if (ins.Operand is string)
                {
                    ops.Append("\"");
                    ops.Append(ins.Operand);
                    ops.Append("\"");
                }
                else if (ins.Operand is int)
                {
                    var i = (int)ins.Operand;
                    ops.Append(i);

                    if (i > 8 || i < -8)
                    {
                        ops.Append(" (0x");
                        ops.Append(i.ToString("X4"));
                        ops.Append(")");
                    }
                }
                else if (ins.Operand is long)
                {
                    var l = (long)ins.Operand;
                    ops.Append(l);

                    ops.Append(" (0x");
                    ops.Append(l.ToString("X8"));
                    ops.Append(")");
                }
                else if (ins.Operand is Instruction)
                {
                    var target = (Instruction)ins.Operand;
                    ops.Append("-> ");
                    ops.Append(target.Offset.ToString("X3"));

                    int targetIdx = body.Instructions.IndexOf(target);
                    int myIdx = body.Instructions.IndexOf(ins);
                    ops.Append(" ; ");

                    int offset = (targetIdx - myIdx);
                    ops.Append(offset.ToString("+0;-0;+0"));
                }
                else if (ins.Operand is IMemberReference)
                {
                    ops.Append(ins.Operand);
                }
                else
                {
                    ops.Append(ins.Operand);
                }
            }

            var bstrOperands = ops.ToString();
            return bstrOperands;
        }

        private static void Align(StringBuilder b, int tabSize)
        {
            int add = (tabSize - b.Length % tabSize) % tabSize;
            b.Append(' ', add);
        }

        /// <summary>
        /// Try to find the source code at offset
        /// </summary>
        /// <returns>null, if not found</returns>
        public SourceCodePosition FindSourceCode(int offset, bool allowSpecial=true)
        {
            return _mapFile.FindSourceCode(_methodEntry, offset, allowSpecial);
        }

        /// <summary>
        /// Beginning at offset, returns the next available source code position.
        /// Will not return positions with "IsSpecial" flag set.
        /// </summary>
        /// <returns>null, if not found</returns>
        public SourceCodePosition FindNextSourceCode(int offset)
        {
            return _mapFile.FindNextSourceCode(_methodEntry, offset);
        }

    }
}