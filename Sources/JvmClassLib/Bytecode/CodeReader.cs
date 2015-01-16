using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Bytecode
{
    internal class CodeReader
    {
        private readonly ConstantPool cp;
        private readonly MethodDefinition method;
        private readonly CodeAttribute codeAttribute;
        private readonly byte[] code;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal CodeReader(MethodDefinition method, CodeAttribute codeAttribute, byte[] code, ConstantPool cp)
        {
            this.method = method;
            this.codeAttribute = codeAttribute;
            this.code = code;
            this.cp = cp;
        }

        /// <summary>
        /// Read the entire code.
        /// </summary>
        public List<Instruction> Read()
        {
            var instructions = Parse();
            Resolve(instructions);
            return instructions;
        }

        /// <summary>
        /// Resolve all jump offsets
        /// </summary>
        private void Resolve(List<Instruction> instructions)
        {
            var lnTable = codeAttribute.Attributes.OfType<LineNumberTableAttribute>().FirstOrDefault();
            foreach (var ins in instructions)
            {
                var resolveable = ins.Operand as IResolveable;
                if (resolveable != null)
                {
                    ins.Operand = resolveable.Resolve(instructions, ins);
                }
                if (lnTable != null)
                {
                    ins.LineNumber = lnTable.GetLineNumber(ins.Offset);
                }
            }
        }

        /// <summary>
        /// Parse the entire byte array into instructions.
        /// </summary>
        private List<Instruction> Parse()
        {
            var instructions = new List<Instruction>();
            for (var offset = 0; offset < code.Length; )
            {
                var startOffset = offset;
                var opcode = (Code) code[offset++];
                object operand = null;
                object operand2 = null;
                switch (opcode)
                {
                    case Code.AALOAD:
                    case Code.AASTORE:
                    case Code.ACONST_NULL:
                    case Code.ARETURN:
                    case Code.ARRAYLENGTH:
                    case Code.ATHROW:
                    case Code.BALOAD:
                    case Code.BASTORE:
                    case Code.CALOAD:
                    case Code.CASTORE:
                    case Code.D2F:
                    case Code.D2I:
                    case Code.D2L:
                    case Code.DADD:
                    case Code.DALOAD:
                    case Code.DASTORE:
                    case Code.DCMPG:
                    case Code.DCMPL:
                    case Code.DDIV:
                    case Code.DMUL:
                    case Code.DNEG:
                    case Code.DREM:
                    case Code.DRETURN:
                    case Code.DSUB:
                    case Code.DUP:
                    case Code.DUP_X1:
                    case Code.DUP_X2:
                    case Code.DUP2:
                    case Code.DUP2_X1:
                    case Code.DUP2_X2:
                    case Code.F2D:
                    case Code.F2I:
                    case Code.F2L:
                    case Code.FADD:
                    case Code.FALOAD:
                    case Code.FASTORE:
                    case Code.FCMPG:
                    case Code.FCMPL:
                    case Code.FDIV:
                    case Code.FMUL:
                    case Code.FNEG:
                    case Code.FREM:
                    case Code.FRETURN:
                    case Code.FSUB:
                    case Code.I2B:
                    case Code.I2C:
                    case Code.I2D:
                    case Code.I2F:
                    case Code.I2L:
                    case Code.I2S:
                    case Code.IADD:
                    case Code.IALOAD:
                    case Code.IAND:
                    case Code.IASTORE:
                    case Code.IDIV:
                    case Code.IMUL:
                    case Code.INEG:
                    case Code.IOR:
                    case Code.IREM:
                    case Code.IRETURN:
                    case Code.ISHL:
                    case Code.ISHR:
                    case Code.ISUB:
                    case Code.IUSHR:
                    case Code.IXOR:
                    case Code.L2D:
                    case Code.L2F:
                    case Code.L2I:
                    case Code.LADD:
                    case Code.LALOAD:
                    case Code.LAND:
                    case Code.LASTORE:
                    case Code.LCMP:
                    case Code.LDIV:
                    case Code.LMUL:
                    case Code.LNEG:
                    case Code.LOR:
                    case Code.LREM:
                    case Code.LRETURN:
                    case Code.LSHL:
                    case Code.LSHR:
                    case Code.LSUB:
                    case Code.LUSHR:
                    case Code.LXOR:
                    case Code.MONITORENTER:
                    case Code.MONITOREXIT:
                    case Code.NOP:
                    case Code.POP:
                    case Code.POP2:
                    case Code.RETURN:
                    case Code.SALOAD:
                    case Code.SASTORE:
                    case Code.SWAP:
                        // Single byte
                        break;
                    case Code.ALOAD:
                    case Code.ASTORE:
                    case Code.DLOAD:
                    case Code.DSTORE:
                    case Code.FLOAD:
                    case Code.FSTORE:
                    case Code.ILOAD:
                    case Code.ISTORE:
                    case Code.LLOAD:
                    case Code.LSTORE:
                        // Single U8 operand
                        operand = GetLocalVariable(startOffset, ReadU8(ref offset));
                        break;
                    case Code.ALOAD_0:
                    case Code.ALOAD_1:
                    case Code.ALOAD_2:
                    case Code.ALOAD_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.ALOAD_0);
                        break;
                    case Code.ANEWARRAY:
                    case Code.CHECKCAST:
                    case Code.INSTANCEOF:
                    case Code.NEW:
                        // U16 class reference
                        operand = cp.GetEntry<ConstantPoolClass>(ReadU16(ref offset));
                        break;
                    case Code.ASTORE_0:
                    case Code.ASTORE_1:
                    case Code.ASTORE_2:
                    case Code.ASTORE_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.ASTORE_0);
                        break;
                    case Code.BIPUSH:
                        operand = ReadS8(ref offset);
                        break;
                    case Code.DCONST_0:
                        operand = 0.0;
                        break;
                    case Code.DCONST_1:
                        operand = 1.0;
                        break;
                    case Code.DLOAD_0:
                    case Code.DLOAD_1:
                    case Code.DLOAD_2:
                    case Code.DLOAD_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.DLOAD_0);
                        break;
                    case Code.DSTORE_0:
                    case Code.DSTORE_1:
                    case Code.DSTORE_2:
                    case Code.DSTORE_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.DSTORE_0);
                        break;
                    case Code.FCONST_0:
                        operand = 0.0f;
                        break;
                    case Code.FCONST_1:
                        operand = 1.0f;
                        break;
                    case Code.FCONST_2:
                        operand = 2.0f;
                        break;
                    case Code.FLOAD_0:
                    case Code.FLOAD_1:
                    case Code.FLOAD_2:
                    case Code.FLOAD_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.FLOAD_0);
                        break;
                    case Code.FSTORE_0:
                    case Code.FSTORE_1:
                    case Code.FSTORE_2:
                    case Code.FSTORE_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.FSTORE_0);
                        break;
                    case Code.GETFIELD:
                    case Code.GETSTATIC:
                    case Code.PUTFIELD:
                    case Code.PUTSTATIC:
                        // U16 field reference
                        operand = cp.GetEntry<ConstantPoolFieldRef>(ReadU16(ref offset));
                        break;
                    case Code.GOTO:
                    case Code.IF_ACMPEQ:
                    case Code.IF_ACMPNE:
                    case Code.IF_ICMPEQ:
                    case Code.IF_ICMPNE:
                    case Code.IF_ICMPLT:
                    case Code.IF_ICMPGE:
                    case Code.IF_ICMPGT:
                    case Code.IF_ICMPLE:
                    case Code.IFEQ:
                    case Code.IFNE:
                    case Code.IFLT:
                    case Code.IFGE:
                    case Code.IFGT:
                    case Code.IFLE:
                    case Code.IFNONNULL:
                    case Code.IFNULL:
                    case Code.JSR:
                        operand = new BranchOffset(ReadS16(ref offset));
                        break;
                    case Code.GOTO_W:
                    case Code.JSR_W:
                        operand = new BranchOffset(ReadS32(ref offset));
                        break;
                    case Code.ICONST_M1:
                    case Code.ICONST_0:
                    case Code.ICONST_1:
                    case Code.ICONST_2:
                    case Code.ICONST_3:
                    case Code.ICONST_4:
                    case Code.ICONST_5:
                        operand = (int)(opcode - Code.ICONST_0);
                        break;
                    case Code.IINC:
                        operand = GetLocalVariable(startOffset, ReadU8(ref offset));
                        operand2 = ReadS8(ref offset);
                        break;
                    case Code.ILOAD_0:
                    case Code.ILOAD_1:
                    case Code.ILOAD_2:
                    case Code.ILOAD_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.ILOAD_0);
                        break;
                    case Code.INVOKEINTERFACE:
                        // U16 method/imethod reference
                        operand = cp[ReadU16(ref offset)];
                        operand2 = ReadU8(ref offset);
                        offset++; // '0'
                        break;
                    case Code.INVOKESPECIAL:
                    case Code.INVOKESTATIC:
                    case Code.INVOKEVIRTUAL:
                        // U16 method/imethod reference
                        operand = cp[ReadU16(ref offset)];
                        break;
                    case Code.ISTORE_0:
                    case Code.ISTORE_1:
                    case Code.ISTORE_2:
                    case Code.ISTORE_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.ISTORE_0);
                        break;
                    case Code.LCONST_0:
                        operand = 0L;
                        break;
                    case Code.LCONST_1:
                        operand = 1L;
                        break;
                    case Code.LDC:
                        operand = cp[ReadU8(ref offset)];
                        break;
                    case Code.LDC_W:
                    case Code.LDC2_W:
                        operand = cp[ReadU16(ref offset)];
                        break;
                    case Code.LLOAD_0:
                    case Code.LLOAD_1:
                    case Code.LLOAD_2:
                    case Code.LLOAD_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.LLOAD_0);
                        break;
                    case Code.LOOKUPSWITCH:
                        operand = ReadLookupSwitch(ref offset);
                        break;
                    case Code.LSTORE_0:
                    case Code.LSTORE_1:
                    case Code.LSTORE_2:
                    case Code.LSTORE_3:
                        operand = GetLocalVariable(startOffset, opcode - Code.LSTORE_0);
                        break;
                    case Code.MULTIANEWARRAY:
                        operand = cp.GetEntry<ConstantPoolClass>(ReadU16(ref offset));
                        operand2 = ReadU8(ref offset);
                        break;
                    case Code.NEWARRAY:
                        operand = ReadU8(ref offset);
                        break;
                    case Code.RET:
                        operand = GetLocalVariable(startOffset, ReadU8(ref offset));
                        break;
                    case Code.SIPUSH:
                        operand = ReadS16(ref offset);
                        break;
                    case Code.TABLESWITCH:
                        operand = ReadTableSwitch(ref offset);
                        break;
                    case Code.WIDE:
                        opcode = (Code) code[offset++];
                        operand = GetLocalVariable(startOffset, ReadU16(ref offset));
                        if (opcode == Code.IINC)
                        {
                            operand2 = ReadS16(ref offset);
                        }
                        break;
                }
                instructions.Add(new Instruction(startOffset, opcode, operand, operand2));
            }
            return instructions;
        }

        /// <summary>
        /// Read an unsigned byte from the next offset
        /// </summary>
        private int ReadU8(ref int offset)
        {
            return code[offset++];
        }

        /// <summary>
        /// Read an signed byte from the next offset
        /// </summary>
        private int ReadS8(ref int offset)
        {
            return (sbyte)code[offset++];
        }

        /// <summary>
        /// Read an unsigned short from the next offset
        /// </summary>
        private int ReadU16(ref int offset)
        {
            var v1 = code[offset++];
            var v2 = code[offset++];
            return (v1 << 8) | v2;
        }

        /// <summary>
        /// Read an signed short from the next offset
        /// </summary>
        private int ReadS16(ref int offset)
        {
            var v1 = (int)code[offset++];
            var v2 = (int)code[offset++];
            return (short)((v1 << 8) | v2);
        }

        /// <summary>
        /// Read an signed int from the next offset
        /// </summary>
        private int ReadS32(ref int offset)
        {
            var v1 = (int)code[offset++];
            var v2 = (int)code[offset++];
            var v3 = (int)code[offset++];
            var v4 = (int)code[offset++];
            return (v1 << 24) | (v2 << 16) | (v3 << 8) | v4;
        }

        /// <summary>
        /// Read a LOOKUPSWITCH payload
        /// </summary>
        private LookupSwitchData ReadLookupSwitch(ref int offset)
        {
            while ((offset % 4) != 0)
                offset++;
            var defByte = new BranchOffset(ReadS32(ref offset));
            var npairs = ReadS32(ref offset);

            var pairs = new List<LookupSwitchData.Pair>();
            for (var i = 0; i < npairs; i++)
            {
                var match = ReadS32(ref offset);
                var _offset = new BranchOffset(ReadS32(ref offset));
                pairs.Add(new LookupSwitchData.Pair(match, _offset));
            }
            return new LookupSwitchData(defByte, pairs.ToArray());
        }

        /// <summary>
        /// Read a TABLESWITCH payload
        /// </summary>
        private TableSwitchData ReadTableSwitch(ref int offset)
        {
            while ((offset % 4) != 0)
                offset++;
            var defByte = new BranchOffset(ReadS32(ref offset));
            var lowByte = ReadS32(ref offset);
            var highByte = ReadS32(ref offset);

            var offsets = new BranchOffset[highByte - lowByte + 1];
            for (var i = 0; i < offsets.Length; i++)
            {
                offsets[i] = new BranchOffset(ReadS32(ref offset));
            }
            return new TableSwitchData(defByte, lowByte, highByte, offsets);
        }

        /// <summary>
        /// Gets a local variable reference used in an instruction starting at the given offset, to a variable with given index in the stack frame.
        /// </summary>
        private LocalVariableReference GetLocalVariable(int instructionOffset, int index)
        {
            return codeAttribute.GetLocalVariable(instructionOffset, index);
        }
    }
}
