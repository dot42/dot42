namespace Dot42.CompilerLib.Ast
{
    public static class AstCodeUtil
    {
        public static string GetName(this AstCode code)
        {
            return code.ToString().ToLowerInvariant().TrimStart('_').Replace('_', '.');
        }

        public static bool IsConditionalControlFlow(this AstCode code)
        {
            switch (code)
            {
                case AstCode.__Brfalse_S:
                case AstCode.__Brtrue_S:
                case AstCode.__Beq_S:
                case AstCode.__Bge_S:
                case AstCode.__Bgt_S:
                case AstCode.__Ble_S:
                case AstCode.__Blt_S:
                case AstCode.__Bne_Un_S:
                case AstCode.__Bge_Un_S:
                case AstCode.__Bgt_Un_S:
                case AstCode.__Ble_Un_S:
                case AstCode.__Blt_Un_S:
                case AstCode.Brfalse:
                case AstCode.Brtrue:
                case AstCode.__Beq:
                case AstCode.__Bge:
                case AstCode.__Bgt:
                case AstCode.__Ble:
                case AstCode.__Blt:
                case AstCode.__Bne_Un:
                case AstCode.__Bge_Un:
                case AstCode.__Bgt_Un:
                case AstCode.__Ble_Un:
                case AstCode.__Blt_Un:
                case AstCode.Switch:
                case AstCode.LookupSwitch:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUnconditionalControlFlow(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Br:
                case AstCode.__Br_S:
                case AstCode.Leave:
                case AstCode.__Leave_S:
                case AstCode.Ret:
                case AstCode.Endfilter:
                case AstCode.Endfinally:
                case AstCode.Throw:
                case AstCode.Rethrow:
                case AstCode.LoopOrSwitchBreak:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given any of the call codes.
        /// </summary>
        public static bool IsCall(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Call:
                case AstCode.Calli:
                case AstCode.CallIntf:
                case AstCode.Callvirt:
                case AstCode.CallSpecial:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the given any of binary operations.
        /// </summary>
        public static bool IsBinaryOperation(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Add:
                case AstCode.Add_Ovf:
                case AstCode.Add_Ovf_Un:
                case AstCode.Sub:
                case AstCode.Sub_Ovf:
                case AstCode.Sub_Ovf_Un:
                case AstCode.Mul:
                case AstCode.Mul_Ovf:
                case AstCode.Mul_Ovf_Un:
                case AstCode.Div:
                case AstCode.Div_Un:
                case AstCode.Rem:
                case AstCode.Rem_Un:
                case AstCode.And:
                case AstCode.Or:
                case AstCode.Xor:
                case AstCode.Shl:
                case AstCode.Shr:
                case AstCode.Shr_Un:
                    return true;
                default:
                    return false;
            }
        }
    }
}
