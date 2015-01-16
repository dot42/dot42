namespace Dot42.ApkLib.Resources
{
    partial class Table
    {
        /// <summary>
        /// ResTable_map
        /// </summary>
        public sealed class Map
        {            

            private readonly EntryInstance parent;
            private readonly Value value;
            private readonly int nameRef;

            /// <summary>
            /// Read ctor
            /// </summary>
            internal Map(EntryInstance parent, ResReader reader)
            {
                this.parent = parent;

                // header
                nameRef = TableRef.Read(reader);
                value = new Value(reader);
            }

            /// <summary>
            /// Gets my value
            /// </summary>
            public Value Value { get { return value; } }

            /// <summary>
            /// Gets the value's data as <see cref="AttributeTypes"/>.
            /// </summary>
            public AttributeTypes ValueAsAttributeType { get { return (AttributeTypes) value.Data; } }

            /// <summary>
            /// Gets the value's data as <see cref="LocalizationModes"/>.
            /// </summary>
            public LocalizationModes ValueAsLocalizationMode { get { return (LocalizationModes) value.Data; } }

            /// <summary>
            /// Is the name an <see cref="AttributeResourceTypes"/>?
            /// </summary>
            public bool IsAttributeResourceType
            {
                get
                {
                    return
                        (nameRef >= (int) AttributeResourceTypes.ATTR_TYPE) &&
                        (nameRef <= (int) AttributeResourceTypes.ATTR_MANY);
                }
            }

            /// <summary>
            /// Gets the name as <see cref="AttributeResourceTypes"/>.
            /// </summary>
            public AttributeResourceTypes AttributeResourceType { get { return (AttributeResourceTypes) nameRef; } }

            /// <summary>
            /// Gets the name of this map entry.
            /// </summary>
            public string Name
            {
                get
                {
                    if (IsAttributeResourceType)
                        return AttributeResourceType.ToString();
                    return parent.Type.TypeSpec.Package.Table.GetResourceIdentifier(nameRef) ?? nameRef.ToString("X8");
                }
            }

            /// <summary>
            /// Gets the value as string.
            /// </summary>
            public string StringValue
            {
                get
                {
                    if (IsAttributeResourceType)
                    {
                        switch (AttributeResourceType)
                        {
                            case AttributeResourceTypes.ATTR_TYPE:
                                return ValueAsAttributeType.ToString();
                            case AttributeResourceTypes.ATTR_L10N:
                                return ValueAsLocalizationMode.ToString();
                        }
                    }
                    if (value.Type != Value.Types.TYPE_STRING)
                        return value.ToString();
                    return parent.Type.TypeSpec.Package.Table.Strings[value.Data];
                }
            }

            public override string ToString()
            {
                return string.Format("{{\"{0}\": {1}}}", Name, StringValue);
            }
        }
    }
}
