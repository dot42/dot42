using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL.Extensions
{
    partial class RLExtensions
    {
        /// <summary>
        /// Convert RCode to OpCodes.
        /// </summary>
        public static OpCodes ToDex(this RCode code)
        {
            switch (code)
            {
                // Special cases                    
                case RCode.Leave:
                    return OpCodes.Goto;
                default:
                    return (OpCodes)code;
            }
        }

        /// <summary>
        /// Is the given code an const_x code?
        /// </summary>
        public static bool IsConst(this RCode code)
        {
            switch (code)
            {
                case RCode.Const:
                case RCode.Const_class:
                case RCode.Const_wide:
                case RCode.Const_string:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code an move_x code?
        /// </summary>
        public static bool IsMove(this RCode code)
        {
            switch (code)
            {
                case RCode.Move:
                case RCode.Move_object:
                case RCode.Move_wide:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code an invoke_x code?
        /// </summary>
        public static bool IsInvoke(this RCode code)
        {
            switch (code)
            {
                case RCode.Invoke_direct:
                case RCode.Invoke_interface:
                case RCode.Invoke_static:
                case RCode.Invoke_super:
                case RCode.Invoke_virtual:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code any branch code?
        /// </summary>
        public static bool IsBranch(this RCode code)
        {
            switch (code)
            {
                case RCode.Goto:
                case RCode.Leave:
                case RCode.If_eq:
                case RCode.If_eqz:
                case RCode.If_ge:
                case RCode.If_gez:
                case RCode.If_gt:
                case RCode.If_gtz:
                case RCode.If_le:
                case RCode.If_lez:
                case RCode.If_lt:
                case RCode.If_ltz:
                case RCode.If_ne:
                case RCode.If_nez:
                case RCode.Packed_switch:
                case RCode.Sparse_switch:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code any unconditional branch code?
        /// </summary>
        public static bool IsUnconditionalBranch(this RCode code)
        {
            switch (code)
            {
                case RCode.Goto:
                case RCode.Leave:
                case RCode.Throw:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code any return code?
        /// </summary>
        public static bool IsReturn(this RCode code)
        {
            switch (code)
            {
                case RCode.Return:
                case RCode.Return_object:
                case RCode.Return_void:
                case RCode.Return_wide:
                    return true;
                default:
                    return false;
            }
        }
    }
}
