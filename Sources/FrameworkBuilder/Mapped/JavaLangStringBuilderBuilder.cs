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
    internal sealed class JavaLangStringBuilderBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangStringBuilderBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangStringBuilderBuilder(ClassFile cf)
            : base(cf, "System.Text.StringBuilder", "java/lang/StringBuilder")
        {
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            switch (method.Name)
            {
                case "Append":
                    if (method.JavaDescriptor == "(Ljava/lang/CharSequence;II)Ljava/lang/StringBuilder;")
                    {
                        renamer.Rename(method, "JavaAppend");
                        method.EditorBrowsableState = EditorBrowsableState.Advanced;
                    }
                    break;
                case "Delete":
                    if (method.JavaDescriptor == "(II)Ljava/lang/StringBuilder;")
                    {
                        renamer.Rename(method, "JavaDelete");
                        method.EditorBrowsableState = EditorBrowsableState.Advanced;
                    }
                    break;
                case "Insert":
                    if (method.JavaDescriptor == "(ILjava/lang/CharSequence;II)Ljava/lang/StringBuilder;")
                    {
                        renamer.Rename(method, "JavaInsert");
                        method.EditorBrowsableState = EditorBrowsableState.Advanced;
                    }
                    break;
                case "Substring":
                    if (method.JavaDescriptor == "(II)Ljava/lang/String;")
                    {
                        renamer.Rename(method, "JavaSubstring");
                        method.EditorBrowsableState = EditorBrowsableState.Advanced;
                    }
                    break;
            }
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new StringBuilderPropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Custom property builder.
        /// </summary>
        private sealed class StringBuilderPropertyBuilder : PropertyBuilder
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public StringBuilderPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
                : base(typeDef, declaringTypeBuilder)
            {
            }

            /// <summary>
            /// Create a property name from the given getter.
            /// </summary>
            protected override string GetPropertyName(NetMethodDefinition getter)
            {
                switch (getter.Name)
                {
                    case "CharAt":
                        return "Chars";
                    default:
                        return base.GetPropertyName(getter);
                }
            }

            /// <summary>
            /// Is the given method a property get method?
            /// </summary>
            protected override bool IsGetter(NetMethodDefinition method)
            {
                var name = method.Name;
                if ((name == "Capacity") && (method.Parameters.Count == 0))
                    return true;
                if ((name == "Length") && (method.Parameters.Count == 0))
                    return true;
                if ((name == "CharAt") && (method.Parameters.Count == 1))
                    return false;
                return base.IsGetter(method);
            }

            /// <summary>
            /// Is the given method a property set method?
            /// </summary>
            protected override bool IsSetter(NetMethodDefinition method)
            {
                var name = method.Name;
                if ((name == "SetLength") && (method.Parameters.Count == 1))
                    return true;
                return base.IsSetter(method);
            }
        }
    }
}
