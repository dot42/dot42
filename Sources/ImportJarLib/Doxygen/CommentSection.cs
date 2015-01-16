using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Dot42.ImportJarLib.Doxygen
{
    internal class CommentSection 
    {
        private readonly string tag;
        private readonly StringBuilder content;

        public CommentSection(string tag)
        {
            this.tag = tag;
            content = new StringBuilder();
        }

        public void Write(string value)
        {
            content.Append(value);
        }

        public void Write(string value, params object[] args)
        {
            content.AppendFormat(value, args);
        }

        public void WriteLine(string value)
        {
            content.AppendLine(value);
        }

        public void WriteLine()
        {
            content.AppendLine();
        }

        /// <summary>
        /// Is this section empty?
        /// </summary>
        public bool IsEmpty { get { return (content.Length == 0) || (content.ToString().Trim().Length == 0); } }

        /// <summary>
        /// Write this section to the given writer.
        /// </summary>
        public void WriteTo(TextWriter writer, string indent)
        {
            if (IsEmpty)
                return;
            writer.Write(indent);
            writer.WriteLine("/// <{0}>", tag);

            MakeReplacements();
            var lines = content.ToString().Split('\n', '\r');
            foreach (var line in lines)
            {
                writer.Write(indent);
                writer.Write("/// ");
                writer.Write(line);
                writer.WriteLine();
            }

            writer.Write(indent);
            writer.WriteLine("/// </{0}>", tag);
        }

        private void MakeReplacements()
        {
            content.Replace("</pre></para><para><pre>", "\n\n");
            content.Replace("\r\n", "\n");
            content.Replace("\n\r", "\n");            
        }
    }
}
