using System;

namespace Dot42.DexLib.Instructions
{
    public static class Extensions
    {
        /// <summary>
        /// Is the given code any branch code?
        /// </summary>
        public static bool IsBranch(this OpCodes code)
        {
            switch (code)
            {
                case OpCodes.Goto:
                case OpCodes.Goto_16:
                case OpCodes.Goto_32:
                case OpCodes.If_eq:
                case OpCodes.If_eqz:
                case OpCodes.If_ge:
                case OpCodes.If_gez:
                case OpCodes.If_gt:
                case OpCodes.If_gtz:
                case OpCodes.If_le:
                case OpCodes.If_lez:
                case OpCodes.If_lt:
                case OpCodes.If_ltz:
                case OpCodes.If_ne:
                case OpCodes.If_nez:
                case OpCodes.Packed_switch:
                case OpCodes.Sparse_switch:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code any unconditional branch code?
        /// </summary>
        public static bool IsUnconditionalBranch(this OpCodes code)
        {
            switch (code)
            {
                case OpCodes.Goto:
                case OpCodes.Goto_16:
                case OpCodes.Goto_32:
                case OpCodes.Throw:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code any normal invoke code?
        /// </summary>
        public static bool IsInvoke(this OpCodes code)
        {
            switch (code)
            {
                case OpCodes.Invoke_virtual:
                case OpCodes.Invoke_super:
                case OpCodes.Invoke_direct:
                case OpCodes.Invoke_static:
                case OpCodes.Invoke_interface:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given code any return code?
        /// </summary>
        public static bool IsReturn(this OpCodes code)
        {
            switch (code)
            {
                case OpCodes.Return:
                case OpCodes.Return_object:
                case OpCodes.Return_void:
                case OpCodes.Return_wide:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Will the given opcode branch, throw, invoke,
        /// or return, i.e. will it not always advance the
        /// instruction pointer to the next instruction?
        /// </summary>
        public static bool IsJump(this OpCodes code)
        {
            return code.IsBranch() 
                || code.IsInvoke()
                || code.IsUnconditionalBranch() 
                || code.IsReturn();
        }

        /// <summary>
        /// Is the given code any normal invoke code?
        /// </summary>
        public static OpCodes InvokeToRange(this OpCodes code)
        {
            switch (code)
            {
                case OpCodes.Invoke_virtual:
                    return OpCodes.Invoke_virtual_range;
                case OpCodes.Invoke_super:
                    return OpCodes.Invoke_super_range;
                case OpCodes.Invoke_direct:
                    return OpCodes.Invoke_direct_range;
                case OpCodes.Invoke_static:
                    return OpCodes.Invoke_static_range;
                case OpCodes.Invoke_interface:
                    return OpCodes.Invoke_interface_range;
                default:
                    throw new ArgumentException("Not an invoke code");
            }
        }

        /// <summary>
        /// Is the given code a move_result_x opcode?
        /// </summary>
        public static bool IsMoveResult(this OpCodes code)
        {
            switch (code)
            {
                case OpCodes.Move_result:
                case OpCodes.Move_result_object:
                case OpCodes.Move_result_wide:
                    return true;
                default:
                    return false;
            }
        }
    }
}
