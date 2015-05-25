using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.JvmClassLib.Attributes;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Single class definition
    /// </summary>
    public sealed class ClassFile : AbstractReference, IModifiableAttributeProvider, IAccessModifiers
    {
        private readonly List<FieldDefinition> fields = new List<FieldDefinition>();
        private readonly List<MethodDefinition> methods = new List<MethodDefinition>();
        private readonly List<Attribute> attributes = new List<Attribute>();
        private readonly IClassLoader loader;
        private ClassSignature signature;
        private ClassFile superClassFile;
        private ClassFile declaringClass;

        internal const uint Magic = 0xCAFEBABE;

        public static readonly ClassFile Empty = new ClassFile();

        /// <summary>
        /// Empty ctor
        /// </summary>
        private ClassFile()
        {            
        }

        /// <summary>
        /// Read the given class from the given stream
        /// </summary>
        public ClassFile(Stream stream, IClassLoader loader)
        {
            this.loader = loader;
            if (stream != null)
            {
                var cfReader = new ClassFileReader(stream);
                cfReader.ReadHeader(this);
            }
        }

        public int MinorVersion { get; set; }
        public int MajorVersion { get; set; }
        public ClassAccessFlags ClassAccessFlags { get; set; }

        /// <summary>
        /// Gets the access flags for this class.
        /// Either from the class itself or from the inner-class in case this is a nested class.
        /// </summary>
        public ClassAccessFlags AccessFlags
        {
            get
            {
                var attr = InnerClassesAttribute;
                if (attr == null)
                    return ClassAccessFlags;
                var innerClass = attr.Classes.FirstOrDefault(x => !x.IsAnonymous && x.Inner.ClassName == ClassName);
                if (innerClass == null)
                    return ClassAccessFlags;
                return (ClassAccessFlags) innerClass.AccessFlags;
            }
        }

        /// <summary>
        /// Visible to class, package, subclass, world
        /// </summary>
        public bool IsPublic
        {
            get { return AccessFlags.HasFlag(ClassAccessFlags.Public); }
        }

        /// <summary>
        /// Visible to class, package, subclass
        /// </summary>
        public bool IsProtected
        {
            get { return AccessFlags.HasFlag((ClassAccessFlags)NestedClassAccessFlags.Protected); }
        }

        /// <summary>
        /// Visible to class, package
        /// </summary>
        public bool IsPackagePrivate
        {
            get { return ((AccessFlags & ClassAccessFlags.AccessMask) == 0); }
        }

        /// <summary>
        /// Visible to class
        /// </summary>
        public bool IsPrivate
        {
            get { return AccessFlags.HasFlag((ClassAccessFlags)NestedClassAccessFlags.Private); }
        }

        public bool IsFinal { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Final); } }
        public bool IsSuper { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Super); } }
        public bool IsInterface { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Interface); } }
        public bool IsAbstract { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Abstract); } }
        public bool IsSynthetic { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Synthetic); } }
        public bool IsAnnotation { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Annotation); } }
        public bool IsEnum { get { return ClassAccessFlags.HasFlag(ClassAccessFlags.Enum); } }

        /// <summary>
        /// Name of this class in java terms
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Name of this class as CLR type name
        /// </summary>
        public string ClrTypeName { get { return JvmClassLib.ClassName.JavaClassNameToClrTypeName(ClassName); } }

        /// <summary>
        /// Name of super class (null if Object)
        /// </summary>        
        public ObjectTypeReference SuperClass { get; set; }

        /// <summary>
        /// Does this class file contain java.lang.Object?
        /// </summary>
        public bool IsObject
        {
            get { return (ClassName == "java/lang/Object"); }
        }

        /// <summary>
        /// Does this class file contain java.lang.Void?
        /// </summary>
        public bool IsVoid
        {
            get { return (ClassName == "java/lang/Void"); }
        }

        /// <summary>
        /// Load the super class.
        /// </summary>
        public bool TryGetSuperClass(out ClassFile result)
        {
            result = null;
            if ((SuperClass == null) || IsObject)
                return false; // Object
            if (superClassFile == null)
            {
                loader.TryLoadClass(SuperClass.ClassName, out superClassFile);
            }
            result = superClassFile;
            return (result != null);
        }

        /// <summary>
        /// Names of all implemented interfaces in java terms
        /// </summary>
        public ObjectTypeReference[] Interfaces { get; set; }

        /// <summary>
        /// Names of all implemented interfaces as CLR type names
        /// </summary>
        public IEnumerable<string> ClrInterfaces { get { return Interfaces.Select(x => x.ClrTypeName); } }

        /// <summary>
        /// Gets my class loader
        /// </summary>
        public IClassLoader Loader { get { return loader; } }

        /// <summary>
        /// Is this class created by the class loader (?) (instead of being loaded from a normal class)
        /// </summary>
        public bool IsCreatedByLoader { get; set; }

        /// <summary>
        /// Gets all fields;
        /// </summary>
        public List<FieldDefinition> Fields { get { return fields; } }

        /// <summary>
        /// Gets all methods
        /// </summary>
        public List<MethodDefinition> Methods { get { return methods; } }

        /// <summary>
        /// Gets all attributes of this class.
        /// </summary>
        public IEnumerable<Attribute> Attributes { get { return attributes; } }

        /// <summary>
        /// Gets the signature of this class (if any).
        /// </summary>
        public ClassSignature Signature { get { return signature; } }

        /// <summary>
        /// Adds the given attribute to this provider.
        /// </summary>
        void IModifiableAttributeProvider.Add(Attribute attribute)
        {
            attributes.Add(attribute);
        }

        /// <summary>
        /// Signals that all attributes have been loaded.
        /// </summary>
        void IModifiableAttributeProvider.AttributesLoaded()
        {
            var signatureAttributes = Attributes.OfType<SignatureAttribute>();
            signature = signatureAttributes.Select(x => Signatures.ParseClassSignature(x.Value)).SingleOrDefault() ??
                        new ClassSignature(null, null, SuperClass, Interfaces);
        }

        /// <summary>
        /// Override the signature
        /// </summary>
        public void SetSignature(string value)
        {
            if (value == null)
                return;
            signature = Signatures.ParseClassSignature(value);
        }

        /// <summary>
        /// Gets all recorded inner classes
        /// </summary>
        public IEnumerable<InnerClass> InnerClasses
        {
            get
            {
                var attr = InnerClassesAttribute;
                if (attr == null)
                    return Enumerable.Empty<InnerClass>();
                var myName = ClassName;
                return attr.Classes.Where(x => x.IsAnonymous || ((x.Outer != null) && (x.Outer.ClassName == myName)));
            }
        }

        /// <summary>
        /// Get the inner classes attribute on this class, if there is one.
        /// </summary>
        public InnerClassesAttribute InnerClassesAttribute
        {
            get { return attributes.OfType<InnerClassesAttribute>().SingleOrDefault(); }
        }

        /// <summary>
        /// Is this a nested class?
        /// </summary>
        public bool IsNested
        {
            get
            {
                var attr = InnerClassesAttribute;
                if (attr == null)
                    return false;
                return attr.Classes.Any(x => !x.IsAnonymous && x.Inner.ClassName == ClassName);
            }
        }

        /// <summary>
        /// Gets the declaring class (if nested)
        /// </summary>
        public ClassFile DeclaringClass
        {
            get
            {
                if ((declaringClass == null) && IsNested)
                {
                    var attr = InnerClassesAttribute;
                    if (attr == null)
                        return null;
                    var innerClass = attr.Classes.FirstOrDefault(x => !x.IsAnonymous && x.Inner.ClassName == ClassName);
                    if (innerClass == null)
                        return null;
                    declaringClass = innerClass.OuterClassFile;
                }
                return declaringClass;
            }
        }

        /// <summary>
        /// Gets the package part of the class name?
        /// </summary>
        public string Package
        {
            get { return JvmClassLib.ClassName.GetPackage(ClassName); }
        }

        /// <summary>
        /// Gets the name part of the class name?
        /// </summary>
        public string Name
        {
            get
            {
                var attr = InnerClassesAttribute;
                var className = ClassName;
                if (attr != null)
                {
                    var innerClass = attr.Classes.FirstOrDefault(x => !x.IsAnonymous && x.Inner.ClassName == className);
                    if (innerClass != null)
                        return innerClass.Name;
                }
                return JvmClassLib.ClassName.StripPackage(className);
            }
        }

        public override string ToString()
        {
            return ClassName;
        }
    }
}
