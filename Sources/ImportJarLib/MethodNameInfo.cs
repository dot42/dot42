using System.ComponentModel;

namespace Dot42.ImportJarLib
{
    public sealed class MethodNameInfo
    {
        private readonly string name;
        private readonly bool isConstructor;
        private readonly bool isDeconstructor;
        private readonly EditorBrowsableState editorBrowsableState;

        public MethodNameInfo(string name)
            : this(name, false, false, EditorBrowsableState.Always)
        {
        }

        public MethodNameInfo(string name, bool isConstructor, bool isDeconstructor)
            : this(name, isConstructor, isDeconstructor, EditorBrowsableState.Always)
        {
        }

        public MethodNameInfo(string name, EditorBrowsableState editorBrowsableState)
            : this(name, false, false, editorBrowsableState)
        {            
        }

        public MethodNameInfo(string name, MethodNameInfo source)
            : this(name, source.isConstructor, source.isDeconstructor, source.editorBrowsableState)
        {
        }

        public MethodNameInfo(string name, bool isConstructor, bool isDeconstructor, EditorBrowsableState editorBrowsableState)
        {
            this.name = name;
            this.isConstructor = isConstructor;
            this.isDeconstructor = isDeconstructor;
            this.editorBrowsableState = editorBrowsableState;
        }

        public string Name
        {
            get { return name; }
        }

        public bool IsConstructor
        {
            get { return isConstructor; }
        }

        public EditorBrowsableState EditorBrowsableState
        {
            get { return editorBrowsableState; }
        }

        public bool IsDeconstructor
        {
            get { return isDeconstructor; }
        }

        public static implicit operator MethodNameInfo(string name)
        {
            return new MethodNameInfo(name);
        }
    }
}
