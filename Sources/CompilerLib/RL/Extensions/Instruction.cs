namespace Dot42.CompilerLib.RL.Extensions
{
    partial class RLExtensions
    {
        /// <summary>
        /// Convert the given instruction to NOP.
        /// </summary>
        public static void ConvertToNop(this Instruction ins)
        {
            ins.Code = RCode.Nop;
            ins.Registers.Clear();
            ins.Operand = null;
        }
    }
}
