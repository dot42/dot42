using System.Collections.Generic;

namespace Dot42.ImportJarLib.Model
{
    public interface INetGenericParameterProvider
    {
        /// <summary>
        /// All generic parameters
        /// </summary>
        List<NetGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Is this a method?
        /// If not it's a type.
        /// </summary>
        bool IsMethod { get; }

        /// <summary>
        /// Is this a type?
        /// If not it's a method.
        /// </summary>
        bool IsType { get; }

        /// <summary>
        /// Is this an interface type?
        /// </summary>
        bool IsInterface { get; }
    }
}
