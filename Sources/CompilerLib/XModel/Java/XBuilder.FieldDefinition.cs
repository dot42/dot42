using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific field definition.
        /// </summary>
        private sealed class JavaFieldDefinition : XFieldDefinition
        {
            private readonly FieldDefinition field;
            private XTypeReference fieldType;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaFieldDefinition(XTypeDefinition declaringType, FieldDefinition field)
                : base(declaringType)
            {
                this.field = field;
            }

            public FieldDefinition OriginalField { get { return field; } }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return field.Name; }
            }

            /// <summary>
            /// Type of field
            /// </summary>
            public override XTypeReference FieldType
            {
                get { return fieldType ?? (fieldType = AsTypeReference(Module, field.FieldType, XTypeUsageFlags.FieldType)); }
            }

            /// <summary>
            /// Is this a static field?
            /// </summary>
            public override bool IsStatic
            {
                get { return field.IsStatic; }
            }

            /// <summary>
            /// Is this field used in code?
            /// </summary>
            public override bool IsReachable
            {
                get { return field.IsReachable; }
            }

            /// <summary>
            /// Is this a readonly field?
            /// </summary>
            public override bool IsReadOnly
            {
                get { return field.IsFinal; }
            }

            /// <summary>
            /// Does this field have a name with a special meaning for the runtime?
            /// </summary>
            public override bool IsRuntimeSpecialName { get { return false; } }

            /// <summary>
            /// Gets the initial value (if any) of this field.
            /// </summary>
            public override object InitialValue
            {
                get { return field.ConstantValue; }
            }

            /// <summary>
            /// Try to get the value of the enum const if this field is an enum const field.
            /// </summary>
            public override bool TryGetEnumValue(out object value)
            {
                value = null;
                return false;
            }

            /// <summary>
            /// Try to get the names from the DexImport or JavaImport attached to this field.
            /// </summary>
            public override bool TryGetDexImportNames(out string fieldName, out string descriptor, out string className)
            {
                fieldName = null;
                descriptor = null;
                className = null;
                return false;
            }

            /// <summary>
            /// Try to get the names from the JavaImport attribute attached to this field.
            /// </summary>
            public override bool TryGetJavaImportNames(out string fieldName, out string descriptor, out string className)
            {
                fieldName = null;
                descriptor = null;
                className = null;
                return false;
            }

            /// <summary>
            /// Try to get the name of the ResourceId attribute attached to this field.
            /// </summary>
            public override bool TryGetResourceIdAttribute(out string resourceName)
            {
                resourceName = null;
                return false;
            }
        }
    }
}
