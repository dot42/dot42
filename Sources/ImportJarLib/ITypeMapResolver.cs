using System;

namespace Dot42.ImportJarLib
{
    public interface ITypeMapResolver
    {
        /// <summary>
        /// Try to resolve the given java class name.
        /// </summary>
        void TryResolve(string javaClassName, TypeNameMap typeNameMap);

        /// <summary>
        /// Try to resolve the given NET name.
        /// </summary>
        void TryResolve(Type netType, TypeNameMap typeNameMap);

        /// <summary>
        /// If true, a lack of generic parameters is accepted.
        /// </summary>
        bool AcceptLackOfGenericParameters { get; }
    }
}
