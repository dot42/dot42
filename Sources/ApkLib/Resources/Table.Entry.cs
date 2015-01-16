using System.Collections.Generic;
using System.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// Set of EntryInstances for specific Type's.
        /// </summary>
        public sealed class Entry
        {
            private readonly TypeSpec parent;
            private readonly int flags;
            private readonly Dictionary<Type, EntryInstance> instances = new Dictionary<Type, EntryInstance>();

            /// <summary>
            /// Read ctor
            /// </summary>
            internal Entry(TypeSpec parent, int flags)
            {
                this.parent = parent;
                this.flags = flags;
            }

            /// <summary>
            /// Gets my flags
            /// </summary>
            internal int Flags
            {
                get { return flags; }
            }

            /// <summary>
            /// Get the key of this entry
            /// </summary>
            public string Key
            {
                get { return (instances.Count == 0) ? null : instances.Values.First().Key; }
            }

            /// <summary>
            /// Gets my parent
            /// </summary>
            internal TypeSpec TypeSpec { get { return parent; } }

            /// <summary>
            /// Add the given instance
            /// </summary>
            public void Add(EntryInstance instance)
            {
                instances.Add(instance.Type, instance);
            }

            /// <summary>
            /// Try to get an instance for the given type.
            /// </summary>
            public bool TryGetInstance(Type type, out EntryInstance instance)
            {
                return instances.TryGetValue(type, out instance);
            }

            /// <summary>
            /// Prepare all instances for writing
            /// </summary>
            internal void PrepareForWrite()
            {
                foreach (var instance in instances.Values)
                {
                    instance.PrepareForWrite();
                }
            }
        }
    }
}
