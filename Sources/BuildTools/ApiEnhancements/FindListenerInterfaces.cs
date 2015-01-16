using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.BuildTools.ApiEnhancements
{
    /// <summary>
    /// Find all unprocessed listener interfaces
    /// </summary>
    internal static class FindListenerInterfaces
    {
        /// <summary>
        /// Find all listener interfaces in the given assembly that are not somehow used in an event.
        /// </summary>
        internal static void Find(AssemblyDefinition assembly)
        {
            var usedInterfaces = new HashSet<string>();
            var foundInterfaces = new HashSet<string>();


            foreach (var type in assembly.MainModule.GetTypes())
            {
                if (type.IsInterface && type.Name.EndsWith("Listener"))
                {
                    // Found possible listener interface
                    var fullName = GetFullName(type);
                    if (fullName == null)
                        continue;
                    if (!usedInterfaces.Contains(fullName))
                    {
                        foundInterfaces.Add(fullName);
                    }
                }
                else if (!type.IsInterface && type.HasEvents)
                {
                    // Scan for events
                    foreach (var evt in type.Events.Where(x => x.HasCustomAttributes))
                    {
                        var attr = evt.CustomAttributes.FirstOrDefault(x => (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                                     (x.AttributeType.Name == AttributeConstants.ListenerInterfaceAttributeName));
                        if (attr != null)
                        {
                            // Ctor argument can be string or type
                            var fullName = attr.ConstructorArguments[0].Value as string;
                            if (fullName == null)
                            {
                                var typeRef = (TypeReference)attr.ConstructorArguments[0].Value;
                                var intfType = typeRef.Resolve();
                                fullName = GetFullName(intfType);
                            }
                            if (fullName != null)
                            {
                                // Found event with ListenerInterfaceAttribute
                                usedInterfaces.Add(fullName);
                                foundInterfaces.Remove(fullName);
                            }
                        }
                    }
                }
            }

            if (foundInterfaces.Count == 0)
                return;

            Console.WriteLine("Found {0} listener interfaces", foundInterfaces.Count);
            foreach (var name in foundInterfaces.OrderBy(x => x))
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();
        }

        private static string GetFullName(TypeDefinition type)
        {
            var attr = type.CustomAttributes.FirstOrDefault(x =>
                (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                (x.AttributeType.Name == AttributeConstants.DexImportAttributeName));
            if (attr == null)
                return null;
            return (string)attr.ConstructorArguments[0].Value;            
        }
    }
}
