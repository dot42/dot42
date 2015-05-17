using System.Text.RegularExpressions;
using Dot42.Utility;

namespace Dot42.CompilerLib.Reachable.PatternMatching
{
    partial class PatternParser
    {
        private const string RexApplyToType = @"Apply +to +type +([^ ]+) *(?:[:]|when +(extends|inherits|implements)\('([^ ')]+)'\) *:)";
        private const string RexApplyToMember = @"Apply +to +member +([^ ]+) *(?:[:]|when +(public) *:)";
        private const string RexDefinition = @"([A-Za-z][A-Za-z0-9]*(?:\.[A-Za-z][A-Za-z0-9]*)*)";

        private static readonly string RexExpression = string.Format("^ *(?:{0})? *(?:{1})? *{2}(?: *, *{2})* *$", 
                                                                        RexApplyToType, RexApplyToMember, RexDefinition);
        private static readonly Regex Parser = new Regex(RexExpression);

        public static Pattern ParseMatcher(string feature, bool applyToMembers)
        {
            Match m = Parser.Match(feature);
            if (!m.Success)
            {
                DLog.Warning(DContext.CompilerStart, "pattern parsing error :" + feature);
                return null;
            }

            Pattern match = new Pattern();
            if (m.Groups[1].Success) match.TypeMatcher = m.Groups[1].Value;
            if (m.Groups[2].Success) match.TypeInheritanceKeyword = m.Groups[2].Value;
            if (m.Groups[3].Success) match.TypeInheritanceName = m.Groups[3].Value;
            if (m.Groups[4].Success) match.MemberMatcher = m.Groups[4].Value;
            if (m.Groups[5].Success) match.MembersPublicOnly = true;
            if (m.Groups[6].Success) match.Attributes.Add(m.Groups[6].Value);
            if (m.Groups[7].Success)
                foreach (Capture cap in m.Groups[7].Captures)
                    match.Attributes.Add(cap.Value);

            match.ApplyToMembers = applyToMembers;

            if (match.TypeMatcher == null && match.MemberMatcher == null)
            {
                DLog.Warning(DContext.CompilerStart, "either type or member matcher should be specified: " + feature);
                return match;
            }
            return match;
        }
    }
}
