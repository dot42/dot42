using System.Collections.Generic;
using Dot42.DexLib;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Holds mapping information for attribute ctors.
    /// </summary>
    internal class AttributeCtorMapping
    {
        public readonly MethodDefinition Builder;
        public readonly List<MethodDefinition> ArgumentGetters;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AttributeCtorMapping(MethodDefinition builder, List<MethodDefinition> argumentGetters)
        {
            this.Builder = builder;
            ArgumentGetters = argumentGetters;
        }
    }
}
