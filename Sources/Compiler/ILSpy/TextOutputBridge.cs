using Dot42.Utility;

namespace Dot42.Compiler.ILSpy
{
    /// <summary>
    /// Bridge internal ITextOutput to ILSpy ITextOutput
    /// </summary>
    internal sealed class TextOutputBridge : ITextOutput
    {
        private readonly ICSharpCode.Decompiler.ITextOutput output;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TextOutputBridge(ICSharpCode.Decompiler.ITextOutput output)
        {
            this.output = output;
        }

        public void Indent()
        {
            output.Indent();
        }

        public void Unindent()
        {
            output.Unindent();
        }

        public void Write(char ch)
        {
            output.Write(ch);
        }

        public void Write(string text)
        {
            output.Write(text);
        }

        public void WriteLine()
        {
            output.WriteLine();
        }

        public void WriteDefinition(string text, object definition, bool isLocal = true)
        {
            output.WriteDefinition(text, definition, isLocal);
        }

        public void WriteReference(string text, object reference, bool isLocal = false)
        {
            output.WriteReference(text, reference, isLocal);
        }
    }
}
