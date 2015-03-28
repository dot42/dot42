using System.Collections.Generic;
using System.Linq;

namespace Dot42.ApkSpy.IPC
{
    class ConsoleReader
    {
        private string _data = "";
        public string[] Add(string data)
        {
            _data += data;
            IList<string> ret = new List<string>();
            while (true)
            {
                int posLine = _data.IndexOf("\r\n");
                if (posLine == -1)
                    posLine = _data.IndexOf("\n");
                else
                    posLine += 1;

                if (posLine == -1) break;

                string line = _data.Substring(0, posLine).TrimEnd('\r', '\n');
                _data = _data.Substring(posLine + 1);

                ret.Add(line);
            }
            return ret.ToArray();
        }
    }
}
