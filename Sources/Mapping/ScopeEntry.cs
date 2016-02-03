using System;
using System.Globalization;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Type entry in map file
    /// </summary>
    public sealed class ScopeEntry
    {
        private readonly string name;
        private readonly string filename;
        private readonly DateTime timestamp;
        private readonly string hashcode;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ScopeEntry(string name, string filename, DateTime timestamp, string hashcode)
        {
            this.name = name;
            this.filename = filename;
            this.timestamp = timestamp;
            this.hashcode = hashcode;
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal ScopeEntry(XElement e)
        {
            name = e.GetAttribute("name");
            filename = e.GetAttribute("filename");
            var ts = e.GetAttribute("timestamp");
            timestamp = DateTime.ParseExact(ts, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            hashcode = e.GetAttribute("hashcode");
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal XElement ToXml(string elementName)
        {
            var e = new XElement(elementName,
                                 new XAttribute("name", name),
                                 new XAttribute("filename", filename),
                                 new XAttribute("timestamp", timestamp.ToString("O", CultureInfo.InvariantCulture)),
                                 new XAttribute("hashcode", hashcode));
            return e;
        }

        /// <summary>
        /// Name of the Scope 
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Filename (assembly file name) of this cope
        /// </summary>
        public string Filename { get { return filename; } }

        /// <summary>
        /// Last Write Time, UTC
        /// </summary>
        public DateTime Timestamp { get { return timestamp; } }

        /// <summary>
        /// MD5 hashcode
        /// </summary>
        public string Hashcode { get { return hashcode; } }
    }
}
