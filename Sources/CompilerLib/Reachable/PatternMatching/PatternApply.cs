using System;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.PatternMatching
{
    class PatternApply
    {
        private readonly PatternMatcher _matcher = new PatternMatcher();

        public bool Apply(Pattern pattern, TypeDefinition type, Action<MemberReference> action)
        {
            if (pattern == null)
                return false;

            bool applied = false;

            if (!_matcher.Matches(pattern, type))
                return false;

            if (pattern.MemberMatcher == null)
            {
                action(type);
                applied = true;
            }

            if (!pattern.ApplyToMembers && pattern.MemberMatcher == null)
                return applied;

            return ApplyToMembers(type, pattern, action) || applied;
        }

        private bool ApplyToMembers(TypeDefinition type, Pattern matcher, Action<MemberReference> action)
        {
            bool applied = false;

            foreach (var member in type.Methods)
            {
                if (matcher.MembersPublicOnly && !member.IsPublic)
                    continue;
                // DO PROCESS INDIVIDUAL GETTERS OR SETTERS.
                //if (member.IsGetter || member.IsSetter) continue; // [do not annotate individual setters]
                applied |= ApplyToMember(matcher, member, action);
            }
            foreach (var member in type.Fields)
            {
                if (matcher.MembersPublicOnly && !member.IsPublic)
                    continue;
                applied |= ApplyToMember(matcher, member, action);
            }
            foreach (var member in type.Properties)
            {
                if (matcher.MembersPublicOnly && (member.GetMethod == null || !member.GetMethod.IsPublic))
                    continue;
                applied |= ApplyToMember(matcher, member, action);
            }
            foreach (var member in type.Events)
            {
                if (matcher.MembersPublicOnly && (member.AddMethod == null || !member.AddMethod.IsPublic))
                    continue;
                applied |= ApplyToMember(matcher, member, action);
            }

            return applied;
        }

        private bool ApplyToMember(Pattern matcher, MemberReference member, Action<MemberReference> action)
        {
            if (matcher.MemberMatcher != null && !_matcher.MatchesWildcard(matcher.MemberMatcher, member.Name))
                return false;

            action(member);
            return true;
        }
    }
}
