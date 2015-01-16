using System.Linq;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using FieldDefinition = Mono.Cecil.FieldDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Field builder for fields added to DexImport classes.
    /// </summary>
    internal class DexImportFieldBuilder : FieldBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DexImportFieldBuilder(AssemblyCompiler compiler, FieldDefinition field)
            : base(compiler, field)
        {
        }

        /// <summary>
        /// Add the given field to its declaring class.
        /// </summary>
        protected override void AddFieldToDeclaringClass(ClassDefinition declaringClass, DexLib.FieldDefinition dfield, DexTargetPackage targetPackage)
        {
            var generatedCodeClass = targetPackage.GetOrCreateGeneratedCodeClass();
            UpdateName(dfield, generatedCodeClass);
            dfield.Owner = generatedCodeClass;
            generatedCodeClass.Fields.Add(dfield);
        }

        /// <summary>
        /// Ensure that the name of the field is unique.
        /// </summary>
        private static void UpdateName(DexLib.FieldDefinition field, ClassDefinition declaringClass)
        {
            var baseName = field.Name;
            var name = field.Name;
            var postfix = 0;
            while (true)
            {
                if (declaringClass.Fields.All(x => x.Name != name))
                {
                    field.Name = name;
                    return;
                }
                name = baseName + ++postfix;
            }
        }
    }
}
