using System;

namespace Dot42.JvmClassLib
{
    public class ResolveException : Exception
    {
        public ResolveException(string msg)
            : base(msg)
        {
        }
    }
}
