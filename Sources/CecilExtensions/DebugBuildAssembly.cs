using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;

namespace Dot42.CecilExtensions
{
    public static partial class Extensions
    {
        private static readonly ConcurrentDictionary<AssemblyDefinition, bool> DebugBuildCache = new ConcurrentDictionary<AssemblyDefinition, bool>();
        
        public static bool IsInDebugBuildAssembly(this TypeDefinition type)
        {
            bool ret;
            var assembly = type.Module.Assembly;
            if (DebugBuildCache.TryGetValue(assembly, out ret))
                return ret;

            // we are conservative in this detection.
            // only return true, if 
            // 1) Has DebuggableAttribute
            // 2) has one constructor argument, that is int
            // 3) the value matches our exprectations

            ret = false;
            var attr = assembly.CustomAttributes.FirstOrDefault(a => 
                a.AttributeType.FullName == "System.Diagnostics.DebuggableAttribute");

            if (attr != null)
            {
                if (attr.ConstructorArguments.Count == 1)
                {
                    int? val = attr.ConstructorArguments[0].Value as int?;
                    if (val.HasValue)
                    {
                        ret = (val.Value & (int)DebuggableAttribute.DebuggingModes.DisableOptimizations) != 0;
                    }
                }
            }

            DebugBuildCache.TryAdd(assembly, ret);

            return ret;
        }
    }
}
