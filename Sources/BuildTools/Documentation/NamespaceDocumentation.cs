using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class NamespaceDocumentation : MemberDocumentation
    {
        private readonly string ns;
        private readonly Dictionary<string, TypeDocumentation> types = new Dictionary<string, TypeDocumentation>();
        public readonly Dictionary<string, NamespaceDocumentation> Children = new Dictionary<string, NamespaceDocumentation>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public NamespaceDocumentation(string @namespace)
            : base(@namespace, null)
        {
            ns = @namespace;
        }

        /// <summary>
        /// Add the given type to this set.
        /// </summary>
        internal void Add(TypeDocumentation type)
        {
            types[type.Xid] = type;
        }

        /// <summary>
        /// Gets all types
        /// </summary>
        public IEnumerable<TypeDocumentation> Types { get { return types.Values; } }

        /// <summary>
        /// Load the summary from the given file.
        /// </summary>
        public override void LoadSummary(SummaryFile summaryFile)
        {
            base.LoadSummary(summaryFile);
            foreach (var type in Types) 
                type.LoadSummary(summaryFile);
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal override string XmlMemberName
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal override string Xid
        {
            get { return "NS." + Name; }
        }

        /// <summary>
        /// Gets the name of the parent namespace or null if there is no parent.
        /// </summary>
        internal string ParentName
        {
            get
            {
                var index = Name.LastIndexOf('.');
                return (index < 0) ? null : Name.Substring(0, index);
            }            
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement("namespace",
                                      new XAttribute("xid", Xid),
                                      new XAttribute("name", Name),
                                      new XAttribute("since", frameworks.First(x => x.ContainsMatching(this)).Version.ToString()));
            result.Add(Types.OrderBy(x => GetTypeName(x.Type)).Select(x => CreateTypeRef("type", x.Type)));
            result.Add(Children.Values.OrderBy(x => x.Name).Select(x => CreateNamespaceRef("namespace", x)));
            return result;
        }

        /// <summary>
        /// Try to find a type documentation in this namespace that matches the given type.
        /// </summary>
        internal bool TryGetMatchingType(TypeDocumentation type, out TypeDocumentation result)
        {
            var xid = type.Xid;
            return types.TryGetValue(xid, out result);
        }

        /// <summary>
        /// Does this set contain a matching type?
        /// </summary>
        public bool ContainsMatching(TypeDocumentation type)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(type, out mine);
        }

        /// <summary>
        /// Does this set contain a matching event?
        /// </summary>
        public bool ContainsMatching(EventDocumentation @event)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(@event.DeclaringType, out mine) && mine.ContainsMatching(@event);
        }

        /// <summary>
        /// Does this set contain a matching field?
        /// </summary>
        public bool ContainsMatching(FieldDocumentation field)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(field.DeclaringType, out mine) && mine.ContainsMatching(field);
        }

        /// <summary>
        /// Does this set contain a matching method?
        /// </summary>
        public bool ContainsMatching(MethodDocumentation method)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(method.DeclaringType, out mine) && mine.ContainsMatching(method);
        }

        /// <summary>
        /// Does this set contain a matching property?
        /// </summary>
        public bool ContainsMatching(PropertyDocumentation property)
        {
            TypeDocumentation mine;
            return TryGetMatchingType(property.DeclaringType, out mine) && mine.ContainsMatching(property);
        }
    }
}
