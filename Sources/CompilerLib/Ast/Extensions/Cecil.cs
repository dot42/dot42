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
        
        /// <summary>
        /// Should we suppress a specific message?
        /// </summary>
        internal static bool HasSuppressMessageAttribute(this ICustomAttributeProvider provider, string messageCode)
        {
            if (provider != null && provider.HasCustomAttributes)
            {
                foreach (CustomAttribute a in provider.CustomAttributes)
                {
                    if (a.AttributeType.FullName != "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute")
                        continue;
                    if (a.ConstructorArguments.Count != 2)
                        continue;
                    if (a.ConstructorArguments[0].Value == null ||
                        a.ConstructorArguments[0].Value.ToString().ToLowerInvariant() != "dot42")
                        continue;
                 
                    string code = (a.ConstructorArguments[1].Value ?? "").ToString().ToLowerInvariant();
                    
                    if (code.Equals(messageCode, StringComparison.InvariantCultureIgnoreCase) ||
                        code.StartsWith(messageCode + ":", StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            return false;
        }
    }
}
