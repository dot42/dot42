using System.Collections.ObjectModel;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Interface implemented by generic parameter providers.
    /// </summary>
    public interface IXGenericParameterProvider
    {
        /// <summary>
        /// Gets the containing module.
        /// </summary>
        XModule Module { get; }

        /// <summary>
        /// Gets all generic parameters
        /// </summary>
        ReadOnlyCollection<XGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Is this provider the same as the given other provider?
        /// </summary>
        bool IsSame(IXGenericParameterProvider other);
    }
}
