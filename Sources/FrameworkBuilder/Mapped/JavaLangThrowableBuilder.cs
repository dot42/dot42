using System.Collections.Generic;
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
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangThrowableBuilder: JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangThrowableBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangThrowableBuilder(ClassFile cf)
            : base(cf, "System.Exception", "java/lang/Throwable")
        {
        }

        /// <summary>
        /// Modify the name of the given method to another name.
        /// By calling renamer.Rename, all methods in the same group are also updated.
        /// </summary>
        public override void ModifyMethodName(NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            if (method.OriginalJavaName == "getMessage")
            {
                method.EditorBrowsableState = EditorBrowsableState.Never;
            }
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new ThrowablePropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Custom property builder.
        /// </summary>
        private sealed class ThrowablePropertyBuilder : PropertyBuilder
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public ThrowablePropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
                : base(typeDef, declaringTypeBuilder)
            {
            }

            protected override bool IsGetter(NetMethodDefinition method)
            {
                if (method.Name == "GetMessage") return false;

                return base.IsGetter(method);
            }

            /// <summary>
            /// Create a property name from the given getter.
            /// </summary>
            protected override string GetPropertyName(NetMethodDefinition getter)
            {
                switch (getter.Name)
                {
                    // don't rename property that gets overridden
                    //case "GetCause":
                    //    return "InnerException";
                    case "GetStackTrace":
                        return "JavaStackTrace";
                    default:
                        return base.GetPropertyName(getter);
                }
            }
        }
    }
}
