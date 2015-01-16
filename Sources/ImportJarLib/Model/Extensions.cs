using System.Reflection;

namespace Dot42.ImportJarLib.Model
{
    internal static class Extensions
    {
        /// <summary>
        /// Set a bit value
        /// </summary>
        internal static TypeAttributes Set(this TypeAttributes source, bool value, TypeAttributes bit)
        {
            if (value)
                return source | bit;
            return source & ~bit;
        }

        /// <summary>
        /// Set a bit value
        /// </summary>
        internal static MethodAttributes Set(this MethodAttributes source, bool value, MethodAttributes bit)
        {
            if (value)
                return source | bit;
            return source & ~bit;
        }

        /// <summary>
        /// Do both members have the same visibility?
        /// This takes into account differences in scope related to "protected internal".
        /// </summary>
        internal static bool HasSameVisibility(this INetMemberVisibility member1, INetMemberVisibility member2)
        {
            if (member1.IsPublic) return member2.IsPublic;
            if (member1.IsPrivate) return member2.IsPrivate;
            if (member1.IsAssembly) return member2.IsAssembly;
            if (member1.IsFamilyAndAssembly) return member2.IsFamilyAndAssembly;
            if (member1.IsFamilyOrAssembly)
            {
                if (member1.HasSameScope(member2))
                    return member2.IsFamilyOrAssembly;
                return member2.IsFamily || member2.IsFamilyOrAssembly;
            }
            if (member1.IsFamily)
            {
                if (member1.HasSameScope(member2))
                    return member2.IsFamily;
                return member2.IsFamily || member2.IsFamilyOrAssembly;
            }
            return true;
        }

        /// <summary>
        /// Copy the visibility from source to target, taking different scopes into account.
        /// </summary>
        internal static void CopyVisibilityFrom(this INetMemberVisibility target, INetMemberVisibility source)
        {
            if (source.IsPublic) target.IsPublic = true;
            else if (source.IsPrivate) target.IsPrivate = true;
            else if (source.IsAssembly) target.IsAssembly = true;
            else if (source.IsFamilyAndAssembly) target.IsFamilyAndAssembly = true;
            else if (source.IsFamilyOrAssembly)
            {
                if (source.HasSameScope(target))
                    target.IsFamilyOrAssembly = true;
                else
                    target.IsFamily = true;
            }
            else if (source.IsFamily) target.IsFamily = true;
        }
    }
}
