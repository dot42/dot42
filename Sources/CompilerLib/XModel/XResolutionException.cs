using System;

namespace Dot42.CompilerLib.XModel
{
    public class XResolutionException : Exception
    {
        public XResolutionException(XReference member)
            : base(string.Format("Cannot resolve {0}", member.FullName))
        {            
        }
    }
}
