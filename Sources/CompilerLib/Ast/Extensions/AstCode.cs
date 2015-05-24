using System;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Extensions
{
    /// <summary>
    /// Generate opcodes for given types.
    /// </summary>
    public static partial class AstExtensions
    {
        /// <summary>
        /// Is the given code a Cxx comparision code?
        /// </summary>
        public static bool IsCompare(this AstCode code)
        {
            switch (code)
            {
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
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code a Cxx comparision code that requires an integer?
        /// </summary>
        public static bool IsIntegerOnlyCompare(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Cle:
                case AstCode.Cle_Un:
                case AstCode.Clt:
                case AstCode.Clt_Un:
                case AstCode.Cgt:
                case AstCode.Cgt_Un:
                case AstCode.Cge:
                case AstCode.Cge_Un:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Reverse conditions
        /// </summary>
        public static AstCode Reverse(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Cle:
                    return AstCode.Cgt;
                case AstCode.Cle_Un:
                    return AstCode.Cgt_Un;
                case AstCode.Clt:
                    return AstCode.Cge;
                case AstCode.Clt_Un:
                    return AstCode.Cge_Un;
                case AstCode.Ceq:
                    return AstCode.Cne;
                case AstCode.Cne:
                    return AstCode.Ceq;
                case AstCode.Cgt:
                    return AstCode.Cle;
                case AstCode.Cgt_Un:
                    return AstCode.Cle_Un;
                case AstCode.Cge:
                    return AstCode.Clt;
                case AstCode.Cge_Un:
                    return AstCode.Clt_Un;
                case AstCode.CIsNull:
                    return AstCode.CIsNotNull;
                case AstCode.CIsNotNull:
                    return AstCode.CIsNull;
                default:
                    throw new ArgumentOutOfRangeException("code", code.ToString());
            }
        }

        /// <summary>
        /// Convert condition to branch
        /// </summary>
        public static AstCode ToBranch(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Cle:
                    return AstCode.__Ble;
                case AstCode.Cle_Un:
                    return AstCode.__Ble_Un;
                case AstCode.Clt:
                    return AstCode.__Blt;
                case AstCode.Clt_Un:
                    return AstCode.__Blt_Un;
                case AstCode.Ceq:
                    return AstCode.__Beq;
                case AstCode.Cne:
                    return AstCode.__Bne_Un;
                case AstCode.Cgt:
                    return AstCode.__Bgt;
                case AstCode.Cgt_Un:
                    return AstCode.__Bgt_Un;
                case AstCode.Cge:
                    return AstCode.__Bge;
                case AstCode.Cge_Un:
                    return AstCode.__Bge_Un;
                default:
                    throw new ArgumentOutOfRangeException("code", code.ToString());
            }
        }

        /// <summary>
        /// Convert condition to branch/zero.
        /// 
        /// </summary>
        public static AstCode ToBranchZ(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Cle:
                case AstCode.Cle_Un:
                    return AstCode.BrIfLe;
                case AstCode.Clt:
                case AstCode.Clt_Un:
                    return AstCode.BrIfLt;
                case AstCode.Ceq:
                    return AstCode.BrIfEq;
                case AstCode.Cne:
                    return AstCode.BrIfNe;
                case AstCode.Cgt:
                case AstCode.Cgt_Un:
                    return AstCode.BrIfGt;
                case AstCode.Cge:
                case AstCode.Cge_Un:
                    return AstCode.BrIfGe;
                default:
                    throw new ArgumentOutOfRangeException("code", code.ToString());
            }
        }


        /// <summary>
        /// Gets the type of stelem code to use for the given element type.
        /// </summary>
        public static AstCode GetStElemCode(this XTypeReference elementType)
        {
            if (elementType.IsByte() || elementType.IsSByte() || elementType.IsBoolean()) return AstCode.Stelem_I1;
            if (elementType.IsChar() || elementType.IsInt16() || elementType.IsUInt16()) return AstCode.Stelem_I2;
            if (elementType.IsInt32() || elementType.IsUInt32()) return AstCode.Stelem_I4;
            if (elementType.IsInt64() || elementType.IsUInt64()) return AstCode.Stelem_I8;
            if (elementType.IsFloat()) return AstCode.Stelem_R4;
            if (elementType.IsDouble()) return AstCode.Stelem_R8;
            return AstCode.Stelem_Ref;
        }

        /// <summary>
        /// Gets the type of ldelem code to use for the given element type.
        /// </summary>
        public static AstCode GetLdElemCode(this XTypeReference elementType)
        {
            if (elementType.IsByte()) return AstCode.Ldelem_U1;
            if (elementType.IsSByte() || elementType.IsBoolean()) return AstCode.Ldelem_I1;
            if (elementType.IsChar() || elementType.IsUInt16()) return AstCode.Ldelem_U2;
            if (elementType.IsInt16()) return AstCode.Ldelem_I2;
            if (elementType.IsUInt32()) return AstCode.Ldelem_U4;
            if (elementType.IsInt32()) return AstCode.Ldelem_I4;
            if (elementType.IsUInt64()) return AstCode.Ldelem_I8;
            if (elementType.IsInt64()) return AstCode.Ldelem_I8;
            if (elementType.IsFloat()) return AstCode.Ldelem_R4;
            if (elementType.IsDouble()) return AstCode.Ldelem_R8;
            return AstCode.Ldelem_Ref;
        }
    }
}
