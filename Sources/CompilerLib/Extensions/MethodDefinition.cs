using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// MethodDefinition extensions
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Is the given method a "direct" method in Dex terms?
        /// </summary>
        internal static bool IsDirect(this MethodDefinition method)
        {
            return (!method.IsStatic) && (method.IsPrivate || method.IsConstructor);
        }

        /// <summary>
        /// Is this method declaring type part of a android framework and the method itself has no implementation in the framework?
        /// </summary>
        internal static bool IsAndroidExtension(this MethodDefinition method)
        {
            return method.DeclaringType.HasDexImportAttribute() && !method.HasDexImportAttribute();
        }
    }
}
