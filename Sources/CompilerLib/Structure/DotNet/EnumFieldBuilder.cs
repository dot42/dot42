using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using FieldDefinition = Mono.Cecil.FieldDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Field builder for enum constants
    /// </summary>
    internal class EnumFieldBuilder : FieldBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal EnumFieldBuilder(AssemblyCompiler compiler, FieldDefinition field)
            : base(compiler, field)
        {
        }

        /// <summary>
        /// Set the access flags of the created field.
        /// </summary>
        protected override void SetAccessFlags(DexLib.FieldDefinition dfield, FieldDefinition field)
        {
            base.SetAccessFlags(dfield, field);
            dfield.IsFinal = true;
        }

        /// <summary>
        /// Set the field type of the given dex field.
        /// </summary>
        protected override void SetFieldType(DexLib.FieldDefinition dfield, FieldDefinition field, DexTargetPackage targetPackage)
        {
            dfield.Type = dfield.Owner;
        }

        /// <summary>
        /// Set the value of the given dex field.
        /// </summary>
        protected override void SetFieldValue(DexLib.FieldDefinition dfield, FieldDefinition field)
        {
            // Do nothing
        }
    }
}
