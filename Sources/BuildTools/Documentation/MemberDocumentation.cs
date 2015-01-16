using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal abstract class MemberDocumentation : CecilFormat
    {
        private readonly string name;
        private readonly TypeDocumentation declaringType;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected MemberDocumentation(string name, TypeDocumentation declaringType)
        {
            this.name = name;
            this.declaringType = declaringType;
        }

        /// <summary>
        /// Name of this member
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Documentation
        /// </summary>
        public string Summary { get; protected set; }

        /// <summary>
        /// Original java name (if any)
        /// </summary>
        public string JavaName { get; private set; }

        /// <summary>
        /// Load the summary from the given file.
        /// </summary>
        public virtual void LoadSummary(SummaryFile summaryFile)
        {
            Summary = summaryFile.GetSummary(XmlMemberName);
            JavaName = summaryFile.GetSummary(XmlMemberName, "java-name", null);
            if (JavaName != null)
                JavaName = JavaName.Trim().Replace('/', '.');
        }

        /// <summary>
        /// Type containing this member
        /// </summary>
        public TypeDocumentation DeclaringType
        {
            get { return declaringType; }
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal abstract string XmlMemberName { get; }

        /// <summary>
        /// Gets the value of the xid attribute for this member in the generated API documentation file.
        /// </summary>
        internal abstract string Xid { get; }

        /// <summary>
        /// Generate the documentation element.
        /// </summary>
        internal abstract XElement Generate(List<FrameworkDocumentationSet> frameworks);
         
        /// <summary>
        /// Add type reference attributes.
        /// </summary>
        internal static void AddTypeRef(XElement element, TypeReference type)
        {
            var xid = GetXid(type);
            element.Add(new XAttribute("type", GetTypeName(type)),
                        new XAttribute("type-name", GetShortTypeName(type)),
                        new XAttribute("type-xid", xid));
        }

        /// <summary>
        /// Add namespace reference attributes.
        /// </summary>
        internal static void AddNamespaceRef(XElement element, NamespaceDocumentation ns)
        {
            var xid = ns.Xid;
            element.Add(new XAttribute("namespace-name", ns.Name),
                        new XAttribute("namespace-xid", xid));
        }

        /// <summary>
        /// Add type reference attributes.
        /// </summary>
        internal static XElement CreateTypeRef(string elementName, TypeReference type)
        {
            var element = new XElement(elementName);
            AddTypeRef(element, type);
            return element;
        }

        /// <summary>
        /// Add namespace reference attributes.
        /// </summary>
        internal static XElement CreateNamespaceRef(string elementName, NamespaceDocumentation ns)
        {
            var element = new XElement(elementName);
            AddNamespaceRef(element, ns);
            return element;
        }

        /// <summary>
        /// Add type reference attributes.
        /// </summary>
        internal static XElement CreateTypeRef(string elementName, TypeDefinition type)
        {
            var element = new XElement(elementName);
            AddTypeRef(element, type);
            element.Add(new XAttribute("icon", Icon.Create(type)));
            return element;
        }

        /// <summary>
        /// Add the original java name to the given element.
        /// </summary>
        internal void AddJavaName(XElement element)
        {
            if (!string.IsNullOrEmpty(JavaName))
                element.Add(new XAttribute("java-name", JavaName));            
        }

        /// <summary>
        /// Add summary info to the given documentation element.
        /// </summary>
        internal void AddSummary(XElement element)
        {
            var summary = Summary ?? "No documentation available";
            var xml = "<summary>" + summary + "</summary>";
            element.Add(XElement.Parse(xml));
        }

        /// <summary>
        /// Add brief summary info to the given documentation element.
        /// </summary>
        internal void AddBriefSummary(XElement element)
        {
            var summary = Summary ?? "";
            var xml = "<brief-summary>" + summary + "</brief-summary>";
            var toAdd = MakeBrief(XElement.Parse(xml));
            if (!toAdd.IsEmpty)
                element.Add(toAdd);
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal static string GetXid(TypeReference type)
        {
            if (type.IsNested)
            {
                return GetXid(type.DeclaringType) + "." + type.Name;
            }
            return "T." + type.FullName;
        }

        /// <summary>
        /// Gets the value of the name attribute for this member in an XML documentation file.
        /// </summary>
        internal static string CreateXmlMemberName(char prefix, TypeDocumentation declaringType, string postfix)
        {
            var declaringTypeName = declaringType.XmlMemberName.Substring(2);
            return string.Empty + prefix + ':' + declaringTypeName + '.' + postfix;
        }

        /// <summary>
        /// Gets the value of the xid attribute for this member the generated API documentation.
        /// </summary>
        internal static string CreateXid(char prefix, TypeDocumentation declaringType, string postfix)
        {
            var declaringTypeName = declaringType.Xid.Substring(2);
            return string.Empty + prefix + '.' + declaringTypeName + '.' + postfix;
        }

        /// <summary>
        /// Gets the name of the given type as used in XML documentation.
        /// </summary>
        internal static string GetXmlTypeName(TypeReference type)
        {
            if (type.IsArray)
            {
                return GetXmlTypeName(((ArrayType)type).ElementType) + "[]";
            }
            if (type.IsGenericParameter) return type.Name;
            if (type.IsGenericInstance)
            {
                var git = (GenericInstanceType)type;
                return GetXmlTypeName(git.ElementType) + '<' + string.Join(",", git.GenericArguments.Select(GetXmlTypeName)) + '>';
            }
            if (type.IsByReference)
            {
                return "ref " + GetXmlTypeName(((ByReferenceType)type).ElementType);
            }
            var fullName = type.FullName;
            return fullName;
        }

        /// <summary>
        /// Shorten the given summary element.
        /// </summary>
        private static XElement MakeBrief(XElement element)
        {
            var firstNode = element.Nodes().FirstOrDefault();
            if (firstNode is XElement)
            {
                // Only use first element
                var toRemove = element.Nodes().Skip(1).ToList();
                foreach (var node in toRemove)
                    node.Remove();                
                // Strip first element
                RemoveAllButFirstSentence((XElement) firstNode);
            }
            else if (firstNode is XText)
            {
                RemoveAllButFirstSentence(element);
            }
            return element;
        }

        /// <summary>
        /// Take the first sentence of the text in the given element.
        /// </summary>
        private static void RemoveAllButFirstSentence(XElement element)
        {
            var removeRemaining = false;
            var toRemove = new List<XNode>();
            foreach (var node in element.Nodes())
            {
                if (removeRemaining)
                {
                    toRemove.Add(node);
                }
                else
                {
                    var text = node as XText;
                    if (text != null)
                    {
                        var index = text.Value.IndexOf('.');
                        if (index < 0)
                            continue;
                        text.Value = text.Value.Substring(0, index + 1);
                        removeRemaining = true;
                    }
                }
            }
            foreach (var node in toRemove)
            {
                node.Remove();
            }
        }
    }
}
