using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class MethodDocumentation : MemberDocumentation
    {
        private readonly MethodDefinition method;
        public readonly List<ParameterDocumentation> Parameters = new List<ParameterDocumentation>();
        public readonly List<GenericParameterDocumentation> GenericParameters = new List<GenericParameterDocumentation>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public MethodDocumentation(MethodDefinition method, TypeDocumentation declaringType)
            : base(method.Name, declaringType)
        {
            this.method = method;
            Parameters.AddRange(method.Parameters.Select(x => new ParameterDocumentation(x, this)));
            GenericParameters.AddRange(method.GenericParameters.Select(x => new GenericParameterDocumentation(x, this)));
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal override string XmlMemberName
        {
            get
            {
                var name = Name;
                if (name[0] == '.')
                    name = '#' + name.Substring(1);
                var postfix = name;
                if (method.HasGenericParameters)
                    postfix = postfix + "``" + method.GenericParameters.Count;

                if (method.Parameters.Any())
                {
                    var parameters = string.Join(",", method.Parameters.Select(x => GetXmlTypeName(x.ParameterType)));
                    postfix = postfix + "(" + parameters + ")";
                }
                return CreateXmlMemberName('M', DeclaringType, postfix);
            }
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal override string Xid
        {
            get
            {
                var name = Name;
                if (name[0] == '.')
                    name = name.Substring(1);
                var parameters = string.Join(",", method.Parameters.Select(x => GetTypeName(x.ParameterType)));
                var postfix = name;
                if (method.HasGenericParameters)
                    postfix = postfix + "``" + method.GenericParameters.Count;
                postfix = postfix + "(" + parameters + ")";
                return CreateXid('M', DeclaringType, postfix);
            }
        }

        /// <summary>
        /// Load the summary from the given file.
        /// </summary>
        public override void LoadSummary(SummaryFile summaryFile)
        {
            base.LoadSummary(summaryFile);
            Parameters.ForEach(x => x.LoadSummary(summaryFile));
            GenericParameters.ForEach(x => x.LoadSummary(summaryFile));
        }

        /// <summary>
        /// Gets the method name with its generic parameters.
        /// </summary>
        private string NameIncludingGenericParameters
        {
            get
            {
                var name = Name;
                if (method.HasGenericParameters)
                {
                    name = name + "<" + string.Join(", ", method.GenericParameters.Select(x => x.Name)) + ">";
                }
                return name;
            }
        }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal override XElement Generate(List<FrameworkDocumentationSet> frameworks)
        {
            var result = new XElement(method.IsConstructor ? "constructor" : "method",
                                      new XAttribute("xid", Xid),
                                      new XAttribute("name", NameIncludingGenericParameters),
                                      new XAttribute("since", frameworks.First(x => x.ContainsMatching(this)).Version.ToString()),
                                      new XAttribute("icon", Icon.Create(method)),
                                      new XAttribute("descriptor", Descriptor.Create(method, true)),
                                      new XAttribute("short-descriptor", Descriptor.Create(method, false)),
                                      new XAttribute("syntax", Syntax.Create(method))
                                      );
            AddTypeRef(result, method.ReturnType);
            result.Add(Parameters.Select(x => x.Generate(frameworks)));
            result.Add(GenericParameters.Select(x => x.Generate(frameworks)));
            AddJavaName(result);
            AddSummary(result);
            AddBriefSummary(result);
            return result;
        }
    }
}
