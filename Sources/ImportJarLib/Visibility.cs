using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    internal static class Visibility
    {
        /// <summary>
        /// Make sure that the visibility of the given type definition is high enough to be used in the signature of the given member.
        /// </summary>
        /// <returns>True if type has changed</returns>
        public static bool EnsureVisibility(this NetTypeReference type, INetMemberVisibility member)
        {
            if (type == null)
                return false;
            return type.Accept(EnsureVisibilityTypeVisitor.Instance, member);
        }

        /// <summary>
        /// Make sure that the visibility of the given member is limited by the visibility of the given type is that is in another scope.
        /// </summary>
        /// <returns>True if member has changed</returns>
        public static bool LimitVisibility(this INetMemberVisibility member, NetTypeReference type)
        {
            if (type == null)
                return false;
            return type.Accept(LimitVisibilityTypeVisitor.Instance, member);
        }

        /// <summary>
        /// Reduce visibility from FamOrAssem to Assembly if declaring type is sealed.
        /// </summary>
        public static void LimitIfDeclaringTypeSealed(this INetMemberVisibility member, NetTypeDefinition declaringType)
        {
            if ((declaringType != null) && (declaringType.IsSealed) && member.IsFamilyOrAssembly)
            {
                member.IsAssembly = true;
            }
        }

        /// <summary>
        /// Make sure that the visibility of the given type definition is high enough to be used in the signature of the given member.
        /// </summary>
        private class EnsureVisibilityTypeVisitor : INetTypeVisitor<bool, INetMemberVisibility>
        {
            internal static readonly EnsureVisibilityTypeVisitor Instance = new EnsureVisibilityTypeVisitor();

            public bool Visit(NetTypeDefinition type, INetMemberVisibility member)
            {
                if (!member.HasSameScope(type))
                    return false;

                if (member.IsPublic || member.IsFamily || member.IsFamilyOrAssembly || member.IsNestedPublic)
                {
                    // Member is publicly visible
                    if (type.IsNotPublic)
                    {
                        type.IsPublic = true;
                        return true;
                    }
                    if (type.IsNestedPrivate || type.IsNestedFamily || type.IsNestedFammilyOrAssembly || type.IsNestedAssembly)
                    {
                        type.IsNestedPublic = true;
                        return true;
                    }
                }
                else if (member.IsAssembly)
                {
                    // Member is internally visible
                    if (type.IsNestedPrivate)
                    {
                        type.IsNestedAssembly = true;
                        return true;
                    }                    
                }
                return false;
            }

            public bool Visit(NetGenericParameter type, INetMemberVisibility member)
            {
                return false;
            }

            public bool Visit(NetGenericInstanceType type, INetMemberVisibility member)
            {
                var rc = type.ElementType.Accept(this, member);
                if (type.DeclaringType != null)
                {
                    if (type.DeclaringType.Accept(this, member))
                        rc = true;
                }
                foreach (var arg in type.GenericArguments)
                {
                    if (arg.Accept(this, member))
                        rc = true;
                }
                return rc;
            }

            public bool Visit(NetArrayType type, INetMemberVisibility member)
            {
                return type.ElementType.Accept(this, member);
            }

            public bool Visit(NetNullableType type, INetMemberVisibility member)
            {
                return type.ElementType.Accept(this, member);
            }
        }

        /// <summary>
        /// Make sure that the visibility of the given member is limited by the visibility of the given type is that is in another scope.
        /// </summary>
        private class LimitVisibilityTypeVisitor : INetTypeVisitor<bool, INetMemberVisibility>
        {
            internal static readonly LimitVisibilityTypeVisitor Instance = new LimitVisibilityTypeVisitor();

            public bool Visit(NetTypeDefinition type, INetMemberVisibility member)
            {
                if (member.HasSameScope(type))
                    return false;

                // TODO
                if (member.IsFamilyOrAssembly)
                {
                    // Member is publicly visible
                    if (type.IsNestedFammilyOrAssembly)
                    {
                        member.IsFamily = true;
                        return true;
                    }
                }
                return false;
            }

            public bool Visit(NetGenericParameter item, INetMemberVisibility member)
            {
                return false;
            }

            public bool Visit(NetGenericInstanceType item, INetMemberVisibility member)
            {
                var rc = item.ElementType.Accept(this, member);
                if (item.DeclaringType != null)
                {
                    if (item.DeclaringType.Accept(this, member))
                        rc = true;
                }
                foreach (var arg in item.GenericArguments)
                {
                    if (arg.Accept(this, member))
                        rc = true;
                }
                return rc;
            }

            public bool Visit(NetArrayType item, INetMemberVisibility member)
            {
                return item.ElementType.Accept(this, member);
            }

            public bool Visit(NetNullableType item, INetMemberVisibility member)
            {
                return item.ElementType.Accept(this, member);
            }
        }
    }
}
