using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Reachable;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Converter from IL only constructs to constructs understoods by Java.
    /// </summary>
    internal static class ILToJava
    {
        private static readonly IEnumerable<ILConverterFactory> ConverterFactories = CompositionRoot.CompositionContainer.GetExportedValues<ILConverterFactory>().OrderBy(x => x.Priority).ToList();

        /// <summary>
        /// Convert all constructs of IL that are not compatible with java.
        /// </summary>
        internal static void Convert(ReachableContext reachableContext)
        {
            foreach (var factory in ConverterFactories)
            {
                var converter = factory.Create();
                converter.Convert(reachableContext);
            }
        }
    }
}
