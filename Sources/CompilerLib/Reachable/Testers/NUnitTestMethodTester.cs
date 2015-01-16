using System.ComponentModel.Composition;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable.DotNet;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.Testers
{
    [Export(typeof(IIncludeMethodTester))]
    public class NUnitTestMethodTester : IIncludeMethodTester
    {
        /// <summary>
        /// Should the given method be included in the APK?
        /// </summary>
        public bool Include(MethodDefinition method, ReachableContext context)
        {
            if (!(method.HasNUnitTestAttribute() && method.DeclaringType.HasNUnitTestFixtureAttribute()))
                return false;
            // Include the NUnit base class
            var xType = context.Compiler.GetDot42InternalType("Dot42.Test", "NUnitTestCase").Resolve();
            ((TypeDefinition)xType.OriginalTypeDefinition).MarkReachable(context);
            return true;
        }
    }
}
