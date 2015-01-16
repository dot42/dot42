namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific field reference.
        /// </summary>
        internal sealed class JavaFieldReference : XFieldReference
        {
            private readonly string name;
            private readonly XTypeReference fieldType;
            private readonly string javaName;
            private readonly string javaDescriptor;
            private readonly string javaClassName;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaFieldReference(string name, XTypeReference fieldType, XTypeReference declaringType, string javaName, string javaDescriptor, string javaClassName)
                : base(declaringType)
            {
                this.name = name;
                this.fieldType = fieldType;
                this.javaName = javaName;
                this.javaDescriptor = javaDescriptor;
                this.javaClassName = javaClassName;
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return name; }
            }

            public string JavaName { get { return javaName; } }
            public string JavaDecriptor { get { return javaDescriptor; } }
            public string JavaClassName { get { return javaClassName; } }

            /// <summary>
            /// Type of field
            /// </summary>
            public override XTypeReference FieldType
            {
                get { return fieldType; }
            }
        }
    }
}
