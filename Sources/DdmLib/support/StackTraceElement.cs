namespace Dot42.DdmLib.support
{
    public class StackTraceElement
    {
        public readonly string className;
        public readonly string fileName;
        public readonly string methodName;
        public readonly int lineNumber;
        public readonly bool nativeMethod;

        public StackTraceElement(string className, string methodName, string fileName, int lineNumber)
        {
            this.className = className;
            this.methodName = methodName;
            this.fileName = fileName;
            this.lineNumber = lineNumber;
        }
    }
}
