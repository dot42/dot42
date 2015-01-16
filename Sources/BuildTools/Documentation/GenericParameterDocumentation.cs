using System.Collections.Generic;
using System.Xml.Linq;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class GenericParameterDocumentation : MemberDocumentation
    {
        private readonly MemberDocumentation owner;
        private readonly GenericParameter parameter;

        /// <summary>
        /// Default ctor
        /// </summary>
        public GenericParameterDocumentation(GenericParameter parameter, MemberDocumentation owner)
            : base(parameter.Name, owner.DeclaringType)
        {
            this.parameter = parameter;
            this.owner = owner;
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
            get { return string.Empty; }
        }

        /// <summary>
        /// Load the summary from the given file.
        /// </summary>
        public override void LoadSummary(SummaryFile summaryFile)
        {
            Summary = summaryFile.GetSummary(owner.XmlMemberName, "typeparam", x => x.GetAttribute("name") == Name);
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement("typeparam",
                                      new XAttribute("name", Name));
            AddJavaName(result);
            AddSummary(result);
            AddBriefSummary(result);
            return result;
        }
    }
}
