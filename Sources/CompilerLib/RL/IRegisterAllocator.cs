using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    internal interface IRegisterAllocator
    {

        /// <summary>
        /// Allocate a register for the given type for use as temporary calculation value.
        /// </summary>
        RegisterSpec AllocateTemp(TypeReference type);
    }
}
