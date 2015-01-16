using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Mapped
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaLangFloatBuilder : JavaBoxedBuilder 
    {
        /// <summary>
        /// Empty ctor
        /// </summary>
        public JavaLangFloatBuilder() : this(ClassFile.Empty) { }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal JavaLangFloatBuilder(ClassFile cf)
            : base(cf, "System.Single", "java/lang/Float")
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
                case "ParseFloat":
                    renamer.Rename(method, "Parse");
                    break;

                case "IsInfinite":
                    if (method.IsStatic) 
                        renamer.Rename(method, "IsInfinity");
                    break;
            }
        }

        /// <summary>
        /// Gets the name of the given field
        /// </summary>
        public override string GetFieldName(FieldDefinition field)
        {
            switch (field.Name)
            {
                case "MIN_VALUE":
                    return "Epsilon";

                case "NEGATIVE_INFINITY":
                    return "NegativeInfinity";

                case "POSITIVE_INFINITY":
                    return "PositiveInfinity";

                default:
                    return base.GetFieldName(field);
            }
        }
    }
}
