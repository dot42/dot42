using Dot42.CompilerLib.Extensions;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.DotNet
{
    partial class XBuilder
    {
        /// <summary>
        /// IL specific field definition.
        /// </summary>
        private sealed class ILFieldDefinition : XFieldDefinition
        {
            private readonly FieldDefinition field;
            private XTypeReference fieldType;
            private string dexImportName;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ILFieldDefinition(XTypeDefinition declaringType, FieldDefinition field)
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
                get { return fieldType ?? (fieldType = AsTypeReference(Module, field.FieldType)); }
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
                get { return field.IsInitOnly; }
            }

            /// <summary>
            /// Does this field have a name with a special meaning for the runtime?
            /// </summary>
            public override bool IsRuntimeSpecialName { get { return field.IsRuntimeSpecialName; } }

            /// <summary>
            /// Gets the initial value (if any) of this field.
            /// </summary>
            public override object InitialValue
            {
                get { return field.InitialValue; }
            }

            /// <summary>
            /// Try to get the value of the enum const if this field is an enum const field.
            /// </summary>
            public override bool TryGetEnumValue(out object value)
            {
                if ((IsStatic) && (field.DeclaringType.IsEnum) && field.HasConstant)
                {
                    value = field.Constant;
                    return true;
                }
                value = null;
                return false;
            }

            /// <summary>
            /// Is this reference equal to the given other reference?
            /// </summary>
            public override bool IsSameExceptDeclaringType(XFieldReference other)
            {
                if (base.IsSameExceptDeclaringType(other))
                    return true;
                if (dexImportName == null)
                {
                    string fieldName;
                    string descriptor;
                    string className;
                    if (TryGetDexImportNames(out fieldName, out descriptor, out className))
                    {
                        dexImportName = fieldName;
                    }
                    else
                    {
                        dexImportName = "<none>";
                    }
                }
                // Check against dex import 
                if (dexImportName == other.Name) 
                {
                    var descriptor = CreateNoGenericsDescriptor(this);
                    var otherDescriptor = CreateNoGenericsDescriptor(other);
                    return (descriptor == otherDescriptor);
                }
                return false;
            }

            /// <summary>
            /// Create a descriptor for comparing the given field without generics.
            /// </summary>
            private static string CreateNoGenericsDescriptor(XFieldReference field)
            {
                return XBuilder.CreateNoGenericsDescriptor(field.FieldType);
            }


            /// <summary>
            /// Try to get the names from the DexImport or JavaImport attached to this field.
            /// </summary>
            public override bool TryGetDexImportNames(out string fieldName, out string descriptor, out string className)
            {
                var attr = field.GetDexImportAttribute();
                if (attr == null)
                {
                    fieldName = null;
                    descriptor = null;
                    className = null;
                    return false;
                }
                attr.GetDexOrJavaImportNames(field, out fieldName, out descriptor, out className);
                return true;
            }

            /// <summary>
            /// Try to get the names from the JavaImport attribute attached to this field.
            /// </summary>
            public override bool TryGetJavaImportNames(out string fieldName, out string descriptor, out string className)
            {
                var attr = field.GetJavaImportAttribute();
                if (attr == null)
                {
                    fieldName = null;
                    descriptor = null;
                    className = null;
                    return false;
                }
                attr.GetDexOrJavaImportNames(field, out fieldName, out descriptor, out className);
                return true;
            }

            /// <summary>
            /// Try to get the name of the ResourceId attribute attached to this field.
            /// </summary>
            public override bool TryGetResourceIdAttribute(out string resourceName)
            {
                var attr = field.GetResourceIdAttribute();
                if (attr == null)
                {
                    resourceName = null;
                    return false;
                }
                resourceName = (string)field.GetResourceIdAttribute().ConstructorArguments[0].Value;
                return true;
            }
        }
    }
}
