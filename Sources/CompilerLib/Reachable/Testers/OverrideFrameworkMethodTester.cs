using System.ComponentModel.Composition;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable.DotNet;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.Testers
{
    [Export(typeof(IIncludeMethodTester))]
    public class OverrideFrameworkMethodTester : IIncludeMethodTester
    {
        /// <summary>
        /// Should the given method be included in the APK?
        /// </summary>
        public bool Include(MethodDefinition method, ReachableContext context)
        {
            if (!method.IsVirtual)
                return false;
            if (method.GetDexImportBaseMethod() != null)
                return true;
            if (method.GetDexImportBaseInterfaceMethod() != null)
                return true;
            return false;
        }
    }
}
