using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.DexLib.IO;
using Dot42.DexLib.Metadata;
using Dot42.Utility;

namespace Dot42.DexLib
{
    public class ClassDefinition : ClassReference, IMemberDefinition
    {
        // TODO: implement the freezable contract.

        private readonly List<ClassDefinition> innerClasses;
        private readonly List<WeakReference> belongsToDex = new List<WeakReference>();
        private List<Annotation> _annotations;
        private List<FieldDefinition> _fields;
        private List<MethodDefinition> _methods;
        private List<ClassReference> _interfaces;

        public ClassDefinition()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;

            Interfaces = new List<ClassReference>();
            Annotations = new List<Annotation>();
            Fields = new List<FieldDefinition>();
            Methods = new List<MethodDefinition>();
            innerClasses = new List<ClassDefinition>();
        }

        internal ClassDefinition(ClassReference cref)
            : this()
        {
            fullNameCache = cref.Fullname;
            ns = cref.Namespace;
            name = cref.Name;
        }

        public int MapFileId { get; set; }
        public ClassReference SuperClass { get; set; }
        public ICollection<ClassDefinition> InnerClasses { get { return innerClasses.AsReadOnly(); } }
        public IList<ClassReference> Interfaces { get { return _interfaces; } set { _interfaces = new List<ClassReference>(value); } }

        public string SourceFile { get; internal set; }
        public IList<FieldDefinition> Fields { get { return _fields; } set { _fields = new List<FieldDefinition>(value); } }
        public IList<MethodDefinition> Methods { get { return _methods; } set { _methods = new List<MethodDefinition>(); } }

        /// <summary>
        /// Gets the underlying InnerClasses list. The caller must not add or remove
        /// items from the list, but may change the order if items.
        /// </summary>
        /// <returns></returns>
        internal IList<ClassDefinition> GetInnerClassesList() { return innerClasses; }

        /// <summary>
        /// Field holding generic type arguments
        /// </summary>
        public FieldDefinition GenericInstanceField { get; set; }

        public ClassDefinition NullableMarkerClass { get; set; }

        #region IMemberDefinition Members

        public AccessFlags AccessFlags { get; set; }
        public IList<Annotation> Annotations { get { return _annotations; } set { _annotations = new List<Annotation>(value); } }

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

        /// <summary>
        /// Set the source file. 
        /// Returns false if the source file has
        /// already been set to another value ealier.
        /// </summary>
        public bool SetSourceFile(string sourceFile)
        {
            if (SourceFile != null)
                return SourceFile == sourceFile;

            SourceFile = sourceFile;
            return true;
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

        public override string Fullname
        { 
            get { return base.Fullname; } 
            set { var prev = Fullname; base.Fullname = value; NotifyDexChangedName(prev);} 
        }

        public override string Namespace
        {
            get { return base.Namespace; }
            set { var prev = Fullname; base.Namespace = value; NotifyDexChangedName(prev); } 
        }

        public override string Name
        {
            get { return base.Name; }
            set { var prev = Fullname; base.Name = value; NotifyDexChangedName(prev); }
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(ClassDefinition other)
        {
            // Should be enough (ownership)
            return base.Equals(other);
        }

        #region Dex Association

        internal void RegisterDex(Dex dex)
        {
            belongsToDex.Add(new WeakReference(dex));
        }

        public void AddInnerClass(ClassDefinition inner)
        {
            innerClasses.Add(inner);

            foreach(var dex in RegisteredDexes())
                dex.RegisterInnerClass(this, inner);
        }

        private void NotifyDexChangedName(string previousFullName)
        {
            foreach (var dex in RegisteredDexes())
                dex.OnNameChanged(this, previousFullName);
        }

        private IEnumerable<Dex> RegisteredDexes()
        {
            for (int i = 0; i < belongsToDex.Count; ++i)
            {
                var dex = belongsToDex[i].Target as Dex;
                if (dex == null)
                {
                    belongsToDex.RemoveAt(i);
                    --i;
                    continue;
                }
                yield return dex;
            }
        }

        #endregion

        #region " Static utilities "

        internal static List<ClassDefinition> Flattenize(IEnumerable<ClassDefinition> container)
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
            var dex = new DexLookup(container, false);

            foreach (var cdef in container)
            {
                int idx = cdef.Fullname.LastIndexOf(DexConsts.InnerClassMarker);

                if (idx == -1)
                {
                    result.Add(cdef);
                }
                else
                {
                    string ownerFullName = cdef.Fullname.Substring(0, idx);
                    var owner = dex.GetClass(ownerFullName);
                    if (owner != null)
                    {
                        owner.AddInnerClass(cdef);
                        cdef.Owner = owner;
                    }
                    else
                    {
                        DLog.Error(DContext.CompilerCodeGenerator, "owner not found for inner class {0}", cdef.Fullname);
                        result.Add(cdef);
                    }
                }
            }
            return result;
        }

        #endregion
    }
}