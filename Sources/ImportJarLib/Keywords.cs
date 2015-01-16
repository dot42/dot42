using System.Linq;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// C# keywords
    /// </summary>
    internal static class Keywords
    {
        private static readonly string[] keywords = new[] 
        {
            "abstract", "event",     "new",       "struct",
            "as",       "explicit",  "null",      "switch",
            "base",     "extern",    "object",    "this",
            "bool",     "false",     "operator",  "throw",
            "break",    "finally",   "out",       "true",
            "byte",     "fixed",     "override",  "try",
            "case",     "float",     "params",    "typeof",
            "catch",    "for",       "private",   "uint",
            "char",     "foreach",   "protected", "ulong",
            "checked",  "goto",      "public",    "unchecked",
            "class",    "if",        "readonly",  "unsafe",
            "const",    "implicit",  "ref",       "ushort",
            "continue", "in",        "return",    "using",
            "decimal",  "int",       "sbyte",     "virtual",
            "default",  "interface", "sealed",    "volatile",
            "delegate", "internal",  "short",     "void",
            "do", "is", "sizeof",    "while",
            "double",   "lock",      "stackalloc",
            "else",     "long",      "static",
            "enum",     "namespace", "string"
        };

        /// <summary>
        /// Is the given string a c# keyword?
        /// </summary>
        internal static bool IsKeyword(string x)
        {
            return keywords.Contains(x);
        }
    }
}
