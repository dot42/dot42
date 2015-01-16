using System;
using System.Net;
using System.Net.Sockets;

namespace Dot42.DdmLib.support
{
    internal class SocketChannel
    {
        internal static SocketChannel open(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void close()
        {
            throw new NotImplementedException();            
        }

        public Socket socket()
        {
            throw new NotImplementedException();
        }

        public void configureBlocking(bool p0)
        {
            throw new NotImplementedException();
        }

        public int read(ByteBuffer buf)
        {
            throw new NotImplementedException();
        }

        public void register(Selector mSelector, int opRead, object client)
        {
            throw new NotImplementedException();
        }

        public int write(ByteBuffer mBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
