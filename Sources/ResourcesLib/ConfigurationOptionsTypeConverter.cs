using System.ComponentModel;
using System.Linq;

namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Base class for configuration options type converters.
    /// </summary>
    public abstract class ConfigurationOptionsTypeConverter<T> : TypeConverter
        where T : struct 
    {
        private static readonly string AnyText = "Any";
        private readonly ConfigurationOptions options;
        private readonly T any;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ConfigurationOptionsTypeConverter(T any, ConfigurationOptions options)
        {
            this.options = options;
            this.any = any;
        }

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if ((sourceType == typeof(string)) || (sourceType == typeof(ConfigurationOption<T>)))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the specified context.
        /// </summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if ((destinationType == typeof(string)) || (destinationType == typeof(T)))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var option = value as ConfigurationOption<T>;
            if (option != null) return option.Value;
            var str = value as string;
            if (str == AnyText) return any;
            if (str != null)
            {
                var first = options.Cast<ConfigurationOption<T>>().First(x => x.Option == str);
                if (first != null) return first.Value;
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var option = value as ConfigurationOption<T>;
                if (option != null) return option.Option;
                var str = value as string;
                if (str == AnyText) return string.Empty;
                if (str != null) return str;
                if (value is T)
                {
                    var t = (T) value;
                    if (Equals(t, any)) return AnyText;
                    var first = options.Cast<ConfigurationOption<T>>().First(x => Equals(x.Value, t));
                    if (first != null) return first.Option;
                }
            }
            if (destinationType == typeof(T))
            {
                var option = value as ConfigurationOption<T>;
                if (option != null) return option.Value;
                var str = value as string;
                if (str == AnyText) return any;
                if (str != null)
                {
                    var first = options.Cast<ConfigurationOption<T>>().First(x => x.Option == str);
                    if (first != null) return first.Value;
                }
                if (value is T)
                {
                    return (T) value;
                }                
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
        /// </summary>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
        /// </summary>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var list = options.ToList();
            list.Insert(0, new ConfigurationOption<T>(any, AnyText, AnyText));
            return new StandardValuesCollection(list);
        }

        /// <summary>
        /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exclusive list of possible values, using the specified context.
        /// </summary>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
