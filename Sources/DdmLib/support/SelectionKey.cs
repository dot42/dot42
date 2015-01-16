using System;

namespace Dot42.DdmLib.support
{
    internal class SelectionKey
    {
        public const int OP_READ = -1;
        public const int OP_ACCEPT = -1;

        public object attachment()
        {
            throw new NotImplementedException();
        }

        public bool readable
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool acceptable
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool valid
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ServerSocketChannel channel()
        {
            throw new NotImplementedException();
        }
    }
}
