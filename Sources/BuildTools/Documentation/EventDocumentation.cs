using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class EventDocumentation : MemberDocumentation
    {
        private readonly EventDefinition evt;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EventDocumentation(EventDefinition evt, TypeDocumentation declaringType)
            : base(evt.Name, declaringType)
        {
            this.evt = evt;
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal override string XmlMemberName
        {
            get { return CreateXmlMemberName('E', DeclaringType, Name); }
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal override string Xid
        {
            get { return CreateXid('E', DeclaringType, Name); }
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement("event",
                                      new XAttribute("xid", Xid),
                                      new XAttribute("name", Name),
                                      new XAttribute("since", frameworks.First(x => x.ContainsMatching(this)).Version.ToString()),
                                      new XAttribute("icon", Icon.Create(evt)),
                                      new XAttribute("descriptor", Descriptor.Create(evt)),
                                      new XAttribute("syntax", Syntax.Create(evt))
                                      );
            AddTypeRef(result, evt.EventType);
            AddJavaName(result);
            AddSummary(result);
            AddBriefSummary(result);
            return result;
        }
    }
}
