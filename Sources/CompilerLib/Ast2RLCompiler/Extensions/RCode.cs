using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using MethodReference = Dot42.DexLib.MethodReference;

namespace Dot42.CompilerLib.Ast2RLCompiler.Extensions
{
    /// <summary>
    /// Generate opcodes for given types.
    /// </summary>
    internal static partial class ILCompilerExtensions
    {
        private static readonly RCode[] Add2AddrOpcodes = new[] { RCode.Add_int_2addr, RCode.Add_long_2addr, RCode.Add_float_2addr, RCode.Add_double_2addr };
        private static readonly RCode[] Sub2AddrOpcodes = new[] { RCode.Sub_int_2addr, RCode.Sub_long_2addr, RCode.Sub_float_2addr, RCode.Sub_double_2addr };
        private static readonly RCode[] Mul2AddrOpcodes = new[] { RCode.Mul_int_2addr, RCode.Mul_long_2addr, RCode.Mul_float_2addr, RCode.Mul_double_2addr };
        private static readonly RCode[] Div2AddrOpcodes = new[] { RCode.Div_int_2addr, RCode.Div_long_2addr, RCode.Div_float_2addr, RCode.Div_double_2addr };
        private static readonly RCode[] Rem2AddrOpcodes = new[] { RCode.Rem_int_2addr, RCode.Rem_long_2addr, RCode.Rem_float_2addr, RCode.Rem_double_2addr };

        private static readonly RCode[] And2AddrOpcodes = new[] { RCode.And_int_2addr, RCode.And_long_2addr, RCode.Nop, RCode.Nop };
        private static readonly RCode[] Or2AddrOpcodes = new[] { RCode.Or_int_2addr, RCode.Or_long_2addr, RCode.Nop, RCode.Nop };
        private static readonly RCode[] Xor2AddrOpcodes = new[] { RCode.Xor_int_2addr, RCode.Xor_long_2addr, RCode.Nop, RCode.Nop };
        private static readonly RCode[] Shl2AddrOpcodes = new[] { RCode.Shl_int_2addr, RCode.Shl_long_2addr, RCode.Nop, RCode.Nop };
        private static readonly RCode[] Shr2AddrOpcodes = new[] { RCode.Shr_int_2addr, RCode.Shr_long_2addr, RCode.Nop, RCode.Nop };
        private static readonly RCode[] UShr2AddrOpcodes = new[] { RCode.Ushr_int_2addr, RCode.Ushr_long_2addr, RCode.Nop, RCode.Nop };

        private static readonly RCode[] AddOpcodes = new[] { RCode.Add_int, RCode.Add_long, RCode.Add_float, RCode.Add_double };
        private static readonly RCode[] SubOpcodes = new[] { RCode.Sub_int, RCode.Sub_long, RCode.Sub_float, RCode.Sub_double };
        private static readonly RCode[] MulOpcodes = new[] { RCode.Mul_int, RCode.Mul_long, RCode.Mul_float, RCode.Mul_double };
        private static readonly RCode[] DivOpcodes = new[] { RCode.Div_int, RCode.Div_long, RCode.Div_float, RCode.Div_double };
        private static readonly RCode[] RemOpcodes = new[] { RCode.Rem_int, RCode.Rem_long, RCode.Rem_float, RCode.Rem_double };

        private static readonly RCode[] AndOpcodes = new[] { RCode.And_int, RCode.And_long, RCode.Nop, RCode.Nop };
        private static readonly RCode[] OrOpcodes = new[] { RCode.Or_int, RCode.Or_long, RCode.Nop, RCode.Nop };
        private static readonly RCode[] XorOpcodes = new[] { RCode.Xor_int, RCode.Xor_long, RCode.Nop, RCode.Nop };
        private static readonly RCode[] ShlOpcodes = new[] { RCode.Shl_int, RCode.Shl_long, RCode.Nop, RCode.Nop };
        private static readonly RCode[] ShrOpcodes = new[] { RCode.Shr_int, RCode.Shr_long, RCode.Nop, RCode.Nop };
        private static readonly RCode[] UShrOpcodes = new[] { RCode.Ushr_int, RCode.Ushr_long, RCode.Nop, RCode.Nop };

        private static readonly RCode[] NegOpcodes = new[] { RCode.Neg_int, RCode.Neg_long, RCode.Neg_float, RCode.Neg_double };
        private static readonly RCode[] NotOpcodes = new[] { RCode.Not_int, RCode.Not_long, RCode.Nop, RCode.Nop };

        private static readonly RCode[] ToIntOpcodes = new[] { RCode.Nop, RCode.Long_to_int, RCode.Float_to_int, RCode.Double_to_int };
        private static readonly RCode[] ToLongOpcodes = new[] { RCode.Int_to_long, RCode.Nop, RCode.Float_to_long, RCode.Double_to_long};
        private static readonly RCode[] ToFloatOpcodes = new[] { RCode.Int_to_float, RCode.Long_to_float, RCode.Nop, RCode.Double_to_float };
        private static readonly RCode[] ToDoubleOpcodes = new[] { RCode.Int_to_double, RCode.Long_to_double, RCode.Float_to_double, RCode.Nop };

        private static readonly RCode[] ConstOpcodes = new[] { RCode.Const, RCode.Const_wide, RCode.Const, RCode.Const_wide };

        /// <summary>
        /// Generate an Add opcode.
        /// </summary>
        internal static RCode Add2Addr(this AstExpression expr) { return OpcodeForType(expr, Add2AddrOpcodes); }

        /// <summary>
        /// Generate a Sub opcode.
        /// </summary>
        internal static RCode Sub2Addr(this AstExpression expr) { return OpcodeForType(expr, Sub2AddrOpcodes); }

        /// <summary>
        /// Generate a Mul opcode.
        /// </summary>
        internal static RCode Mul2Addr(this AstExpression expr) { return OpcodeForType(expr, Mul2AddrOpcodes); }

        /// <summary>
        /// Generate a Div opcode.
        /// </summary>
        internal static RCode Div2Addr(this AstExpression expr) { return OpcodeForType(expr, Div2AddrOpcodes); }

        /// <summary>
        /// Generate a Rem opcode.
        /// </summary>
        internal static RCode Rem2Addr(this AstExpression expr) { return OpcodeForType(expr, Rem2AddrOpcodes); }

        /// <summary>
        /// Generate a And opcode.
        /// </summary>
        internal static RCode And2Addr(this AstExpression expr) { return OpcodeForType(expr, And2AddrOpcodes); }

        /// <summary>
        /// Generate a Or opcode.
        /// </summary>
        internal static RCode Or2Addr(this AstExpression expr) { return OpcodeForType(expr, Or2AddrOpcodes); }

        /// <summary>
        /// Generate a Xor opcode.
        /// </summary>
        internal static RCode Xor2Addr(this AstExpression expr) { return OpcodeForType(expr, Xor2AddrOpcodes); }

        /// <summary>
        /// Generate a Shl opcode.
        /// </summary>
        internal static RCode Shl2Addr(this AstExpression expr) { return OpcodeForType(expr, Shl2AddrOpcodes); }

        /// <summary>
        /// Generate a Shr opcode.
        /// </summary>
        internal static RCode Shr2Addr(this AstExpression expr) { return OpcodeForType(expr, Shr2AddrOpcodes); }

        /// <summary>
        /// Generate a UShr opcode.
        /// </summary>
        internal static RCode UShr2Addr(this AstExpression expr) { return OpcodeForType(expr, UShr2AddrOpcodes); }

        /// <summary>
        /// Generate an Add opcode.
        /// </summary>
        internal static RCode Add(this AstExpression expr) { return OpcodeForType(expr, AddOpcodes); }

        /// <summary>
        /// Generate a Sub opcode.
        /// </summary>
        internal static RCode Sub(this AstExpression expr) { return OpcodeForType(expr, SubOpcodes); }

        /// <summary>
        /// Generate a Mul opcode.
        /// </summary>
        internal static RCode Mul(this AstExpression expr) { return OpcodeForType(expr, MulOpcodes); }

        /// <summary>
        /// Generate a Div opcode.
        /// </summary>
        internal static RCode Div(this AstExpression expr) { return OpcodeForType(expr, DivOpcodes); }

        /// <summary>
        /// Generate a Rem opcode.
        /// </summary>
        internal static RCode Rem(this AstExpression expr) { return OpcodeForType(expr, RemOpcodes); }

        /// <summary>
        /// Generate a And opcode.
        /// </summary>
        internal static RCode And(this AstExpression expr) { return OpcodeForType(expr, AndOpcodes); }

        /// <summary>
        /// Generate a Or opcode.
        /// </summary>
        internal static RCode Or(this AstExpression expr) { return OpcodeForType(expr, OrOpcodes); }

        /// <summary>
        /// Generate a Xor opcode.
        /// </summary>
        internal static RCode Xor(this AstExpression expr) { return OpcodeForType(expr, XorOpcodes); }

        /// <summary>
        /// Generate a Shl opcode.
        /// </summary>
        internal static RCode Shl(this AstExpression expr) { return OpcodeForType(expr, ShlOpcodes); }

        /// <summary>
        /// Generate a Shr opcode.
        /// </summary>
        internal static RCode Shr(this AstExpression expr) { return OpcodeForType(expr, ShrOpcodes); }

        /// <summary>
        /// Generate a UShr opcode.
        /// </summary>
        internal static RCode UShr(this AstExpression expr) { return OpcodeForType(expr, UShrOpcodes); }

        /// <summary>
        /// Generate a Neg opcode.
        /// </summary>
        internal static RCode Neg(this AstExpression expr) { return OpcodeForType(expr, NegOpcodes); }

        /// <summary>
        /// Generate a Not opcode.
        /// </summary>
        internal static RCode Not(this AstExpression expr) { return OpcodeForType(expr, NotOpcodes); }

        /// <summary>
        /// Generate a Const opcode.
        /// </summary>
        internal static RCode Const(this AstExpression expr) { return OpcodeForType(expr, ConstOpcodes); }

        /// <summary>
        /// Generate a Const opcode.
        /// </summary>
        internal static RCode Const(this XTypeReference type) { return OpcodeForType(type, ConstOpcodes); }

        /// <summary>
        /// Generate a conv to I4 opcode.
        /// </summary>
        internal static RCode ConvI4(this AstExpression expr) { return OpcodeForType(expr, ToIntOpcodes); }

        /// <summary>
        /// Generate a conv to I8 opcode.
        /// </summary>
        internal static RCode ConvI8(this AstExpression expr) { return OpcodeForType(expr, ToLongOpcodes); }

        /// <summary>
        /// Generate a conv to R4 opcode.
        /// </summary>
        internal static RCode ConvR4(this AstExpression expr) { return OpcodeForType(expr, ToFloatOpcodes); }

        /// <summary>
        /// Generate a conv to R8 opcode.
        /// </summary>
        internal static RCode ConvR8(this AstExpression expr) { return OpcodeForType(expr, ToDoubleOpcodes); }

        /// <summary>
        /// Generate an Add opcode.
        /// </summary>
        private static RCode OpcodeForType(AstExpression expr, RCode[] opcodes)
        {
            return OpcodeForType(expr.GetResultType(), opcodes);
        }

        /// <summary>
        /// Generate an Add opcode.
        /// </summary>
        private static RCode OpcodeForType(XTypeReference type, RCode[] opcodes)
        {
            if (type.IsInt32() || type.IsUInt32() || type.IsInt16() || type.IsUInt16() || type.IsChar() || type.IsByte() || type.IsSByte() || type.IsBoolean()) return opcodes[0];
            if (type.IsInt64() || type.IsUInt64()) return opcodes[1];
            if (type.IsFloat()) return opcodes[2];
            if (type.IsDouble()) return opcodes[3];

            XTypeDefinition typeDef;
            if (type.TryResolve(out typeDef))
            {
                if (typeDef.IsEnum)
                {
                    return OpcodeForType(typeDef.GetEnumUnderlyingType(), opcodes);
                }
            }

            throw new ArgumentException("Unsupported type " + type);
        }

        /// <summary>
        /// Generate an Move opcode.
        /// </summary>
        internal static RCode Move(this AstExpression expr)
        {
            var type = expr.GetResultType();
            if (type.IsDexWide()) return RCode.Move_wide;
            if (type.IsVoid()) throw new ArgumentException("Unexpected void expression type");
            if (type.IsDexValue()) return RCode.Move;
            if (type.IsDexObject()) return RCode.Move_object;
            throw new ArgumentException("Unknown type in move " + type);
        }

        /// <summary>
        /// Generate an Move_result opcode.
        /// </summary>
        internal static RCode MoveResult(this XMethodDefinition method)
        {
            return method.ReturnType.MoveResult();
        }

        /// <summary>
        /// Generate an Move_result opcode.
        /// </summary>
        internal static RCode MoveResult(this XTypeReference type)
        {
            if (type.IsDexWide()) return RCode.Move_result_wide;
            if (type.IsVoid()) throw new ArgumentException("Unexpected void expression type");
            if (type.IsDexValue()) return RCode.Move_result;
            if (type.IsDexObject()) return RCode.Move_result_object;
            throw new ArgumentException("Unknown type in move_result " + type);
        }

        /// <summary>
        /// Generate an Return opcode.
        /// </summary>
        internal static RCode Return(this AstExpression expr, MethodSource currentMethod)
        {
            if (currentMethod.ReturnsDexWide) return RCode.Return_wide;
            if (currentMethod.ReturnsVoid) return RCode.Return_void;
            if (currentMethod.ReturnsDexValue) return RCode.Return;
            if (currentMethod.ReturnsDexObject) return RCode.Return_object;
            throw new ArgumentException("Unknown type in return " + currentMethod.FullName);
        }

        /// <summary>
        /// Generate an Invoke opcode.
        /// </summary>
        internal static RCode Invoke(this XMethodDefinition targetMethod, XMethodReference targetMethodRef, MethodSource currentMethod, bool isSpecial = false)
        {
            if (targetMethod != null)
            {
                if (targetMethod.DeclaringType.IsDelegate())
                {
                    return RCode.Invoke_interface;
                }
                if (targetMethod.IsStatic || targetMethod.IsAndroidExtension)
                {
                    return RCode.Invoke_static;
                }
                if ((currentMethod != null) && targetMethod.UseInvokeSuper(currentMethod.Method))
                {
                    return RCode.Invoke_super;
                }
                if (isSpecial && !targetMethod.IsConstructor && (currentMethod != null) && targetMethod.DeclaringType.IsBaseOf(currentMethod.Method.DeclaringType))
                {
                    return RCode.Invoke_super;
                }
                if (targetMethod.UseInvokeInterface)
                {
                    return RCode.Invoke_interface;                    
                }
                if (targetMethod.IsDirect)
                {
                    return RCode.Invoke_direct;
                }
                if (targetMethod.DeclaringType.IsInterface)
                {
                    return RCode.Invoke_interface;
                }
            }
            if (targetMethodRef != null)
            {
                if (!targetMethodRef.HasThis)
                {
                    return RCode.Invoke_static;
                }
                switch (targetMethodRef.Name)
                {
                    case "<init>":
                    case "<clinit>":
                    case ".ctor":
                    case ".cctor":
                        return RCode.Invoke_direct;                        
                }
            }
            return RCode.Invoke_virtual;
        }

        /// <summary>
        /// Generate a aput opcode.
        /// </summary>
        internal static RCode APut(this AstExpression expr)
        {
            var arrayType = expr.Arguments[0].GetResultType();
            if (!arrayType.IsArray)
                throw new ArgumentException("First argument must be of type array.");
            var elementType = arrayType.ElementType;
            if (elementType.IsEnum())
                return RCode.Aput_object;
            switch (expr.Code)
            {
                case AstCode.Stelem_I1:
                    return elementType.IsBoolean() ? RCode.Aput_boolean : RCode.Aput_byte;
                case AstCode.Stelem_I2:
                    return (elementType.IsInt16() || elementType.IsUInt16()) ? RCode.Aput_short : RCode.Aput_char;
                case AstCode.Stelem_I:
                case AstCode.Stelem_I4:
                case AstCode.Stelem_R4:
                    return RCode.Aput;
                case AstCode.Stelem_I8:
                case AstCode.Stelem_R8:
                    return RCode.Aput_wide;                    
                case AstCode.Stelem_Ref:
                case AstCode.Stelem_Any:
                    return APut(arrayType);
                    //return RCode.Aput_object;
                default:
                    throw new NotSupportedException("Unknown code " + expr.Code);
            }
        }

        /// <summary>
        /// Generate a aput opcode.
        /// </summary>
        internal static RCode APut(this XTypeReference arrayType)
        {
            if (!(arrayType.IsArray || arrayType.IsByReference))
                throw new ArgumentException("First argument must be of type array.");
            var elementType = arrayType.ElementType;
            if (elementType.IsDexBoolean()) return RCode.Aput_boolean;
            if (elementType.IsDexByte()) return RCode.Aput_byte;
            if (elementType.IsDexChar()) return RCode.Aput_char;
            if (elementType.IsDexShort()) return RCode.Aput_short;
            if (elementType.IsDexValue()) return RCode.Aput;
            if (elementType.IsDexWide()) return RCode.Aput_wide;
            if (elementType.IsDexObject() && !elementType.IsVoid()) return RCode.Aput_object;
            throw new NotSupportedException("Unknown type for aput " + arrayType);
        }

        /// <summary>
        /// Generate a convert opcode.
        /// </summary>
        internal static RCode AConstConvertBeforePut(this XTypeReference arrayType)
        {
            if (!(arrayType.IsArray || arrayType.IsByReference))
                throw new ArgumentException("First argument must be of type array.");
            var elementType = arrayType.ElementType;
            if (elementType.IsDexByte()) return RCode.Int_to_byte;
            if (elementType.IsDexChar()) return RCode.Int_to_char;
            if (elementType.IsDexShort()) return RCode.Int_to_short;
            return RCode.Nop;
        }

        /// <summary>
        /// Generate converter for the value of an Const instruction.
        /// </summary>
        internal static Func<object, object> ConstValueConverter(this XTypeReference elementType, bool keepUnsigned)
        {
            if (elementType.IsBoolean()) return x => Convert.ToBoolean(x) ? 1 : 0;
            if (elementType.IsByte())
            {
                if (keepUnsigned)
                    return x => (int)Convert.ToByte(x);
                return x => (int)((sbyte)unchecked(Convert.ToByte(x)));
            }
            if (elementType.IsSByte()) return x => (int)(Convert.ToSByte(x));
            if (elementType.IsChar()) return x => (int)(Convert.ToChar(x));
            if (elementType.IsUInt16()) return x => (int)(Convert.ToUInt16(x));
            if (elementType.IsInt16()) return x => (int)(Convert.ToInt16(x));
            if (elementType.IsInt32()) return x => XConvert.ToInt(x);
            if (elementType.IsUInt32()) return x => XConvert.ToInt(x);// unchecked((int)Convert.ToUInt32(Convert.ToInt64(x) & 0xFFFFFFFF)); 
            if (elementType.IsFloat()) return x => Convert.ToSingle(x);
            if (elementType.IsInt64()) return x => XConvert.ToLong(x);
            if (elementType.IsUInt64()) return x => XConvert.ToLong(x); // unchecked((long)Convert.ToInt64(x));
            if (elementType.IsDouble()) return x => Convert.ToDouble(x);
            if (elementType.IsDexObject() && !elementType.IsVoid()) return x => x;
            throw new NotSupportedException("Unknown type for constValueConverter " + elementType);
        }

        /// <summary>
        /// Generate a aget opcode.
        /// </summary>
        internal static RCode AGet(this AstExpression expr)
        {
            var arrayType = expr.Arguments[0].GetResultType();
            if (!(arrayType.IsArray || arrayType.IsByReference))
                throw new ArgumentException("First argument must be of type array.");
            var elementType = arrayType.ElementType;
            if (elementType.IsEnum())
                return RCode.Aget_object;            

            switch (expr.Code)
            {
                case AstCode.Ldelem_I1:
                case AstCode.Ldelem_U1:
                    return elementType.IsBoolean() ? RCode.Aget_boolean : RCode.Aget_byte;
                case AstCode.Ldelem_I2:
                case AstCode.Ldelem_U2:
                    return (elementType.IsInt16() || elementType.IsUInt16()) ? RCode.Aget_short : RCode.Aget_char;
                case AstCode.Ldelem_I:
                case AstCode.Ldelem_I4:
                case AstCode.Ldelem_R4:
                case AstCode.Ldelem_U4:
                    return RCode.Aget;
                case AstCode.Ldelem_I8:
                case AstCode.Ldelem_R8:
                    return RCode.Aget_wide;                    
                case AstCode.Ldelem_Ref:
                case AstCode.Ldelem_Any:
                    return AGet(arrayType);
                    //return RCode.Aget_object;                    
                default:
                    throw new NotSupportedException("Unknown code " + expr.Code);
            }
        }

        /// <summary>
        /// Generate a aget opcode.
        /// </summary>
        internal static RCode AGet(this XTypeReference arrayType)
        {
            if (!(arrayType.IsArray || arrayType.IsByReference))
                throw new ArgumentException("First argument must be of type array.");
            var elementType = arrayType.ElementType;

            if (elementType.IsDexBoolean()) return RCode.Aget_boolean;
            if (elementType.IsDexByte()) return RCode.Aget_byte;
            if (elementType.IsDexChar()) return RCode.Aget_char;
            if (elementType.IsDexShort()) return RCode.Aget_short;
            if (elementType.IsDexValue()) return RCode.Aget;
            if (elementType.IsDexWide()) return RCode.Aget_wide;
            if (elementType.IsDexObject() && !elementType.IsVoid()) return RCode.Aget_object;
            throw new NotSupportedException("Unknown type for aput " + arrayType);
        }

        /// <summary>
        /// Generate a iput opcode.
        /// </summary>
        internal static RCode IPut(this XFieldDefinition field)
        {
            var type = field.FieldType;
            return type.IPut();
        }

        /// <summary>
        /// Generate a iput opcode.
        /// </summary>
        internal static RCode IPut(this XTypeReference type)
        {
            if (type.IsDexWide()) return RCode.Iput_wide;
            if (type.IsVoid()) throw new ArgumentException("Unexpected void expression type");
            if (type.IsDexBoolean()) return RCode.Iput_boolean;
            if (type.IsDexChar()) return RCode.Iput_char;
            if (type.IsDexShort()) return RCode.Iput_short;
            if (type.IsDexByte()) return RCode.Iput_byte;
            if (type.IsDexValue()) return RCode.Iput;
            if (type.IsDexObject()) return RCode.Iput_object;
            throw new ArgumentException("Unknown type in iput " + type);
        }

        /// <summary>
        /// Generate a iget opcode.
        /// </summary>
        internal static RCode IGet(this XFieldDefinition field)
        {
            var type = field.FieldType;

            if (type.IsDexWide()) return RCode.Iget_wide;
            if (type.IsVoid()) throw new ArgumentException("Unexpected void expression type");
            if (type.IsDexBoolean()) return RCode.Iget_boolean;
            if (type.IsDexChar()) return RCode.Iget_char;
            if (type.IsDexShort()) return RCode.Iget_short;
            if (type.IsDexByte()) return RCode.Iget_byte;
            if (type.IsDexValue()) return RCode.Iget;
            if (type.IsDexObject()) return RCode.Iget_object;
            throw new ArgumentException("Unknown type in iget " + type);
        }

        /// <summary>
        /// Generate a sput opcode.
        /// </summary>
        internal static RCode SPut(this AstExpression expr)
        {
            var type = expr.GetResultType();

            if (type.IsDexWide()) return RCode.Sput_wide;
            if (type.IsVoid()) throw new ArgumentException("Unexpected void expression type");
            if (type.IsDexBoolean()) return RCode.Sput_boolean;
            if (type.IsDexChar()) return RCode.Sput_char;
            if (type.IsDexShort()) return RCode.Sput_short;
            if (type.IsDexByte()) return RCode.Sput_byte;
            if (type.IsDexValue()) return RCode.Sput;
            if (type.IsDexObject()) return RCode.Sput_object;
            throw new ArgumentException("Unknown type in sput " + type);
        }

        /// <summary>
        /// Generate a sget opcode.
        /// </summary>
        internal static RCode SGet(this AstExpression expr)
        {
            var type = expr.GetResultType();

            if (type.IsDexWide()) return RCode.Sget_wide;
            if (type.IsVoid()) throw new ArgumentException("Unexpected void expression type");
            if (type.IsDexBoolean()) return RCode.Sget_boolean;
            if (type.IsDexChar()) return RCode.Sget_char;
            if (type.IsDexShort()) return RCode.Sget_short;
            if (type.IsDexByte()) return RCode.Sget_byte;
            if (type.IsDexValue()) return RCode.Sget;
            if (type.IsDexObject()) return RCode.Sget_object;
            throw new ArgumentException("Unknown type in sget " + type);
        }

        /// <summary>
        /// Gets a dex code for cmpgfloat, cmplfloat or cmplong.
        /// </summary>
        internal static RCode CmpFloatOrLong(this AstExpression expr)
        {
            switch (expr.Code)
            {
                case AstCode.CmpLong:
                    return RCode.Cmp_long;
                case AstCode.CmpGFloat:
                    return expr.Arguments[0].IsFloat() ? RCode.Cmpg_float : RCode.Cmpg_double;
                case AstCode.CmpLFloat:
                    return expr.Arguments[0].IsFloat() ? RCode.Cmpl_float : RCode.Cmpl_double;
                default:
                    throw new ArgumentException("Unknown code " + (int)expr.Code);
            }
            
        }

        /// <summary>
        /// Is and invoke...range opcode required to call a method with the given arguments?
        /// </summary>
        internal static bool RequiresInvokeRange(this List<Register> argumentRegisters)
        {
            if (argumentRegisters.Count > 5)
                return true;
            // TODO check register in 4bit
            return false;
        }
        /// <summary>
        /// Get a dex if-test opcode for the given code
        /// </summary>
        internal static RCode ToIfTest(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Cle:
                    return RCode.If_le;
                case AstCode.Cle_Un:
                    return RCode.If_le;
                case AstCode.Clt:
                    return RCode.If_lt;
                case AstCode.Clt_Un:
                    return RCode.If_lt;
                case AstCode.Ceq:
                    return RCode.If_eq;
                case AstCode.Cne:
                    return RCode.If_ne;
                case AstCode.Cgt:
                    return RCode.If_gt;
                case AstCode.Cgt_Un:
                    return RCode.If_gt;
                case AstCode.Cge:
                    return RCode.If_ge;
                case AstCode.Cge_Un:
                    return RCode.If_ge;
                case AstCode.__Beq:
                    return RCode.If_eq;
                case AstCode.__Bne_Un:
                    return RCode.If_ne;
                case AstCode.__Ble:
                case AstCode.__Ble_Un:
                    return RCode.If_le;
                case AstCode.__Blt:
                case AstCode.__Blt_Un:
                    return RCode.If_lt;
                case AstCode.__Bgt:
                case AstCode.__Bgt_Un:
                    return RCode.If_gt;
                case AstCode.__Bge:
                case AstCode.__Bge_Un:
                    return RCode.If_ge;
                default:
                    throw new ArgumentOutOfRangeException("code", code.ToString());
            }
        }

        /// <summary>
        /// Get a dex if-testz opcode for the given code
        /// </summary>
        internal static RCode ToIfTestZ(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Cle:
                    return RCode.If_lez;
                case AstCode.Cle_Un:
                    return RCode.If_lez;
                case AstCode.Clt:
                    return RCode.If_ltz;
                case AstCode.Clt_Un:
                    return RCode.If_ltz;
                case AstCode.Ceq:
                    return RCode.If_eqz;
                case AstCode.Cne:
                    return RCode.If_nez;
                case AstCode.Cgt:
                    return RCode.If_gtz;
                case AstCode.Cgt_Un:
                    return RCode.If_gtz;
                case AstCode.Cge:
                    return RCode.If_gez;
                case AstCode.Cge_Un:
                    return RCode.If_gez;
                case AstCode.__Beq:
                    return RCode.If_eqz;
                case AstCode.__Bne_Un:
                    return RCode.If_nez;
                case AstCode.__Ble:
                case AstCode.__Ble_Un:
                    return RCode.If_lez;
                case AstCode.__Blt:
                case AstCode.__Blt_Un:
                    return RCode.If_ltz;
                case AstCode.__Bgt:
                case AstCode.__Bgt_Un:
                    return RCode.If_gtz;
                case AstCode.__Bge:
                case AstCode.__Bge_Un:
                    return RCode.If_gez;
                case AstCode.CIsNotNull:
                    return RCode.If_nez;
                case AstCode.CIsNull:
                    return RCode.If_eqz;
                case AstCode.Brtrue:
                    return RCode.If_nez;
                case AstCode.Brfalse:
                    return RCode.If_eqz;
                case AstCode.BrIfEq:
                    return RCode.If_eqz;
                case AstCode.BrIfNe:
                    return RCode.If_nez;
                case AstCode.BrIfGe:
                    return RCode.If_gez;
                case AstCode.BrIfGt:
                    return RCode.If_gtz;
                case AstCode.BrIfLe:
                    return RCode.If_lez;
                case AstCode.BrIfLt:
                    return RCode.If_ltz;
                default:
                    throw new ArgumentOutOfRangeException("code", code.ToString());
            }
        }

        /// <summary>
        /// Gets a box method for the given primitive type.
        /// </summary>
        internal static MethodReference GetBoxValueOfMethod(this XTypeReference type)
        {
            return BoxInfo.GetBoxValueOfMethod(type);
        }

        /// <summary>
        /// Gets a unbox method for the given primitive type.
        /// </summary>
        internal static MethodReference GetUnboxValueMethod(this XTypeReference type, AssemblyCompiler compiler, DexTargetPackage targetPackage, out RCode convertAfterCode)
        {
            return BoxInfo.GetUnboxValueMethod(type, compiler, targetPackage, out convertAfterCode);
        }

        /// <summary>
        /// Gets a unbox method for the given primitive type.
        /// </summary>
        internal static ClassReference GetBoxedType(this XTypeReference type)
        {
            return BoxInfo.GetBoxedType(type);
        }

        /// <summary>
        /// Is the given operand on an <see cref="AstCode.InitArray"/> suitable for
        /// using the <see cref="RCode.Fill_array_data"/> opcode?
        /// </summary>
        internal static bool IsSupportedByFillArrayData(this InitArrayData data)
        {
            if (data.ArrayType.Dimensions.Count() != 1)
                return false;
            var elementType = data.ArrayType.ElementType;
            return elementType.IsSByte() || elementType.IsInt16() || elementType.IsInt32() || elementType.IsInt64();
        }
    }
}
