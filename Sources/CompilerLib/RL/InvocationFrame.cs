using System.Collections.Generic;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Method invocation frame.
    /// This class is used to allocate registers.
    /// </summary>
    internal abstract class InvocationFrame : IRegisterAllocator
    {
        protected readonly List<ArgumentRegisterSpec> arguments = new List<ArgumentRegisterSpec>();
        protected readonly Dictionary<IVariable, VariableRegisterSpec> variables = new Dictionary<IVariable, VariableRegisterSpec>();

        /// <summary>
        /// Gets the register spec used to hold "this".
        /// Can be null.
        /// </summary>
        internal abstract ArgumentRegisterSpec ThisArgument { get; }

        /// <summary>
        /// Gets the register spec used to hold the type genericinstance value.
        /// </summary>
        public IList<ArgumentRegisterSpec> GenericInstanceTypeArguments { get; protected set; }

        /// <summary>
        /// Gets the register spec used to hold the method genericinstance value.
        /// </summary>
        public IList<ArgumentRegisterSpec> GenericInstanceMethodArguments { get; protected set; }

        /// <summary>
        /// Gets the register specs used for arguments passed to this method.
        /// </summary>
        internal IEnumerable<ArgumentRegisterSpec> Arguments { get { return arguments; } }

        /// <summary>
        /// Gets the register specs used for variables used in this method.
        /// </summary>
        internal IEnumerable<VariableRegisterSpec> Variables { get { return variables.Values; } }

        /// <summary>
        /// Allocate a register for the given type for use as temporary calculation value.
        /// </summary>
        public RegisterSpec AllocateTemp(TypeReference type)
        {
            return Allocate(type, false, RCategory.Temp, null);
        }

        /// <summary>
        /// Allocate a register for the given type.
        /// </summary>
        protected abstract RegisterSpec Allocate(TypeReference type, bool forceObject, RCategory category, object parameter);
    }
}
