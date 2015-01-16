using System;
using System.ComponentModel.Composition;
using Dot42.CompilerLib.Reachable.DotNet;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.Testers
{
    [Export(typeof(IIncludeMethodTester))]
    public class TestCaseMethodTester : IIncludeMethodTester
    {
        /// <summary>
        /// Should the given method be included in the APK?
        /// </summary>
        public bool Include(MethodDefinition method, ReachableContext context)
        {
            return method.Name.StartsWith("test", StringComparison.OrdinalIgnoreCase) && IsTestCase(method.DeclaringType);
        }

        /// <summary>
        /// Is the given type (or its base types) Junit.Framework.TestCase?
        /// </summary>
        private static bool IsTestCase(TypeDefinition type)
        {
            while (type != null)
            {
                if (type.FullName == "Junit.Framework.TestCase")
                    return true;
                if (type.BaseType == null)
                    return false;
                type = type.BaseType.GetElementType().Resolve();
            }
            return false;
        }
    }
}
