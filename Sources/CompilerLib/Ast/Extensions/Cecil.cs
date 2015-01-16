using System;
using Mono.Cecil;

namespace Dot42.CompilerLib.Ast.Extensions
{
    public static class Cecil
    {
        public static bool HasGeneratedName(this MemberReference member)
        {
            return member.Name.StartsWith("<", StringComparison.Ordinal);
        }

        public static bool IsCompilerGenerated(this ICustomAttributeProvider provider)
        {
            if (provider != null && provider.HasCustomAttributes)
            {
                foreach (CustomAttribute a in provider.CustomAttributes)
                {
                    if (a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
                        return true;
                }
            }
            return false;
        }

        public static bool IsCompilerGeneratedOrIsInCompilerGeneratedClass(this IMemberDefinition member)
        {
            if (member == null)
                return false;
            if (member.IsCompilerGenerated())
                return true;
            return IsCompilerGeneratedOrIsInCompilerGeneratedClass(member.DeclaringType);
        }

    }
}
