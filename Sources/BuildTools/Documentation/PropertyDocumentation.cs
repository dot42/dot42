using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class PropertyDocumentation : MemberDocumentation
    {
        private readonly PropertyDefinition prop;
        public readonly List<ParameterDocumentation> Parameters = new List<ParameterDocumentation>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public PropertyDocumentation(PropertyDefinition prop, TypeDocumentation declaringType)
            : base(prop.Name, declaringType)
        {
            this.prop = prop;
            Parameters.AddRange(prop.Parameters.Select(x => new ParameterDocumentation(x, this)));
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal override string XmlMemberName
        {
            get { return CreateXmlMemberName('P', DeclaringType, Name); }
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal override string Xid
        {
            get { return CreateXid('P', DeclaringType, Name); }
        }

        /// <summary>
        /// Load the summary from the given file.
        /// </summary>
        public override void LoadSummary(SummaryFile summaryFile)
        {
            base.LoadSummary(summaryFile);
            Parameters.ForEach(x => x.LoadSummary(summaryFile));
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement("property",
                                      new XAttribute("xid", Xid),
                                      new XAttribute("name", Name),
                                      new XAttribute("since", frameworks.First(x => x.ContainsMatching(this)).Version.ToString()),
                                      new XAttribute("icon", Icon.Create(prop)),
                                      new XAttribute("descriptor", Descriptor.Create(prop, true)),
                                      new XAttribute("short-descriptor", Descriptor.Create(prop, false)),
                                      new XAttribute("syntax", Syntax.Create(prop)));
            AddTypeRef(result, prop.PropertyType);
            result.Add(Parameters.Select(x => x.Generate(frameworks)));
            AddJavaName(result);
            AddSummary(result);
            AddBriefSummary(result);
            return result;
        }
    }
}
