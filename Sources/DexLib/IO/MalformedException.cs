using System;

namespace Dot42.DexLib.IO
{
    public class MalformedException : Exception
    {
        public MalformedException(String message)
            : base(message)
        {
        }
    }
}