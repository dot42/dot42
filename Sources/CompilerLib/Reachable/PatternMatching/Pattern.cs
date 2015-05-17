using System;
using System.Collections.Generic;

namespace Dot42.CompilerLib.Reachable.PatternMatching
{
    internal class Pattern
    {
        public string TypeMatcher { get; set; }
        public string TypeInheritanceKeyword { get; set; }
        public string TypeInheritanceName { get; set; }
        public string MemberMatcher { get; set; }
        public bool MembersPublicOnly { get; set; }

        public List<string> Attributes { get; set; }
        public bool ApplyToMembers { get; set; }

        public Pattern()
        {
            Attributes = new List<string>();
        }

        public override string ToString()
        {
            return String.Format("type={0} | inherit={1} | inherit type={2} | members={3} | public={4} | applyToMembers={5} | {6}",
                TypeMatcher, TypeInheritanceKeyword, TypeInheritanceName, MemberMatcher, MembersPublicOnly, ApplyToMembers, String.Join(",", Attributes));
        }
    }
}