using System.ComponentModel.Composition;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Reachable.DotNet;
using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.Testers
{
    /// <summary>
    /// include all property setters/getters of anonymous types,
    /// so that they can be serialized correctly.
    /// </summary>
    [Export(typeof(IIncludeMethodTester))]
    public class AnonymousTypeMethodTester : IIncludeMethodTester
    {
        /// <summary>
        /// Should the given method be included in the APK?
        /// </summary>
        public bool Include(MethodDefinition method, ReachableContext context)
        {
            var isAnon = method.DeclaringType.IsAnonymousType();
            if (isAnon && (method.IsGetter || method.IsSetter))
                return true;
            return false;
        }
    }
}
