using System;

namespace Dot42.CompilerLib.Target.Dx
{
    public class DxException : Exception
    {
        public DxException( string msg) : base(msg)
        {
        }
    }
}