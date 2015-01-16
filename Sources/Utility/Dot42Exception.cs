using System;

namespace Dot42.Utility
{
    public class Dot42Exception : Exception 
    {
        public Dot42Exception(string message)
            : base(message)
        {            
        }

        public Dot42Exception(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
