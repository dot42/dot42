using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.FrameworkDefinitions;
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
    internal sealed class JavaLangStringBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangStringBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangStringBuilder(ClassFile cf)
            : base(cf, "System.String", "java/lang/String")
        {
        }

        /// <summary>
        /// Helps in sorting type builders
        /// </summary>
        public override int Priority { get { return 1; } }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new StringPropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Gets the name of the given method
        /// </summary>
        public override void ModifyMethodName(ImportJarLib.Model.NetMethodDefinition method, MethodRenamer renamer)
        {
            base.ModifyMethodName(method, renamer);
            switch (method.Name)
            {
                case "Format":
                    renamer.RenameMethodOnly(method, "JavaFormat");
                    method.EditorBrowsableState = EditorBrowsableState.Advanced;
                    break;
                case "Substring":
                    renamer.Rename(method, "JavaSubstring");
                    method.EditorBrowsableState = EditorBrowsableState.Advanced;
                    break;
                case "ToLowerCase":
                    renamer.Rename(method, "ToLower");
                    break;
                case "Length":
                    method.SetExplicitImplementation(method.Overrides.First(),method.Overrides.First().DeclaringType);
                    break;
                case "CharAt":
                    method.SetExplicitImplementation(method.Overrides.First(), method.Overrides.First().DeclaringType);
                    break;
                case "ToUpperCase":
                    renamer.Rename(method, "ToUpper");
                    break;
            }
        }

        /// <summary>
        /// Custom property builder.
        /// </summary>
        private sealed class StringPropertyBuilder : PropertyBuilder
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public StringPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
                : base(typeDef, declaringTypeBuilder)
            {
            }

            /// <summary>
            /// Is the given method a property get method?
            /// </summary>
            protected override bool IsGetter(NetMethodDefinition method)
            {
                var name = method.OriginalJavaName;
                
                if (name == "getBytes")
                    return false;
                return base.IsGetter(method);
            }
        }
    }
}
