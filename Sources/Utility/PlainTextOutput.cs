using System.IO;
using System.Linq;

namespace Dot42.Utility
{
    public class PlainTextOutput : ITextOutput 
    {
        private readonly StringWriter writer;
        private int indentLevel = 0;
        private bool startOfLine = true;
        private string indent = "";

        /// <summary>
        /// Default ctor
        /// </summary>
        public PlainTextOutput(StringWriter writer)
        {
            this.writer = writer;
        }

        public void Indent()
        {
            indentLevel++;
            indent = string.Join("", Enumerable.Repeat("    ", indentLevel));
        }

        public void Unindent()
        {
            indentLevel--;
            indent = string.Join("", Enumerable.Repeat("    ", indentLevel));
        }

        /// <summary>
        /// Append a line
        /// </summary>
        public void WriteLine()
        {
            writer.WriteLine();
            startOfLine = true;
        }

        public void Write(char ch)
        {
            if (startOfLine)
            {
                writer.Write(indent);
                startOfLine = false;
            }
            writer.Write(ch);
        }
        public void Write(string text)
        {
            if (startOfLine)
            {
                writer.Write(indent);
                startOfLine = false;
            }
            writer.Write(text);
        }

        public void WriteDefinition(string text, object definition, bool isLocal = true)
        {
            Write(text);
        }

        public void WriteReference(string text, object reference, bool isLocal = false)
        {
            Write(text);            
        }
    }
}
