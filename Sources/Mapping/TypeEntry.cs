using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Type entry in map file
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public sealed class TypeEntry
    {
        private readonly string name;
        private readonly string scope;
        private readonly string dexName;
        private readonly List<MethodEntry> methods;
        private readonly List<FieldEntry> fields;
        private readonly List<PropertyEntry> properties;
        private readonly List<EventEntry> events;
        private readonly int mapFileId;
        private readonly string scopeId;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TypeEntry(string name, string scope, string dexName, int mapFileId, string scopeId=null)
        {
            this.name = name;
            this.scope = scope;
            this.dexName = dexName;
            this.mapFileId = mapFileId;
            this.scopeId = scopeId;

            methods = new List<MethodEntry>();
            fields = new List<FieldEntry>();
            properties = new List<PropertyEntry>();
            events = new List<EventEntry>();
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal TypeEntry(XElement e)
        {
            name = e.GetAttribute("name");
            scope = e.GetAttribute("scope");
            dexName = e.GetAttribute("dname");
            mapFileId = int.Parse(e.GetAttribute("id") ?? "0");

            if ((dexName != null) && (dexName.StartsWith("[" + scope + "]")))
            {
                dexName = dexName.Substring(scope.Length + 2);
            }

            scopeId = e.GetAttribute("scopeid");

            methods = e.Elements("method").Select(e1 => new MethodEntry(e1)).ToList();
            fields = ReadMembers(e, "field", x => new FieldEntry(x));
            properties = ReadMembers(e, "property", x => new PropertyEntry(x)); 
            events = ReadMembers(e, "event", x => new EventEntry(x)); 
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal XElement ToXml(string elementName)
        {
            var e = new XElement(elementName,
                                 new XAttribute("name", name),
                                 new XAttribute("scope", scope),
                                 new XAttribute("id", mapFileId.ToString()));
            
            if (dexName != null) e.Add(new XAttribute("dname", dexName));
            if (scopeId != null) e.Add(new XAttribute("scopeid", scopeId));

            e.Add(methods.Select(x => x.ToXml("method")));
            e.Add(fields.Select(x => x.ToXml("field")));
            e.Add(properties.Select(x => x.ToXml("property")));
            e.Add(events.Select(x => x.ToXml("event")));

            return e;
        }

        /// <summary>
        /// Name of the type in .NET
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Scope (assembly) of the type in .NET
        /// </summary>
        public string Scope { get { return scope; } }

        /// <summary>
        /// Id of the type in its Scope (assembly)
        /// </summary>
        public string ScopeId { get { return scopeId; } }

        /// <summary>
        /// Name of the type in Dex
        /// </summary>
        public string DexName { get { return dexName; } }

        /// <summary>
        /// Signature of the type in Dex
        /// </summary>
        public string DexSignature { get { return dexName == null? null : "L" + dexName.Replace('.', '/') + ";"; } }

        /// <summary>
        /// Unique id of this entry in the map file.
        /// </summary>
        public int Id { get { return mapFileId; } }

        /// <summary>
        /// Find the method based on its dex name and signature.
        /// </summary>
        public MethodEntry FindDexMethod(string dexName, string dexSignature)
        {
            foreach (var entry in Methods)
            {
                if (entry.IsDexMatch(dexName, dexSignature))
                {
                    return entry;
                }
            }

            // Does dexName have a '[' ... ']' postfix?
            var openIndex = dexName.IndexOf('[');
            if (openIndex < 0)
                return null;
            var closeIndex = dexName.LastIndexOf(']');
            if (closeIndex < 0)
                return null;

            // Try name without generics
            return FindDexMethod(dexName.Substring(0, openIndex), dexSignature);
        }

        /// <summary>
        /// Gets a method by its map file id.
        /// </summary>
        /// <returns>Null if not found</returns>
        public MethodEntry GetMethodById(int id)
        {
            return methods.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets the methods in the map.
        /// </summary>
        public List<MethodEntry> Methods { get { return methods; } }

        /// <summary>
        /// Gets the fields in the map.
        /// </summary>
        public List<FieldEntry> Fields{get { return fields; }}

        /// <summary>
        /// Gets the properties in the map.
        /// </summary>
        public List<PropertyEntry> Properties { get { return properties; } }

        /// <summary>
        /// Gets the events in the map.
        /// </summary>
        public List<EventEntry> Events { get { return events; } }

        /// <summary>
        /// Read a list of members from XML.
        /// </summary>
        private static List<T> ReadMembers<T>(XElement e, string nodeName, Func<XElement, T> entryBuilder)
            where T : MemberEntry
        {
            return e.Elements(nodeName).Select(entryBuilder).ToList();
        }
    }
}
