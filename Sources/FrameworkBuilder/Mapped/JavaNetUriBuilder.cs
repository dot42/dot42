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
    internal sealed class JavaNetUriBuilder : JavaBaseTypeBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaNetUriBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaNetUriBuilder(ClassFile cf)
            : base(cf, "System.Uri", "java/net/URI")
        {
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new UriPropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Custom property builder.
        /// </summary>
        private sealed class UriPropertyBuilder : PropertyBuilder
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public UriPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
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
                    case "GetPath":
                        return "AbsolutePath";
                    default:
                        return base.GetPropertyName(getter);
                }
            }
        }
    }

}
