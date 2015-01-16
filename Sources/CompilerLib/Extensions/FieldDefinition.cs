using Mono.Cecil;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// FieldDefinition extensions
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Is this field declaring type part of a android framework and the field itself has no implementation in the framework?
        /// </summary>
        internal static bool IsAndroidExtension(this FieldDefinition field)
        {
            return field.DeclaringType.HasDexImportAttribute() && !field.HasDexImportAttribute();
        }
    }
}
