using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.JvmClassLib.Bytecode;
using Dot42.JvmClassLib.Structures;

namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        /// <summary>
        /// Create a bytecode from a java instruction.
        /// </summary>
        private IEnumerable<ByteCode> Create(Instruction inst, XModule module)
        {
            AstCode code;
            object operand;
            int popCount;
            int pushCount;
            Category category;

            switch (inst.Opcode)
            {
                case Code.NOP:
                    code = AstCode.Nop;
                    operand = null;
                    popCount = 0;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.ACONST_NULL:
                    code = AstCode.Ldnull;
                    operand = null;
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.ICONST_M1:
                case Code.ICONST_0:
                case Code.ICONST_1:
                case Code.ICONST_2:
                case Code.ICONST_3:
                case Code.ICONST_4:
                case Code.ICONST_5:
                    code = AstCode.Ldc_I4;
                    operand = (int)(inst.Opcode - Code.ICONST_0);
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LCONST_0:
                case Code.LCONST_1:
                    code = AstCode.Ldc_I8;
                    operand = (long)(inst.Opcode - Code.LCONST_0);
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.FCONST_0:
                case Code.FCONST_1:
                case Code.FCONST_2:
                    code = AstCode.Ldc_R4;
                    operand = (float)(inst.Opcode - Code.FCONST_0);
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.DCONST_0:
                case Code.DCONST_1:
                    code = AstCode.Ldc_R8;
                    operand = (double)(inst.Opcode - Code.DCONST_0);
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.BIPUSH:
                case Code.SIPUSH:
                    code = AstCode.Ldc_I4;
                    operand = (int)inst.Operand;
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LDC:
                case Code.LDC_W:
                case Code.LDC2_W:
                    popCount = 0;
                    pushCount = 1;
                    switch (((ConstantPoolEntry)inst.Operand).Tag)
                    {
                        case ConstantPoolTags.Integer:
                            code = AstCode.Ldc_I4;
                            operand = ((ConstantPoolInteger)inst.Operand).Value;
                            category = Category.Category1;
                            break;
                        case ConstantPoolTags.Float:
                            code = AstCode.Ldc_R4;
                            operand = ((ConstantPoolFloat)inst.Operand).Value;
                            category = Category.Category1;
                            break;
                        case ConstantPoolTags.Long:
                            code = AstCode.Ldc_I8;
                            operand = ((ConstantPoolLong)inst.Operand).Value;
                            category = Category.Category2;
                            break;
                        case ConstantPoolTags.Double:
                            code = AstCode.Ldc_R8;
                            operand = ((ConstantPoolDouble)inst.Operand).Value;
                            category = Category.Category2;
                            break;
                        case ConstantPoolTags.String:
                            code = AstCode.Ldstr;
                            operand = ((ConstantPoolString)inst.Operand).Value;
                            category = Category.Category1;
                            break;
                        case ConstantPoolTags.Class:
                            code = AstCode.LdClass;
                            operand = AsTypeReference(((ConstantPoolClass)inst.Operand).Name, XTypeUsageFlags.TypeOf);
                            category = Category.Category1;
                            break;
                        default:
                            throw new ArgumentException("Unknown LDC operand " + inst.Operand);
                    }
                    break;

                case Code.ILOAD:
                case Code.ILOAD_0:
                case Code.ILOAD_1:
                case Code.ILOAD_2:
                case Code.ILOAD_3:
                case Code.FLOAD:
                case Code.FLOAD_0:
                case Code.FLOAD_1:
                case Code.FLOAD_2:
                case Code.FLOAD_3:
                case Code.ALOAD:
                case Code.ALOAD_0:
                case Code.ALOAD_1:
                case Code.ALOAD_2:
                case Code.ALOAD_3:
                    code = AstCode.Ldloc;
                    operand = inst.Operand;
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LLOAD:
                case Code.LLOAD_0:
                case Code.LLOAD_1:
                case Code.LLOAD_2:
                case Code.LLOAD_3:
                case Code.DLOAD:
                case Code.DLOAD_0:
                case Code.DLOAD_1:
                case Code.DLOAD_2:
                case Code.DLOAD_3:
                    code = AstCode.Ldloc;
                    operand = inst.Operand;
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IALOAD:
                    code = AstCode.Ldelem_I4;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LALOAD:
                    code = AstCode.Ldelem_I8;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.FALOAD:
                    code = AstCode.Ldelem_R4;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.DALOAD:
                    code = AstCode.Ldelem_R8;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.AALOAD:
                    code = AstCode.Ldelem_Ref;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.BALOAD:
                    code = AstCode.Ldelem_I1;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.CALOAD:
                    code = AstCode.Ldelem_U2;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.SALOAD:
                    code = AstCode.Ldelem_I2;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.ISTORE:
                case Code.ISTORE_0:
                case Code.ISTORE_1:
                case Code.ISTORE_2:
                case Code.ISTORE_3:
                case Code.LSTORE:
                case Code.LSTORE_0:
                case Code.LSTORE_1:
                case Code.LSTORE_2:
                case Code.LSTORE_3:
                case Code.FSTORE:
                case Code.FSTORE_0:
                case Code.FSTORE_1:
                case Code.FSTORE_2:
                case Code.FSTORE_3:
                case Code.DSTORE:
                case Code.DSTORE_0:
                case Code.DSTORE_1:
                case Code.DSTORE_2:
                case Code.DSTORE_3:
                case Code.ASTORE:
                case Code.ASTORE_0:
                case Code.ASTORE_1:
                case Code.ASTORE_2:
                case Code.ASTORE_3:
                    code = AstCode.Stloc;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IASTORE:
                    code = AstCode.Stelem_I4;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.LASTORE:
                    code = AstCode.Stelem_I8;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.FASTORE:
                    code = AstCode.Stelem_R4;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.DASTORE:
                    code = AstCode.Stelem_R8;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.AASTORE:
                    code = AstCode.Stelem_Ref;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.BASTORE:
                    code = AstCode.Stelem_I1;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.CASTORE:
                    code = AstCode.Stelem_I2;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.SASTORE:
                    code = AstCode.Stelem_I2;
                    operand = null;
                    popCount = 3;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.POP:
                    code = AstCode.Pop;
                    operand = null;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.DUP:
                    code = AstCode.Dup;
                    operand = null;
                    popCount = 1;
                    pushCount = 2;
                    category = Category.Category1;
                    break;

                case Code.POP2:
                    code = AstCode.Pop2;
                    operand = null;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.DUP_X1:
                    code = AstCode.Dup_x1;
                    operand = null;
                    popCount = 1; // Value should match Dup. (which is different from the JVM spec, but works with our stack analisys)
                    pushCount = 2; 
                    category = Category.Category1;
                    break;

                case Code.DUP_X2:
                    code = AstCode.Dup_x2;
                    operand = null;
                    popCount = 1; // Value should match Dup. (which is different from the JVM spec, but works with our stack analisys)
                    pushCount = 2; 
                    category = Category.Unknown;
                    break;

                case Code.DUP2:
                    code = AstCode.Dup2;
                    operand = null;
                    popCount = 1; // Value should match Dup. (which is different from the JVM spec, but works with our stack analisys)
                    pushCount = 2; 
                    category = Category.Unknown;
                    break;

                case Code.DUP2_X1:
                    code = AstCode.Dup2_x1;
                    operand = null;
                    popCount = 1; // Value should match Dup. (which is different from the JVM spec, but works with our stack analisys)
                    pushCount = 2;
                    category = Category.Unknown;
                    break;

                case Code.DUP2_X2:
                    code = AstCode.Dup2_x2;
                    operand = null;
                    popCount = 1; // Value should match Dup. (which is different from the JVM spec, but works with our stack analisys)
                    pushCount = 2;
                    category = Category.Unknown;
                    break;

                case Code.SWAP:
                    code = AstCode.Swap;
                    operand = null;
                    popCount = 1; // Value should match Dup. (which is different from the JVM spec, but works with our stack analisys)
                    pushCount = 2; 
                    category = Category.Category1;
                    break;


                case Code.IADD:
                case Code.FADD:
                    code = AstCode.Add;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LADD:
                case Code.DADD:
                    code = AstCode.Add;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.ISUB:
                case Code.FSUB:
                    code = AstCode.Sub;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LSUB:
                case Code.DSUB:
                    code = AstCode.Sub;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IMUL:
                case Code.FMUL:
                    code = AstCode.Mul;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LMUL:
                case Code.DMUL:
                    code = AstCode.Mul;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IDIV:
                case Code.FDIV:
                    code = AstCode.Div;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LDIV:
                case Code.DDIV:
                    code = AstCode.Div;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IREM:
                case Code.FREM:
                    code = AstCode.Rem;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LREM:
                case Code.DREM:
                    code = AstCode.Rem;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.INEG:
                case Code.FNEG:
                    code = AstCode.Neg;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LNEG:
                case Code.DNEG:
                    code = AstCode.Neg;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.ISHL:
                    code = AstCode.Shl;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LSHL:
                    code = AstCode.Shl;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.ISHR:
                    code = AstCode.Shr;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LSHR:
                    code = AstCode.Shr;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IUSHR:
                    code = AstCode.Shr_Un;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LUSHR:
                    code = AstCode.Shr_Un;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IAND:
                    code = AstCode.And;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LAND:
                    code = AstCode.And;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IOR:
                    code = AstCode.Or;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LOR:
                    code = AstCode.Or;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IXOR:
                    code = AstCode.Xor;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LXOR:
                    code = AstCode.Xor;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.IINC:
                    {
                        var local = (LocalVariableReference) inst.Operand;
                        yield return CreateByteCode(inst, AstCode.Ldloc, local, 0, 1, Category.Category1);
                        yield return CreateByteCode(inst, AstCode.Ldc_I4, (int) inst.Operand2, 0, 1, Category.Category1);
                        yield return CreateByteCode(inst, AstCode.Add, null, 2, 1, Category.Category1);
                        yield return CreateByteCode(inst, AstCode.Stloc, local, 1, 0, Category.Unknown, module.TypeSystem.Int);
                        yield break;
                    }
                case Code.I2L:
                case Code.F2L:
                case Code.D2L:
                    code = AstCode.Conv_I8;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.I2F:
                case Code.L2F:
                case Code.D2F:
                    code = AstCode.Conv_R4;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.I2D:
                case Code.L2D:
                case Code.F2D:
                    code = AstCode.Conv_R8;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category2;
                    break;

                case Code.L2I:
                case Code.F2I:
                case Code.D2I:
                    code = AstCode.Conv_I4;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.I2B:
                    code = AstCode.Conv_I1;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.I2C:
                    code = AstCode.Conv_U2;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.I2S:
                    code = AstCode.Conv_I2;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.LCMP:
                    code = AstCode.CmpLong;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.FCMPL:
                case Code.DCMPL:
                    code = AstCode.CmpLFloat;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.FCMPG:
                case Code.DCMPG:
                    code = AstCode.CmpGFloat;
                    operand = null;
                    popCount = 2;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.IFEQ:
                    code = AstCode.BrIfEq;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IFNE:
                    code = AstCode.BrIfNe;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IFLT:
                    code = AstCode.BrIfLt;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IFGE:
                    code = AstCode.BrIfGe;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IFGT:
                    code = AstCode.BrIfGt;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IFLE:
                    code = AstCode.BrIfLe;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;


                case Code.IF_ICMPEQ:
                case Code.IF_ACMPEQ:
                    code = AstCode.__Beq;
                    operand = inst.Operand;
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IF_ICMPNE:
                case Code.IF_ACMPNE:
                    code = AstCode.__Bne_Un;
                    operand = inst.Operand;
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IF_ICMPLT:
                    code = AstCode.__Blt;
                    operand = inst.Operand;
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IF_ICMPGE:
                    code = AstCode.__Bge;
                    operand = inst.Operand;
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IF_ICMPGT:
                    code = AstCode.__Bgt;
                    operand = inst.Operand;
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IF_ICMPLE:
                    code = AstCode.__Ble;
                    operand = inst.Operand;
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.GOTO:
                case Code.GOTO_W:
                    code = AstCode.Br;
                    operand = inst.Operand;
                    popCount = 0;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.JSR:
                    throw new NotSupportedException("JSR");
                case Code.JSR_W:
                    throw new NotSupportedException("JSR_W");
                case Code.RET:
                    throw new NotSupportedException("RET");

                case Code.TABLESWITCH:
                    {
                        var data = (TableSwitchData)inst.Operand;
                        yield return CreateByteCode(inst, AstCode.Ldc_I4, (int)data.LowByte, 0, 1, Category.Category1); // Subtract low to start at '0' 
                        yield return CreateByteCode(inst, AstCode.Sub, null, 2, 1, Category.Category1);
                        yield return CreateByteCode(inst, AstCode.Switch, data.Targets, 1, 0, Category.Unknown);
                        yield return CreateByteCode(inst, AstCode.Br, data.DefaultTarget, 0, 0, Category.Unknown);
                        yield break;
                    }

                case Code.LOOKUPSWITCH:
                    {
                        var data = (LookupSwitchData)inst.Operand;
                        yield return CreateByteCode(inst, AstCode.LookupSwitch, data, 1, 0, Category.Unknown);
                        yield return CreateByteCode(inst, AstCode.Br, data.DefaultTarget, 0, 0, Category.Unknown);
                        yield break;
                    }

                case Code.IRETURN:
                case Code.FRETURN:
                case Code.LRETURN:
                case Code.DRETURN:
                case Code.ARETURN:
                    code = AstCode.Ret;
                    operand = null;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.RETURN:
                    code = AstCode.Ret;
                    operand = null;
                    popCount = 0;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.GETSTATIC:
                    {
                        code = AstCode.Ldsfld;
                        var fieldRef = (ConstantPoolFieldRef) inst.Operand;
                        operand = AsFieldReference(fieldRef);
                        popCount = 0;
                        pushCount = 1;
                        category = fieldRef.IsWide ? Category.Category2 : Category.Category1;
                    }
                    break;

                case Code.PUTSTATIC:
                    code = AstCode.Stsfld;
                    operand = AsFieldReference((ConstantPoolFieldRef)inst.Operand);
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.GETFIELD:
                    {
                        code = AstCode.Ldfld;
                        var fieldRef = (ConstantPoolFieldRef) inst.Operand;
                        operand = AsFieldReference(fieldRef);
                        popCount = 1;
                        pushCount = 1;
                        category = fieldRef.IsWide ? Category.Category2 : Category.Category1;
                    }
                    break;

                case Code.PUTFIELD:
                    code = AstCode.Stfld;
                    operand = AsFieldReference((ConstantPoolFieldRef)inst.Operand);
                    popCount = 2;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.INVOKEVIRTUAL:
                    {
                        var method = AsMethodReference((ConstantPoolMethodRef)inst.Operand, true);
                        code = AstCode.Callvirt;
                        operand = method;
                        popCount = method.Parameters.Count + 1;
                        pushCount = method.ReturnType.IsVoid() ? 0 : 1;
                        category = method.ReturnType.IsWide() ? Category.Category2 : Category.Category1;
                    }
                    break;

                case Code.INVOKESPECIAL:
                    {
                        var method = AsMethodReference((ConstantPoolMethodRef)inst.Operand, true);
                        code = AstCode.CallSpecial;
                        operand = method;
                        popCount = method.Parameters.Count + 1;
                        pushCount = method.ReturnType.IsVoid() ? 0 : 1;
                        category = method.ReturnType.IsWide() ? Category.Category2 : Category.Category1;
                    }
                    break;

                case Code.INVOKESTATIC:
                    {
                        var method = AsMethodReference((ConstantPoolMethodRef)inst.Operand, false);
                        code = AstCode.Call;
                        operand = method;
                        popCount = method.Parameters.Count;
                        pushCount = method.ReturnType.IsVoid() ? 0 : 1;
                        category = method.ReturnType.IsWide() ? Category.Category2 : Category.Category1;
                    }
                    break;

                case Code.INVOKEINTERFACE:
                    {
                        var method = AsMethodReference((ConstantPoolMethodRef)inst.Operand, true);
                        code = AstCode.CallIntf;
                        operand = method;
                        popCount = method.Parameters.Count + 1;
                        pushCount = method.ReturnType.IsVoid() ? 0 : 1;
                        category = method.ReturnType.IsWide() ? Category.Category2 : Category.Category1;
                    }
                    break;

                case Code.NEW:
                    code = AstCode.New;
                    operand = AsTypeReference(((ConstantPoolClass)inst.Operand).Type, XTypeUsageFlags.ExpressionType);
                    popCount = 0;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.NEWARRAY:
                    code = AstCode.Newarr;
                    operand = AsTypeReference((int)inst.Operand); 
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.ANEWARRAY:
                    code = AstCode.Newarr;
                    operand = AsTypeReference(((ConstantPoolClass)inst.Operand).Type, XTypeUsageFlags.ExpressionType);
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.ARRAYLENGTH:
                    code = AstCode.Ldlen;
                    operand = null;
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.ATHROW:
                    code = AstCode.Throw;
                    operand = null;
                    popCount = 1;
                    pushCount = 0; // Not JVM spec, but consistent with Ast implementation
                    category = Category.Unknown;
                    break;

                case Code.CHECKCAST:
                    code = AstCode.Castclass;
                    operand = AsTypeReference(((ConstantPoolClass)inst.Operand).Type, XTypeUsageFlags.Cast);
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.INSTANCEOF:
                    code = AstCode.InstanceOf;
                    operand = AsTypeReference(((ConstantPoolClass)inst.Operand).Type, XTypeUsageFlags.Cast);
                    popCount = 1;
                    pushCount = 1;
                    category = Category.Category1;
                    break;

                case Code.MONITORENTER:
                    code = AstCode.Call;
                    operand = MonitorMethodReference("Enter");
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.MONITOREXIT:
                    code = AstCode.Call;
                    operand = MonitorMethodReference("Exit");
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.WIDE:
                    throw new NotSupportedException("WIDE should be resolved by now");

                case Code.MULTIANEWARRAY:
                    {
                        code = AstCode.MultiNewarr;
                        operand = AsTypeReference(((ConstantPoolClass)inst.Operand).Type, XTypeUsageFlags.Other);
                        popCount = (int) inst.Operand2;
                        pushCount = 1;
                        category = Category.Category1;
                    }
                    break;

                case Code.IFNULL:
                    code = AstCode.Brfalse;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                case Code.IFNONNULL:
                    code = AstCode.Brtrue;
                    operand = inst.Operand;
                    popCount = 1;
                    pushCount = 0;
                    category = Category.Unknown;
                    break;

                default:
                    throw new ArgumentException("Unknown java bytecode " + (int)inst.Opcode);
            }

            yield return CreateByteCode(inst, code, operand, popCount, pushCount, category);
        }

        /// <summary>
        /// Create a single bytecode.
        /// </summary>
        private ByteCode CreateByteCode(Instruction inst, AstCode code, object operand, int popCount, int pushCount, Category category, XTypeReference type = null)
        {
            var next = codeAttr.GetNext(inst);
            var byteCode = new ByteCode
            {
                Category = category,
                Offset = inst.Offset,
                EndOffset = (next != null) ? next.Offset : codeAttr.Code.Length,
                Code = code,
                Operand = operand,
                PopCount = popCount,
                PushCount = pushCount,
                SourceLocation = new SourceLocation(codeAttr, inst),
                Type = type
            };
            return byteCode;
        }
    }
}
