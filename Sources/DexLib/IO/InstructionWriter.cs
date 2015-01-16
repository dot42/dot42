using System;
using System.Collections.Generic;
using System.IO;
using Dot42.DexLib.Instructions;

namespace Dot42.DexLib.IO
{
    internal class InstructionWriter
    {
        private int ExtraOffset;
        private int Ip;

        private readonly DexWriter dexWriter;
        private readonly MethodDefinition methodDefinition;
        internal ushort[] Codes { get; set; }

        internal Dictionary<Instruction, int> LookupLast; // ending offsets by instruction

        public InstructionWriter(DexWriter dexWriter, MethodDefinition method)
        {
            this.dexWriter = dexWriter;
            methodDefinition = method;
            LookupLast = new Dictionary<Instruction, int>();
            Ip = 0;
            ExtraOffset = 0;
        }

        public void WriteTo(BinaryWriter writer)
        {
            var stats = methodDefinition.Body.UpdateInstructionOffsets();
            ExtraOffset = stats.CodeUnits;
            Codes = new ushort[stats.CodeUnits + stats.ExtraCodeUnits];
            // Fill initialy with nop's.
            for (var i = 0; i < Codes.Length; i++)
                Codes[i] = (ushort) OpCodes.Nop;
            int registerMask = 0;

            foreach (Instruction ins in methodDefinition.Body.Instructions)
            {
                if (Ip != ins.Offset)
                    throw new InstructionException(ins, "Instruction pointer do not match");

                Codes[Ip] = (ushort) ins.OpCode;
                switch (ins.OpCode)
                {
                    case OpCodes.Nop:
                    case OpCodes.Return_void:
                        Ip++;
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
                        WritevAA(ins);
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
                        WritevA(ins);
                        WritevB(ins);
                        break;
                    case OpCodes.Move_wide_from16:
                    case OpCodes.Move_from16:
                    case OpCodes.Move_object_from16:
                        // vAA, vBBBB
                        WritevAA(ins);
                        WritevBBBB(ins);
                        break;
                    case OpCodes.Move_16:
                    case OpCodes.Move_wide_16:
                    case OpCodes.Move_object_16:
                        // vAAAA, vBBBB
                        Ip++;
                        WritevAAAA(ins);
                        WritevBBBB(ins);
                        break;
                    case OpCodes.Const_4:
                        // vA, #+B
                        WritevA(ins);
                        WriteNibble(ins);
                        break;
                    case OpCodes.Const_16:
                    case OpCodes.Const_wide_16:
                        // vAA, #+BBBB
                        WritevAA(ins);
                        WriteShort(ins);
                        break;
                    case OpCodes.Const:
                    case OpCodes.Const_wide_32:
                        // vAA, #+BBBBBBBB
                        WritevAA(ins);
                        WriteInt(ins);
                        break;
                    case OpCodes.Fill_array_data:
                        // vAA, #+BBBBBBBB
                        WritevAA(ins);
                        WriteInt(ExtraOffset - ins.Offset, ref Ip);
                        WriteArrayData(ins);
                        break;
                    case OpCodes.Const_high16:
                        // vAA, #+BBBB0000
                        WritevAA(ins);
                        WriteShort(ins, 16);
                        break;
                    case OpCodes.Const_wide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        WritevAA(ins);
                        WriteLong(ins);
                        break;
                    case OpCodes.Const_wide_high16:
                        // vAA, #+BBBB000000000000
                        WritevAA(ins);
                        WriteShort(ins, 48);
                        break;
                    case OpCodes.Const_string:
                        // vAA, string@BBBB
                        WritevAA(ins);
                        WriteShortStringIndex(ins);
                        break;
                    case OpCodes.Const_string_jumbo:
                        // vAA, string@BBBBBBBB
                        WritevAA(ins);
                        WriteIntStringIndex(ins);
                        break;
                    case OpCodes.Const_class:
                    case OpCodes.New_instance:
                    case OpCodes.Check_cast:
                        // vAA, type@BBBB
                        WritevAA(ins);
                        WriteShortTypeIndex(ins);
                        break;
                    case OpCodes.Instance_of:
                    case OpCodes.New_array:
                        // vA, vB, type@CCCC
                        WritevA(ins);
                        WritevB(ins);
                        WriteShortTypeIndex(ins);
                        break;
                    case OpCodes.Filled_new_array:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        registerMask = GetRegisterMask(ins);
                        Codes[Ip++] |= (ushort) (registerMask >> 16 << 8);
                        WriteShortTypeIndex(ins);
                        Codes[Ip++] |= (ushort) (registerMask << 12 >> 12);
                        break;
                    case OpCodes.Filled_new_array_range:
                        // {vCCCC .. vNNNN}, type@BBBB
                        /*registerCount = Upper[Ip++] << 16;
                        ins.Operand = Dex.TypeReferences[ReadShort(ref Ip)];
                        ReadvBBBB(ins);
                        for (int i = 1; i < registerCount; i++)
                            ins.Registers.Add(registers[i + ins.Registers[0].Index]);*/
                        throw new NotImplementedException();
                    case OpCodes.Goto:
                        // +AA
                        WriteSbyteInstructionOffset(ins);
                        break;
                    case OpCodes.Goto_16:
                        // +AAAA
                        Ip++;
                        WriteShortInstructionOffset(ins);
                        break;
                    case OpCodes.Goto_32:
                        // +AAAAAAAA
                        Ip++;
                        WriteIntInstructionOffset(ins);
                        break;
                    case OpCodes.Packed_switch:
                        // vAA, +BBBBBBBB
                        WritevAA(ins);
                        WriteInt(ExtraOffset - ins.Offset, ref Ip);
                        WritePackedSwitch(ins);
                        break;
                    case OpCodes.Sparse_switch:
                        // vAA, +BBBBBBBB
                        WritevAA(ins);
                        WriteInt(ExtraOffset - ins.Offset, ref Ip);
                        WriteSparseSwitch(ins);
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
                        WritevAA(ins);
                        WritevBB(ins);
                        WritevCC(ins);
                        break;
                    case OpCodes.If_eq:
                    case OpCodes.If_ne:
                    case OpCodes.If_lt:
                    case OpCodes.If_ge:
                    case OpCodes.If_gt:
                    case OpCodes.If_le:
                        // vA, vB, +CCCC
                        WritevA(ins);
                        WritevB(ins);
                        WriteShortInstructionOffset(ins);
                        break;
                    case OpCodes.If_eqz:
                    case OpCodes.If_nez:
                    case OpCodes.If_ltz:
                    case OpCodes.If_gez:
                    case OpCodes.If_gtz:
                    case OpCodes.If_lez:
                        // vAA, +BBBB
                        WritevAA(ins);
                        WriteShortInstructionOffset(ins);
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
                        WritevA(ins);
                        WritevB(ins);
                        WriteShortFieldIndex(ins);
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
                        WritevAA(ins);
                        WriteShortFieldIndex(ins);
                        break;
                    case OpCodes.Invoke_virtual:
                    case OpCodes.Invoke_super:
                    case OpCodes.Invoke_direct:
                    case OpCodes.Invoke_static:
                    case OpCodes.Invoke_interface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        registerMask = GetRegisterMask(ins);
                        Codes[Ip++] |= (ushort) (registerMask >> 16 << 8);
                        WriteShortMethodIndex(ins);
                        Codes[Ip++] |= (ushort) (registerMask << 12 >> 12);
                        break;
                    case OpCodes.Invoke_virtual_range:
                    case OpCodes.Invoke_super_range:
                    case OpCodes.Invoke_direct_range:
                    case OpCodes.Invoke_static_range:
                    case OpCodes.Invoke_interface_range:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        WriteSByte(ins.Registers.Count, ref Ip);
                        WriteShortMethodIndex(ins);
                        Codes[Ip++] |= (ushort) CheckRegister(ins, 0, 0xFFFF);
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
                        WritevA(ins);
                        WritevB(ins);
                        WriteShort(ins);
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
                        WritevAA(ins);
                        WritevBB(ins);
                        WriteSByte(ins);
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }

                LookupLast.Add(ins, Ip - 1);
            }

            if ((stats.ExtraCodeUnits > 0) && ((Ip % 2) != 0))
            {
                // Align extra data
                Ip++;
            }

            if (Ip != stats.CodeUnits)
                throw new MalformedException("Instruction pointer out of range");

            if (ExtraOffset != stats.CodeUnits + stats.ExtraCodeUnits)
                throw new MalformedException("Data pointer out of range");

            writer.Write(ExtraOffset);
            for (int i = 0; i < ExtraOffset; i++)
                writer.Write(Codes[i]);
        }

        private int GetRegisterMask(Instruction ins)
        {
            int registerCount = ins.Registers.Count;
            int registerMask = registerCount << 20;

            for (int i = 0; i < registerCount; i++)
                registerMask |= CheckRegister(ins, i, 0xF) << (i*4);

            return registerMask;
        }

        #region " Nibble "

        private void WriteNibble(Instruction ins)
        {
            Codes[Ip++] |= (ushort) ((int) ins.Operand << 12);
        }

        #endregion

        #region " SByte "

        private void WriteSByte(object value, ref int codeUnitOffset)
        {
            Codes[Ip++] |= (ushort) (Convert.ToSByte(value) << 8);
        }

        private void WriteSbyteInstructionOffset(Instruction ins)
        {
            if (!(ins.Operand is Instruction))
                throw new InstructionException(ins, "Expecting Instruction");

            WriteSByte((ins.Operand as Instruction).Offset - ins.Offset, ref Ip);
        }

        private void WriteSByte(Instruction ins)
        {
            WriteSByte(ins.Operand, ref Ip);
        }

        #endregion

        #region " Short "

        private void WriteShortInstructionOffset(Instruction ins)
        {
            if (!(ins.Operand is Instruction))
                throw new InstructionException(ins, "Expecting Instruction");

            WriteShort((ins.Operand as Instruction).Offset - ins.Offset, ref Ip);
        }

        private void WriteShortFieldIndex(Instruction ins)
        {
            if (!(ins.Operand is FieldReference))
                throw new InstructionException(ins, "Expecting FieldReference");

            WriteShort(dexWriter.LookupFieldId(ins.Operand as FieldReference), ref Ip);
        }

        private void WriteShortMethodIndex(Instruction ins)
        {
            if (!(ins.Operand is MethodReference))
                throw new InstructionException(ins, "Expecting MethodReference");

            WriteShort(dexWriter.LookupMethodId(ins.Operand as MethodReference), ref Ip);
        }

        private void WriteShortStringIndex(Instruction ins)
        {
            if (!(ins.Operand is String))
                throw new InstructionException(ins, "Expecting String");

            WriteShort(dexWriter.LookupStringId(ins.Operand as String), ref Ip);
        }

        private void WriteShortTypeIndex(Instruction ins)
        {
            if (!(ins.Operand is TypeReference))
                throw new InstructionException(ins, "Expecting TypeReference");

            WriteShort(dexWriter.LookupTypeId(ins.Operand as TypeReference), ref Ip);
        }

        private void WriteShort(Instruction ins, int shift)
        {
            long value = Convert.ToInt64(ins.Operand) >> shift;
            WriteShort(value, ref Ip);
        }

        private void WriteShort(Instruction ins)
        {
            WriteShort(ins.Operand, ref Ip);
        }

        private void WriteShort(object value, ref int codeUnitOffset)
        {
            Codes[codeUnitOffset++] = (ushort) Convert.ToInt16(value);
        }

        #endregion

        #region " Int "

        private void WriteIntInstructionOffset(Instruction ins)
        {
            if (!(ins.Operand is Instruction))
                throw new InstructionException(ins, "Expecting Instruction");

            WriteInt((ins.Operand as Instruction).Offset - ins.Offset, ref Ip);
        }

        private void WriteIntStringIndex(Instruction ins)
        {
            throw new System.NotImplementedException();
        }

        private void WriteInt(object value, ref int codeUnitOffset)
        {
            int result;
            if (value is float)
            {
                var bytes = BitConverter.GetBytes((float) value);
                result = BitConverter.ToInt32(bytes, 0);
            }
            else if (value is int)
            {
                result = (int)value;
            }
            else if (value is long)
            {
                result = Convert.ToInt32((long) value);
            }
            else
            {
                throw new ArgumentException("Unknown value " + value);
            }
            Codes[codeUnitOffset++] = (ushort) (result & 0xffff);
            Codes[codeUnitOffset++] = (ushort) (result >> 16);
        }

        private void WriteInt(Instruction ins)
        {
            WriteInt(ins.Operand, ref Ip);
        }

        #endregion

        #region " Long "

        private void WriteLong(object value, ref int codeUnitOffset)
        {
            long result;
            if (value is double)
            {
                result = BitConverter.DoubleToInt64Bits((double) value);
            }
            else if (value is long)
            {
                result = (long) value;
            } else
            {
                throw new ArgumentException("Unknown value " + value);
            }
            Codes[codeUnitOffset++] = (ushort) (result & 0xffff);
            Codes[codeUnitOffset++] = (ushort) ((result >> 16) & 0xffff);
            Codes[codeUnitOffset++] = (ushort) ((result >> 32) & 0xffff);
            Codes[codeUnitOffset++] = (ushort) (result >> 48);
        }

        private void WriteLong(Instruction ins)
        {
            WriteLong(ins.Operand, ref Ip);
        }

        #endregion

        #region " Pseudo OpCodes "

        private void WriteSparseSwitch(Instruction ins)
        {
            if (!(ins.Operand is SparseSwitchData))
                throw new InstructionException(ins, "Expecting SparseSwitchData");
            var data = ins.Operand as SparseSwitchData;

            WriteShort((short) PseudoOpCodes.Sparse_switch, ref ExtraOffset);
            WriteShort(data.Targets.Count, ref ExtraOffset);

            foreach (int key in data.Targets.Keys)
                WriteInt(key, ref ExtraOffset);

            foreach (int key in data.Targets.Keys)
                WriteInt(data.Targets[key].Offset - ins.Offset, ref ExtraOffset);
        }

        private void WritePackedSwitch(Instruction ins)
        {
            if (!(ins.Operand is PackedSwitchData))
                throw new InstructionException(ins, "Expecting PackedSwitchData");
            var data = ins.Operand as PackedSwitchData;

            WriteShort((short) PseudoOpCodes.Packed_switch, ref ExtraOffset);
            WriteShort(data.Targets.Count, ref ExtraOffset);
            WriteInt(data.FirstKey, ref ExtraOffset);

            foreach (Instruction target in data.Targets)
                WriteInt(target.Offset - ins.Offset, ref ExtraOffset);
        }

        private void WriteArrayData(Instruction ins)
        {
            Array elements;
            Type elementtype;
            int elementsize;
            MethodBody.CheckArrayData(ins, out elements, out elementtype, out elementsize);

            WriteShort(PseudoOpCodes.Fill_array_data, ref ExtraOffset);
            WriteShort(elementsize, ref ExtraOffset);
            WriteInt(elements.Length, ref ExtraOffset);

            var firstByte = true;
            foreach (object element in elements)
            {
                switch (elementsize)
                {
                    case 1:
                        if (firstByte)
                            Codes[ExtraOffset++] |= (ushort) ((byte) Convert.ToSByte(element));
                        else
                            Codes[ExtraOffset - 1] |= (ushort)((byte)(Convert.ToSByte(element)) << 8);
                        firstByte = !firstByte;
                        break;
                    case 2:
                        WriteShort(element, ref ExtraOffset);
                        break;
                    case 4:
                        WriteInt(element, ref ExtraOffset);
                        break;
                    case 8:
                        WriteLong(element, ref ExtraOffset);
                        break;
                    default:
                        throw new InstructionException(ins, "Unexpected Fill-array-data element size");
                }
            }

            // Align on 4 bytes (2 units)
            if (ExtraOffset % 2 != 0)
                ExtraOffset++;
        }

        #endregion

        #region " Registers "

        private void WritevA(Instruction ins)
        {
            Codes[Ip] |= (ushort) (CheckRegister(ins, 0, 0xF) << 8);
        }

        private void WritevAA(Instruction ins)
        {
            Codes[Ip++] |= (ushort) (CheckRegister(ins, 0, 0xFF) << 8);
        }

        private void WritevAAAA(Instruction ins)
        {
            Codes[Ip++] |= (ushort)CheckRegister(ins, 0, 0xFFFF);
        }

        private void WritevB(Instruction ins)
        {
            Codes[Ip++] |= (ushort) (CheckRegister(ins, 1, 0xF) << 12);
        }

        private void WritevBB(Instruction ins)
        {
            Codes[Ip] |= (ushort) CheckRegister(ins, 1, 0xFF);
        }

        private void WritevBBBB(Instruction ins)
        {
            Codes[Ip++] |= (ushort) CheckRegister(ins, 1, 0xFFFF);
        }

        private void WritevCC(Instruction ins)
        {
            Codes[Ip++] |= (ushort) (CheckRegister(ins, 2, 0xFF) << 8);
        }

        private int CheckRegister(Instruction ins, int position, int maxIndex)
        {
            if (ins.Registers.Count <= position)
                throw new InstructionException(ins, string.Format("Expecting register at position {0}", position));

            int index = ins.Registers[position].Index;
            if (index < 0 || index > maxIndex)
                throw new InstructionException(ins, string.Format("Register index out of range [0..{0}]", maxIndex));

            return index;
        }

        #endregion
    }
}