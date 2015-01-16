using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Android
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangConcurrentAtomicAtomicIntegerBuilder : AndroidBuilder
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangConcurrentAtomicAtomicIntegerBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangConcurrentAtomicAtomicIntegerBuilder(ClassFile cf)
            : base(cf, "java/util/concurrent/atomic/AtomicInteger")
        {
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new JavaLangConcurrentAtomicAtomicIntegerPropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Custom property builder.
        /// </summary>
        private sealed class JavaLangConcurrentAtomicAtomicIntegerPropertyBuilder : PropertyBuilder
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaLangConcurrentAtomicAtomicIntegerPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder)
                : base(typeDef, declaringTypeBuilder)
            {
            }


            /// <summary>
            /// Is the given method a property get method?
            /// </summary>
            protected override bool IsGetter(NetMethodDefinition method)
            {
                var name = method.OriginalJavaName;
               
                if (name == "getAndDecrement") return false;
                if (name == "getAndIncrement") return false;
                
                return base.IsGetter(method);
            }
        }
    }
}
