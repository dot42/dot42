using System;

namespace Dot42.DebuggerLib
{
    public class DebuggerException : Exception
    {
        public DebuggerException()
        {
            
        }

        public DebuggerException(string message)
            : base(message)
        {

        }
    }
}
