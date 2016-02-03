using System.Reflection;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Custom;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Android
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal abstract class AndroidBuilder : StandardTypeBuilder, IMappedTypeBuilder
    {
        private readonly string className;


        /// <summary>
        /// Default ctor
        /// </summary>
        protected AndroidBuilder(ClassFile cf, string className)
            : base(cf)
        {
            this.className = className;
        }

        /// <summary>
        /// Gets the name of the java class that this builder will map.
        /// </summary>
        public string ClassName
        {
            get { return className; }
        }

        /// <summary>
        /// Create a new type builder for the given class.
        /// </summary>
        TypeBuilder IMappedTypeBuilder.Create(ClassFile cf)
        {
            var ctor = GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof (ClassFile)}, null);
            return (TypeBuilder) ctor.Invoke(new object[] {cf});
        }
    }
}
