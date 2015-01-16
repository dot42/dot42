using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    /// <summary>
    /// Record spillings.
    /// </summary>
    internal class RegisterSpillingMapping
    {
        private readonly DexLib.Instructions.Register highRegister;
        private readonly DexLib.Instructions.Register lowRegister;

        /// <summary>
        /// Default ctor
        /// </summary>
        public RegisterSpillingMapping(Register highRegister, Register lowRegister)
        {
            this.highRegister = highRegister;
            this.lowRegister = lowRegister;
        }

        /// <summary>
        /// Register being replaced with a low-index register
        /// </summary>
        public Register HighRegister
        {
            get { return highRegister; }
        }

        /// <summary>
        /// The low-index replacement
        /// </summary>
        public Register LowRegister
        {
            get { return lowRegister; }
        }

        /// <summary>
        /// The first instruction where the low-register is valid.
        /// </summary>
        public Instruction FirstInstruction { get; set; }

        /// <summary>
        /// The last instruction where the low-register is valid.
        /// </summary>
        public Instruction LastInstruction { get; set; }
    }
}
