using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Map Android type names to .NET type names.
    /// </summary>
    public sealed class FrameworkTypeMap
    {
        private readonly Dictionary<string, TypeEntry> typeMap = new Dictionary<string, TypeEntry>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, TypeEntry> classMap = new Dictionary<string, TypeEntry>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Empty ctor
        /// </summary>
        public FrameworkTypeMap()
        {            
        }

        /// <summary>
        /// Create from a reader.
        /// </summary>
        public FrameworkTypeMap(Stream stream)
        {
            var doc = CompressedXml.Load(stream);
            foreach (var element in doc.Root.Elements("type"))
            {
                var entry = new TypeEntry(element);
                classMap[entry.ClassName] = entry;
                typeMap[entry.FullName] = entry;
            }
        }

        /// <summary>
        /// Try to get a type from the given java class name.
        /// Will ignore the case.
        /// </summary>
        public bool TryGetFromClassName(string javaClassName, out TypeEntry entry)
        {
            return classMap.TryGetValue(javaClassName, out entry);
        }

        /// <summary>
        /// Try to get a type from the given clr type name name.
        /// Will ignore the case.
        /// </summary>
        public bool TryGetFromClrName(string clrTypeName, out TypeEntry entry)
        {
            return typeMap.TryGetValue(clrTypeName, out entry);
        }


        /// <summary>
        /// Type information.
        /// </summary>
        public sealed class TypeEntry
        {
            public readonly string FullName;
            public readonly string ClassName;

            /// <summary>
            /// XML ctor
            /// </summary>
            internal TypeEntry(XElement element)
            {
                FullName = element.GetAttribute("fullname");
                ClassName = element.GetAttribute("classname");
            }
        }

        /// <summary>
        /// Gets all type entries.
        /// </summary>
        public IEnumerable<TypeEntry> Values { get { return classMap.Values; } }
    }
}
