using System.Reflection;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    internal abstract class JavaBaseTypeBuilder : StandardTypeBuilder, IMappedTypeBuilder
    {
        private readonly string className;
        private readonly string clrTypeName;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected JavaBaseTypeBuilder(ClassFile cf, string clrTypeName, string className)
            : base(cf)
        {
            this.clrTypeName = clrTypeName;
            this.className = className;
        }

        /// <summary>
        /// Gets the full typename of the CLR type being created
        /// </summary>
        public string ClrTypeName { get { return clrTypeName; } }

        /// <summary>
        /// Helps in sorting type builders
        /// </summary>
        public override int Priority { get { return 2; } }

        /// <summary>
        /// Gets the full type name for the given java class.
        /// </summary>
        protected sealed override string GetFullName()
        {
            return ClrTypeName;
        }

        /// <summary>
        /// Gets the name of the given field
        /// </summary>
        public override string GetFieldName(FieldDefinition field)
        {
            switch (field.Name)
            {
                case "MAX_VALUE":
                    return "MaxValue";
                case "MIN_VALUE":
                    return "MinValue";
                default:
                    return field.Name;
            }
        }

        /// <summary>
        /// Gets the name of the java class that this builder will map.
        /// </summary>
        public string ClassName { get { return className; } }

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
