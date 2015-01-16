using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific type reference.
        /// </summary>
        internal sealed class JavaTypeReference : XTypeReference
        {
            private readonly ObjectTypeReference type;
            private readonly string javaClassName;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaTypeReference(XModule module, ObjectTypeReference type, string javaClassName)
                : base(module, false, null)
            {
                this.type = type;
                this.javaClassName = javaClassName;
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return ClassName.StripPackage(type.ClassName); }
            }

            /// <summary>
            /// Gets the java class name
            /// </summary>
            public string JavaClassName { get { return javaClassName; } }

            /// <summary>
            /// Create a fullname of this type reference.
            /// </summary>
            public override string GetFullName(bool noGenerics)
            {
                return javaClassName;
            }

            /// <summary>
            /// Gets the reference to compare in <see cref="IsSame"/>.
            /// </summary>
            protected override XTypeReference ToCompareReference()
            {
                XTypeDefinition typeDef;
                if (TryResolve(out typeDef))
                    return typeDef;
                return this;
            }

            /// <summary>
            /// Resolve this reference to it's definition.
            /// </summary>
            public override bool TryResolve(out XTypeDefinition type)
            {
                if (base.TryResolve(out type))
                    return true;
                switch (FullName)
                {
                    case "B":
                        return Module.TypeSystem.SByte.TryResolve(out type);
                    case "C":
                        return Module.TypeSystem.Char.TryResolve(out type);
                    case "D":
                        return Module.TypeSystem.Double.TryResolve(out type);
                    case "F":
                        return Module.TypeSystem.Float.TryResolve(out type);
                    case "I":
                        return Module.TypeSystem.Int.TryResolve(out type);
                    case "J":
                        return Module.TypeSystem.Long.TryResolve(out type);
                    case "S":
                        return Module.TypeSystem.Short.TryResolve(out type);
                    case "Z":
                        return Module.TypeSystem.Bool.TryResolve(out type);
                }

                // Try clumsy nested like classes
                if (Module.TryGetType(javaClassName, out type))
                    return true;

                return false;
            }
        }
    }
}
