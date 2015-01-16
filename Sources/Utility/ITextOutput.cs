namespace Dot42.Utility
{
    public interface ITextOutput
    {
        void Indent();
        void Unindent();
        void Write(char ch);
        void Write(string text);
        void WriteLine();
        void WriteDefinition(string text, object definition, bool isLocal = true);
        void WriteReference(string text, object reference, bool isLocal = false);
    }

    public static class TextOutputExtensions
    {
        public static void Write(this ITextOutput output, string format, params object[] args)
        {
            output.Write(string.Format(format, args));
        }

        public static void WriteLine(this ITextOutput output, string text)
        {
            output.Write(text);
            output.WriteLine();
        }

        public static void WriteLine(this ITextOutput output, string format, params object[] args)
        {
            output.WriteLine(string.Format(format, args));
        }
    }
}
