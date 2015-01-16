using System.Collections.Generic;
using System.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_entry (complex flag set)
        /// </summary>
        public sealed class ComplexEntryInstance : EntryInstance 
        {
            private readonly List<Map> maps = new List<Map>();
            
            /// <summary>
            /// Read ctor
            /// </summary>
            internal ComplexEntryInstance(Type parent, ResReader reader)
                : base(parent, reader)
            {
                var parentResId = TableRef.Read(reader);
                var count = reader.ReadInt32();

                //reader.Skip(count*12);
                for (var i = 0; i < count; i++)
                {
                    maps.Add(new Map(this, reader));
                }
            }

            /// <summary>
            /// Gets the key+value maps.
            /// </summary>
            public IEnumerable<Map> Maps { get { return maps; } }

            /// <summary>
            /// Try to get the type (types because it can be multiple) of this entry instance.
            /// </summary>
            public bool TryGetAttributeType(out AttributeTypes attributeType)
            {
                var map = maps.FirstOrDefault(x => x.AttributeResourceType == AttributeResourceTypes.ATTR_TYPE);
                if (map != null)
                {
                    attributeType = map.ValueAsAttributeType;
                    return true;
                }
                attributeType = AttributeTypes.TYPE_ANY;
                return false;
            }

            /// <summary>
            /// Gets all map keys that are enum/flag values.
            /// </summary>
            public IEnumerable<string> GetEnumOrFlagValueNames()
            {
                return maps.Where(x => !x.IsAttributeResourceType).Select(x => x.Name);
            }

            /// <summary>
            /// Convert to readable string.
            /// </summary>
            public override string ToString()
            {
                return string.Join(", ", maps.Select(x => x.ToString()));
            }
        }
    }
}
