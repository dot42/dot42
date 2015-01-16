using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.BuildTools.Documentation
{
    internal class SummaryFile
    {
        private readonly Dictionary<string, XElement> members;

        /// <summary>
        /// Load the given file
        /// </summary>
        public SummaryFile(string path)
        {
            var doc = File.Exists(path) ? XDocument.Load(path) : null;
            members = new Dictionary<string, XElement>();
            if (doc != null)
            {
                var membersE = doc.Root.Element("members");
                if (membersE != null)
                {
                    foreach (var member in membersE.Elements("member"))
                    {
                        var nameAttr = member.Attribute("name");
                        if (nameAttr == null)
                            continue;
                        if (!members.ContainsKey(nameAttr.Value)) 
                            members.Add(nameAttr.Value, member);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a summary by name.
        /// </summary>
        public string GetSummary(string memberName)
        {
            return GetSummary(memberName, "summary", null);
        }

        /// <summary>
        /// Gets a summary element by name.
        /// </summary>
        public string GetSummary(string memberName, string elementName, Func<XElement, bool> filter)
        {
            var summary = GetSummaryElement(memberName, elementName, filter);
            if (summary == null)
                return null;
            var reader = summary.CreateReader();
            reader.MoveToContent();
            return MakeReplacements(reader.ReadInnerXml());
        }

        /// <summary>
        /// Gets a summary element by name.
        /// </summary>
        private XElement GetSummaryElement(string memberName, string elementName, Func<XElement, bool> filter)
        {
            XElement member;
            if (!members.TryGetValue(memberName, out member))
                return null;
            var selection = member.Elements(elementName);
            if (filter != null)
                selection = selection.Where(filter);
            return selection.FirstOrDefault();
        }

        /// <summary>
        /// Replace all unwantend constructs.
        /// </summary>
        private static string MakeReplacements(string value)
        {
            value = ReplaceUnicodeTags(value);
            return value;
        }

        private static string ReplaceUnicodeTags(string value)
        {
            const string prefix = "&amp;#x";
            while (true)
            {
                var index = value.IndexOf(prefix);
                if (index < 0)
                    return value;
                var endIndex = index + prefix.Length;
                while (value[endIndex] != ';')
                    endIndex++;
                //Debugger.Launch();
                var hex = value.Substring(index + prefix.Length, (endIndex - index) - prefix.Length);
                var num = int.Parse(hex, NumberStyles.HexNumber);
                value = value.Substring(0, index) + ((char)num) + value.Substring(endIndex + 1);
            }
        }
    }
}
