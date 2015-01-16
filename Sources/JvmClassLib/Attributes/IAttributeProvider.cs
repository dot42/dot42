using System.Collections.Generic;

namespace Dot42.JvmClassLib.Attributes
{
    public interface IAttributeProvider
    {
        /// <summary>
        /// Gets the list of attributes
        /// </summary>
        IEnumerable<Attribute> Attributes { get; }
    }
}
