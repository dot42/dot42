using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Map methods from java to .NET
    /// </summary>
    public sealed class MethodMap
    {
        private readonly Dictionary<MethodDefinition, NetMethodDefinition> map = new Dictionary<MethodDefinition, NetMethodDefinition>();

        /// <summary>
        /// Add the given mapping
        /// </summary>
        public void Add(MethodDefinition javaMethod, NetMethodDefinition netMethod)
        {
            map.Add(javaMethod, netMethod);
        }

        /// <summary>
        /// Gets a mapping.
        /// Throws an error if not found.
        /// </summary>
        public NetMethodDefinition Get(MethodDefinition javaMethod)
        {
            NetMethodDefinition result;
            if (map.TryGetValue(javaMethod, out result))
                return result;
            throw new ArgumentOutOfRangeException(string.Format("No matching .NET method for {0}", javaMethod));
        }

        /// <summary>
        /// Gets a mapping.
        /// Throws an error if not found.
        /// </summary>
        public bool TryGet(MethodDefinition javaMethod, out NetMethodDefinition  result)
        {
            return map.TryGetValue(javaMethod, out result);
        }

        /// <summary>
        /// Gets all methods
        /// </summary>
        public IEnumerable<NetMethodDefinition> AllMethods
        {
            get
            {
                var allTypes = new HashSet<NetTypeDefinition>(map.Values.Where(x => x.DeclaringType != null).Select(x => x.DeclaringType.GetElementType()));
                // Add nested types
                while (true)
                {
                    var additionalTypes = new HashSet<NetTypeDefinition>(allTypes);
                    var changed = false;
                    foreach (var type in allTypes.SelectMany(x => x.NestedTypes))
                    {
                        if (additionalTypes.Add(type))
                            changed = true;
                    }
                    if (changed)
                    {
                        allTypes = additionalTypes;
                    }
                    else
                    {
                        break;
                    }
                }
                return allTypes.SelectMany(x => x.Methods);
                //return map.Values;
            }
        }
    }
}
