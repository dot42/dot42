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
            var ownerRef = (ClassReference) methodRef.Owner;
            var owner = target.GetClass(ownerRef.Fullname);
            if (owner == null)
                return false;
            result = owner.Methods.SingleOrDefault(x => (x.Name == methodRef.Name) && (x.Prototype.Equals(methodRef.Prototype)));
            return (result != null);
        }
    }
}
