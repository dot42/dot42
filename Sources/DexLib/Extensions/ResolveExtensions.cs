using System.Linq;

namespace Dot42.DexLib.Extensions
{
    public static class ResolveExtensions
    {
        /// <summary>
        /// Resolve the given method to a method definition.
        /// </summary>
        public static bool TryResolve(this MethodReference methodRef, Dex target, out MethodDefinition result)
        {
            result = methodRef as MethodDefinition;
            if (result != null)
                return true;
            result = target.GetMethod(methodRef.Owner, methodRef.Name, methodRef.Prototype);
            return result != null;
        }
    }
}
