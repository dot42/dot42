using System;
using System.Collections.Generic;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Compiler related extension methods
    /// </summary>
    public static partial class AssemblyCompilerExtensions
    {
        internal static void ForEach<T>(this IEnumerable<T> set, Action<T> action)
        {
            foreach (var item in set)
            {
                action(item);
            }
        }
    }
}
