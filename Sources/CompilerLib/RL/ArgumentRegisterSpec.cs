using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// RegisterSpec used for method arguments
    /// </summary>
    public sealed class ArgumentRegisterSpec : RegisterSpec
    {
        private readonly IParameter parameter;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ArgumentRegisterSpec(Register register, Register wideRegister, TypeReference typeReference, IParameter parameter) :
            base(register, wideRegister, typeReference)
        {
            this.parameter = parameter;
        }

        /// <summary>
        /// Gets the original parameter
        /// </summary>
        public IParameter Parameter
        {
            get { return parameter; }
        }
    }
}
