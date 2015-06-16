using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;

namespace Dot42.DebuggerLib
{
    [Flags]
    public enum FormatOptions
    {
        Default = ShowJumpTargets,
        EmbedSourceCode = 0x01,
        EmbedSourcePositions = 0x02,
        ShowControlFlow = 0x04,
        ShowJumpTargets = 0x08,
        DebugOperandTypes = 0x10,
        FullTypeNames = 0x20,
    }

    /// <summary>
    /// Provides a disassembled method and helps with formatting.
    /// </summary>
    // TODO: move to a place where it is available from ApkSpy as well,
    //       without ApkSpy needing a reference to the DebuggerLib.
    //       (maybe a DisassemblyLib?)
    public class MethodDisassembly
    {
        public static string JumpMarker { get { return "->"; } }

        private readonly TypeEntry _typeEntry;
        private readonly MethodEntry _methodEntry;
        private readonly MethodDefinition _methodDef;
        private readonly MapFileLookup _mapFile;

        public ClassDefinition Class { get { return _methodDef.Owner; } }
        public MethodDefinition Method { get { return _methodDef; } }
        public MethodEntry MethodEntry { get { return _methodEntry; } }
        public TypeEntry TypeEntry { get { return _typeEntry; } }

        public FormatOptions Format { get; set; }

        public ISet<int> JumpTargetOffsets { get; private set; }
        public ISet<int> ExceptionHandlerOffsets { get; private set; }


        public MethodDisassembly(MethodDefinition methodDef, MapFileLookup mapFile = null, TypeEntry typeEntry = null, MethodEntry methodEntry = null)
        {
            _typeEntry = typeEntry;
            _methodEntry = methodEntry;
            _methodDef = methodDef;
            _mapFile = mapFile;

            JumpTargetOffsets=new HashSet<int>();
            ExceptionHandlerOffsets = new HashSet<int>();

            if (methodDef.Body != null)
            {
                foreach (var i in methodDef.Body.Instructions)
                {
                    var op = i.Operand as Instruction;
                    if (op != null)
                        JumpTargetOffsets.Add(op.Offset);
                }
                foreach (var e in methodDef.Body.Exceptions)
                {
                    foreach(var c in e.Catches)
                        ExceptionHandlerOffsets.Add(c.Instruction.Offset);
                    if (e.CatchAll != null)
                        ExceptionHandlerOffsets.Add(e.CatchAll.Offset);
                }
            }

            Format = FormatOptions.Default;
        }

        public string FormatAddress(Instruction ins)
        {
            var offset = ins.Offset.ToString("X3").PadLeft(4);

            if(Format.HasFlag(FormatOptions.ShowJumpTargets))
            {
                if (ExceptionHandlerOffsets.Contains(ins.Offset))
                    return offset + "!";
                if (JumpTargetOffsets.Contains(ins.Offset))
                    return offset + ":";
                else 
                    return offset + " ";
            }
            return offset;    
        }

        public static string FormatOffset(int offset)
        {
            return offset.ToString("X3").PadLeft(4);
        }

        public string FormatOpCode(Instruction ins)
        {
            return OpCodesNames.GetName(ins.OpCode).ToLowerInvariant().PadLeft(20) + " ";
        }

        public string FormatOperands(Instruction ins)
        {
            return FormatOperands(ins, _methodDef.Body, Format);
        }

        public string FormatInstruction(Instruction ins)
        {
            return FormatAddress(ins) + " " + FormatOpCode(ins) + " " + FormatOperands(ins);
        }

        public string FormatRegister(Register r)
        {
            return FormatRegister(r, _methodDef.Body);
        }

        public string FormatRegister(int index)
        {
            return FormatRegister(new Register(index));
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

        public static string FormatOperands(Instruction ins, MethodBody body, FormatOptions options = FormatOptions.Default)
        {
            StringBuilder ops = new StringBuilder();

            bool fullTypeNames = options.HasFlag(FormatOptions.FullTypeNames);

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
                else if (ins.Operand is sbyte)
                {
                    FormatOperand_Integer(ops, (int)(byte)(sbyte)ins.Operand, "X2");
                }
                else if (ins.Operand is short)
                {
                    FormatOperand_Integer(ops, (int) (short) ins.Operand);
                }
                else if (ins.Operand is int)
                {
                    FormatOperand_Integer(ops, (int) ins.Operand);
                }
                else if (ins.Operand is long)
                {
                    var l = (long) ins.Operand;
                    ops.Append(l);

                    ops.Append(" (0x");
                    ops.Append(l.ToString("X8"));
                    ops.Append(")");
                }
                else if (ins.Operand is Instruction)
                {
                    var target = (Instruction) ins.Operand;
                    FormatOperand_Instruction(ops, ins, body, target, false);
                }
                else if (ins.Operand is ClassReference)
                {
                    var m = (ClassReference) ins.Operand;
                    ops.Append(fullTypeNames ? m.ToString() : m.Name);
                }
                else if (ins.Operand is MethodReference)
                {
                    var m = (MethodReference) ins.Operand;
                    var owner = fullTypeNames || !(m.Owner is ClassReference)
                        ? m.ToString()
                        : ((ClassReference) m.Owner).Name;
                    ops.Append(owner + "::" + m.Name + m.Prototype);
                }
                else if (ins.Operand is FieldReference)
                {
                    var m = (FieldReference) ins.Operand;
                    ops.Append(fullTypeNames ? m.ToString() : m.Owner.Name + "::" + m.Name + " : " + m.Type);
                }
                else if (ins.Operand is PackedSwitchData)
                {
                    var d = (PackedSwitchData) ins.Operand;
                    FormatOperand_Integer(ops, d.FirstKey);
                    ops.Append(":");
                    foreach (var target in d.Targets)
                    {
                        ops.Append(" ");
                        FormatOperand_Instruction(ops, ins, body, target, true);
                    }
                }
                else if (ins.Operand is SparseSwitchData)
                {
                    var d = (SparseSwitchData) ins.Operand;
                    bool isFirst = true;
                    foreach (var target in d.Targets)
                    {
                        if (!isFirst)
                            ops.Append(" ");
                        ops.Append(target.Key);
                        ops.Append(": ");
                        FormatOperand_Instruction(ops, ins, body, target.Value, true);
                        isFirst = false;
                    }
                }
                else
                {
                    ops.Append(ins.Operand);
                }

                if (options.HasFlag(FormatOptions.DebugOperandTypes))
                    ops.AppendFormat(" [{0}]", ins.Operand.GetType().Name);
            }

            var bstrOperands = ops.ToString();
            return bstrOperands;
        }

        private static void FormatOperand_Instruction(StringBuilder ops, Instruction ins, MethodBody body, Instruction target, bool compact)
        {
            ops.Append(JumpMarker);
            ops.Append(" ");
            ops.Append(target.Offset.ToString("X3"));

            
                int targetIdx = body.Instructions.IndexOf(target);
                int myIdx = body.Instructions.IndexOf(ins);

            ops.Append(!compact ? " ; " : "(");

            int offset = (targetIdx - myIdx);
            ops.Append(offset.ToString("+0;-0;+0"));

            if (compact)
                ops.Append(")");

        }

        private static void FormatOperand_Integer(StringBuilder ops, int i, string defaultHexFormat="X4")
        {
            ops.Append(i);

            if (i > 8 || i < -8)
            {
                ops.Append(" (0x");
                ops.Append(i.ToString(defaultHexFormat));
                ops.Append(")");
            }
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
            if (_mapFile == null || _methodEntry == null)
                return null;
            return _mapFile.FindSourceCode(_methodEntry, offset, allowSpecial);
        }

        /// <summary>
        /// Beginning at offset, returns the next available source code position.
        /// Will not return positions with "IsSpecial" flag set.
        /// </summary>
        /// <returns>null, if not found</returns>
        public SourceCodePosition FindNextSourceCode(int offset)
        {
            if (_mapFile == null || _methodEntry == null)
                return null;
            return _mapFile.FindNextSourceCode(_methodEntry, offset);
        }

    }
}