using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    public sealed class ResourceIdMap
    {
        private const string AndroidSchema = "http://schemas.android.com/apk/res/android";

        private readonly Dictionary<XName, Entry> idMap = new Dictionary<XName, Entry>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResourceIdMap(Table frameworkResources, XmlTree manifest)
        {
            var package = frameworkResources.Packages.First();
            var resourceNames = package.KeyStrings;

            for (var i = 0; i < resourceNames.Count; i++)
            {
                var name = XName.Get(resourceNames[i], AndroidSchema);
                idMap.Add(name, new Entry(MakeId(0, 0, i)));
            }

            // Find attribute types
            manifest.VisitAttributes(a => {
                Entry entry;
                if (idMap.TryGetValue(a.XName, out entry))
                {
                    entry.ValueType = a.TypedValue.Type;
                }
            });

        }

        /// <summary>
        /// Try to load an id for the given attribute name
        /// </summary>
        public bool TryGetId(XName name, out int id, out Value.Types valueType)
        {
            Entry entry;
            id = -1;
            valueType = Value.Types.TYPE_STRING;
            if (idMap.TryGetValue(name, out entry))
            {
                id = entry.Id;
                valueType = entry.ValueType;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create a resource ID.
        /// See frameworks-base\include\utils\ResourceTypes.h # Res_MAKEID()
        /// </summary>
        public static int MakeId(int package, int type, int entry)
        {
            return ((package + 1) << 24) | (((type + 1) & 0xFF) << 16) | (entry & 0xFFFF);
        }

        private class Entry
        {
            private readonly int id;

            /// <summary>
            /// Default ctor
            /// </summary>
            public Entry(int id)
            {
                this.id = id;
                ValueType = Value.Types.TYPE_STRING;                
            }

            /// <summary>
            /// Gets the resource ID
            /// </summary>
            public int Id { get { return id; } }

            /// <summary>
            /// Value type
            /// </summary>
            public Value.Types ValueType { get; set; }
        }
    }
}
