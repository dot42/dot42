using System.ComponentModel;
using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof (IMappedTypeBuilder))]
    internal sealed class JavaLangClassBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangClassBuilder() : this(ClassFile.Empty)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangClassBuilder(ClassFile cf)
            : base(cf, "System.Type", "java/lang/Class")
        {
        }

        /// <summary>
        /// Add generic parameters to my type?
        /// </summary>
        protected override bool AddGenericParameters
        {
            get { return false; }
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new TypePropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            switch (method.Name)
            {
                case "GetConstructor":
                    renamer.Rename(method, "JavaGetConstructor");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetConstructors":
                    renamer.Rename(method, "JavaGetConstructors");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetField":
                    renamer.Rename(method, "JavaGetField");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetFields":
                    renamer.Rename(method, "JavaGetFields");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetInterfaces":
                    renamer.Rename(method, "JavaGetInterfaces");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetMethod":
                    renamer.Rename(method, "JavaGetMethod");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetMethods":
                    renamer.Rename(method, "JavaGetMethods");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredConstructor":
                    renamer.Rename(method, "JavaGetDeclaredConstructor");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredConstructors":
                    renamer.Rename(method, "JavaGetDeclaredConstructors");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredField":
                    renamer.Rename(method, "JavaGetDeclaredField");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredFields":
                    renamer.Rename(method, "JavaGetDeclaredFields");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredInterfaces":
                    renamer.Rename(method, "JavaGetDeclaredInterfaces");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredMethod":
                    renamer.Rename(method, "JavaGetDeclaredMethod");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetDeclaredMethods":
                    renamer.Rename(method, "JavaGetDeclaredMethods");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetName":
                    renamer.Rename(method, "JavaGetName");
                    method.EditorBrowsableState = EditorBrowsableState.Never;
                    break;
                case "GetComponentType":
                    renamer.Rename(method, "GetElementType");
                    break;
            }
        }

        /// <summary>
        /// Custom property builder.
        /// </summary>
        private sealed class TypePropertyBuilder : PropertyBuilder
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public TypePropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
                : base(typeDef, declaringTypeBuilder)
            {
            }

            /// <summary>
            /// Is the given method a property get method?
            /// </summary>
            protected override bool IsGetter(NetMethodDefinition method)
            {
                if (method.Parameters.Count == 0 && method.Name.StartsWith("Is"))
                {
                    return true;
                }
                return false;
            }
 
        }
    }
}
