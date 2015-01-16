using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using Dot42.ResourcesLib;

namespace Dot42.Ide.Extenders
{
    [Obfuscation(Feature = "make-internal", Exclude = true)]
    public abstract class ResourceExtenderBase : IResourceExtender
    {
        internal const string AnyText = "Any";
        private const string CatDot42 = "Dot42 Alternate Resource Qualifiers";
        private ConfigurationQualifiers qualifiers;
        private string qualifiersFileName;

        /// <summary>
        /// Mobile country code?
        /// </summary>
        [DisplayName("Mobile Country Code")]
        [Category(CatDot42)]
        [Description("Mobile country code for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        public string MobileCountryCode
        {
            get { return NumberToString(Qualifiers.MobileCountryCode); }
            set { var q = Qualifiers; q.MobileCountryCode = ParsePositiveNumber(value); Save(q); }
        }

        /// <summary>
        /// Mobile network carrier?
        /// </summary>
        [DisplayName("Mobile Network Carrier")]
        [Category(CatDot42)]
        [Description("Mobile network carrier for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        public string MobileNetworkCarrier
        {
            get { return NumberToString(Qualifiers.MobileNetworkCarrier); }
            set { var q = Qualifiers; q.MobileNetworkCarrier = ParsePositiveNumber(value); Save(q); }
        }

        /// <summary>
        /// Language?
        /// </summary>
        [DisplayName("Language")]
        [Category(CatDot42)]
        [Description("Language country code for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        [Editor(typeof(LanguageEditor), typeof(UITypeEditor))]
        public string Language
        {
            get { return StringToString(Qualifiers.Language); }
            set { var q = Qualifiers; q.Language = LimitToList(value, ConfigurationQualifiers.CountryCodes); Save(q); }
        }

        /// <summary>
        /// Region?
        /// </summary>
        [DisplayName("Region")]
        [Category(CatDot42)]
        [Description("Region code for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        [Editor(typeof(RegionEditor), typeof(UITypeEditor))]
        public string Region
        {
            get { return StringToString(Qualifiers.Region); }
            set { var q = Qualifiers; q.Region = LimitToList(value, ConfigurationQualifiers.RegionCodes); Save(q); }
        }

        /// <summary>
        /// Smallest screen width?
        /// </summary>
        [DisplayName("Smallest screen width")]
        [Category(CatDot42)]
        [Description("Smallest fundamental size of a screen which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        public string SmallestWidth
        {
            get { return NumberToString(Qualifiers.SmallestWidth); }
            set { var q = Qualifiers; q.SmallestWidth = ParsePositiveNumber(value); Save(q); }
        }

        /// <summary>
        /// Minimum available screen width?
        /// </summary>
        [DisplayName("Available width")]
        [Category(CatDot42)]
        [Description("Specifies minimum available screen width for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        public string AvailableWidth
        {
            get { return NumberToString(Qualifiers.AvailableWidth); }
            set { var q = Qualifiers; q.AvailableWidth = ParsePositiveNumber(value); Save(q); }
        }

        /// <summary>
        /// Minimum available screen height?
        /// </summary>
        [DisplayName("Available height")]
        [Category(CatDot42)]
        [Description("Specifies minimum available screen height for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        public string AvailableHeight
        {
            get { return NumberToString(Qualifiers.AvailableHeight); }
            set { var q = Qualifiers; q.AvailableHeight = ParsePositiveNumber(value); Save(q); }
        }

        /// <summary>
        /// Screen size qualifier?
        /// </summary>
        [DisplayName("Screen size")]
        [Category(CatDot42)]
        [Description("Screen size for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(ScreenSizeTypeConverter))]
        [DefaultValue((int)ScreenSizes.Any)]
        public ScreenSizes ScreenSize
        {
            get { return Qualifiers.ScreenSize; }
            set { var q = Qualifiers; q.ScreenSize = value; Save(q); }
        }

        /// <summary>
        /// Screen aspect qualifier?
        /// </summary>
        [DisplayName("Screen aspect")]
        [Category(CatDot42)]
        [Description("Screen aspect for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(ScreenAspectTypeConverter))]
        [DefaultValue((int)ScreenAspects.Any)]
        public ScreenAspects ScreenAspect
        {
            get { return Qualifiers.ScreenAspect; }
            set { var q = Qualifiers; q.ScreenAspect = value; Save(q); }
        }

        /// <summary>
        /// Screen orientation qualifier?
        /// </summary>
        [DisplayName("Screen orientation")]
        [Category(CatDot42)]
        [Description("Screen orientation for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(ScreenOrientationTypeConverter))]
        [DefaultValue((int)ScreenOrientations.Any)]
        public ScreenOrientations ScreenOrientation
        {
            get { return Qualifiers.ScreenOrientation; }
            set { var q = Qualifiers; q.ScreenOrientation = value; Save(q); }
        }

        /// <summary>
        /// UI mode qualifier?
        /// </summary>
        [DisplayName("UI mode")]
        [Category(CatDot42)]
        [Description("UI mode for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(UIModeTypeConverter))]
        [DefaultValue((int)UIModes.Any)]
        public UIModes UIMode
        {
            get { return Qualifiers.UIMode; }
            set { var q = Qualifiers; q.UIMode = value; Save(q); }
        }

        /// <summary>
        /// Night mode qualifier?
        /// </summary>
        [DisplayName("Night mode")]
        [Category(CatDot42)]
        [Description("Night mode for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(NightModeTypeConverter))]
        [DefaultValue((int)NightModes.Any)]
        public NightModes NightMode
        {
            get { return Qualifiers.NightMode; }
            set { var q = Qualifiers; q.NightMode = value; Save(q); }
        }

        /// <summary>
        /// Screen pixel density qualifier?
        /// </summary>
        [DisplayName("Screen pixel density")]
        [Category(CatDot42)]
        [Description("Screen pixel density for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(ScreenPixelDensityTypeConverter))]
        [DefaultValue((int)ScreenPixelDensities.Any)]
        public ScreenPixelDensities ScreenPixelDensity
        {
            get { return Qualifiers.ScreenPixelDensity; }
            set { var q = Qualifiers; q.ScreenPixelDensity = value; Save(q); }
        }

        /// <summary>
        /// Touch screen type qualifier?
        /// </summary>
        [DisplayName("Touch screen type")]
        [Category(CatDot42)]
        [Description("Touch screen type for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(TouchScreenTypeTypeConverter))]
        [DefaultValue((int)TouchScreenTypes.Any)]
        public TouchScreenTypes TouchScreenType
        {
            get { return Qualifiers.TouchScreenType; }
            set { var q = Qualifiers; q.TouchScreenType = value; Save(q); }
        }

        /// <summary>
        /// Keyboard availability qualifier?
        /// </summary>
        [DisplayName("Keyboard availability")]
        [Category(CatDot42)]
        [Description("Keyboard availability for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(KeyboardAvailabilityTypeConverter))]
        [DefaultValue((int)KeyboardAvailabilities.Any)]
        public KeyboardAvailabilities KeyboardAvailability
        {
            get { return Qualifiers.KeyboardAvailability; }
            set { var q = Qualifiers; q.KeyboardAvailability = value; Save(q); }
        }

        /// <summary>
        /// Primary text input method qualifier?
        /// </summary>
        [DisplayName("Primary text input method")]
        [Category(CatDot42)]
        [Description("Primary text input method for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(PrimaryTextInputTypeConverter))]
        [DefaultValue((int)PrimaryTextInputMethods.Any)]
        public PrimaryTextInputMethods PrimaryTextInputMethod
        {
            get { return Qualifiers.PrimaryTextInputMethod; }
            set { var q = Qualifiers; q.PrimaryTextInputMethod = value; Save(q); }
        }

        /// <summary>
        /// Navigation key availability qualifier?
        /// </summary>
        [DisplayName("Navigation key availability")]
        [Category(CatDot42)]
        [Description("Navigation key availability for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(NavigationKeyAvailabilityTypeConverter))]
        [DefaultValue((int)NavigationKeyAvailabilities.Any)]
        public NavigationKeyAvailabilities NavigationKeyAvailability
        {
            get { return Qualifiers.NavigationKeyAvailability; }
            set { var q = Qualifiers; q.NavigationKeyAvailability = value; Save(q); }
        }

        /// <summary>
        /// Primary non touch navigation method qualifier?
        /// </summary>
        [DisplayName("Primary non-touch navigation method")]
        [Category(CatDot42)]
        [Description("Primary non-touch navigation method for which this resource is intended.")]
        [Obfuscation]
        [TypeConverter(typeof(PrimaryNonTouchNavigationMethodTypeConverter))]
        [DefaultValue((int)PrimaryNonTouchNavigationMethods.Any)]
        public PrimaryNonTouchNavigationMethods PrimaryNonTouchNavigationMethod
        {
            get { return Qualifiers.PrimaryNonTouchNavigationMethod; }
            set { var q = Qualifiers; q.PrimaryNonTouchNavigationMethod = value; Save(q); }
        }

        /// <summary>
        /// Minimum available screen height?
        /// </summary>
        [DisplayName("Platform version")]
        [Category(CatDot42)]
        [Description("Platform version (level) for which this resource is intended.")]
        [Obfuscation]
        [DefaultValue(AnyText)]
        public string PlatformVersion
        {
            get { return NumberToString(Qualifiers.PlatformVersion); }
            set { var q = Qualifiers; q.PlatformVersion = ParsePositiveNumber(value); Save(q); }
        }

        /// <summary>
        /// Gets qualifiers from the filename.
        /// </summary>
        private ConfigurationQualifiers Qualifiers
        {
            get
            {
                var fileName = FileName;
                if ((qualifiers == null) || (qualifiersFileName != fileName))
                {
                    qualifiersFileName = fileName;
                    qualifiers = ConfigurationQualifiers.Parse(fileName);
                }
                return qualifiers;
            }
        }

        /// <summary>
        /// Change the filename based on the given qualifiers
        /// </summary>
        private void Save(ConfigurationQualifiers newQualifiers)
        {
            var fileName = FileName;
            var ext = ConfigurationQualifiers.GetExtension(fileName);
            var name = ConfigurationQualifiers.StripQualifiers(fileName, true, false);
            // Update filename
            fileName = name + newQualifiers + ext;
            FileName = fileName;
            // Update cache
            qualifiersFileName = fileName;
            this.qualifiers = newQualifiers;
        }

        /// <summary>
        /// Gets/sets the filename of the extended object
        /// </summary>
        protected abstract string FileName { get; set; }

        /// <summary>
        /// Convert an optional number to string.
        /// </summary>
        private static string NumberToString(int? number)
        {
            if (!number.HasValue)
                return AnyText;
            return number.Value.ToString();
        }

        /// <summary>
        /// Convert an optional string to string.
        /// </summary>
        private static string StringToString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return AnyText;
            return value;
        }

        /// <summary>
        /// Try to parse a string into a positive integer.
        /// </summary>
        private static int? ParsePositiveNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (string.Equals(value, AnyText, StringComparison.InvariantCultureIgnoreCase))
                return null;
            var number = int.Parse(value);
            if (number < 0)
                throw new ArgumentOutOfRangeException("Negative numbers are not allowed");
            return number;
        }

        /// <summary>
        /// Limit the given value to a value in the list or null.
        /// </summary>
        private static string LimitToList(string value, string[] list)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (value == AnyText)
                return null;
            if (Array.IndexOf(list, value) < 0)
                throw new ArgumentOutOfRangeException(string.Format("{0} is not a valid value.", value));
            return value;
        }
    }
}
