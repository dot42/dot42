using System.Collections.Generic;

namespace Dot42.JvmClassLib.Attributes
{
    internal interface IModifiableAttributeProvider : IAttributeProvider
    {
        /// <summary>
        /// Adds the given attribute to this provider.
        /// </summary>
        void Add(Attribute attribute);

        /// <summary>
        /// Signals that all attributes have been loaded.
        /// </summary>
        void AttributesLoaded();
    }
}
