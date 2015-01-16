using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    internal interface IRLTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        void Transform(Dex target, MethodBody body);
    }
}
