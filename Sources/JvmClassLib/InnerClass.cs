using System.Diagnostics;

namespace Dot42.JvmClassLib
{
    [DebuggerDisplay("{Inner} {Outer} {AccessFlags}")]
    public class InnerClass
    {
        private readonly IClassLoader loader;
        private readonly TypeReference innerClass;
        private readonly TypeReference outerClass;
        private readonly string name;
        private readonly NestedClassAccessFlags accessFlags;
        private ClassFile outerClassFile;
        private ClassFile innerClassFile;

        internal InnerClass(IClassLoader loader, TypeReference innerClass, TypeReference outerClass, string name, NestedClassAccessFlags accessFlags)
        {
            this.loader = loader;
            this.innerClass = innerClass;
            this.outerClass = outerClass;
            this.name = name;
            this.accessFlags = accessFlags;
        }

        public NestedClassAccessFlags AccessFlags
        {
            get { return accessFlags; }
        }

        public bool IsPublic { get { return ((AccessFlags & NestedClassAccessFlags.Public) != 0); } }
        public bool IsPrivate { get { return ((AccessFlags & NestedClassAccessFlags.Private) != 0); } }
        public bool IsProtected  { get { return ((AccessFlags & NestedClassAccessFlags.Protected) != 0); } }
        public bool IsStatic { get { return ((AccessFlags & NestedClassAccessFlags.Static) != 0); } }
        public bool IsFinal { get { return ((AccessFlags & NestedClassAccessFlags.Final) != 0); } }
        public bool IsInterface { get { return ((AccessFlags & NestedClassAccessFlags.Interface) != 0); } }
        public bool IsAbstract { get { return ((AccessFlags & NestedClassAccessFlags.Abstract) != 0); } }
        public bool IsSynthetic { get { return ((AccessFlags & NestedClassAccessFlags.Synthetic) != 0); } }
        public bool IsAnnotation { get { return ((AccessFlags & NestedClassAccessFlags.Annotation) != 0); } }
        public bool IsEnum { get { return ((AccessFlags & NestedClassAccessFlags.Enum) != 0); } }

        public bool IsAnonymous
        {
            get { return (name == null); }
        }

        public string Name
        {
            get { return name; }
        }

        public TypeReference Outer
        {
            get { return outerClass; }
        }

        /// <summary>
        /// Gets the resolved outer class
        /// </summary>
        public ClassFile OuterClassFile
        {
            get
            {
                if (outerClass == null)
                    return null;
                if (outerClassFile == null)
                {
                    if (!loader.TryLoadClass(outerClass.ClassName, out outerClassFile))
                        throw new ResolveException(outerClass.ClassName);
                }
                return outerClassFile;
            }
        }

        public TypeReference Inner
        {
            get { return innerClass; }
        }

        /// <summary>
        /// Gets the resolved inner class
        /// </summary>
        public ClassFile InnerClassFile
        {
            get
            {
                if (innerClassFile == null)
                {
                    if (!loader.TryLoadClass(innerClass.ClassName, out innerClassFile))
                        throw new ResolveException(innerClass.ClassName);                    
                }
                return innerClassFile;
            }
        }
    }
}