using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dot42.CompilerLib.Ast.Extensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.PatternMatching
{
    class PatternMatcher
    {
        public bool Matches(Pattern pattern, TypeDefinition type)
        {
            bool matchesTypeName = pattern.TypeMatcher == null 
                                || MatchesWildcard(pattern.TypeMatcher, type.FullName);

            if (!matchesTypeName) 
                return false;

            var typeInheritanceName = pattern.TypeInheritanceName;
            if (typeInheritanceName != null)
            {
                bool checkInterfaces = pattern.TypeInheritanceKeyword == "implements" || pattern.TypeInheritanceKeyword == "inherits";
                bool checkBaseClasses = pattern.TypeInheritanceKeyword == "inherits" || pattern.TypeInheritanceKeyword == "extends";

                if (checkInterfaces)
                {
                    if (type.GetImplementedInterfaces().Any(i => i.Name == typeInheritanceName
                                                              || i.FullName == typeInheritanceName))
                        return true;
                }

                if (checkBaseClasses)
                {
                    TypeDefinition current = type;

                    while (current.BaseType != null)
                    {
                        current = current.BaseType.GetElementType().Resolve();
                        if (current.Name == typeInheritanceName || current.FullName == typeInheritanceName)
                            return true;
                    }
                }

                return false;
            }
                

            return true;
        }

        private readonly Dictionary<string, Regex> _matchRex = new Dictionary<string, Regex>();

        public bool MatchesWildcard(string wildcardExpression, string name)
        {
            Regex reg;
            if (!_matchRex.TryGetValue(wildcardExpression, out reg))
            {
                var rex = Regex.Escape(wildcardExpression.Replace("*", "§1")
                                                         .Replace("?", "§2"))
                                                         .Replace("§1", ".*")
                                                         .Replace("§2", ".?");
                reg = new Regex("^" + rex + "$", RegexOptions.Compiled | RegexOptions.Singleline);
                _matchRex[wildcardExpression] = reg;
            }
            return reg.IsMatch(name);
        }
    }
}
