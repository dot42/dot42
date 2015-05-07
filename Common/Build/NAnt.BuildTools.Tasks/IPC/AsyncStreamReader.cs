using System;
using System.Diagnostics;
using System.IO;

namespace NAnt.BuildTools.Tasks.IPC
{
    public class AsyncStreamReader
    {
        private readonly byte[] _buffer;
        private readonly Stream _stream;

        public bool Closed { get; private set; }

        public AsyncStreamReader(Stream stream) : this(stream, 0)
        {
            _stream = stream;
            Closed = true;
        }
        public AsyncStreamReader(Stream stream, int bufsize)
        {
            _stream = stream;
            _buffer = new byte[bufsize==0?4096:bufsize];
        }

        public int BufferLen { get { return _buffer.Length; } }

        public void Begin()
        {
            if(!_stream.CanRead)
            {
                OnClosed();
                return;
            }

            Closed = false;
            _stream.BeginRead(_buffer, 0, _buffer.Length, MyCallback, null);            
        }

        public void End()
        {
            Closed = true;
        }
        
        [DebuggerNonUserCode]
        private void MyCallback(IAsyncResult ar)
        {
            try
            {
                int len = _stream.EndRead(ar);
                if (Closed) return;

                if (len == 0)
                {
                    OnClosed();
                    return;
                }

                if(DataRead != null) DataRead(_buffer, len);
                Begin();
            }
            catch (Exception)
            {
                OnClosed();
            }
        }

        private void OnClosed()
        {
            bool wasClosed = Closed;
            Closed = true;
            if (DataRead != null && !wasClosed)
                DataRead(null, 0);
        }

        public event Action<byte[], int> DataRead;
    }
}
