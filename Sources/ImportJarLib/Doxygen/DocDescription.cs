using System.IO;
using System.Xml;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.ImportJarLib.Doxygen
{
    public class DocDescription
    {
        private readonly DocModel model;
        private readonly XElement element;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DocDescription(XElement element, DocModel model)
        {
            this.element = element;
            this.model = model;
        }

        /// <summary>
        /// Write as C# code.
        /// </summary>
        public void WriteAsCode(TextWriter writer, string indent, IDocTypeNameResolver resolver, string originalJavaName)
        {
            if (element == null)
                return;

            var builder = new CommentBuilder();
            foreach (var child in element.Nodes())
            {
                WriteAsCode(child, builder, builder.Summary, false, resolver);
            }
            if (!string.IsNullOrEmpty(originalJavaName))
            {
                builder.JavaName.Write(originalJavaName);
            }
            builder.WriteTo(writer, indent);
        }

        /// <summary>
        /// Write as C# code.
        /// </summary>
        private void WriteAsCode(XNode node, CommentBuilder builder,  CommentSection section, bool inCode, IDocTypeNameResolver resolver)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Text:
                    WriteAsCode((XText)node, section, inCode);
                    break;
                case XmlNodeType.Element:
                    WriteAsCode((XElement) node, builder, section, inCode, resolver);
                    break;
            }
        }

        /// <summary>
        /// Write as C# code.
        /// </summary>
        private static void WriteAsCode(XText text, CommentSection section, bool inCode)
        {
            var lineNo = 0;
            var content = text.Value;
            if (string.IsNullOrEmpty(content))
                return;
            foreach (var part in content.Split('\n'))
            {
                if ((lineNo > 0) && inCode)
                    section.WriteLine();
                section.Write(part.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"));
                lineNo++;
            }
        }

        /// <summary>
        /// Write as C# code.
        /// </summary>
        private void WriteAsCode(XElement element, CommentBuilder builder, CommentSection section, bool inCode, IDocTypeNameResolver resolver)
        {
            string tag = null;
            string postfix = null;
            var appendNewline = false;

            if (IsEmpty(element))
                return;

            switch (element.Name.LocalName)
            {
                case "para":
                    tag = "para";
                    break;
                case "ref":
                    var id = element.GetAttribute("refid");
                    DocClass @class;
                    string name = null;
                    if (model.TryGetClassById(id, out @class))
                    {
                        name = resolver.ResolveTypeName(@class.Name);
                    }
                    if (name != null)
                    {
                        section.Write("<see cref=\"{0}\">", name);
                        postfix = "</see>";
                    } 
                    break;
                case "heading":
                    var level = element.GetAttribute("level");
                    tag = "h" + level;
                    break;
                case "preformatted":
                    tag = "pre";
                    inCode = true;
                    break;
                case "itemizedlist":
                    tag = "ul";
                    break;
                case "orderedlist":
                    tag = "ol";
                    break;
                case "listitem":
                    tag = "li";
                    break;
                case "linebreak":
                    tag = "br";
                    break;
                case "bold":
                case "emphasis":
                    tag = "b";
                    break;
                case "computeroutput":
                    tag = "code";
                    break;
                case "programlisting":
                    if (!inCode) tag = "c";
                    break;
                case "codeline":
                    if (inCode) appendNewline = true;
                    break;
                case "table":
                    tag = "table";
                    break;
                case "superscript":
                case "verbatim":
                    tag = "span";
                    break;
                case "simplesect":
                    var kind = element.GetAttribute("kind");
                    if (kind == "return") section = builder.Returns;
                    break;
                case "nonbreakablespace":
                case "sp":
                    section.Write(" ");
                    return;
                case "mdash":
                case "ndash":
                case "ulink":
                case "parameterlist":
                case "variablelist":
                case "anchor":
                    return;
                case "highlight":
                    break;
                default:
                    tag = element.Name.LocalName;
                    break;
            }

            if (tag != null)
            {
                section.Write("<{0}>", tag);
            }

            foreach (var child in element.Nodes())
            {
                WriteAsCode(child, builder, section, inCode, resolver);
            }

            if (tag != null)
            {
                section.Write("</{0}>", tag);
            }
            if (postfix != null)
            {
                section.Write("{0}", postfix);
            }
            if (appendNewline)
            {
                section.WriteLine();
            }
        }

        /// <summary>
        /// This this an empty element?
        /// </summary>
        private static bool IsEmpty(XElement element)
        {
            var value = element.Value.Trim();
            if (!string.IsNullOrEmpty(value))
                return false;
            return (element.Name.LocalName == "p");
        }
    }
}
