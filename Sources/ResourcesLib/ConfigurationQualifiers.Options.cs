namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Alternative configuration qualifiers.
    /// </summary>
    partial class ConfigurationQualifiers
    {
        public static readonly string[] CountryCodes = new[] {
            "aa", "ab", "af", "ak", "sq", "am", "ar", "an", "hy", "as", "av", "ae", "ay", "az", "ba", "bm", "eu", "be", "bn", "bh", "bi", "bo", "bs", 
            "br", "bg", "my", "ca", "cs", "ch", "ce", "zh", "cu", "cv", "kw", "co", "cr", "cy", "cs", "da", "de", "dv", "nl", "dz", "el", "en", "eo", 
            "et", "eu", "ee", "fo", "fa", "fj", "fi", "fr", "fr", "fy", "ff", "ka", "de", "gd", "ga", "gl", "gv", "el", "gn", "gu", "ht", "ha", "he", 
            "hz", "hi", "ho", "hr", "hu", "hy", "ig", "is", "io", "ii", "iu", "ie", "ia", "id", "ik", "is", "it", "jv", "ja", "kl", "kn", "ks", "ka", 
            "kr", "kk", "km", "ki", "rw", "ky", "kv", "kg", "ko", "kj", "ku", "lo", "la", "lv", "li", "ln", "lt", "lb", "lu", "lg", "mk", "mh", "ml", 
            "mi", "mr", "ms", "mk", "mg", "mt", "mn", "mi", "ms", "my", "na", "nv", "nr", "nd", "ng", "ne", "nl", "nn", "nb", "no", "ny", "oc", "oj", 
            "or", "om", "os", "pa", "fa", "pi", "pl", "pt", "ps", "qu", "rm", "ro", "ro", "rn", "ru", "sg", "sa", "si", "sk", "sk", "sl", "se", "sm", 
            "sn", "sd", "so", "st", "es", "sq", "sc", "sr", "ss", "su", "sw", "sv", "ty", "ta", "tt", "te", "tg", "tl", "th", "bo", "ti", "to", "tn", 
            "ts", "tk", "tr", "tw", "ug", "uk", "ur", "uz", "ve", "vi", "vo", "cy", "wa", "wo", "xh", "yi", "yo", "za", "zh", "zu"
        };

        public static readonly string[] RegionCodes = new[] {
            "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ", "BA", "BB", "BD", "BE", "BF", "BG", "BH", 
            "BI", "BJ", "BL", "BM", "BN", "BO", "BQ", "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", 
            "CM", "CN", "CO", "CR", "CU", "CV", "CW", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET", 
            "FI", "FJ", "FK", "FM", "FO", "FR", "GA", "GB", "GD", "GE", "GF", "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", 
            "GW", "GY", "HK", "HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM", "JO", "JP", "KE", 
            "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", 
            "MD", "ME", "MF", "MG", "MH", "MK", "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA", "NC", 
            "NE", "NF", "NG", "NI", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", 
            "PW", "PY", "QA", "RE", "RO", "RS", "RU", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", 
            "SS", "ST", "SV", "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL (changed from TP)", "TM", "TN", "TO", "TR", "TT", "TV", 
            "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI", "VN", "VU", "WF", "WS", "YE", "YT", "ZA", "ZM", "ZW"
        };

        public static readonly ConfigurationOptions<ScreenSizes> ScreenSizeOptions = new ConfigurationOptions<ScreenSizes>(new[] {
            new ConfigurationOption<ScreenSizes>(ScreenSizes.Small, "small", "Screens that are of similar size to a low-density QVGA screen. The minimum layout size for a small screen is approximately 320x426 dp units. Examples are QVGA low density and VGA high density."), 
            new ConfigurationOption<ScreenSizes>(ScreenSizes.Normal, "normal", "Screens that are of similar size to a medium-density HVGA screen. The minimum layout size for a normal screen is approximately 320x470 dp units. Examples of such screens a WQVGA low density, HVGA medium density, WVGA high density."), 
            new ConfigurationOption<ScreenSizes>(ScreenSizes.Large, "large", "Screens that are of similar size to a medium-density VGA screen. The minimum layout size for a large screen is approximately 480x640 dp units. Examples are VGA and WVGA medium density screens."), 
            new ConfigurationOption<ScreenSizes>(ScreenSizes.XLarge, "xlarge", "Screens that are considerably larger than the traditional medium-density HVGA screen. The minimum layout size for an xlarge screen is approximately 720x960 dp units. In most cases, devices with extra large screens would be too large to carry in a pocket and would most likely be tablet-style devices. Added in API level 9."), 
        });

        public static readonly ConfigurationOptions<ScreenAspects> ScreenAspectOptions = new ConfigurationOptions<ScreenAspects>(new[] {
            new ConfigurationOption<ScreenAspects>(ScreenAspects.Long, "long", "Long screens, such as WQVGA, WVGA, FWVGA"), 
            new ConfigurationOption<ScreenAspects>(ScreenAspects.NotLong, "notlong", "Not long screens, such as QVGA, HVGA, and VGA.")
        });

        public static readonly ConfigurationOptions<ScreenOrientations> ScreenOrientationOptions = new ConfigurationOptions<ScreenOrientations>(new[] {
            new ConfigurationOption<ScreenOrientations>(ScreenOrientations.Port, "port", "Device is in portrait orientation (vertical)."),
            new ConfigurationOption<ScreenOrientations>(ScreenOrientations.Land, "land", "Device is in landscape orientation (horizontal).")
        });

        public static readonly ConfigurationOptions<UIModes> UIModeOptions = new ConfigurationOptions<UIModes>(new[] {
            new ConfigurationOption<UIModes>(UIModes.Car, "car", "Device is displaying in a car dock"),
            new ConfigurationOption<UIModes>(UIModes.Desk, "desk", "Device is displaying in a desk dock"),
            new ConfigurationOption<UIModes>(UIModes.Television, "television", "Device is displaying on a television, providing a \"ten foot\" experience where its UI is on a large screen that the user is far away from, primarily oriented around DPAD or other non-pointer interaction"),
            new ConfigurationOption<UIModes>(UIModes.Appliance, "appliance", "Device is serving as an appliance, with no display.")
        });

        public static readonly ConfigurationOptions<NightModes> NightModeOptions = new ConfigurationOptions<NightModes>(new[] {
            new ConfigurationOption<NightModes>(NightModes.Night, "night", "Night time"),
            new ConfigurationOption<NightModes>(NightModes.NotNight, "notnight", "Day time"),
        });

        public static readonly ConfigurationOptions<ScreenPixelDensities> ScreenPixelDensityOptions = new ConfigurationOptions<ScreenPixelDensities>(new[] {
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.ldpi, "ldpi", "Low-density screens; approximately 120dpi."),
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.mdpi, "mdpi", "Medium-density (on traditional HVGA) screens; approximately 160dpi."),
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.hdpi, "hdpi", "High-density screens; approximately 240dpi."),
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.xhdpi, "xhdpi", "Extra high-density screens; approximately 320dpi."),
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.nodpi, "nodpi", "This can be used for bitmap resources that you do not want to be scaled to match the device density."),
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.tvdpi, "tvdpi", "Screens somewhere between mdpi and hdpi; approximately 213dpi. This is not considered a \"primary\" density group. It is mostly intended for televisions and most apps shouldn't need it—providing mdpi and hdpi resources is sufficient for most apps and the system will scale them as appropriate."),
            new ConfigurationOption<ScreenPixelDensities>(ScreenPixelDensities.anydpi, "anydpi", "can be used on >= v21"),
        });

        public static readonly ConfigurationOptions<TouchScreenTypes> TouchScreenTypeOptions = new ConfigurationOptions<TouchScreenTypes>(new[] {
            new ConfigurationOption<TouchScreenTypes>(TouchScreenTypes.NoTouch, "notouch", "Device does not have a touchscreen."),
            new ConfigurationOption<TouchScreenTypes>(TouchScreenTypes.Finger, "finger", "Device has a touchscreen that is intended to be used through direction interaction of the user's finger."),
        });

        public static readonly ConfigurationOptions<KeyboardAvailabilities> KeyboardAvailabilityOptions = new ConfigurationOptions<KeyboardAvailabilities>(new[] {
            new ConfigurationOption<KeyboardAvailabilities>(KeyboardAvailabilities.KeysExposed, "keysexposed", "Device has a keyboard available. If the device has a software keyboard enabled (which is likely), this may be used even when the hardware keyboard is not exposed to the user, even if the device has no hardware keyboard. If no software keyboard is provided or it's disabled, then this is only used when a hardware keyboard is exposed."),
            new ConfigurationOption<KeyboardAvailabilities>(KeyboardAvailabilities.KeysHidden, "keyshidden", "Device has a hardware keyboard available but it is hidden and the device does not have a software keyboard enabled."),
            new ConfigurationOption<KeyboardAvailabilities>(KeyboardAvailabilities.KeysSoft, "keyssoft", "Device has a software keyboard enabled, whether it's visible or not."),
        });

        public static readonly ConfigurationOptions<PrimaryTextInputMethods> PrimaryTextInputMethodOptions = new ConfigurationOptions<PrimaryTextInputMethods>(new[] {
            new ConfigurationOption<PrimaryTextInputMethods>(PrimaryTextInputMethods.NoKeys, "nokeys", "Device has no hardware keys for text input."),
            new ConfigurationOption<PrimaryTextInputMethods>(PrimaryTextInputMethods.Qwerty, "qwerty", "Device has a hardware qwerty keyboard, whether it's visible to the user or not."),
            new ConfigurationOption<PrimaryTextInputMethods>(PrimaryTextInputMethods.TwelveKeys, "12key", "Device has a hardware 12-key keyboard, whether it's visible to the user or not."),
        });

        public static readonly ConfigurationOptions<NavigationKeyAvailabilities> NavigationKeyAvailabilityOptions = new ConfigurationOptions<NavigationKeyAvailabilities>(new[] {
            new ConfigurationOption<NavigationKeyAvailabilities>(NavigationKeyAvailabilities.NavExposed, "navexposed", "Navigation keys are available to the user."),
            new ConfigurationOption<NavigationKeyAvailabilities>(NavigationKeyAvailabilities.NavHidden, "navhidden", "Navigation keys are not available (such as behind a closed lid)."),
        });

        public static readonly ConfigurationOptions<PrimaryNonTouchNavigationMethods> PrimaryNonTouchNavigationMethodOptions = new ConfigurationOptions<PrimaryNonTouchNavigationMethods>(new[] {
            new ConfigurationOption<PrimaryNonTouchNavigationMethods>(PrimaryNonTouchNavigationMethods.NoNav, "nonav", "Device has no navigation facility other than using the touchscreen."),
            new ConfigurationOption<PrimaryNonTouchNavigationMethods>(PrimaryNonTouchNavigationMethods.Dpad, "dpad", "Device has a directional-pad (d-pad) for navigation."),
            new ConfigurationOption<PrimaryNonTouchNavigationMethods>(PrimaryNonTouchNavigationMethods.Trackball, "trackball", "Device has a trackball for navigation."),
            new ConfigurationOption<PrimaryNonTouchNavigationMethods>(PrimaryNonTouchNavigationMethods.Wheel, "wheel", "Device has a directional wheel(s) for navigation (uncommon)."),
        });
    }
}
