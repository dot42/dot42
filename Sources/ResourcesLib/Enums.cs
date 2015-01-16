using System.Reflection;

namespace Dot42.ResourcesLib
{
    [Obfuscation(Feature = "make-internal")]
    public enum ScreenSizes
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        Small,
        [Obfuscation]
        Normal,
        [Obfuscation]
        Large,
        [Obfuscation]
        XLarge
    }

    [Obfuscation(Feature = "make-internal")]
    public enum ScreenAspects
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        Long,
        [Obfuscation]
        NotLong
    }

    [Obfuscation(Feature = "make-internal")]
    public enum ScreenOrientations
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        Port,
        [Obfuscation]
        Land
    }

    [Obfuscation(Feature = "make-internal")]
    public enum UIModes
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        Car,
        [Obfuscation]
        Desk,
        [Obfuscation]
        Television,
        [Obfuscation]
        Appliance
    }

    [Obfuscation(Feature = "make-internal")]
    public enum NightModes
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        Night,
        [Obfuscation]
        NotNight
    }

    [Obfuscation(Feature = "make-internal")]
    public enum ScreenPixelDensities
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        ldpi,
        [Obfuscation]
        mdpi,
        [Obfuscation]
        hdpi,
        [Obfuscation]
        xhdpi,
        [Obfuscation]
        nodpi,
        [Obfuscation]
        tvdpi
    }

    [Obfuscation(Feature = "make-internal")]
    public enum TouchScreenTypes
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        NoTouch,
        [Obfuscation]
        Finger
    }

    [Obfuscation(Feature = "make-internal")]
    public enum KeyboardAvailabilities
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        KeysExposed,
        [Obfuscation]
        KeysHidden,
        [Obfuscation]
        KeysSoft
    }

    [Obfuscation(Feature = "make-internal")]
    public enum PrimaryTextInputMethods
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        NoKeys,
        [Obfuscation]
        Qwerty,
        [Obfuscation]
        TwelveKeys
    }

    [Obfuscation(Feature = "make-internal")]
    public enum NavigationKeyAvailabilities
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        NavExposed,
        [Obfuscation]
        NavHidden
    }

    [Obfuscation(Feature = "make-internal")]
    public enum PrimaryNonTouchNavigationMethods
    {
        [Obfuscation]
        Any,
        [Obfuscation]
        NoNav,
        [Obfuscation]
        Dpad,
        [Obfuscation]
        Trackball,
        [Obfuscation]
        Wheel
    }
}
