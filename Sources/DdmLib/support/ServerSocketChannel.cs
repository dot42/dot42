using System;
using System.Net.Sockets;

namespace Dot42.DdmLib.support
{
    internal class ServerSocketChannel
    {
        internal static ServerSocketChannel open()
        {
            throw new NotImplementedException();
        }

        public Socket socket()
        {
            throw new NotImplementedException();
        }

        public void configureBlocking(bool b)
        {
            throw new NotImplementedException();
        }

        public void close()
        {
            throw new NotImplementedException();
        }

        public void register(Selector sel, int opAccept, object client)
        {
            throw new NotImplementedException();
        }

        public SocketChannel accept()
        {
            throw new NotImplementedException();
        }
    }
}
