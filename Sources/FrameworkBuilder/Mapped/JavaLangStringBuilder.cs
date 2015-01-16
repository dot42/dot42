using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
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
                var name = method.OriginalJavaName;
                if ((name == "length") && (method.Parameters.Count == 0))
                    return true;
                if ((name == "charAt") && (method.Parameters.Count == 1))
                    return true;
                if (name == "getBytes")
                    return false;
                return base.IsGetter(method);
            }

            /// <summary>
            /// Customize the custom attributes collection of the given method.
            /// </summary>
            protected override void AddCustomAttributes(NetMethodDefinition method, List<NetCustomAttribute> customAttributes)
            {
                if(method.OriginalJavaName == "charAt")
                    customAttributes.Add(new NetCustomAttribute(new NetTypeDefinition(ClassFile.Empty, method.Target, AttributeConstants.Dot42Scope) { Namespace = "System.Runtime.CompilerServices", Name = "IndexerName" }, "Chars"));
            }

        }
    }
}
