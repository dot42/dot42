using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.BuildTools.ApiEnhancements
{
    /// <summary>
    /// Find all unprocessed methods with Runnable arguments
    /// </summary>
    internal static class FindRunnableArguments
    {
        /// <summary>
        /// Find all methods that have a java.lang.Runnable argument.
        /// </summary>
        internal static void Find(AssemblyDefinition assembly)
        {
            var overridenMethods = new HashSet<string>();
            var foundMethods = new HashSet<string>();

            foreach (var type in assembly.MainModule.GetTypes())
            {
                if (!type.HasMethods || type.IsInterface)
                    continue;

                var ignoreNames = GetApiEnhancementsIgnoreMethodNames(type);

                foreach (var method in type.Methods.Where(x => x.IsPublic))
                {
                    if (ignoreNames.Contains(method.Name))
                        continue;

                    int paramIndex;
                    string key = null;
                    if (HasRunnableArgument(method, out paramIndex))
                    {
                        key = key ?? type.FullName + " " + method.Name + "@" + paramIndex;
                        if (!overridenMethods.Contains(key))
                        {
                            foundMethods.Add(key);
                        }  
                    }
                    if (HasActionArgument(method, out paramIndex))
                    {
                        key = key ?? type.FullName + " " + method.Name + "@" + paramIndex;
                        overridenMethods.Add(key);
                        foundMethods.Remove(key);
                    }
                }
            }

            if (foundMethods.Count == 0)
                return;

            Console.WriteLine("Found {0} runnable methods", foundMethods.Count);
            foreach (var name in foundMethods.OrderBy(x => x))
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();
        }

        private static bool HasRunnableArgument(MethodDefinition method, out int paramIndex)
        {
            paramIndex = 0;
            foreach (var p in method.Parameters)
            {
                if (p.ParameterType.FullName == "Java.Lang.IRunnable")
                {
                    return true;
                }
                paramIndex++;
            }
            return false;
        }

        private static bool HasActionArgument(MethodDefinition method, out int paramIndex)
        {
            paramIndex = 0;
            foreach (var p in method.Parameters)
            {
                if (p.ParameterType.FullName == "System.Action")
                {
                    return true;
                }
                paramIndex++;
            }
            return false;
        }

        private static HashSet<string> GetApiEnhancementsIgnoreMethodNames(TypeDefinition type)
        {
            var attr = type.CustomAttributes.FirstOrDefault(
                    x => (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                         (x.AttributeType.Name == AttributeConstants.ApiEnhancementIgnoreMethodsAttributeName));
            if (attr == null)
                return new HashSet<string>();
            var names = (CustomAttributeArgument[])attr.ConstructorArguments[0].Value;
            return new HashSet<string>(names.Select(x => (string)x.Value));
        }
    }
}
