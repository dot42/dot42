using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.ImportJarLib.Doxygen
{
    [DebuggerDisplay("{Name}")]
    public class DocClass : DocMember<DocClass>, IDocTypeRef, IDocResolvedTypeRef
    {
        private readonly DocMemberList<DocClass, DocClass> innerClasses;
        private readonly DocMemberList<DocMethod, DocClass> methods;
        private readonly DocMemberList<DocField, DocClass> fields;
        private readonly List<string> innerClassIds = new List<string>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public DocClass(string name)
            : base(name.Replace("::", "."))
        {
            innerClasses = new DocMemberList<DocClass, DocClass>(this);
            methods = new DocMemberList<DocMethod, DocClass>(this);
            fields = new DocMemberList<DocField, DocClass>(this);
        }

        /// <summary>
        /// Compound ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the namespace part of the class name
        /// </summary>
        public string Namespace
        {
            get
            {
                var fullName = base.Name;
                var index = fullName.LastIndexOf('.');
                return (index < 0) ? string.Empty : fullName.Substring(0, index);
            }
        }

        /// <summary>
        /// Is this an interface?
        /// </summary>
        public bool IsInterface { get; set; }

        /// <summary>
        /// Classes nested in this class
        /// </summary>
        public DocMemberList<DocClass, DocClass> InnerClasses { get { return innerClasses; } }

        /// <summary>
        /// Add an id of an inner class.
        /// </summary>
        internal void AddInnerClass(string id)
        {
            innerClassIds.Add(id);
        }

        /// <summary>
        /// Methods defined in this class
        /// </summary>
        public DocMemberList<DocMethod, DocClass> Methods { get { return methods; } }

        /// <summary>
        /// Fields defined in this class
        /// </summary>
        public DocMemberList<DocField, DocClass> Fields { get { return fields; } }

        /// <summary>
        /// Resolve this reference into an XmlClass.
        /// </summary>
        public IDocResolvedTypeRef Resolve(DocModel model)
        {
            return this;
        }

        /// <summary>
        /// Link all references.
        /// </summary>
        internal override void Link(DocModel model)
        {
            foreach (var id in innerClassIds)
            {
                DocClass @class;
                if (model.TryGetClassById(id, out @class)) 
                    innerClasses.Add(@class);
            }
        }

        /// <summary>
        /// Is this typeref equal to other?
        /// </summary>
        bool IDocResolvedTypeRef.Equals(IDocResolvedTypeRef other)
        {
            var otherClass = other as DocClass;
            return (otherClass != null) && (otherClass.Name == Name) && (otherClass.Namespace == Namespace);
        }
    }
}
