using System;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable.PatternMatching;
using Dot42.JvmClassLib;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable
{
    [DebuggerDisplay("{_pattern}")]
    internal sealed class PatternInclude
    {
        private static readonly PatternApply ApplyByPattern = new PatternApply();

        private readonly Pattern _pattern;

        public bool AppliesToMembers { get { return _pattern.ApplyToMembers || _pattern.MemberMatcher != null; } }
        public bool IsGlobal { get; private set; }
        public bool IsEmpty { get { return _pattern == null || (_pattern.MemberMatcher == null && _pattern.TypeMatcher == null); } }

        /// <summary>
        /// Default ctor
        /// </summary>
        public PatternInclude(string pattern, bool applyToMembers, bool isGlobal)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            IsGlobal = isGlobal;
            
            _pattern = PatternParser.ParseMatcher(pattern, applyToMembers);
            if (IsEmpty)
                return;


            bool hasInclude = _pattern.Attributes.Any(a => a == "Dot42.Include");
            bool hasIncludeType = _pattern.Attributes.Any(a => a == "Dot42.IncludeType");

            if (hasIncludeType)
            {
                _pattern.ApplyToMembers = true;
            }
            else if(!hasInclude)
            {
                _pattern = null;
            }
        }

        /// <summary>
        /// The given type has been made reachable.
        /// </summary>
        /// <returns>True if the type was marked reachable, false otherwise</returns>
        public bool IncludeIfNeeded(ReachableContext context, TypeDefinition type, TypeDefinition dot42IncludeType)
        {
            if (_pattern == null)
                return false;

            return ApplyByPattern.Apply(_pattern, type, member => Include(member, context, dot42IncludeType));
        }

        private void Include(MemberReference member, ReachableContext context, TypeDefinition dot42IncludeType)
        {
            if (member is TypeDefinition)
            {
                member.MarkReachable(context);
            }
            else if (member.DeclaringType.IsReachable)
            {
                member.MarkReachable(context);
            }
            else
            {
                // add Include attribute for later use.
                var attrprov = (ICustomAttributeProvider)member;
                if(!attrprov.HasIncludeAttribute())
                    attrprov.CustomAttributes.Add(new CustomAttribute(dot42IncludeType.Methods.First(m => m.IsConstructor && m.Parameters.Count == 0)));
            }
        }

        public bool IncludeIfNeeded(ReachableContext context, ClassFile javaClass)
        {
            return false;
        }
    }
}
