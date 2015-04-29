using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;
using NinjaTools.Collections;

namespace Dot42.DebuggerLib.Model
{
    public class MethodDisassembly
    {
        private readonly TypeEntry _typeEntry;
        private readonly MethodEntry _methodEntry;
        private readonly ClassDefinition _classDef;
        private readonly MethodDefinition _methodDef;
        private readonly MapFile _mapFile;
        private readonly Lazy<IList<Tuple<Document, DocumentPosition>>> _locations;

        public ClassDefinition Class { get { return _classDef; } }
        public MethodDefinition Method { get { return _methodDef; } }
        public MethodEntry MethodEntry { get { return _methodEntry; } }
        public TypeEntry TypeEntry { get { return _typeEntry; } }
        public IList<Tuple<Document, DocumentPosition>> Locations { get { return _locations.Value; } }

        public MethodDisassembly(TypeEntry typeEntry, ClassDefinition classDef, MethodEntry methodEntry, MethodDefinition methodDef, MapFile mapFile)
        {
            _typeEntry = typeEntry;
            _classDef = classDef;
            _methodEntry = methodEntry;
            _methodDef = methodDef;
            _mapFile = mapFile;
            _locations = new Lazy<IList<Tuple<Document, DocumentPosition>>>(() =>
                _mapFile.GetLocations(_typeEntry, _methodEntry)
                        .OrderBy(p => p.Item2.MethodOffset)
                        .ToList()
                        .AsReadOnly());
        }

        /// <summary>
        /// Returns the source code position at the specified method offset,
        /// or null of none.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Tuple<Document, DocumentPosition> GetSourceFromOffset(int offset)
        {
            var loc = Locations;

            int idx = loc.FindLastIndexSmallerThanOrEqualTo(offset, p => p.Item2.MethodOffset);
            if (idx != -1)
                return loc[idx];

            return null;
        }

        /// <summary>
        /// Beginning at offset, returns the next available position with source code.
        /// Will not return positions with "IsSpecial" flag set.
        /// </summary>
        public Tuple<Document, DocumentPosition> GetNextSourceFromOffset(int offset)
        {
            var loc = Locations;

            int idx = loc.FindFirstIndexGreaterThanOrEqualTo(offset, p => p.Item2.MethodOffset);
            while (idx != -1 && idx < loc.Count)
            {
                var ret = loc[idx];
                if (ret.Item2.IsSpecial)
                    continue;
                return ret;
            }

            return null;
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
    }
}