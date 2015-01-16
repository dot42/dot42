using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Dot42.ResourcesLib;

namespace Dot42.Ide.Extenders
{
    /// <summary>
    /// Extend Android resource items
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IResourceExtender
    {
        /// <summary>
        /// Mobile country code?
        /// </summary>
        [Obfuscation]
        string MobileCountryCode { get; set; }

        /// <summary>
        /// Mobile network carrier?
        /// </summary>
        [Obfuscation]
        string MobileNetworkCarrier { get; set; }

        /// <summary>
        /// Language code?
        /// </summary>
        [Obfuscation]
        string Language { get; set; }

        /// <summary>
        /// Region code?
        /// </summary>
        [Obfuscation]
        string Region { get; set; }

        /// <summary>
        /// Smallest screen width?
        /// </summary>
        [Obfuscation]
        string SmallestWidth{ get; set; }

        /// <summary>
        /// Minimum available screen width?
        /// </summary>
        [Obfuscation]
        string AvailableWidth { get; set; }

        /// <summary>
        /// Minimum available screen height?
        /// </summary>
        [Obfuscation]
        string AvailableHeight { get; set; }

        /// <summary>
        /// Screen size qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(ScreenSizes.Any)]
        ScreenSizes ScreenSize { get; set; }

        /// <summary>
        /// Screen aspect qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(ScreenAspects.Any)]
        ScreenAspects ScreenAspect { get; set; }

        /// <summary>
        /// Screen orientation qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(ScreenOrientations.Any)]
        ScreenOrientations ScreenOrientation { get; set; }

        /// <summary>
        /// UI mode qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(UIModes.Any)]
        UIModes UIMode { get; set; }

        /// <summary>
        /// Night mode qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(NightModes.Any)]
        NightModes NightMode { get; set; }

        /// <summary>
        /// Screen pixel density qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(ScreenPixelDensities.Any)]
        ScreenPixelDensities ScreenPixelDensity { get; set; }

        /// <summary>
        /// Touch screen type qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(TouchScreenTypes.Any)]
        TouchScreenTypes TouchScreenType { get; set; }

        /// <summary>
        /// Keyboard availability qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(KeyboardAvailabilities.Any)]
        KeyboardAvailabilities KeyboardAvailability { get; set; }

        /// <summary>
        /// Primary text input method qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(PrimaryTextInputMethods.Any)]
        PrimaryTextInputMethods PrimaryTextInputMethod { get; set; }

        /// <summary>
        /// Navigation key availability qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(NavigationKeyAvailabilities.Any)]
        NavigationKeyAvailabilities NavigationKeyAvailability { get; set; }

        /// <summary>
        /// Primary non touch navigation method qualifier?
        /// </summary>
        [Obfuscation]
        [DefaultValue(PrimaryNonTouchNavigationMethods.Any)]
        PrimaryNonTouchNavigationMethods PrimaryNonTouchNavigationMethod { get; set; }

        /// <summary>
        /// Minimum platform version?
        /// </summary>
        [Obfuscation]
        string PlatformVersion { get; set; }

    }
}
