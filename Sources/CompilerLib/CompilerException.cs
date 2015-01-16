using System;
using Dot42.Utility;

namespace Dot42.CompilerLib
{
    public class CompilerException : Dot42Exception
    {
        public CompilerException(string message)
            : base(message)
        {
        }

        public CompilerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
