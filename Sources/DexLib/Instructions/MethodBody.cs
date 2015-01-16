using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dot42.DexLib.IO;

namespace Dot42.DexLib.Instructions
{
    public class MethodBody
    {
        private readonly List<Instruction> instructions;
        private readonly List<ExceptionHandler> exceptions;
        private readonly List<Register> registers;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MethodBody(MethodDefinition method, int registersSize)
        {
            Owner = method;
            registers = new List<Register>();
            instructions = new List<Instruction>();
            exceptions = new List<ExceptionHandler>();
            for (var i = 0; i < registersSize; i++)
            {
                Registers.Add(new Register(i));
            }
        }

        public DebugInfo DebugInfo { get; set; }
        public List<Register> Registers
        {
            get { return registers; }
        }

        public List<Instruction> Instructions
        {
            get { return instructions; }
        }

        public List<ExceptionHandler> Exceptions
        {
            get { return exceptions; }
        }

        public ushort IncomingArguments { get; set; }
        public ushort OutgoingArguments { get; set; }
        public MethodDefinition Owner { get; set; }

        /// <summary>
        /// Is the given register an incoming register?
        /// </summary>
        public bool IsComing(Register reg)
        {
            return (reg.Index >= (registers.Count - IncomingArguments));
        }

        internal static void CheckArrayData(Instruction ins, out Array elements, out Type elementtype,
                                            out int elementsize)
        {
            if (!(ins.Operand is Array) || (ins.Operand as Array).Length == 0)
                throw new InstructionException(ins, "Expecting non empty Array");

            elements = ins.Operand as Array;
            elementtype = elements.GetValue(0).GetType();
            elementsize = Marshal.SizeOf(elementtype);

            if (!(elementtype == typeof (sbyte)
                  || elementtype == typeof (short)
                  || elementtype == typeof (int)
                  || elementtype == typeof (long)))
            {
                throw new InstructionException(ins, "Expecting sbyte/short/int/long element type");
            }
        }

        public OffsetStatistics UpdateInstructionOffsets()
        {
            int ip = 0;
            int extra = 0;

            foreach (Instruction ins in Instructions)
            {
                ins.Offset = ip;
                switch (ins.OpCode)
                {
                    case OpCodes.Nop:
                    case OpCodes.Return_void:
                        ip++;
                        break;
                    case OpCodes.Move_result:
                    case OpCodes.Move_result_wide:
                    case OpCodes.Move_result_object:
                    case OpCodes.Move_exception:
                    case OpCodes.Return:
                    case OpCodes.Return_wide:
                    case OpCodes.Return_object:
                    case OpCodes.Monitor_enter:
                    case OpCodes.Monitor_exit:
                    case OpCodes.Throw:
                        // vAA
                        ip += 1;
                        break;
                    case OpCodes.Move_object:
                    case OpCodes.Move_wide:
                    case OpCodes.Move:
                    case OpCodes.Array_length:
                    case OpCodes.Neg_int:
                    case OpCodes.Not_int:
                    case OpCodes.Neg_long:
                    case OpCodes.Not_long:
                    case OpCodes.Neg_float:
                    case OpCodes.Neg_double:
                    case OpCodes.Int_to_long:
                    case OpCodes.Int_to_float:
                    case OpCodes.Int_to_double:
                    case OpCodes.Long_to_int:
                    case OpCodes.Long_to_float:
                    case OpCodes.Long_to_double:
                    case OpCodes.Float_to_int:
                    case OpCodes.Float_to_long:
                    case OpCodes.Float_to_double:
                    case OpCodes.Double_to_int:
                    case OpCodes.Double_to_long:
                    case OpCodes.Double_to_float:
                    case OpCodes.Int_to_byte:
                    case OpCodes.Int_to_char:
                    case OpCodes.Int_to_short:
                    case OpCodes.Add_int_2addr:
                    case OpCodes.Sub_int_2addr:
                    case OpCodes.Mul_int_2addr:
                    case OpCodes.Div_int_2addr:
                    case OpCodes.Rem_int_2addr:
                    case OpCodes.And_int_2addr:
                    case OpCodes.Or_int_2addr:
                    case OpCodes.Xor_int_2addr:
                    case OpCodes.Shl_int_2addr:
                    case OpCodes.Shr_int_2addr:
                    case OpCodes.Ushr_int_2addr:
                    case OpCodes.Add_long_2addr:
                    case OpCodes.Sub_long_2addr:
                    case OpCodes.Mul_long_2addr:
                    case OpCodes.Div_long_2addr:
                    case OpCodes.Rem_long_2addr:
                    case OpCodes.And_long_2addr:
                    case OpCodes.Or_long_2addr:
                    case OpCodes.Xor_long_2addr:
                    case OpCodes.Shl_long_2addr:
                    case OpCodes.Shr_long_2addr:
                    case OpCodes.Ushr_long_2addr:
                    case OpCodes.Add_float_2addr:
                    case OpCodes.Sub_float_2addr:
                    case OpCodes.Mul_float_2addr:
                    case OpCodes.Div_float_2addr:
                    case OpCodes.Rem_float_2addr:
                    case OpCodes.Add_double_2addr:
                    case OpCodes.Sub_double_2addr:
                    case OpCodes.Mul_double_2addr:
                    case OpCodes.Div_double_2addr:
                    case OpCodes.Rem_double_2addr:
                        // vA, vB
                        ip += 1;
                        break;
                    case OpCodes.Move_wide_from16:
                    case OpCodes.Move_from16:
                    case OpCodes.Move_object_from16:
                        // vAA, vBBBB
                        ip += 2;
                        break;
                    case OpCodes.Move_16:
                    case OpCodes.Move_wide_16:
                    case OpCodes.Move_object_16:
                        // vAAAA, vBBBB
                        ip += 3;
                        break;
                    case OpCodes.Const_4:
                        // vA, #+B
                        ip++;
                        break;
                    case OpCodes.Const_16:
                    case OpCodes.Const_wide_16:
                        // vAA, #+BBBB
                        ip += 2;
                        break;
                    case OpCodes.Const:
                    case OpCodes.Const_wide_32:
                        // vAA, #+BBBBBBBB
                        ip += 3;
                        break;
                    case OpCodes.Fill_array_data:
                        // vAA, #+BBBBBBBB
                        ip += 3;

                        Array elements;
                        Type elementtype;
                        int elementsize;
                        CheckArrayData(ins, out elements, out elementtype, out elementsize);

                        extra = AlignOn2Units(extra + (elements.Length*elementsize + 1)/2 + 4);
                        break;
                    case OpCodes.Const_high16:
                        // vAA, #+BBBB0000
                        ip += 2;
                        break;
                    case OpCodes.Const_wide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        ip += 5;
                        break;
                    case OpCodes.Const_wide_high16:
                        // vAA, #+BBBB000000000000
                        ip += 2;
                        break;
                    case OpCodes.Const_string:
                        // vAA, string@BBBB
                        ip += 2;
                        break;
                    case OpCodes.Const_string_jumbo:
                        // vAA, string@BBBBBBBB
                        ip += 3;
                        break;
                    case OpCodes.Const_class:
                    case OpCodes.New_instance:
                    case OpCodes.Check_cast:
                        // vAA, type@BBBB
                        ip += 2;
                        break;
                    case OpCodes.Instance_of:
                    case OpCodes.New_array:
                        // vA, vB, type@CCCC
                        ip += 2;
                        break;
                    case OpCodes.Filled_new_array:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        ip += 3;
                        break;
                    case OpCodes.Filled_new_array_range:
                        // {vCCCC .. vNNNN}, type@BBBB
                        ip += 4;
                        break;
                    case OpCodes.Goto:
                        // +AA
                        ip += 1;
                        break;
                    case OpCodes.Goto_16:
                        // +AAAA
                        ip += 2;
                        break;
                    case OpCodes.Goto_32:
                        // +AAAAAAAA
                        ip += 3;
                        break;
                    case OpCodes.Packed_switch:
                        // vAA, +BBBBBBBB
                        if (!(ins.Operand is PackedSwitchData))
                            throw new InstructionException(ins, "Expecting PackedSwitchData");
                        var pdata = ins.Operand as PackedSwitchData;

                        ip += 3;
                        extra = AlignOn2Units(extra + (pdata.Targets.Count*2) + 4);
                        break;
                    case OpCodes.Sparse_switch:
                        // vAA, +BBBBBBBB
                        if (!(ins.Operand is SparseSwitchData))
                            throw new InstructionException(ins, "Expecting SparseSwitchData");
                        var sdata = ins.Operand as SparseSwitchData;

                        ip += 3;
                        extra = AlignOn2Units(extra + (sdata.Targets.Count*4) + 2);
                        break;
                    case OpCodes.Cmpl_float:
                    case OpCodes.Cmpg_float:
                    case OpCodes.Cmpl_double:
                    case OpCodes.Cmpg_double:
                    case OpCodes.Cmp_long:
                    case OpCodes.Aget:
                    case OpCodes.Aget_wide:
                    case OpCodes.Aget_object:
                    case OpCodes.Aget_boolean:
                    case OpCodes.Aget_byte:
                    case OpCodes.Aget_char:
                    case OpCodes.Aget_short:
                    case OpCodes.Aput:
                    case OpCodes.Aput_wide:
                    case OpCodes.Aput_object:
                    case OpCodes.Aput_boolean:
                    case OpCodes.Aput_byte:
                    case OpCodes.Aput_char:
                    case OpCodes.Aput_short:
                    case OpCodes.Add_int:
                    case OpCodes.Sub_int:
                    case OpCodes.Mul_int:
                    case OpCodes.Div_int:
                    case OpCodes.Rem_int:
                    case OpCodes.And_int:
                    case OpCodes.Or_int:
                    case OpCodes.Xor_int:
                    case OpCodes.Shl_int:
                    case OpCodes.Shr_int:
                    case OpCodes.Ushr_int:
                    case OpCodes.Add_long:
                    case OpCodes.Sub_long:
                    case OpCodes.Mul_long:
                    case OpCodes.Div_long:
                    case OpCodes.Rem_long:
                    case OpCodes.And_long:
                    case OpCodes.Or_long:
                    case OpCodes.Xor_long:
                    case OpCodes.Shl_long:
                    case OpCodes.Shr_long:
                    case OpCodes.Ushr_long:
                    case OpCodes.Add_float:
                    case OpCodes.Sub_float:
                    case OpCodes.Mul_float:
                    case OpCodes.Div_float:
                    case OpCodes.Rem_float:
                    case OpCodes.Add_double:
                    case OpCodes.Sub_double:
                    case OpCodes.Mul_double:
                    case OpCodes.Div_double:
                    case OpCodes.Rem_double:
                        // vAA, vBB, vCC
                        ip += 2;
                        break;
                    case OpCodes.If_eq:
                    case OpCodes.If_ne:
                    case OpCodes.If_lt:
                    case OpCodes.If_ge:
                    case OpCodes.If_gt:
                    case OpCodes.If_le:
                        // vA, vB, +CCCC
                        ip += 2;
                        break;
                    case OpCodes.If_eqz:
                    case OpCodes.If_nez:
                    case OpCodes.If_ltz:
                    case OpCodes.If_gez:
                    case OpCodes.If_gtz:
                    case OpCodes.If_lez:
                        // vAA, +BBBB
                        ip += 2;
                        break;
                    case OpCodes.Iget:
                    case OpCodes.Iget_wide:
                    case OpCodes.Iget_object:
                    case OpCodes.Iget_boolean:
                    case OpCodes.Iget_byte:
                    case OpCodes.Iget_char:
                    case OpCodes.Iget_short:
                    case OpCodes.Iput:
                    case OpCodes.Iput_wide:
                    case OpCodes.Iput_object:
                    case OpCodes.Iput_boolean:
                    case OpCodes.Iput_byte:
                    case OpCodes.Iput_char:
                    case OpCodes.Iput_short:
                        // vA, vB, field@CCCC
                        ip += 2;
                        break;
                    case OpCodes.Sget:
                    case OpCodes.Sget_wide:
                    case OpCodes.Sget_object:
                    case OpCodes.Sget_boolean:
                    case OpCodes.Sget_byte:
                    case OpCodes.Sget_char:
                    case OpCodes.Sget_short:
                    case OpCodes.Sput:
                    case OpCodes.Sput_wide:
                    case OpCodes.Sput_object:
                    case OpCodes.Sput_boolean:
                    case OpCodes.Sput_byte:
                    case OpCodes.Sput_char:
                    case OpCodes.Sput_short:
                        // vAA, field@BBBB
                        ip += 2;
                        break;
                    case OpCodes.Invoke_virtual:
                    case OpCodes.Invoke_super:
                    case OpCodes.Invoke_direct:
                    case OpCodes.Invoke_static:
                    case OpCodes.Invoke_interface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        ip += 3;
                        break;
                    case OpCodes.Invoke_virtual_range:
                    case OpCodes.Invoke_super_range:
                    case OpCodes.Invoke_direct_range:
                    case OpCodes.Invoke_static_range:
                    case OpCodes.Invoke_interface_range:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        ip += 3;
                        break;
                    case OpCodes.Add_int_lit16:
                    case OpCodes.Rsub_int:
                    case OpCodes.Mul_int_lit16:
                    case OpCodes.Div_int_lit16:
                    case OpCodes.Rem_int_lit16:
                    case OpCodes.And_int_lit16:
                    case OpCodes.Or_int_lit16:
                    case OpCodes.Xor_int_lit16:
                        // vA, vB, #+CCCC
                        ip += 2;
                        break;
                    case OpCodes.Add_int_lit8:
                    case OpCodes.Rsub_int_lit8:
                    case OpCodes.Mul_int_lit8:
                    case OpCodes.Div_int_lit8:
                    case OpCodes.Rem_int_lit8:
                    case OpCodes.And_int_lit8:
                    case OpCodes.Or_int_lit8:
                    case OpCodes.Xor_int_lit8:
                    case OpCodes.Shl_int_lit8:
                    case OpCodes.Shr_int_lit8:
                    case OpCodes.Ushr_int_lit8:
                        // vAA, vBB, #+CC
                        ip += 2;
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }
            }

            if (extra > 0)
            {
                ip = AlignOn2Units(ip);
            }

            return new OffsetStatistics {CodeUnits = ip, ExtraCodeUnits = extra};
        }

        /// <summary>
        /// Align the given value (upwards) to a multiple of 2.
        /// </summary>
        private static int AlignOn2Units(int value)
        {
            if ((value % 2) == 1)
                return value + 1;
            return value;
        }
    }
}