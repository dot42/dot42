namespace Dot42.LoaderLib.Java
{
    public class ClassNotFoundException : LoaderException
    {
        private readonly string className;

        public ClassNotFoundException(string className) : base(string.Format("Class {0} not found", className))
        {
            this.className = className;
        }

        public string ClassName
        {
            get { return className; }
        }
    }
}
