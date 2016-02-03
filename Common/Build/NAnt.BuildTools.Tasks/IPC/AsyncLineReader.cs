using System;
using System.Text;

namespace NAnt.BuildTools.Tasks.IPC
{
    public class AsyncLineReader
    {
        private readonly AsyncStreamReader _streamReader;
        private readonly ConsoleReader _consoleReader = new ConsoleReader();

        public event Action<string> LineRead;

        public Encoding Encoding { get; set; }

        public AsyncLineReader(AsyncStreamReader sr)
        {
            _streamReader = sr;
            _streamReader.DataRead += StreamReaderDataRead;
            Encoding = Encoding.Default;
        }

        void StreamReaderDataRead(byte[] data, int len)
        {
            if(LineRead == null) return;
            if (data == null) { LineRead(null); return; }

            string s = Encoding.GetString(data, 0, len);
            foreach (string l in _consoleReader.Add(s))
                LineRead(l);
        }

        public void Begin()
        {
            _streamReader.Begin();
        }

        public void End()
        {
            _streamReader.End();
        }
    }
}
