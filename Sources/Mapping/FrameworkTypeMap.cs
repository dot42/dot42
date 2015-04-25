using System.Collections;
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
        private readonly Dictionary<string, TypeEntry> typeMap = new Dictionary<string, TypeEntry>();

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
                typeMap[entry.ClassName] = entry;
            }
        }

        /// <summary>
        /// Try to get a type from the given java class name.
        /// </summary>
        public bool TryGet(string javaClassName, out TypeEntry entry)
        {
            return typeMap.TryGetValue(javaClassName, out entry);
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
        public IEnumerable<TypeEntry> Values { get { return typeMap.Values; } }
    }
}
