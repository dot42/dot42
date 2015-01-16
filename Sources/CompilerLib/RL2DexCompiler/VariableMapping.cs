using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    /// <summary>
    /// Mapping between variable and register.
    /// </summary>
    internal class VariableMapping
    {
        private readonly DexLib.Instructions.Register register;
        private readonly IVariable variable;

        /// <summary>
        /// Initialize default mapping.
        /// </summary>
        public VariableMapping(Register register, IVariable variable)
        {
            this.register = register;
            this.variable = variable;
        }

        /// <summary>
        /// Gets the allocated register for the variable.
        /// </summary>
        public Register Register
        {
            get { return register; }
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        public IVariable Variable
        {
            get { return variable; }
        }
    }
}
