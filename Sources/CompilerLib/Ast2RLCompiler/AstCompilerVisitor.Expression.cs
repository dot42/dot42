using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Converters;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.FrameworkDefinitions;
using ArrayType = Dot42.DexLib.ArrayType;
using FieldDefinition = Dot42.DexLib.FieldDefinition;
using Instruction = Dot42.CompilerLib.RL.Instruction;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    /// <summary>
    /// AstNode visitor that generates RL instructions.
    /// </summary>
    partial class AstCompilerVisitor 
    {
        /// <summary>
        /// Generate code for the given expression.
        /// </summary>
        private RLRange VisitExpression(AstExpression node, List<RLRange> args, AstNode parent)
        {

            switch (node.Code)
            {
                case AstCode.Nop:
                case AstCode.Endfinally:
                case AstCode.Endfilter:
                    return new RLRange(this.Add(node.SourceLocation, RCode.Nop), null);
                case AstCode.Ldexception:
                    throw new InvalidOperationException("ldexception should not occur");
                case AstCode.Dup:
                    return new RLRange(this.Add(node.SourceLocation, RCode.Nop), args[0].Result);

                    #region Constants

                case AstCode.Ldc_I4:
                case AstCode.Ldc_R4:
                    {
                        var type = node.GetResultType();
                        var converter = type.ConstValueConverter(true);
                        var value = converter(node.Operand);
                        var r = frame.AllocateTemp(type.IsFloat() ? PrimitiveType.Float : PrimitiveType.Int);
                        var first = this.Add(node.SourceLocation, RCode.Const, value, r);
                        return new RLRange(first, r);
                    }
                case AstCode.Ldc_I8:
                case AstCode.Ldc_R8:
                    {
                        var type = node.GetResultType();
                        var converter = type.ConstValueConverter(false);
                        var value = converter(node.Operand);
                        var r = frame.AllocateTemp(type.IsDouble() ? PrimitiveType.Double : PrimitiveType.Long);
                        return new RLRange(args, this.Add(node.SourceLocation, RCode.Const_wide, value, r), r);
                    }
                case AstCode.Ldnull:
                    {
                        //Debugger.Launch();
                        var r = frame.AllocateTemp(node.GetResultType().GetReference(targetPackage));
                        return new RLRange(args, this.Add(node.SourceLocation, RCode.Const, 0, r), r);
                    }
                case AstCode.Ldstr:
                    {
                        var str = (string) node.Operand;
                        var r = frame.AllocateTemp(node.GetResultType().GetReference(targetPackage));
                        return new RLRange(args, this.Add(node.SourceLocation, RCode.Const_string, str, r), r);
                    }
                case AstCode.DefaultValue:
                    {
                        var type = (XTypeReference) node.Operand;
                        if (type.IsPrimitive)
                        {
                            var r = frame.AllocateTemp(type.GetReference(targetPackage));
                            return new RLRange(args, this.Add(node.SourceLocation, node.Arguments[0].Const(), 0, r), r);
                        }
                        if (type.IsEnum())
                        {
                            var r = frame.AllocateTemp(type.GetReference(targetPackage));
                            var denumType = type.GetClassReference(targetPackage);
                            var defaultField = new FieldReference(denumType, NameConstants.Enum.DefaultFieldName, denumType);
                            return new RLRange(this.Add(node.SourceLocation, RCode.Sget_object, defaultField, r), r);
                        }
                        else
                        {
                            var r = frame.AllocateTemp(type.GetReference(targetPackage));
                            return new RLRange(args, this.Add(node.SourceLocation, RCode.Const, 0, r), r);
                        }
                    }
                case AstCode.TypeOf:
                    {
                        var type = (XTypeReference) node.Operand;
                        var dtype = type.IsVoid() ? PrimitiveType.Void : type.GetReference(targetPackage);
                        var typeReg = frame.AllocateTemp(FrameworkReferences.Class);
                        var first = this.Add(node.SourceLocation, RCode.Const_class, dtype, typeReg);
                        return new RLRange(first, typeReg);
                    }
                case AstCode.BoxedTypeOf:
                    {
                        var type = (XTypeReference) node.Operand;
                        var typeReg = frame.AllocateTemp(FrameworkReferences.Class);
                        var first = this.Add(node.SourceLocation, RCode.Const_class, type.GetBoxedType(), typeReg);
                        return new RLRange(first, typeReg);
                    }

                case AstCode.NullableTypeOf:
                    {
                        var type = (XTypeReference)node.Operand;
                        var typeReg = frame.AllocateTemp(FrameworkReferences.Class);
                        var dbaseType = (ClassReference)type.GetReference(targetPackage);
                        var dnullabeType = targetPackage.DexFile.GetClass(dbaseType.Fullname)   
                                                        .NullableMarkerClass;
                        var first = this.Add(node.SourceLocation, RCode.Const_class, dnullabeType, typeReg);
                        return new RLRange(first, typeReg);
                    }

                    #endregion

                    #region Arithmetic

                case AstCode.Neg:
                    {
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        return new RLRange(args, this.Add(node.SourceLocation, node.Neg(), tmp.Result, tmp.Result),
                                           tmp.Result);
                    }
                case AstCode.Not:
                    {
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        return new RLRange(args, this.Add(node.SourceLocation, node.Not(), tmp.Result, tmp.Result),
                                           tmp.Result);
                    }
                case AstCode.Add:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Add2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Add(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.Add_Ovf:
                    {
                        var isLong = node.Arguments[0].IsInt64();
                        var addMethods =
                            compiler.GetDot42InternalType("Checked").Resolve().Methods.Where(x => x.Name == "Add");
                        var ilMethod = addMethods.First(x => isLong ? x.ReturnType.IsInt64() : x.ReturnType.IsInt32());
                        var method = ilMethod.GetReference(targetPackage);
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        var registers = tmp.Result.Registers.Concat(args[1].Result.Registers);
                        this.Add(node.SourceLocation, RCode.Invoke_static, method, registers);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation,
                                                    isLong ? RCode.Move_result_wide : RCode.Move_result, tmp.Result),
                                           tmp.Result);
                    }
                case AstCode.CompoundAdd:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Add2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Sub:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Sub2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Sub(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.Sub_Ovf:
                    {
                        var isLong = node.Arguments[0].IsInt64();
                        var addMethods =
                            compiler.GetDot42InternalType("Checked").Resolve().Methods.Where(x => x.Name == "Sub");
                        var ilMethod = addMethods.First(x => isLong ? x.ReturnType.IsInt64() : x.ReturnType.IsInt32());
                        var method = ilMethod.GetReference(targetPackage);
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        var registers = tmp.Result.Registers.Concat(args[1].Result.Registers);
                        this.Add(node.SourceLocation, RCode.Invoke_static, method, registers);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation,
                                                    isLong ? RCode.Move_result_wide : RCode.Move_result, tmp.Result),
                                           tmp.Result);
                    }
                case AstCode.CompoundSub:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Sub2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Mul:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Mul2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Mul(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.Mul_Ovf:
                    {
                        var isLong = node.Arguments[0].IsInt64();
                        var addMethods =
                            compiler.GetDot42InternalType("Checked").Resolve().Methods.Where(x => x.Name == "Mul");
                        var ilMethod = addMethods.First(x => isLong ? x.ReturnType.IsInt64() : x.ReturnType.IsInt32());
                        var method = ilMethod.GetReference(targetPackage);
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        var registers = tmp.Result.Registers.Concat(args[1].Result.Registers);
                        this.Add(node.SourceLocation, RCode.Invoke_static, method, registers);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation,
                                                    isLong ? RCode.Move_result_wide : RCode.Move_result, tmp.Result),
                                           tmp.Result);
                    }
                case AstCode.CompoundMul:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Mul2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Div:
                case AstCode.Div_Un:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Div2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Div(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.CompoundDiv:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Div2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Rem:
                case AstCode.Rem_Un:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Rem2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Rem(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.CompoundRem:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Rem2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.And:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.And2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.And(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.CompoundAnd:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.And2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Or:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Or2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Or(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.CompoundOr:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args, this.Add(node.SourceLocation, node.Or2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Xor:
                    {
                        if (args[0].Result.Register.IsTemp)
                            return new RLRange(args,
                                               this.Add(node.SourceLocation, node.Xor2Addr(), args[0].Result,
                                                        args[1].Result), args[0].Result);
                        var tmp = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Xor(), tmp, args[0].Result, args[1].Result),
                                           tmp);
                    }
                case AstCode.CompoundXor:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Xor2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Shl:
                    {
                        var r = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Shl(), r, args[0].Result, args[1].Result),
                                           r);
                    }
                case AstCode.CompoundShl:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Shl2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Shr:
                    {
                        var r = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Shr(), r, args[0].Result, args[1].Result),
                                           r);
                    }
                case AstCode.CompoundShr:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.Shr2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Shr_Un:
                    {
                        var r = frame.AllocateTemp(args[0].Result.Type);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.UShr(), r, args[0].Result, args[1].Result),
                                           r);
                    }
                case AstCode.CompoundShr_Un:
                    {
                        var localReg = frame.GetArgument((AstVariable) node.Operand);
                        return new RLRange(args,
                                           this.Add(node.SourceLocation, node.UShr2Addr(), localReg, args[0].Result),
                                           localReg);
                    }
                case AstCode.Conditional: // arg[0] ? arg[1] : arg[2]
                    {
                        var valueType = (XTypeReference) node.Operand;
                        var result = frame.AllocateTemp(valueType.GetReference(targetPackage));
                        var move = node.Arguments[1].Move();
                        var move2 = node.Arguments[2].Move();
                        if (move2 == RCode.Move_object) move = move2;

                        // condition
                        var gotoArg2 = this.Add(node.SourceLocation, RCode.If_eqz, null, args[0].Result.Registers);

                        // Generate code for arg[1]
                        var arg1 = node.Arguments[1].Accept(this, node);
                        this.Add(node.SourceLocation, move, result, arg1.Result);
                        var gotoEnd = this.Add(node.SourceLocation, RCode.Goto, null);

                        // Generate code for arg[2]
                        var arg2Start = this.Add(node.SourceLocation, RCode.Nop);
                        var arg2 = node.Arguments[2].Accept(this, node);
                        this.Add(node.SourceLocation, move, result, arg2.Result);

                        var end = this.Add(node.SourceLocation, RCode.Nop);
                        // Set branch targets
                        gotoArg2.Operand = arg2Start;
                        gotoEnd.Operand = end;

                        return new RLRange(gotoArg2, end, result);
                    }

                    #endregion

                    #region Conversion

                    /*case AstCode.Conv_U4:
                case AstCode.Conv_Ovf_U4:
                    {
                        return new RLRange(this.Add(node.SourceLocation, RCode.Nop), args[0].Result);
                    }*/
                case AstCode.Conv_I1:
                case AstCode.Conv_Ovf_I1:
                case AstCode.Conv_Ovf_I1_Un:
                    return ConvX(node.SourceLocation, RCode.Int_to_byte, PrimitiveType.Byte,
                                 ConvToInt(node.SourceLocation, args[0]));
                case AstCode.Conv_U1:
                case AstCode.Conv_Ovf_U1:
                case AstCode.Conv_Ovf_U1_Un:
                    {
                        var result = ConvX(node.SourceLocation, RCode.Int_to_byte, PrimitiveType.Byte,
                                           ConvToInt(node.SourceLocation, args[0]));
                        var last = this.Add(node.SourceLocation, RCode.And_int_lit, 0xFF, result.Result, result.Result);
                        return new RLRange(result.First, last, result.Result);
                    }
                case AstCode.Conv_I2:
                case AstCode.Conv_Ovf_I2:
                case AstCode.Conv_Ovf_I2_Un:
                    return ConvX(node.SourceLocation, RCode.Int_to_short, PrimitiveType.Short,
                                 ConvToInt(node.SourceLocation, args[0]));
                case AstCode.Conv_U2:
                case AstCode.Conv_Ovf_U2:
                case AstCode.Conv_Ovf_U2_Un:
                    {
                        if (node.GetResultType().IsUInt16())
                        {
                            var result = ConvX(node.SourceLocation, RCode.Int_to_short, PrimitiveType.Short,
                                               ConvToInt(node.SourceLocation, args[0]));
                            var r2 = frame.AllocateTemp(PrimitiveType.Int);
                            this.Add(node.SourceLocation, RCode.Const, 0xFFFF, r2);
                            var last = this.Add(node.SourceLocation, RCode.And_int_2addr, result.Result, r2);
                            return new RLRange(result.First, last, result.Result);
                        }
                        else
                        {
                            return ConvX(node.SourceLocation, RCode.Int_to_char, PrimitiveType.Char,
                                         ConvToInt(node.SourceLocation, args[0]));
                        }
                    }
                case AstCode.Conv_I4:
                case AstCode.Conv_Ovf_I4:
                case AstCode.Conv_Ovf_I4_Un:
                case AstCode.Conv_I: // Convert to native int
                case AstCode.Conv_Ovf_I: // Convert to native with overflow check
                case AstCode.Conv_Ovf_I_Un: // Convert to native without overflow check
                case AstCode.Conv_U: // Convert to native uint
                case AstCode.Conv_Ovf_U: // Convert to native uint with overflow check
                case AstCode.Conv_Ovf_U_Un: // Convert to native uint without overflow check
                case AstCode.Conv_U4:
                case AstCode.Conv_Ovf_U4:
                case AstCode.Conv_Ovf_U4_Un:
                    return ConvX(node.SourceLocation, node.Arguments[0].ConvI4(), PrimitiveType.Int, args[0]);
                case AstCode.Conv_I8:
                case AstCode.Conv_Ovf_I8:
                case AstCode.Conv_Ovf_I8_Un:
                case AstCode.Conv_U8:
                case AstCode.Conv_Ovf_U8:
                case AstCode.Conv_Ovf_U8_Un:
                    return ConvX(node.SourceLocation, node.Arguments[0].ConvI8(), PrimitiveType.Long, args[0]);
                case AstCode.Conv_R4:
                    return ConvX(node.SourceLocation, node.Arguments[0].ConvR4(), PrimitiveType.Float, args[0]);
                case AstCode.Conv_R8:
                case AstCode.Conv_R_Un:
                    return ConvX(node.SourceLocation, node.Arguments[0].ConvR8(), PrimitiveType.Double, args[0]);
                case AstCode.Int_to_ubyte:
                    {
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        var last = this.Add(node.SourceLocation, RCode.And_int_lit, 0xFF, tmp.Result, tmp.Result);
                        return new RLRange(args, tmp.First, last, tmp.Result);
                    }
                case AstCode.Int_to_ushort:
                    {
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        var r2 = frame.AllocateTemp(PrimitiveType.Int);
                        this.Add(node.SourceLocation, RCode.Const, 0xFFFF, r2);
                        var last = this.Add(node.SourceLocation, RCode.And_int_2addr, tmp.Result, r2);
                        return new RLRange(args, tmp.First, last, tmp.Result);
                    }
                case AstCode.Box:
                    {
                        var type = (XTypeReference) node.Operand;
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        return this.Box(node.SourceLocation, tmp.Result, type, targetPackage, frame);
                    }
                case AstCode.Unbox:
                case AstCode.Unbox_Any:
                    {
                        var type = (XTypeReference) node.Operand;
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        return this.Unbox(node.SourceLocation, tmp.Result, type, compiler, targetPackage, frame);
                    }
                case AstCode.AddressOf:
                    {
                        return args[0];
                    }
                case AstCode.Enum_to_int:
                case AstCode.Enum_to_long:
                    {
                        var enumType = node.Arguments[0].GetResultType().Resolve();
                        var isWide = enumType.GetEnumUnderlyingType().IsWide();
                        var internalEnumType = compiler.GetDot42InternalType("Enum").GetClassReference(targetPackage);
                        var methodName = isWide ? "LongValue" : "IntValue";
                        var enumNumericType = isWide ? PrimitiveType.Long : PrimitiveType.Int;
                        var rvalue = frame.AllocateTemp(enumNumericType);
                        var convMethodDex = new MethodReference(internalEnumType, methodName,
                                                                new Prototype(enumNumericType));
                        var call = this.Add(node.SourceLocation, RCode.Invoke_virtual, convMethodDex, args[0].Result);
                        var last = this.Add(node.SourceLocation, isWide ? RCode.Move_result_wide : RCode.Move_result,
                                            rvalue);
                        return new RLRange(call, last, rvalue);
                    }

                case AstCode.Int_to_enum:
                case AstCode.Long_to_enum:
                    {
                        var enumType = node.GetResultType().Resolve();
                        var denumType = enumType.GetClassReference(targetPackage);
                        var isWide = enumType.GetEnumUnderlyingType().IsWide();
                        var literalIsWide = node.Arguments[0].GetResultType().IsWide();
                        var internalEnumType = compiler.GetDot42InternalType("Enum").GetClassReference(targetPackage);
                        var internalEnumInfoType = compiler.GetDot42InternalType("EnumInfo").GetClassReference(targetPackage);
                        var enumNumericType = isWide ? PrimitiveType.Long : PrimitiveType.Int;
                        var getValueMethod = new MethodReference(internalEnumInfoType, "GetValue",
                                                                 new Prototype(internalEnumType,
                                                                               new Parameter(enumNumericType, "value")));
                        var rinfo = frame.AllocateTemp(internalEnumInfoType);
                        var infoField = new FieldReference(denumType, NameConstants.Enum.InfoFieldName,
                                                           internalEnumInfoType);
                        var getInfo = this.Add(node.SourceLocation, RCode.Sget_object, infoField, rinfo);
                        var valueR = args[0].Result;
                        if (isWide != literalIsWide)
                        {
                            var convValueR = frame.AllocateTemp(isWide ? PrimitiveType.Long : PrimitiveType.Int);
                            var convCode = isWide ? RCode.Int_to_long : RCode.Long_to_int;
                            this.Add(node.SourceLocation, convCode, convValueR, valueR);
                            valueR = convValueR;
                        }
                        this.Add(node.SourceLocation, RCode.Invoke_virtual, getValueMethod,
                                 rinfo.Registers.Concat(valueR.Registers));
                        var renum = frame.AllocateTemp(denumType);
                        this.Add(node.SourceLocation, RCode.Move_result_object, renum);
                        var castClass = this.Add(node.SourceLocation, RCode.Check_cast, denumType, renum);
                        return new RLRange(getInfo, castClass, renum);
                    }

                    #endregion

                    #region Branching

                case AstCode.Cle:
                case AstCode.Cle_Un:
                case AstCode.Clt:
                case AstCode.Clt_Un:
                case AstCode.Ceq:
                case AstCode.Cne:
                case AstCode.Cgt:
                case AstCode.Cgt_Un:
                case AstCode.Cge:
                case AstCode.Cge_Un:
                    {
                        var r = frame.AllocateTemp(PrimitiveType.Int);
                        var start = this.Add(node.SourceLocation, RCode.Const, 0, r);
                        Instruction test;
                        var narg0 = node.Arguments[0];
                        var narg1 = node.Arguments[1];
                        if (narg0.IsWide() || narg1.IsWide())
                        {
                            var r2 = frame.AllocateTemp(PrimitiveType.Int);
                            var code = narg0.IsDouble() ? RCode.Cmpg_double : RCode.Cmp_long;
                            this.Add(node.SourceLocation, code, r2, args[0].Result, args[1].Result);
                            test = this.Add(node.SourceLocation, node.Code.Reverse().ToIfTestZ(), r2);
                        }
                        else if (narg0.IsFloat() || narg1.IsFloat())
                        {
                            var r2 = frame.AllocateTemp(PrimitiveType.Int);
                            this.Add(node.SourceLocation, RCode.Cmpg_float, r2, args[0].Result, args[1].Result);
                            test = this.Add(node.SourceLocation, node.Code.Reverse().ToIfTestZ(), r2);
                        }
                        else
                        {
                            test = this.Add(node.SourceLocation, node.Code.Reverse().ToIfTest(), args[0].Result,
                                            args[1].Result);
                        }
                        this.Add(node.SourceLocation, RCode.Const, 1, r);
                        var end = this.Add(node.SourceLocation, RCode.Nop, null);
                        test.Operand = end;
                        return new RLRange(args, start, end, r);
                    }
                case AstCode.CmpLFloat:
                case AstCode.CmpGFloat:
                case AstCode.CmpLong:
                    {
                        var r = frame.AllocateTemp(PrimitiveType.Int);
                        var code = node.CmpFloatOrLong();
                        var test = this.Add(node.SourceLocation, code, r, args[0].Result, args[1].Result);
                        return new RLRange(args, test, r);
                    }
                case AstCode.CIsNotNull:
                case AstCode.CIsNull:
                    {
                        var r = frame.AllocateTemp(PrimitiveType.Int);
                        var start = this.Add(node.SourceLocation, RCode.Const, 0, r);
                        var test = this.Add(node.SourceLocation, node.Code.Reverse().ToIfTestZ(), args[0].Result);
                        this.Add(node.SourceLocation, RCode.Const, 1, r);
                        var end = this.Add(node.SourceLocation, RCode.Nop, null);
                        test.Operand = end;
                        return new RLRange(args, start, end, r);
                    }
                case AstCode.Brtrue:
                case AstCode.Brfalse:
                case AstCode.BrIfEq:
                case AstCode.BrIfNe:
                case AstCode.BrIfGe:
                case AstCode.BrIfGt:
                case AstCode.BrIfLe:
                case AstCode.BrIfLt:
                    {
                        var label = (AstLabel) node.Operand;
                        var opcode = node.Code.ToIfTestZ();
                        var branch = this.Add(node.SourceLocation, opcode, args[0].Result);
                        labelManager.AddResolveAction(label, x => branch.Operand = x);
                        return new RLRange(branch, null);
                    }
                case AstCode.__Beq:
                case AstCode.__Bne_Un:
                case AstCode.__Ble:
                case AstCode.__Ble_Un:
                case AstCode.__Blt:
                case AstCode.__Blt_Un:
                case AstCode.__Bgt:
                case AstCode.__Bgt_Un:
                case AstCode.__Bge:
                case AstCode.__Bge_Un:
                    {
                        var label = (AstLabel) node.Operand;
                        var opcode = node.Code.ToIfTest();
                        var branch = this.Add(node.SourceLocation, opcode, args[0].Result, args[1].Result);
                        labelManager.AddResolveAction(label, x => branch.Operand = x);
                        return new RLRange(branch, null);
                    }
                case AstCode.Br:
                    {
                        var label = (AstLabel) node.Operand;
                        var branch = this.Add(node.SourceLocation, RCode.Goto, null);
                        labelManager.AddResolveAction(label, x => branch.Operand = x);
                        return new RLRange(branch, null);
                    }
                case AstCode.Leave:
                    {
                        var label = (AstLabel) node.Operand;
                        var branch = this.Add(node.SourceLocation, RCode.Leave, null);
                        labelManager.AddResolveAction(label, x => branch.Operand = x);
                        return new RLRange(branch, null);
                    }
                case AstCode.Switch:
                    {
                        var labels = (AstLabel[]) node.Operand;
                        var targets = new Instruction[labels.Length];
                        for (var i = 0; i < labels.Length; i++)
                        {
                            var index = i;
                            labelManager.AddResolveAction(labels[i], x => targets[index] = x);
                        }
                        var @switch = this.Add(node.SourceLocation, RCode.Packed_switch, targets, args[0].Result);
                        return new RLRange(@switch, null);
                    }
                case AstCode.LookupSwitch:
                    {
                        var labelKeyPairs = (AstLabelKeyPair[]) node.Operand;
                        var targetPairs = new Tuple<int, Instruction>[labelKeyPairs.Length];
                        for (var i = 0; i < labelKeyPairs.Length; i++)
                        {
                            var index = i;
                            var key = labelKeyPairs[i].Key;
                            var label = labelKeyPairs[i].Label;
                            labelManager.AddResolveAction(label, x => targetPairs[index] = Tuple.Create(key, x));
                        }
                        var @switch = this.Add(node.SourceLocation, RCode.Sparse_switch, targetPairs, args[0].Result);
                        return new RLRange(@switch, null);
                    }
                case AstCode.NullCoalescing:
                    {
                        var r = frame.AllocateTemp(node.InferredType.GetReference(targetPackage));
                        var first = this.Add(node.SourceLocation, RCode.Move_object, r, args[0].Result);
                        // r := leftExpr
                        var if_nez = this.Add(node.SourceLocation, RCode.If_nez, r); // if r not null, skip
                        this.Add(node.SourceLocation, RCode.Move_object, r, args[1].Result); // r := rightExpr
                        var end = this.Add(node.SourceLocation, RCode.Nop);
                        if_nez.Operand = end;
                        return new RLRange(first, end, r);
                    }

                    #endregion

                    #region Call

                case AstCode.Call:
                case AstCode.Callvirt:
                case AstCode.CallIntf:
                case AstCode.CallSpecial:
                    {
                        return VisitCallExpression(node, args, parent);
                    }
                case AstCode.Ret:
                    if (currentMethod.ReturnsVoid)
                        return new RLRange(args, this.Add(node.SourceLocation, RCode.Return_void), null);
                    return new RLRange(args, this.Add(node.SourceLocation, node.Return(currentMethod), args[0].Result),
                                       null);

                    #endregion

                    #region Array

                case AstCode.Newarr:
                    {
                        var type = (XTypeReference) node.Operand;
                        var dType = new ArrayType(type.GetReference(targetPackage));
                        var r = frame.AllocateTemp(dType);
                        var newArray = this.Add(node.SourceLocation, RCode.New_array, dType, r, args[0].Result);
                        return new RLRange(newArray, r);
                    }
                case AstCode.ArrayNewInstance:
                    {
                        // Resolve type to a Class<?>
                        var newInstance = FrameworkReferences.ArrayNewInstance;
                        var first = this.Add(node.SourceLocation, RCode.Invoke_static, newInstance, args[0].Result,
                                             args[1].Result);
                        var r = frame.AllocateTemp(newInstance.Prototype.ReturnType);
                        var last = this.Add(node.SourceLocation, RCode.Move_result_object, r);
                        return new RLRange(first, last, r);
                    }
                case AstCode.ArrayNewInstance2:
                    {
                        // Call java.lang.reflect.Array.newInstance(type, int[])
                        var newInstance = FrameworkReferences.ArrayNewInstance2;
                        var first = this.Add(node.SourceLocation, RCode.Invoke_static, newInstance, args[0].Result,
                                             args[1].Result);
                        var resultReg = frame.AllocateTemp(FrameworkReferences.Object);
                        var last = this.Add(node.SourceLocation, RCode.Move_result_object, resultReg);
                        return new RLRange(args, first, last, resultReg);
                    }
                case AstCode.InitEnumArray:
                    {
                        var type = (XTypeReference) node.Operand;
                        var typeDef = type.Resolve();
                        // Initialize array
                        var fillMethod = FrameworkReferences.ArraysFillObject;
                        var denumType = typeDef.GetClassReference(targetPackage);
                        var defaultField = new FieldReference(denumType, NameConstants.Enum.DefaultFieldName, denumType);
                        var rdefault = frame.AllocateTemp(denumType);
                        var first = this.Add(node.SourceLocation, RCode.Sget_object, defaultField, rdefault);
                        var arrayR = args[0].Result;
                        var last = this.Add(node.SourceLocation, RCode.Invoke_static, fillMethod, arrayR, rdefault);
                        return new RLRange(first, last, arrayR);
                    }
                case AstCode.InitStructArray:
                    {
                        var defaultCtor = (XMethodReference) node.Operand;
                        var dDefaultCtor = defaultCtor.GetReference(targetPackage);
                        var arrayR = args[0].Result;
                        var indexR = frame.AllocateTemp(PrimitiveType.Int);
                        var oneR = frame.AllocateTemp(PrimitiveType.Int);
                        var elementR = frame.AllocateTemp(dDefaultCtor.Owner);
                        var first = this.Add(node.SourceLocation, RCode.Array_length, indexR, arrayR);
                        this.Add(node.SourceLocation, RCode.Const, 1, oneR);
                        var ifZero = this.Add(node.SourceLocation, RCode.If_eqz, indexR);
                            // if (index == 0) goto end;  (Operand set later)
                        this.Add(node.SourceLocation, RCode.Sub_int_2addr, indexR, oneR); // index--;
                        this.Add(node.SourceLocation, RCode.New_instance, dDefaultCtor.Owner, elementR);
                            // element = new Struct;
                        this.Add(node.SourceLocation, RCode.Invoke_direct, dDefaultCtor, elementR);
                            // invoke element.ctor()
                        this.Add(node.SourceLocation, RCode.Aput_object, elementR, arrayR, indexR);
                            // Store element in array
                        this.Add(node.SourceLocation, RCode.Goto, ifZero); // End of loop
                        var end = this.Add(node.SourceLocation, RCode.Nop);
                        ifZero.Operand = end;
                        return new RLRange(first, end, arrayR);
                    }
                case AstCode.MultiNewarr:
                    {
                        var arrType = (XTypeReference) node.Operand;
                        var darrType = arrType.GetReference(targetPackage);
                        var compType = arrType;
                        // Unwind array type to component type
                        for (var i = 0; i < node.Arguments.Count; i++)
                        {
                            compType = compType.ElementType;
                        }
                        var dcompType = new ArrayType(compType.GetReference(targetPackage));
                        var dimArrayR = frame.AllocateTemp(new ArrayType(PrimitiveType.Int));
                        var lengthR = frame.AllocateTemp(PrimitiveType.Int);
                        var first = this.Add(node.SourceLocation, RCode.Nop);
                        // Allocate dimensions array
                        this.Add(node.SourceLocation, RCode.Const, node.Arguments.Count, lengthR);
                        this.Add(node.SourceLocation, RCode.New_array, PrimitiveType.Int, dimArrayR, lengthR);
                        var indexR = lengthR;
                        // Initialize dimensions array
                        for (var i = 0; i < node.Arguments.Count; i++)
                        {
                            this.Add(node.SourceLocation, RCode.Const, i, indexR);
                            this.Add(node.SourceLocation, RCode.Aput, args[i], dimArrayR, indexR);
                        }

                        // Load component type
                        var compTypeR = frame.AllocateTemp(new ClassReference("java/lang/Class"));
                        this.Add(node.SourceLocation, RCode.Const_class, dcompType, compTypeR);

                        // Call java/lang/reflect/Array/newInstance
                        var reflectArrayType = new ClassReference("java/lang/reflect/Array");
                        var prototype = PrototypeBuilder.ParseMethodSignature("(Ljava/lang/Class;[I)Ljava/lang/Object;");
                        var methodRef = new MethodReference(reflectArrayType, "newInstance", prototype);
                        var arrayR = frame.AllocateTemp(darrType);
                        this.Add(node.SourceLocation, RCode.Invoke_static, methodRef, compTypeR, dimArrayR);
                        var last = this.Add(node.SourceLocation, RCode.Move_result_object, arrayR);

                        return new RLRange(first, last, arrayR);
                    }
                case AstCode.ByRefArray:
                case AstCode.ByRefOutArray:
                    {
                        // Create array
                        var type = (XTypeReference) node.Operand;
                        var dType = new ArrayType(type.GetReference(targetPackage));
                        var arrayR = frame.AllocateTemp(dType);
                        var lengthR = frame.AllocateTemp(PrimitiveType.Int);
                        // length=1
                        var initLength = this.Add(node.SourceLocation, RCode.Const, 1, lengthR);
                        // newarray
                        this.Add(node.SourceLocation, RCode.New_array, dType, arrayR, lengthR);
                        if (node.Code == AstCode.ByRefArray)
                        {
                            var valueR = args[0].Result;
                            var arrayType = node.InferredType;

                            // Perform type conversion if needed
                            bool isConverted;
                            var converted = this.ConvertTypeBeforeStore(node.SourceLocation, type, arrayType.ElementType,
                                                                        valueR, targetPackage, frame, compiler,
                                                                        out isConverted);
                            if (isConverted) valueR = converted.Result;

                            // array[0]=value
                            var indexR = frame.AllocateTemp(PrimitiveType.Int);
                            this.Add(node.SourceLocation, RCode.Const, 0, indexR);
                            this.Add(node.SourceLocation, arrayType.APut(), valueR, arrayR, indexR);
                        }
                        var end = this.Add(node.SourceLocation, RCode.Nop);
                        return new RLRange(initLength, end, arrayR);
                    }
                case AstCode.Ldlen:
                    {
                        var r = frame.AllocateTemp(PrimitiveType.Int);
                        return new RLRange(this.Add(node.SourceLocation, RCode.Array_length, r, args[0].Result), r);
                    }
                case AstCode.Stelem_I:
                case AstCode.Stelem_I1:
                case AstCode.Stelem_I2:
                case AstCode.Stelem_I4:
                case AstCode.Stelem_I8:
                case AstCode.Stelem_R4:
                case AstCode.Stelem_R8:
                case AstCode.Stelem_Ref:
                case AstCode.Stelem_Any:
                    {
                        var first = this.Add(node.SourceLocation, RCode.Nop);
                        var arrayR = args[0].Result;
                        var indexR = args[1].Result;
                        var valueR = args[2].Result;
                        var valueType = node.Arguments[2].GetResultType();
                        var arrayType = node.Arguments[0].GetResultType();

                        // Perform type conversion if needed
                        bool isConverted;
                        var converted = this.ConvertTypeBeforeStore(node.SourceLocation, valueType,
                                                                    arrayType.ElementType, valueR, targetPackage,
                                                                    frame, compiler, out isConverted);
                        if (isConverted) valueR = converted.Result;

                        // Store in array
                        var aput = this.Add(node.SourceLocation, node.APut(), valueR, arrayR, indexR);
                        return new RLRange(first, aput, valueR);
                    }
                case AstCode.Ldelem_I:
                case AstCode.Ldelem_I1:
                case AstCode.Ldelem_I2:
                case AstCode.Ldelem_I4:
                case AstCode.Ldelem_I8:
                case AstCode.Ldelem_R4:
                case AstCode.Ldelem_R8:
                case AstCode.Ldelem_U1:
                case AstCode.Ldelem_U2:
                case AstCode.Ldelem_U4:
                case AstCode.Ldelem_Ref:
                case AstCode.Ldelem_Any:
                    {
                        var arrayR = args[0].Result;
                        var indexR = args[1].Result;
                        var arrayType = node.Arguments[0].GetResultType();
                        var elementType = arrayType.ElementType;

                        // Allocate registry for value
                        var valueR = frame.AllocateTemp(elementType.GetReference(targetPackage));

                        // Get from array
                        var first = this.Add(node.SourceLocation, node.AGet(), valueR, arrayR, indexR);
                        return new RLRange(first, valueR);
                    }

                    #endregion

                    #region Local variables / arguments

                case AstCode.Ldloc:
                    {
                        var variable = (AstVariable) node.Operand;
                        var valueR = frame.GetArgument(variable);
                        return new RLRange(args, valueR);
                    }
                case AstCode.Ldthis:
                    if (frame.ThisArgument == null)
                        throw new ArgumentException("No this in current method");
                    return new RLRange(args, frame.ThisArgument);
                case AstCode.Stloc:
                    {
                        var variable = (AstVariable) node.Operand;
                        var resultType = node.Arguments[0].GetResultType();
                        var valueR = args[0].Result;
                        var first = this.Add(node.SourceLocation, RCode.Nop);
                        // Convert value if needed
                        bool isConverted;
                        var converted = this.ConvertTypeBeforeStore(node.SourceLocation, resultType, variable.Type,
                                                                    valueR, targetPackage, frame, compiler,
                                                                    out isConverted);
                        if (isConverted) valueR = converted.Result;
                        // Store in variable
                        var variableR = frame.GetArgument(variable);
                        return new RLRange(args, first, this.Add(node.SourceLocation, node.Move(), variableR, valueR),
                                           variableR);
                    }

                    #endregion

                    #region Fields

                case AstCode.Ldfld:
                    {
                        var fieldRef = (XFieldReference) node.Operand;
                        var field = fieldRef.Resolve();
                        var dField = field.GetReference(targetPackage);
                        var fieldType = field.FieldType;
                        // Allocate register
                        var valueR = frame.AllocateTemp(fieldType.GetReference(targetPackage));
                        // Get from field
                        var iget = this.Add(node.SourceLocation, field.IGet(), dField, valueR, args[0].Result);
                        return new RLRange(iget, valueR);
                    }
                case AstCode.Stfld:
                    {
                        var fieldRef = (XFieldReference) node.Operand;
                        var field = fieldRef.Resolve();
                        var dField = field.GetReference(targetPackage);
                        var type = node.InferredType;
                        var first = this.Add(node.SourceLocation, RCode.Nop);
                        var valueR = args[1].Result;

                        // Perform type conversion if needed
                        bool isConverted;
                        var converted = this.ConvertTypeBeforeStore(node.SourceLocation, type, field.FieldType, valueR,
                                                                    targetPackage, frame, compiler,
                                                                    out isConverted);
                        if (isConverted) valueR = converted.Result;

                        // Store in field
                        var iput = this.Add(node.SourceLocation, field.IPut(), dField, valueR, args[0].Result);
                        return new RLRange(first, iput, valueR);
                    }
                case AstCode.Ldsfld:
                    {
                        var field = (XFieldReference) node.Operand;
                        XFieldDefinition fieldDef;
                        field.TryResolve(out fieldDef);
                        var fieldType = field.FieldType;
                        // Allocate register
                        var valueR = frame.AllocateTemp(fieldType.GetReference(targetPackage));

                        string resourceName;
                        if ((fieldDef != null) && fieldDef.TryGetResourceIdAttribute(out resourceName))
                        {
                            // Replace by resource id.
                            var id = FindResourceId(compiler.ResourceTable, resourceName);
                            return new RLRange(args, this.Add(node.SourceLocation, RCode.Const, id, valueR), valueR);
                        }
                        else
                        {
                            // Normal get from field
                            var dField = field.GetReference(targetPackage);
                            var iget = this.Add(node.SourceLocation, node.SGet(), dField, valueR);
                            return new RLRange(iget, valueR);
                        }
                    }
                case AstCode.Stsfld:
                    {
                        var field = (XFieldReference) node.Operand;
                        var dField = field.GetReference(targetPackage);
                        var type = node.InferredType;
                        var first = this.Add(node.SourceLocation, RCode.Nop);
                        var valueR = args[0].Result;

                        // Perform type conversion if needed
                        bool isConverted;
                        var converted = this.ConvertTypeBeforeStore(node.SourceLocation, type, field.FieldType, valueR,
                                                                    targetPackage, frame, compiler,
                                                                    out isConverted);
                        if (isConverted) valueR = converted.Result;

                        // Store in field
                        var sput = this.Add(node.SourceLocation, node.SPut(), dField, valueR);
                        return new RLRange(first, sput, valueR);
                    }

                    #endregion

                    #region Object model

                case AstCode.Newobj: // IL new
                    {
                        var ilCtorRef = (XMethodReference) node.Operand;
                        var ilType = ilCtorRef.DeclaringType;

                        // New normal object
                        var ilCtor = ilCtorRef.Resolve();
                        var dCtor = ilCtor.GetReference(targetPackage);
                        var dType = ilType.GetReference(targetPackage);

                        // Create instance
                        var r = frame.AllocateTemp(dType);
                        var first = this.Add(node.SourceLocation, RCode.New_instance, dType, r);

                        // Prepare arguments
                        int argsOffset;
                        List<RLRange> originalArgs;
                        ConvertParametersBeforeCall(node, args, ilCtor, out argsOffset, out originalArgs);

                        // Collect arguments
                        var arguments = args.SelectMany(x => x.Result.Registers).ToList();
                        // Insert this argument
                        arguments.Insert(0, r);

                        // Invoke ctor
                        this.Add(node.SourceLocation, RCode.Invoke_direct, dCtor, arguments);

                        // Post process arguments
                        ConvertParametersAfterCall(node, args, ilCtor, argsOffset, originalArgs);
                        var last = this.Add(node.SourceLocation, RCode.Nop);
                        return new RLRange(args, first, last, r);
                    }
                case AstCode.New: // Java new
                    {
                        var ilType = (XTypeReference) node.Operand;
                        // New normal object
                        var dType = ilType.GetReference(targetPackage);
                        // Create instance
                        var r = frame.AllocateTemp(dType);
                        var first = this.Add(node.SourceLocation, RCode.New_instance, dType, r);
                        return new RLRange(args, first, r);
                    }
                case AstCode.CallBaseCtor:
                    {
                        var dtype = currentDexMethod.Owner;
                        var dBaseType = dtype.SuperClass as ClassDefinition;
                        if (dBaseType == null)
                            throw new CompilerException(string.Format("Type {0} base no superclass as definition", dtype.Fullname));
                        var paramCount = node.Arguments.Count - 1;
                        var baseCtor = dBaseType.Methods.Single(x => x.IsConstructor && (x.Prototype.Parameters.Count == paramCount));
                        var call = this.Add(node.SourceLocation, RCode.Invoke_direct, baseCtor, args.SelectMany(x => x.Result.Registers));
                        return new RLRange(call, args[0].Result);
                    }
            case AstCode.Castclass:
                    {
                        throw new NotSupportedException("castclass is not supported");
                    }
                case AstCode.SimpleCastclass:
                    {
                        var type = (XTypeReference) node.Operand;
                        var dType = type.GetReference(targetPackage);
                        if (type.IsPrimitive)
                        {
                            // Convert type to boxed type
                            dType = BoxInfo.GetBoxedType(type);
                        }

                        // Normal cast
                        var tmp = this.EnsureTemp(node.SourceLocation, args[0].Result, frame);
                        var checkCast = this.Add(node.SourceLocation, RCode.Check_cast, dType, tmp.Result);
                        return new RLRange(tmp, checkCast, tmp.Result);
                    }
                case AstCode.Isinst: // "as" operator
                    {
                        throw new NotSupportedException("isinst is not supported");
                    }
                case AstCode.InstanceOf: // "is" operator
                    {
                        throw new NotSupportedException("instanceof is not supported");
                    }
                case AstCode.SimpleInstanceOf: // "is" operator
                    {
                        var type = (XTypeReference) node.Operand;
                        var dType = type.GetReference(targetPackage);
                        if (type.IsPrimitive)
                        {
                            // Convert type to boxed type
                            dType = BoxInfo.GetBoxedType(type);
                        }
                        // Normal "is"
                        var rResult = frame.AllocateTemp(PrimitiveType.Boolean);
                        var instanceOf = this.Add(node.SourceLocation, RCode.Instance_of, dType, rResult, args[0].Result);
                        return new RLRange(instanceOf, rResult);
                    }

                    #endregion

#region Generics
                case AstCode.LdGenericInstanceField:
                    {
                        var giField = GetGenericInstanceField();
                        var r = frame.AllocateTemp(FrameworkReferences.ClassArray);
                        var iget = this.Add(node.SourceLocation, RCode.Iget_object, giField, r, frame.ThisArgument);
                        return new RLRange(iget, r);
                    }
                case AstCode.StGenericInstanceField:
                    {
                        var giField = GetGenericInstanceField();
                        var r = args[0].Result;
                        var iput = this.Add(node.SourceLocation, RCode.Iput_object, giField, r, frame.ThisArgument);
                        return new RLRange(iput, r);
                    }
                case AstCode.LdGenericInstanceTypeArgument:
                    {
                        if (frame.GenericInstanceTypeArgument == null)
                        {
                            throw new CompilerException(string.Format("Method {0} has no generic instance type argument", currentMethod.FullName));
                        }
                        return new RLRange(frame.GenericInstanceTypeArgument);
                    }
                case AstCode.LdGenericInstanceMethodArgument:
                    {
                        if (frame.GenericInstanceMethodArgument == null)
                        {
                            throw new CompilerException(string.Format("Method {0} has no generic instance method argument", currentMethod.FullName));
                        }
                        return new RLRange(frame.GenericInstanceMethodArgument);
                    }
                case AstCode.UnboxFromGeneric:
                    {
                        // Get result
                        var elementType = (XTypeReference) node.Operand;
                        var r = args[0].Result;
                        var start = this.Add(node.SourceLocation, RCode.Nop);
                        var last = start;

                        // Convert result when needed
                        if (elementType.IsGenericParameter)
                        {
                            var returnType = node.GetResultType();
                            var tmp = this.Unbox(node.SourceLocation, r, returnType, compiler, targetPackage, frame);
                            last = tmp.Last;
                            r = tmp.Result;
                        }
                        else if (elementType.IsGenericParameterArray())
                        {
                            var returnType = node.GetResultType();
                            if (returnType.IsPrimitiveArray())
                            {
                                var tmp = this.UnboxGenericArrayResult(node.SourceLocation, r, returnType, targetPackage, frame, compiler);
                                last = tmp.Last;
                                r = tmp.Result;
                            }
                            else
                            {
                                var tmp = this.Unbox(node.SourceLocation, r, returnType, compiler, targetPackage, frame);
                                last = tmp.Last;
                                r = tmp.Result;
                            }
                        }
                        return new RLRange(start, last, r);
                    }
#endregion

                case AstCode.Throw:
                    {
                        var @throw = this.Add(node.SourceLocation, RCode.Throw, args[0].Result);
                        return new RLRange(@throw, null);
                    }
                case AstCode.Rethrow:
                    {
                        if (currentExceptionRegister == null)
                            throw new CompilerException("retrow outside catch block");
                        var @throw = this.Add(node.SourceLocation, RCode.Throw, currentExceptionRegister);
                        return new RLRange(@throw, null);                        
                    }
                case AstCode.Delegate:
                    {
                        //Debugger.Launch();
                        var delegateInfo = (Tuple<XTypeDefinition, XMethodReference>)node.Operand;
                        var delegateType = compiler.GetDelegateType(delegateInfo.Item1);
                        var delegateInstanceType = delegateType.GetOrCreateInstance(node.SourceLocation, targetPackage, delegateInfo.Item2.GetElementMethod().Resolve());

                        var r = frame.AllocateTemp(delegateInstanceType.InstanceDefinition);

                        // Create instance
                        var newobj = this.Add(node.SourceLocation, RCode.New_instance, delegateInstanceType.InstanceDefinition, r);

                        // collect parameters.
                        List<RL.Register> registerArgs = new List<RL.Register>();
                        
                        registerArgs.Add(r);
                        
                        if (delegateInstanceType.ConstructorNeedsInstanceArgument)
                            registerArgs.Add(args[0].Result);

                        if (delegateInstanceType.ConstructorNeedsGenericInstanceTypeArgument)
                            registerArgs.Add(args[1].Result);
                        if (delegateInstanceType.ConstructorNeedsGenericInstanceMethodArgument &&
                            delegateInstanceType.ConstructorNeedsGenericInstanceTypeArgument)
                        {
                            registerArgs.Add(args[2].Result);
                        }
                        else if (delegateInstanceType.ConstructorNeedsGenericInstanceMethodArgument)
                            registerArgs.Add(args[1].Result);

                        // call .ctor
                        var invokeCtor = this.Add(node.SourceLocation, RCode.Invoke_direct,
                                                  delegateInstanceType.InstanceCtor, registerArgs);
                        return new RLRange(newobj, invokeCtor, r);
                    }
                case AstCode.InitArray:
                    {
                        var arrayData = (InitArrayData) node.Operand;
                        var size = arrayData.Length;
                        var type = arrayData.ArrayType;
                        var dType = type.GetReference(targetPackage);

                        // Allocate new array
                        var rArray = frame.AllocateTemp(dType);
                        var rSize = frame.AllocateTemp(PrimitiveType.Int);
                        var start = this.Add(node.SourceLocation, RCode.Const, size, rSize);
                        this.Add(node.SourceLocation, RCode.New_array, dType, rArray, rSize);

                        // Initialize array
                        if (arrayData.IsSupportedByFillArrayData())
                        {
                            // Use fill-array-data
                            this.Add(node.SourceLocation, RCode.Fill_array_data, arrayData.Values, rArray);
                        }
                        else
                        {
                            // Use const/aput sequence
                            var rValue = frame.AllocateTemp(arrayData.ArrayType.ElementType.GetReference(targetPackage));
                            var rIndex = frame.AllocateTemp(PrimitiveType.Int);
                            var isWide = arrayData.ArrayType.ElementType.IsWide();
                            var constCode = isWide ? RCode.Const_wide : RCode.Const;
                            var aputCode = arrayData.ArrayType.APut();
                            var convertCode = arrayData.ArrayType.AConstConvertBeforePut();
                            var valueConverter = arrayData.ArrayType.ElementType.ConstValueConverter(false);

                            // Initialize index
                            this.Add(node.SourceLocation, RCode.Const, 0, rIndex);

                            // aput for each
                            for (var i = 0; i < arrayData.Length; i++)
                            {
                                var value = valueConverter(arrayData.Values.GetValue(i));
                                this.Add(node.SourceLocation, constCode, value, rValue);
                                if (convertCode != RCode.Nop)
                                {
                                    this.Add(node.SourceLocation, convertCode, rValue, rValue);                                    
                                }
                                this.Add(node.SourceLocation, aputCode, rValue, rArray, rIndex);
                                if (i + 1 < arrayData.Length)
                                {
                                    // Increment index
                                    this.Add(node.SourceLocation, RCode.Add_int_lit, 1, rIndex, rIndex);
                                }
                            }
                        }


                        //throw new NotImplementedException();
                        return new RLRange(this.Add(node.SourceLocation, RCode.Nop), rArray);
                    }
                case AstCode.InitArrayFromArguments:
                    {
                        var size = args.Count;
                        var type = (XArrayType) node.Operand;
                        var dType = type.GetReference(targetPackage);

                        // Allocate new array
                        var rArray = frame.AllocateTemp(dType);
                        var rSize = frame.AllocateTemp(PrimitiveType.Int);
                        var start = this.Add(node.SourceLocation, RCode.Nop);
                        this.Add(node.SourceLocation, RCode.Const, size, rSize);
                        this.Add(node.SourceLocation, RCode.New_array, dType, rArray, rSize);

                        // Use const/aput sequence
                        var rIndex = frame.AllocateTemp(PrimitiveType.Int);
                        var aputCode = type.APut();

                        // Initialize index
                        this.Add(node.SourceLocation, RCode.Const, 0, rIndex);

                        // aput for each
                        for (var i = 0; i < size; i++)
                        {
                            this.Add(node.SourceLocation, aputCode, args[i].Result, rArray, rIndex);
                            if (i + 1 < size)
                            {
                                // Increment index
                                this.Add(node.SourceLocation, RCode.Add_int_lit, 1, rIndex, rIndex);
                            }
                        }
                        return new RLRange(start, this.Add(node.SourceLocation, RCode.Nop), rArray);
                    }
                case AstCode.LdClass:
                    {
                        var ilType = (XTypeReference)node.Operand;
                        var dType = ilType.GetReference(targetPackage);
                        // Load type
                        var r = frame.AllocateTemp(FrameworkReferences.Class);
                        var first = this.Add(node.SourceLocation, RCode.Const_class, dType, r);
                        return new RLRange(args, first, r);                        
                    }
                case AstCode.Ldtoken:
                    {
                        throw new NotSupportedException("ldtoken is not supported");
                    }
                default:
                    string opcodeName;
#if DEBUG
                    //Debugger.Launch();
                    opcodeName = node.Code.ToString();
#else
                    opcodeName = ((int)node.Code).ToString();
#endif
                    var source = FormatSource(node);
                    var msg = string.Format("Unexpected opcode {0} in {1}.", opcodeName, source);
                    throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Generate code for a call or callvirtual opcode.
        /// </summary>
        private RLRange VisitCallExpression(AstExpression node, List<RLRange> args, AstNode parent)
        {
            var targetMethodRef = ((XMethodReference) node.Operand);
            var targetMethodDefOrRef = targetMethodRef;
            var methodReturnType = targetMethodRef.ReturnType;
            if (targetMethodRef.DeclaringType.IsArray)
            {
                RLRange result;
                if (TryVisitArrayTypeMethodCallExpression(node, targetMethodRef, args, parent, out result))
                    return result;
            }

            XTypeDefinition targetDeclaringType = null;
            XMethodDefinition targetMethodDef;
            if (targetMethodRef.TryResolve(out targetMethodDef))
            {
                if (targetMethodDef.HasDexNativeAttribute())
                {
                    // Handle [DexNative] methods different.
                    return VisitDexNativeCall(node, args, parent, targetMethodDef);
                }
                targetMethodDefOrRef = targetMethodDef;
                targetDeclaringType = targetMethodDef.DeclaringType;
                methodReturnType = targetMethodDef.ReturnType;
            }

            var dMethodRef = targetMethodDefOrRef.GetReference(targetPackage);
            var first = this.Add(node.SourceLocation, RCode.Nop);
            var last = first;

            // Convert parameters when needed
            List<RLRange> originalArgs;
            int argsOffset;
            ConvertParametersBeforeCall(node, args, targetMethodDefOrRef, out argsOffset, out originalArgs);

            // Invoke
            var arguments = args.SelectMany(x => x.Result.Registers).ToList();
            MaxOutgoingRegisters = Math.Max(MaxOutgoingRegisters, arguments.Count);

            // Determine opcode used in this call
            RCode opcode ;
            switch (node.Code)
            {
                case AstCode.Callvirt:
                    if ((targetMethodDef != null) && targetMethodDef.UseInvokeInterface)
                        opcode = RCode.Invoke_interface;
                    else
                        opcode = ((targetDeclaringType != null) && targetDeclaringType.IsInterface) ? RCode.Invoke_interface : RCode.Invoke_virtual;
                    break;
                case AstCode.Call:
                    opcode = targetMethodDef.Invoke(targetMethodDefOrRef, currentMethod, false);
                    break;
                case AstCode.CallIntf:
                    opcode = RCode.Invoke_interface;                    
                    break;
                case AstCode.CallSpecial:
                    opcode = targetMethodDef.Invoke(targetMethodDefOrRef, currentMethod, true);
                    break;
                default:
                    throw new ArgumentException("Unknown call code " + (int)node.Code);

            }
            if ((targetDeclaringType != null) && targetDeclaringType.IsDelegate())
            {
                opcode = RCode.Invoke_interface;
            }

            // Process constraint prefix
            if (arguments.Count > 0)
            {
                var constraint = node.GetPrefix(AstCode.Constrained);
                if (constraint != null)
                {
                    var type = constraint.Operand as XTypeReference;
                    if (type != null)
                    {
                        if (type.IsGenericParameter)
                        {
                            // (olaf: todo: here the faulty call gets inserted into the ast)
                            // Cast to declaring type of method
                            this.Add(node.SourceLocation, RCode.Check_cast, targetMethodRef.DeclaringType.GetReference(targetPackage), arguments[0]);
                        }
                    }
                }
            }

            // Actual call
            this.Add(node.SourceLocation, opcode, dMethodRef, arguments);

            // Handle result
            RegisterSpec r = null;
            if ((!methodReturnType.IsVoid()) && (parent is AstExpression))
            {
                // Get result
                r = frame.AllocateTemp(methodReturnType.GetReference(targetPackage));
                last = this.Add(node.SourceLocation, methodReturnType.MoveResult(), r);
            }

            // Handle unboxing/updating of parameters after the call
            ConvertParametersAfterCall(node, args, targetMethodRef, argsOffset, originalArgs);

            return new RLRange(args, first, last, r);
        }

        /// <summary>
        /// Apply type/content conversion of arguments just before a method call.
        /// </summary>
        private void ConvertParametersBeforeCall(AstExpression node, List<RLRange> args, XMethodReference targetMethod, out int argsOffset, out List<RLRange> originalArgs)
        {
            // Convert parameters when needed
            argsOffset = (args.Count - targetMethod.Parameters.Count) - node.GenericInstanceArgCount;
            originalArgs = args.ToList();
            for (var i = 0; i < targetMethod.Parameters.Count; i++)
            {
                var parameterType = targetMethod.Parameters[i].ParameterType;
                if (parameterType.IsByte())
                {
                    // Convert from byte to sbyte
                    var rArg = args[argsOffset + i].Result.Register;
                    this.Add(node.SourceLocation, RCode.Int_to_byte, rArg, rArg);
                }
                else if (parameterType.IsUInt16())
                {
                    // Convert from ushort to short
                    var rArg = args[argsOffset + i].Result.Register;
                    this.Add(node.SourceLocation, RCode.Int_to_short, rArg, rArg);
                }
                else
                {
                    var nodeArgI = node.Arguments[argsOffset + i];
                    var nodeArgIType = nodeArgI.GetResultType();
                    var originalArg = args[argsOffset + i];
                    var rx = originalArg.Result;
                    if ((parameterType.IsGenericParameter) && (nodeArgIType.IsPrimitive))
                    {
                        // Convert using (valueOf)
                        var tmp = this.Box(node.SourceLocation, rx, nodeArgIType, targetPackage, frame);
                        args[argsOffset + i] = new RLRange(originalArg.First, originalArg.Last, tmp.Result);
                    }
                    else if ((parameterType.IsGenericParameterArray()) && nodeArgIType.IsPrimitiveArray())
                    {
                        // Convert using Boxing class
                        var tmp = this.BoxGenericArray(node.SourceLocation, rx, nodeArgIType, targetPackage, frame, compiler);
                        args[argsOffset + i] = new RLRange(originalArg.First, originalArg.Last, tmp.Result);
                    }
                    else
                    {
                        bool isConverted;
                        var tmp = this.ConvertTypeBeforeStore(node.SourceLocation, nodeArgIType, parameterType, rx, targetPackage, frame, compiler, out isConverted);
                        if (isConverted)
                        {
                            args[argsOffset + i] = new RLRange(originalArg.First, originalArg.Last, tmp.Result);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply type/content conversion of arguments just before a method call.
        /// </summary>
        private void ConvertParametersAfterCall(AstExpression node, List<RLRange> args, XMethodReference targetMethod, int argsOffset, List<RLRange> originalArgs)
        {
            // Unbox generic arrays
            for (var i = 0; i < targetMethod.Parameters.Count; i++)
            {
                var parameterType = targetMethod.Parameters[i].ParameterType;
                var nodeArgI = node.Arguments[argsOffset + i];
                var nodeArgIType = nodeArgI.GetResultType();
                var boxedArg = args[argsOffset + i];
                var original = originalArgs[argsOffset + i];
                if ((parameterType.IsGenericParameterArray()) && nodeArgIType.IsPrimitiveArray())
                {
                    // Convert back using Boxing class
                    this.UnboxGenericArray(node.SourceLocation, original.Result, boxedArg.Result, nodeArgIType, targetPackage, frame, compiler);
                }
            }

            // Store byref/out arguments
            foreach (var argument in node.Arguments.Where(x => x.Code == AstCode.ByRefArray || x.Code == AstCode.ByRefOutArray))
            {
                var stExpression = argument.StoreByRefExpression;
                stExpression.Accept(this, node);
            }
        }

        /// <summary>
        /// Generate code for a call to a method of an array type.
        /// </summary>
        private bool TryVisitArrayTypeMethodCallExpression(AstExpression node, XMethodReference ilMethodRef, List<RLRange> args, AstNode parent, out RLRange result)
        {
            var methodName = ilMethodRef.Name;
            var dimensions = (methodName == "Set") ? args.Count - 2 : args.Count - 1;

            // Get all but last dimensions
            var arr = frame.AllocateTemp(FrameworkReferences.Object);
            var first = this.Add(node.SourceLocation, RCode.Move_object, arr, args[0].Result);
            for (var d = 0; d < dimensions - 1; d++)
            {
                this.Add(node.SourceLocation, RCode.Aget_object, arr, arr, args[d + 1].Result);
            }

            // Get/Set value
            switch (methodName)
            {
                case "Get":
                    {
                        var valueType = node.GetResultType();
                        var lastIndexArg = args[args.Count - 1];
                        var agetCode = new XArrayType(valueType).AGet();
                        var resultReg = frame.AllocateTemp(valueType.GetReference(targetPackage));
                        var last = this.Add(node.SourceLocation, agetCode, resultReg, arr, lastIndexArg.Result);
                        result = new RLRange(first, last, resultReg);
                        return true;
                    }
                case "Set":
                    {
                        var valueType = node.Arguments[node.Arguments.Count - 1].GetResultType();
                        var lastIndexArg = args[args.Count - 2];
                        var aputCode = new XArrayType(valueType).APut();

                        // Perform type conversion if needed
                        bool isConverted;
                        var valueR = args[args.Count - 1].Result;
                        var converted = this.ConvertTypeBeforeStore(node.SourceLocation, valueType, valueType, valueR, targetPackage, frame, compiler, out isConverted);
                        if (isConverted) valueR = converted.Result;
                        
                        var last = this.Add(node.SourceLocation, aputCode, valueR, arr, lastIndexArg.Result);
                        result = new RLRange(first, last, arr);
                        return true;
                    }
                default:
                    result = null;
                    return false;
            }
        }

        
        /// <summary>
        /// Generate code for a call or callvirtual opcode.
        /// </summary>
        private RLRange VisitDexNativeCall(AstExpression node, List<RLRange> args, AstNode parent, XMethodDefinition ilMethod)
        {
            var fullName = ilMethod.DeclaringType.FullName;
            switch (fullName)
            {
                case "System.Threading.Monitor":
                    {
                        if (ilMethod.Parameters.Count == 1)
                        {
                            switch (ilMethod.Name)
                            {
                                case "Enter":
                                    return new RLRange(this.Add(node.SourceLocation, RCode.Monitor_enter, args[0].Result), null);
                                case "Exit":
                                    return new RLRange(this.Add(node.SourceLocation, RCode.Monitor_exit, args[0].Result), null);
                            }
                        }
                    }
                    break;
                case "System.Object":
                    {
                        if ((ilMethod.Parameters.Count == 2) && (ilMethod.Name == "ReferenceEquals"))
                        {
                            var r = frame.AllocateTemp(PrimitiveType.Int);
                            var start = this.Add(node.SourceLocation, RCode.Const, 0, r);
                            var test = this.Add(node.SourceLocation, AstCode.Ceq.Reverse().ToIfTest(), args[0].Result, args[1].Result);
                            this.Add(node.SourceLocation, RCode.Const, 1, r);
                            var end = this.Add(node.SourceLocation, RCode.Nop, null);
                            test.Operand = end;
                            return new RLRange(args, start, end, r);                           
                        }
                    }
                    break;
                case "System.Array":
                    {
                        if ((args.Count == 1) && (ilMethod.Name == "get_Length"))
                        {
                            var r = frame.AllocateTemp(PrimitiveType.Int);
                            var argType = node.Arguments[0].GetResultType();
                            if (argType.IsArray)
                            {
                                // Standard array
                                var first = this.Add(node.SourceLocation, RCode.Array_length, r, args[0].Result);
                                return new RLRange(args, first, r);
                            }
                            {
                                // Object, call via java.lang.reflect.Array.getLength
                                var getLength = FrameworkReferences.ArrayGetLength;
                                var first = this.Add(node.SourceLocation, RCode.Invoke_static, getLength, args[0].Result);
                                var last = this.Add(node.SourceLocation, RCode.Move_result, r);
                                return new RLRange(args, first, last, r);
                            }
                        }                        
                    }
                    break;
                case "System.Nullable`1":
                    {
                        if ((args.Count == 1) && (ilMethod.Name == "get_RawValue"))
                        {
                            return (RLRange) node.Arguments[0].Result;
                        }
                    }
                    break;
                case "Dot42.Internal.TypeHelper":
                    {
                        string typeName = null;
                        switch (ilMethod.Name)
                        {
                            case "BooleanType":
                                typeName = "java.lang.Boolean";
                                break;
                            case "ByteType":
                                typeName = "java.lang.Byte";
                                break;
                            case "CharacterType":
                                typeName = "java.lang.Character";
                                break;
                            case "ShortType":
                                typeName = "java.lang.Short";
                                break;
                            case "IntegerType":
                                typeName = "java.lang.Integer";
                                break;
                            case "LongType":
                                typeName = "java.lang.Long";
                                break;
                            case "FloatType":
                                typeName = "java.lang.Float";
                                break;
                            case "DoubleType":
                                typeName = "java.lang.Double";
                                break;
                        }
                        if (typeName != null)
                        {
                            var typeRef = new ClassReference(typeName);
                            var r = frame.AllocateTemp(new ClassReference("java.lang.Class"));
                            var constClass = this.Add(node.SourceLocation, RCode.Const_class, typeRef, r);
                            return new RLRange(args, constClass, r);
                        }
                    }
                    break;
            }
            throw new ArgumentException(string.Format("Unknown extern method {0} at {1}", ilMethod.FullName, FormatSource(node)));
        }

        /// <summary>
        /// Create a conversion code sequence.
        /// </summary>
        private RLRange ConvX(ISourceLocation sequencePoint,  RCode code, PrimitiveType type, RLRange arg)
        {
            if (code == RCode.Nop)
                return new RLRange(this.Add(sequencePoint, code), arg.Result);
            var r = frame.AllocateTemp(type);
            return new RLRange(this.Add(sequencePoint, code, r, arg.Result), r);
        }

        /// <summary>
        /// Create a conversion to int code sequence.
        /// </summary>
        private RLRange ConvToInt(ISourceLocation sequencePoint, RLRange arg)
        {
            RegisterSpec r;
            if (!arg.Result.IsWide)
            {
                // Float?
                if (arg.Result.Type.IsFloat())
                {
                    r = frame.AllocateTemp(PrimitiveType.Int);
                    return new RLRange(this.Add(sequencePoint, RCode.Float_to_int, r, arg.Result), r);
                }
                return arg;
            }
            else
            {
                // Long/double -> int
                r = frame.AllocateTemp(PrimitiveType.Int);
                var code = (arg.Result.Type.IsDouble()) ? RCode.Double_to_int : RCode.Long_to_int;
                return new RLRange(this.Add(sequencePoint, code, r, arg.Result), r);
            }
        }

        /// <summary>
        /// Lookup the value of a resource id with given name.
        /// </summary>
        private static int FindResourceId(Table resourceTable, string name)
        {
            foreach (var typeSpec in resourceTable.Packages[0].TypeSpecs)
            {
                if (typeSpec.EntryCount == 0)
                    continue;

                var index = 0;
                foreach (var entry in typeSpec.Entries)
                {
                    if (entry.Key == name)
                    {
                        var resId = 0x7F000000 | ((typeSpec.Id) << 16) | index;
                        return resId;
                    }
                    index++;
                }
            }
            throw new ArgumentException(string.Format("Unknown resource name '{0}'", name));
        }

        /// <summary>
        /// Gets a description of the source of the given node in the current method.
        /// </summary>
        private string FormatSource(AstExpression node)
        {
            var source = currentMethod.ToString();
            var seqp = node.SourceLocation;
            if (seqp != null)
            {

                source += " (";
                if (!string.IsNullOrEmpty(seqp.Document))
                {
                    source += seqp.Document;
                    source += ", ";
                }
                source += string.Format("position {0},{1}", seqp.StartLine, seqp.StartColumn);
                source += ")";
            }
            return source;
        }

        /// <summary>
        /// Gets the generic instance field of the current method.
        /// Throws an exception when there is no such field.
        /// </summary>
        private FieldDefinition GetGenericInstanceField()
        {
            var owner = currentDexMethod.Owner;
            var giField = owner.GenericInstanceField;
            if (giField == null)
            {
                throw new CompilerException(string.Format("Expected GenericInstance field in {0}", owner.Fullname));
            }
            return giField;
        }
    }
}