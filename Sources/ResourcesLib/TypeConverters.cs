using System.Reflection;

namespace Dot42.ResourcesLib
{
    [Obfuscation(Feature = "make-internal")]
    public class ScreenSizeTypeConverter : ConfigurationOptionsTypeConverter<ScreenSizes>
    {
        public ScreenSizeTypeConverter() : base(ScreenSizes.Any, ConfigurationQualifiers.ScreenSizeOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class ScreenAspectTypeConverter : ConfigurationOptionsTypeConverter<ScreenAspects>
    {
        public ScreenAspectTypeConverter() : base(ScreenAspects.Any, ConfigurationQualifiers.ScreenAspectOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class ScreenOrientationTypeConverter : ConfigurationOptionsTypeConverter<ScreenOrientations>
    {
        public ScreenOrientationTypeConverter() : base(ScreenOrientations.Any, ConfigurationQualifiers.ScreenOrientationOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class UIModeTypeConverter : ConfigurationOptionsTypeConverter<UIModes>
    {
        public UIModeTypeConverter() : base(UIModes.Any, ConfigurationQualifiers.UIModeOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class NightModeTypeConverter : ConfigurationOptionsTypeConverter<NightModes>
    {
        public NightModeTypeConverter() : base(NightModes.Any, ConfigurationQualifiers.NightModeOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class ScreenPixelDensityTypeConverter : ConfigurationOptionsTypeConverter<ScreenPixelDensities>
    {
        public ScreenPixelDensityTypeConverter() : base(ScreenPixelDensities.Any, ConfigurationQualifiers.ScreenPixelDensityOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class TouchScreenTypeTypeConverter : ConfigurationOptionsTypeConverter<TouchScreenTypes>
    {
        public TouchScreenTypeTypeConverter() : base(TouchScreenTypes.Any, ConfigurationQualifiers.TouchScreenTypeOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class KeyboardAvailabilityTypeConverter : ConfigurationOptionsTypeConverter<KeyboardAvailabilities>
    {
        public KeyboardAvailabilityTypeConverter() : base(KeyboardAvailabilities.Any, ConfigurationQualifiers.KeyboardAvailabilityOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class PrimaryTextInputTypeConverter : ConfigurationOptionsTypeConverter<PrimaryTextInputMethods>
    {
        public PrimaryTextInputTypeConverter() : base(PrimaryTextInputMethods.Any, ConfigurationQualifiers.PrimaryTextInputMethodOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class NavigationKeyAvailabilityTypeConverter : ConfigurationOptionsTypeConverter<NavigationKeyAvailabilities>
    {
        public NavigationKeyAvailabilityTypeConverter() : base(NavigationKeyAvailabilities.Any, ConfigurationQualifiers.NavigationKeyAvailabilityOptions) { }
    }

    [Obfuscation(Feature = "make-internal")]
    public class PrimaryNonTouchNavigationMethodTypeConverter : ConfigurationOptionsTypeConverter<PrimaryNonTouchNavigationMethods>
    {
        public PrimaryNonTouchNavigationMethodTypeConverter() : base(PrimaryNonTouchNavigationMethods.Any, ConfigurationQualifiers.PrimaryNonTouchNavigationMethodOptions) { }
    }
}
