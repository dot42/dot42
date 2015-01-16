using System.Collections.ObjectModel;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Interface implemented by generic instances.
    /// </summary>
    public interface IXGenericInstance
    {
        /// <summary>
        /// Gets the containing module.
        /// </summary>
        XModule Module { get; }

        /// <summary>
        /// Gets all generic parameters
        /// </summary>
        ReadOnlyCollection<XTypeReference> GenericArguments { get; }
    }
}
