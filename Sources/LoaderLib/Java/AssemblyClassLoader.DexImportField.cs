using System.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using ILFieldDefinition = Mono.Cecil.FieldDefinition;
using JavaFieldDefinition = Dot42.JvmClassLib.FieldDefinition;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load java classes from JavaClass attributes included in assemblies.
    /// </summary>
    partial class AssemblyClassLoader
    {
        /// <summary>
        /// Data of a DexImportAttribute on a field
        /// </summary>
        private sealed class DexImportField
        {
            private readonly ILFieldDefinition field;
            private JavaFieldDefinition resolved;

            /// <summary>
            /// Default ctor
            /// </summary>
            public DexImportField(ILFieldDefinition field)
            {
                this.field = field;
            }

            /// <summary>
            /// Gets the field
            /// </summary>
            public ILFieldDefinition Field
            {
                get { return field; }
            }

            /// <summary>
            /// Resolve the field into a java field definition.
            /// </summary>
            public JavaFieldDefinition Resolve(ClassFile declaringClass)
            {
                return resolved ?? (resolved = BuildField(declaringClass));
            }

            /// <summary>
            /// Build a java field from my data.
            /// </summary>
            private JavaFieldDefinition BuildField(ClassFile declaringClass)
            {
                var attr = field.GetDexOrJavaImportAttribute(true);
                var nCtorArgs = attr.ConstructorArguments.Count;
                var name = (string)attr.ConstructorArguments[(nCtorArgs == 2) ? 0 : 1].Value;
                var descriptor = (string)attr.ConstructorArguments[(nCtorArgs == 2) ? 1 : 2].Value;
                var accessFlags = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeAccessFlagsName).Select(x => (int) x.Argument.Value).FirstOrDefault();
                var signature = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeSignature).Select(x => (string)x.Argument.Value).FirstOrDefault();
                return new JavaFieldDefinition(declaringClass, (FieldAccessFlags)accessFlags, name, descriptor, signature);
            }
        }
    }
}
