using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    internal interface IRLTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// <returns>for implementations that support incremental optimizations,
        /// returns true if changes were made.</returns>
        /// </summary>
        bool Transform(Dex target, MethodBody body);
    }
}
