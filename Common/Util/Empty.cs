using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TallComponents.Common.Util
{
    /// <summary>
    /// Generic wrapper for empty arrays.
    /// </summary>
    public static class Empty<T>
    {
        public static readonly T[] Array = new T[0];
    }
}
