using System;

namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_entry (not complex)
        /// </summary>
        public sealed class SimpleEntryInstance : EntryInstance 
        {
            private readonly Value value;
            
            /// <summary>
            /// Read ctor
            /// </summary>
            public SimpleEntryInstance(Type parent, string key, Value value)
                : base(parent, EntryFlags.None, key)
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.value = value;
            }

            /// <summary>
            /// Read ctor
            /// </summary>
            internal SimpleEntryInstance(Type parent, ResReader reader)
                : base(parent, reader)
            {
                // Read value
                value = new Value(reader);
            }

            /// <summary>
            /// Gets my value
            /// </summary>
            public Value Value { get { return value; } }

            /// <summary>
            /// Gets the value as string (in case of string types values)
            /// </summary>
            public string StringValue
            {
                get
                {
                    if (value.Type != Value.Types.TYPE_STRING)
                        return null;
                    return Type.TypeSpec.Package.Table.Strings[value.Data];
                }
            }

            /// <summary>
            /// Write this entry.
            /// </summary>
            internal override void Write(ResWriter writer)
            {
                base.Write(writer);
                value.Write(writer);
            }
        }
    }
}
