using System.Collections.Generic;
using System.Linq;
using Dot42.DexLib.IO;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public class ClassDefinition : ClassReference, IMemberDefinition
    {
        public ClassDefinition()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;

            Interfaces = new List<ClassReference>();
            Annotations = new List<Annotation>();
            Fields = new List<FieldDefinition>();
            Methods = new List<MethodDefinition>();
            InnerClasses = new List<ClassDefinition>();
        }

        internal ClassDefinition(ClassReference cref)
            : this()
        {
            Fullname = cref.Fullname;
            Namespace = cref.Namespace;
            Name = cref.Name;
        }

        public int MapFileId { get; set; }
        public ClassReference SuperClass { get; set; }
        public List<ClassDefinition> InnerClasses { get; set; }
        public List<ClassReference> Interfaces { get; set; }
        public string SourceFile { get; set; }
        public List<FieldDefinition> Fields { get; set; }
        public List<MethodDefinition> Methods { get; set; }

        /// <summary>
        /// Field holding generic type arguments
        /// </summary>
        public FieldDefinition GenericInstanceField { get; set; }

        #region IMemberDefinition Members

        public AccessFlags AccessFlags { get; set; }
        public List<Annotation> Annotations { get; set; }
        public ClassDefinition Owner { get; set; }

        #endregion

        public IEnumerable<MethodDefinition> GetMethods(string name)
        {
            foreach (var mdef in Methods)
                if (mdef.Name == name)
                    yield return mdef;
        }

        public MethodDefinition GetMethod(string name)
        {
            return Enumerable.First(GetMethods(name));
        }

        public FieldDefinition GetField(string name)
        {
            foreach (var fdef in Fields)
                if (fdef.Name == name)
                    return fdef;
            return null;
        }

        #region " AccessFlags "

        public bool IsPublic
        {
            get { return (AccessFlags & AccessFlags.Public) != 0; }
            set { SetAccessFlags(AccessFlags.Public, value); }
        }

        public bool IsPrivate
        {
            get { return (AccessFlags & AccessFlags.Private) != 0; }
            set { SetAccessFlags(AccessFlags.Private, value); }
        }

        public bool IsProtected
        {
            get { return (AccessFlags & AccessFlags.Protected) != 0; }
            set { SetAccessFlags(AccessFlags.Protected, value); }
        }

        public bool IsStatic
        {
            get { return (AccessFlags & AccessFlags.Static) != 0; }
            set { SetAccessFlags(AccessFlags.Static, value); }
        }

        public bool IsFinal
        {
            get { return (AccessFlags & AccessFlags.Final) != 0; }
            set { SetAccessFlags(AccessFlags.Final, value); }
        }

        public bool IsInterface
        {
            get { return (AccessFlags & AccessFlags.Interface) != 0; }
            set { SetAccessFlags(AccessFlags.Interface, value); }
        }

        public bool IsAbstract
        {
            get { return (AccessFlags & AccessFlags.Abstract) != 0; }
            set { SetAccessFlags(AccessFlags.Abstract, value); }
        }

        public bool IsSynthetic
        {
            get { return (AccessFlags & AccessFlags.Synthetic) != 0; }
            set { SetAccessFlags(AccessFlags.Synthetic, value); }
        }

        public bool IsAnnotation
        {
            get { return (AccessFlags & AccessFlags.Annotation) != 0; }
            set { SetAccessFlags(AccessFlags.Annotation, value); }
        }

        public bool IsEnum
        {
            get { return (AccessFlags & AccessFlags.Enum) != 0; }
            set { SetAccessFlags(AccessFlags.Enum, value); }
        }

        private void SetAccessFlags(AccessFlags flags, bool value)
        {
            if (value)
            {
                AccessFlags |= flags;
            }
            else
            {
                AccessFlags &= ~flags;
            }
        }

        #endregion

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(ClassDefinition other)
        {
            // Should be enough (ownership)
            return base.Equals(other);
        }

        #region " Static utilities "

        internal static List<ClassDefinition> Flattenize(List<ClassDefinition> container)
        {
            var result = new List<ClassDefinition>();
            foreach (var cdef in container)
            {
                result.Add(cdef);
                result.AddRange(Flattenize(cdef.InnerClasses));
            }
            return result;
        }

        internal static List<ClassDefinition> Hierarchicalize(List<ClassDefinition> container)
        {
            var result = new List<ClassDefinition>();
            foreach (var cdef in container)
            {
                if (cdef.Fullname.Contains(DexConsts.InnerClassMarker))
                {
                    var items = cdef.Fullname.Split(DexConsts.InnerClassMarker);
                    var fullname = items[0];
                    var name = items[1];
                    var owner = Dex.GetClass(fullname, container);
                    if (owner != null)
                    {
                        owner.InnerClasses.Add(cdef);
                        cdef.Owner = owner;
                    }
                }
                else
                {
                    result.Add(cdef);
                }
            }
            return result;
        }

        #endregion
    }
}