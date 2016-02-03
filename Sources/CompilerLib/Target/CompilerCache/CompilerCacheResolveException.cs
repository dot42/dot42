using System;

namespace Dot42.CompilerLib.Target.CompilerCache
{
    public class CompilerCacheResolveException : Exception
    {
        public CompilerCacheResolveException(string msg) : base(msg)
        {
        }
    }
}
