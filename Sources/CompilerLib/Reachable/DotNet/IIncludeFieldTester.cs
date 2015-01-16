using Mono.Cecil;

namespace Dot42.CompilerLib.Reachable.DotNet
{
    public interface IIncludeFieldTester
    {
        /// <summary>
        /// Should the given field be included in the APK?
        /// </summary>
        bool Include(FieldDefinition field, ReachableContext context);
    }
}
