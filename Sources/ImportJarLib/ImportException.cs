using System;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Import related exception.
    /// </summary>
    public class ImportException : Exception
    {
        public ImportException(string msg)
            : base(msg)
        {
        }
    }
}
