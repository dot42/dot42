using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib.Mapped
{
    public static class MappedTypeBuilder
    {
        private static Dictionary<string, IMappedTypeBuilder> mappedTypeBuilders;

        /// <summary>
        /// Static ctor
        /// </summary>
        public static void Initialize(CompositionContainer compositionContainer)
        {
            mappedTypeBuilders = new Dictionary<string, IMappedTypeBuilder>();
            foreach (var builder in compositionContainer.GetExportedValues<IMappedTypeBuilder>())
            {
                mappedTypeBuilders.Add(builder.ClassName, builder);
            }
        }

        /// <summary>
        /// Try to create a type builder for the given class.
        /// </summary>
        internal static bool TryCreateTypeBuilder(ClassFile cf, out TypeBuilder builder)
        {
            if (mappedTypeBuilders != null)
            {
                IMappedTypeBuilder mappedBuilder;
                if (mappedTypeBuilders.TryGetValue(cf.ClassName, out mappedBuilder))
                {
                    builder = mappedBuilder.Create(cf);
                    return true;
                }
            }
            builder = null;
            return false;
        }
    }
}
