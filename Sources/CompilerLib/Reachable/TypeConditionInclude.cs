using System.Diagnostics;
using Dot42.CompilerLib.Extensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable
{
    [DebuggerDisplay("{member}, {typeCondition}")]
    internal sealed class TypeConditionInclude
    {
        private readonly MemberReference member;
        private readonly TypeDefinition typeCondition;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TypeConditionInclude(MemberReference member, TypeReference typeCondition)
        {
            this.member = member;
            this.typeCondition = (typeCondition != null) ? typeCondition.GetElementType().Resolve() : null;
        }

        /// <summary>
        /// The given type has been made reachable.
        /// </summary>
        internal void IncludeIfNeeded(ReachableContext context)
        {
            if ((typeCondition == null) || (typeCondition.IsReachable))
            {
                member.MarkReachable(context);
            }
        }
    }
}
