using System;
using System.Linq;
using System.ComponentModel;

namespace Dot42.Ide.TypeConverters
{
    /// <summary>
    /// Base class for type converters that provide a standard set of strings.
    /// </summary>
    internal class StringTypeConverter : TypeConverter
    {
        private readonly string[] options;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected StringTypeConverter(params string[] options)
        {
            this.options = options;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(options);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return options.Contains(value as string);
        }
    }
}
