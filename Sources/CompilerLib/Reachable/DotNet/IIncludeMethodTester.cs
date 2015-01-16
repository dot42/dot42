using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.DotNet
{
    public interface IIncludeMethodTester
    {
        /// <summary>
        /// Should the given method be included in the APK?
        /// </summary>
        bool Include(MethodDefinition method, ReachableContext context);
    }
}
