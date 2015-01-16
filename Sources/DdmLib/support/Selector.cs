using System;
using System.Collections.Generic;

namespace Dot42.DdmLib.support
{
    internal class Selector
    {
        public void wakeup()
        {
            throw new NotImplementedException();
        }

        public void close()
        {
            throw new NotImplementedException();
        }

        public static Selector open()
        {
            throw new NotImplementedException();
        }

        public int @select()
        {
            throw new NotImplementedException();
        }

        public ISet<SelectionKey> selectedKeys()
        {
            throw new NotImplementedException();
        }
    }
}
