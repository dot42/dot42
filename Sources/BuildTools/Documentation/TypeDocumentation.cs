using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class TypeDocumentation : MemberDocumentation
    {
        private readonly TypeDefinition type;
        private readonly string @namespace;
        private readonly string fullName;
        public readonly List<EventDocumentation> Events = new List<EventDocumentation>();
        public readonly List<FieldDocumentation> Fields = new List<FieldDocumentation>();
        public readonly List<MethodDocumentation> Methods = new List<MethodDocumentation>();
        public readonly List<PropertyDocumentation> Properties = new List<PropertyDocumentation>();
        public readonly List<GenericParameterDocumentation> GenericParameters = new List<GenericParameterDocumentation>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public TypeDocumentation(TypeDefinition type, TypeDocumentation declaringType)
            : base(type.Name, declaringType)
        {
            this.type = type;
            fullName = type.FullName;
            @namespace = type.Namespace;
            GenericParameters.AddRange(type.GenericParameters.Select(x => new GenericParameterDocumentation(x, this)));
        }

        /// <summary>
        /// Gets the original type.
        /// </summary>
        public TypeDefinition Type { get { return type; } }

        /// <summary>
        /// Full type name
        /// </summary>
        public string FullName
        {
            get { return fullName; }
        }

        /// <summary>
        /// Namespace containing this type.
        /// </summary>
        public string Namespace
        {
            get { return (DeclaringType != null) ? DeclaringType.Namespace : @namespace; }
        }

        /// <summary>
        /// Load the summary from the given file.
        /// </summary>
        public override void LoadSummary(SummaryFile summaryFile)
        {
            base.LoadSummary(summaryFile);
            Events.ForEach(x => x.LoadSummary(summaryFile));
            Fields.ForEach(x => x.LoadSummary(summaryFile));
            Methods.ForEach(x => x.LoadSummary(summaryFile));
            Properties.ForEach(x => x.LoadSummary(summaryFile));
            GenericParameters.ForEach(x => x.LoadSummary(summaryFile));
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal override string XmlMemberName
        {
            get
            {
                if (DeclaringType == null)
                    return "T:" + fullName;
                return CreateXmlMemberName('T', DeclaringType, type.Name);
            }
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal override string Xid
        {
            get
            {
                if (DeclaringType == null)
                    return "T." + fullName;
                return CreateXid('T', DeclaringType, type.Name);
            }
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement(type.IsInterface ? "interface" : "type",
                                      new XAttribute("xid", Xid),
                                      new XAttribute("name", GetShortTypeName(type)),
                                      new XAttribute("since", frameworks.First(x => x.ContainsMatching(this)).Version.ToString()),
                                      new XAttribute("icon", Icon.Create(type)),
                                      new XAttribute("fullname", FullName),
                                      new XAttribute("namespace", Namespace));
            AddFlags(result, type);
            if (DeclaringType != null)
            {
                AddTypeRef(result, DeclaringType.Type);
            }
            result.Add(GenericParameters.Select(x => x.Generate(frameworks)));
            AddJavaName(result);
            AddSummary(result);
            AddBriefSummary(result);
            result.Add(type.Interfaces.Select(x => CreateTypeRef("implements", x.Interface)));
            result.Add(Events.Select(x => x.Generate(frameworks)));
            result.Add(Fields.Select(x => x.Generate(frameworks)));
            result.Add(Methods.Select(x => x.Generate(frameworks)));
            result.Add(Properties.Select(x => x.Generate(frameworks)));
            return result;
        }

        /// <summary>
        /// Add flags data
        /// </summary>
        internal static void AddFlags(XElement element, TypeDefinition type)
        {
            element.Add(new XAttribute("abstract", type.IsAbstract),
                        new XAttribute("nested", type.IsNested),
                        new XAttribute("public", type.IsPublic),
                        new XAttribute("sealed", type.IsSealed),
                        new XAttribute("valuetype", type.IsValueType));
        }

        /// <summary>
        /// Does this set contain a matching event?
        /// </summary>
        public bool ContainsMatching(EventDocumentation @event)
        {
            var xid = @event.Xid;
            return Events.Any(x => x.Xid == xid);
        }

        /// <summary>
        /// Does this set contain a matching field?
        /// </summary>
        public bool ContainsMatching(FieldDocumentation field)
        {
            var xid = field.Xid;
            return Fields.Any(x => x.Xid == xid);
        }

        /// <summary>
        /// Does this set contain a matching method?
        /// </summary>
        public bool ContainsMatching(MethodDocumentation method)
        {
            var xid = method.Xid;
            return Methods.Any(x => x.Xid == xid);
        }

        /// <summary>
        /// Does this set contain a matching property?
        /// </summary>
        public bool ContainsMatching(PropertyDocumentation property)
        {
            var xid = property.Xid;
            return Properties.Any(x => x.Xid == xid);
        }
    }
}
