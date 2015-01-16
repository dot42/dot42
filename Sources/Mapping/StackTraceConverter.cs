using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dot42.Mapping
{
    /// <summary>
    /// Class used to convert obfuscated stack traces to 
    /// unobfuscated stack traces.
    /// </summary>
    internal sealed class StackTraceConverter
    {
        private static Regex elementExpr;
        private int startCopyIndex = 0;
        private readonly string stacktrace;
        private readonly StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Deobfuscate the given stacktrace using the given map file.
        /// </summary>
        internal static string DeObfuscate(string stacktrace, IEnumerable<MapFile> maps)
        {
            // Add a whitespace before the stacktrace. The regular expressions may need them.
            stacktrace = stacktrace.Replace("\\r\\n", "\\n");
            stacktrace = stacktrace.Replace("\\n", Environment.NewLine);
            var converter = new StackTraceConverter(" " + stacktrace, maps.ToList());
            return converter.sb.ToString().Substring(1);
        }

        /// <summary>
        /// Deobfuscate the given stacktrace using the given map file.
        /// </summary>
        private StackTraceConverter(string stacktrace, List<MapFile> maps)
        {
            this.stacktrace = stacktrace;

            foreach (Match match in ElementExpr.Matches(stacktrace))
            {
                Group typeMethodGroup = match.Groups["typeMethod"];
                Group argsGroup = match.Groups["args"];
                Group typeNameGroup = match.Groups["typeName"];

                if ((typeMethodGroup.Success && argsGroup.Success))
                {
                    // Copy all until start of group
                    CopyTraceUntilGroup(typeMethodGroup);

                    // Parse arguments
                    var arguments = stacktrace.Substring(argsGroup.Index, argsGroup.Length);
                    var args = ParseArguments(arguments, maps);

                    // Convert type and method
                    var typeMethod = stacktrace.Substring(typeMethodGroup.Index, typeMethodGroup.Length);
                    sb.Append(ConvertTypeMethod(typeMethod, args, maps, ref arguments));

                    // Copy all until start of group
                    CopyTraceUntilGroup(argsGroup);

                    // Convert arguments
                    sb.Append(arguments);

#if DEBUG
                    Debug.WriteLine(string.Format("Method: {0}, args: {1}", typeMethod, arguments));
#endif
                }
                else if (typeNameGroup.Success)
                {
                    // Copy all until start of group
                    CopyTraceUntilGroup(typeNameGroup);

                    var typeName = stacktrace.Substring(typeNameGroup.Index, typeNameGroup.Length);
                    var convertedTypeName = ConvertTypeName(typeName, maps);
                    sb.Append(convertedTypeName);

#if DEBUG
                    Debug.WriteLine(string.Format("Type: {0} -> {1}", typeName, convertedTypeName));
#endif
                }
            }

            // Copy remaining trace
            if (startCopyIndex < stacktrace.Length)
            {
                sb.Append(stacktrace, startCopyIndex, stacktrace.Length - startCopyIndex);
            }
        }

        /// <summary>
        /// Copy the part of the stacktrace from startCopyIndex to the start of the group.
        /// Then update startCopyIndex with the end of the group.
        /// </summary>
        /// <param name="group"></param>
        private void CopyTraceUntilGroup(Group group)
        {
            if (startCopyIndex < group.Index)
            {
                sb.Append(stacktrace, startCopyIndex, group.Index - startCopyIndex);
                startCopyIndex = group.Index + group.Length;
            }
        }

        /// <summary>
        /// Convert a typename to an unobfuscated typename.
        /// </summary>
        private static string ConvertTypeName(string typeName, List<MapFile> maps)
        {
            var typeEntry = maps.Select(x => x.GetTypeByNewName(typeName)).FirstOrDefault(x => x != null);
            return (typeEntry != null) ? typeEntry.Name : typeName;
        }

        /// <summary>
        /// Convert the type and method part of a trace element
        /// </summary>
        private static string ConvertTypeMethod(string typeMethod, Argument[] args, List<MapFile> maps, ref string arguments)
        {
            string typeName;
            string method;

            // Split into type and method
            if (typeMethod.EndsWith(".ctor"))
            {
                typeName = typeMethod.Substring(0, typeMethod.Length - (".ctor".Length + 1));
                method = ".ctor";
            }
            else
            {
                int idx = typeMethod.LastIndexOf('.');
                if (idx <= 0) { return typeMethod; }

                typeName = typeMethod.Substring(0, idx);
                method = typeMethod.Substring(idx + 1);
            }

            // Look for type and method
            foreach (var typeEntry in maps.Select(x => x.GetTypeByNewName(typeName)).Where(x => x != null))
            {
                if (typeEntry == null)
                {
                    continue;
                    //return typeMethod;
                }

                // Convert method name
                string dexSignature = null; // TODO
                var methodEntry = typeEntry.FindDexMethod(method, dexSignature);
                if (methodEntry == null) 
                    continue;

                method = methodEntry.Name;
                var signature = methodEntry.Signature;
                arguments = signature.Substring(1, signature.Length - 2);
                return typeEntry.Name + "." + method;
            }

            // Look for type match only
            var firstTypeEntry = maps.Select(x => x.GetTypeByNewName(typeName)).FirstOrDefault(x => x != null);
            return (firstTypeEntry == null) ? typeMethod : firstTypeEntry.Name + "." + method;
        }

        /// <summary>
        /// Convert method arguments
        /// </summary>
        /// <param name="args"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private static string ConvertMethodArguments(Argument[] args, MapFile map)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Argument arg in args)
            {
                if (sb.Length > 0) { sb.Append(", "); }

                sb.Append(arg.Type);
                sb.Append(' ');
                sb.Append(arg.Name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parse arguments string into individual
        /// </summary>
        private static Argument[] ParseArguments(string args, List<MapFile> maps)
        {
            if (string.IsNullOrEmpty(args)) { return new Argument[0]; }

            var list = new List<Argument>();
            foreach (string argPart in args.Split(','))
            {
                string arg = argPart.Trim();
                int spaceIndex = arg.LastIndexOf(' ');

                string typeName;
                string argName;

                if (spaceIndex > 0)
                {
                    typeName = arg.Substring(0, spaceIndex);
                    argName = arg.Substring(spaceIndex + 1);
                }
                else
                {
                    typeName = arg;
                    argName = "?";
                }

                // Convert typename
                var typeEntry = maps.Select(x => x.GetTypeByNewName(typeName)).FirstOrDefault(x => x != null);
                if (typeEntry != null)
                {
                    //typeName = typeEntry.Name;
                }

                list.Add(new Argument(typeName, argName));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Regular expression used to get a stack trace element
        /// </summary>
        private static Regex ElementExpr
        {
            get
            {
                if (elementExpr == null)
                {
                    // Whitespace followed by a type and '(' arguments ')'
                    string methodExpr = @"\s(?<typeMethod>[\p{Lu}\p{Ll}\p{Lt}\p{Lo}]\S*)\s*\((?<args>.*)\)";
                    string typeExpr = @"\W(?<typeName>[\p{Lu}\p{Ll}\p{Lt}\p{Lo}]([\.\+\w]*))";
                    elementExpr = new Regex(methodExpr + "|" + typeExpr, RegexOptions.Multiline);
                }
                return elementExpr;
            }
        }

        private struct Argument
        {
            public string Name;
            public string Type;

            internal Argument(string typeName, string argName)
            {
                this.Name = argName;
                this.Type = typeName;
            }
        }
    }
}
