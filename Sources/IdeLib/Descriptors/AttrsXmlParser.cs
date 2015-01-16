using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dot42.ApkLib.Resources;

namespace Dot42.Ide.Descriptors
{
    internal class AttrsXmlParser
    {
        private readonly XDocument document;
        private readonly Dictionary<string, ElementDescriptor> cachedElementDescriptors = new Dictionary<string, ElementDescriptor>();
        private readonly object cacheLock = new object();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AttrsXmlParser(Stream stream)
        {
            document = (stream != null) ? XDocument.Load(stream) : null;
        }

        /// <summary>
        /// R.attr in resources.arsc from base.apk.
        /// </summary>
        internal Table.TypeSpec AttrTypeSpec { get; set; }

        /// <summary>
        /// Lookup an element descriptor by the given short name.
        /// </summary>
        /// <returns>Null if not found</returns>
        internal ElementDescriptor FindElementDescriptor(string shortName)
        {
            if (document == null)
                return null;
            ElementDescriptor result;
            lock (cacheLock)
            {
                if (cachedElementDescriptors.TryGetValue(shortName, out result))
                    return result;
            }
            result = LoadElementDescriptor(shortName);
            lock (cacheLock)
            {
                cachedElementDescriptors[shortName] = result;
            }
            return result;
        }

        /// <summary>
        /// Lookup an element descriptor by the given short name.
        /// </summary>
        /// <returns>Null if not found</returns>
        private ElementDescriptor LoadElementDescriptor(string shortName)
        {
            var element = document.Root.Elements("declare-styleable").FirstOrDefault(x => GetName(x) == shortName);
            if (element == null)
                return null;
            var result = new ElementDescriptor(shortName);
            foreach (var attr in element.Elements("attr"))
            {
                result.Add(ParseAttr(attr));
            }
            return result;
        }

        /// <summary>
        /// Parse an attr element into an <see cref="AttributeDescriptor"/>.
        /// </summary>
        private AttributeDescriptor ParseAttr(XElement attr)
        {
            var name = GetName(attr);
            var formatAsString = GetFormat(attr);
            if (string.IsNullOrEmpty(formatAsString))
            {
                // Auto detect format
                if (attr.Elements("enum").Any())
                    formatAsString = "enum";
                else if (attr.Elements("flag").Any())
                    formatAsString = "flag";
            }

            // Try to parse the format string
            var format = ParseFormat(formatAsString);

            // If not available, try R.attr
            AttributeDescriptor result = null;
            if ((format == 0) && (AttrTypeSpec != null))
            {
                // Detect format from R.attr
                var entry = AttrTypeSpec.Entries.FirstOrDefault(x => x.Key == name);
                if (entry != null)
                {
                    var anyType = AttrTypeSpec.Types.FirstOrDefault(x => x.Configuration.IsAny);
                    if (anyType != null)
                    {
                        Table.EntryInstance instance;
                        Table.ComplexEntryInstance cInstance;
                        if (entry.TryGetInstance(anyType, out instance) &&
                            ((cInstance = instance as Table.ComplexEntryInstance) != null))
                        {
                            AttributeTypes attributeType;
                            if (cInstance.TryGetAttributeType(out attributeType))
                            {
                                // We've found it
                                format = AttributeTypeToFormat(attributeType);

                                // Instantiate descriptor
                                result = new AttributeDescriptor(name, format);
                                if (((attributeType & AttributeTypes.TYPE_ENUM) != 0) || 
                                    ((attributeType & AttributeTypes.TYPE_FLAGS) != 0))
                                {
                                    // Get enum/flag values from resources.arsc
                                    foreach (var value in cInstance.GetEnumOrFlagValueNames())
                                    {
                                        result.Add(new AttributeValueDescriptor(value));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Instantiate descriptor
            result = result ?? new AttributeDescriptor(name, format);
            foreach (var child in attr.Elements("enum"))
            {
                result.Add(new AttributeValueDescriptor(GetName(child)));
            }
            foreach (var child in attr.Elements("flag"))
            {
                result.Add(new AttributeValueDescriptor(GetName(child)));
            }
            return result;
        }

        /// <summary>
        /// Gets the name attribute of the given element.
        /// </summary>
        private static string GetName(XElement element)
        {
            var attr = element.Attribute("name");
            return (attr != null) ? attr.Value : null;
        }

        /// <summary>
        /// Gets the format attribute of the given element.
        /// </summary>
        private static string GetFormat(XElement element)
        {
            var attr = element.Attribute("format");
            return (attr != null) ? attr.Value : null;
        }

        /// <summary>
        /// Parse an attr format value into an enum value.
        /// </summary>
        private static AttributeFormat ParseFormat(string format)
        {
            AttributeFormat result = 0;
            if (string.IsNullOrEmpty(format))
                return result;
            foreach (var part in format.Split('|'))
            {
                switch (part)
                {
                    case "reference":
                        result |= AttributeFormat.Reference;
                        break;
                    case "string":
                        result |= AttributeFormat.String;
                        break;
                    case "color":
                        result |= AttributeFormat.Color;
                        break;
                    case "dimension":
                        result |= AttributeFormat.Dimension;
                        break;
                    case "boolean":
                        result |= AttributeFormat.Boolean;
                        break;
                    case "integer":
                        result |= AttributeFormat.Integer;
                        break;
                    case "float":
                        result |= AttributeFormat.Float;
                        break;
                    case "fraction":
                        result |= AttributeFormat.Fraction;
                        break;
                    case "enum":
                        result |= AttributeFormat.Enum;
                        break;
                    case "flag":
                        result |= AttributeFormat.Flag;
                        break;
                    default:
                        result |= AttributeFormat.Unknown;
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Convert a resources.arsc AttributeTypes to an AttributeFormat.
        /// </summary>
        private static AttributeFormat AttributeTypeToFormat(AttributeTypes attributeTypes)
        {
            AttributeFormat result = 0;

            if ((attributeTypes & AttributeTypes.TYPE_REFERENCE) != 0)
                result |= AttributeFormat.Reference;
            if ((attributeTypes & AttributeTypes.TYPE_STRING) != 0)
                result |= AttributeFormat.String;
            if ((attributeTypes & AttributeTypes.TYPE_INTEGER) != 0)
                result |= AttributeFormat.Integer;
            if ((attributeTypes & AttributeTypes.TYPE_BOOLEAN) != 0)
                result |= AttributeFormat.Boolean;
            if ((attributeTypes & AttributeTypes.TYPE_COLOR) != 0)
                result |= AttributeFormat.Color;
            if ((attributeTypes & AttributeTypes.TYPE_FLOAT) != 0)
                result |= AttributeFormat.Float;
            if ((attributeTypes & AttributeTypes.TYPE_DIMENSION) != 0)
                result |= AttributeFormat.Dimension;
            if ((attributeTypes & AttributeTypes.TYPE_FRACTION) != 0)
                result |= AttributeFormat.Fraction;
            if ((attributeTypes & AttributeTypes.TYPE_ENUM) != 0)
                result |= AttributeFormat.Enum;
            if ((attributeTypes & AttributeTypes.TYPE_FLAGS) != 0)
                result |= AttributeFormat.Flag;
            return result;
        }
    }
}
