using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class FieldDocumentation : MemberDocumentation
    {
        private readonly FieldDefinition field;

        /// <summary>
        /// Default ctor
        /// </summary>
        public FieldDocumentation(FieldDefinition field, TypeDocumentation declaringType)
            : base(field.Name, declaringType)
        {
            this.field = field;
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal override string XmlMemberName
        {
            get { return CreateXmlMemberName('F', DeclaringType, Name); }
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal override string Xid
        {
            get { return CreateXid('F', DeclaringType, Name); }
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement("field",
                                      new XAttribute("xid", Xid),
                                      new XAttribute("name", Name),
                                      new XAttribute("since", frameworks.First(x => x.ContainsMatching(this)).Version.ToString()),
                                      new XAttribute("icon", Icon.Create(field)),
                                      new XAttribute("descriptor", Descriptor.Create(field)),
                                      new XAttribute("syntax", Syntax.Create(field)));
            AddTypeRef(result, field.FieldType);
            AddJavaName(result);
            AddSummary(result);
            AddBriefSummary(result);
            return result;
        }
    }
}
