using System;

namespace Dot42.CompilerLib.CompilerCache
{
    public class CompilerCacheResolveException : Exception
    {
        public CompilerCacheResolveException(string msg) : base(msg)
        {
        }
    }
}
