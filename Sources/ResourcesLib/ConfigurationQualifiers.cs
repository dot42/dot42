using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Alternative configuration qualifiers.
    /// </summary>
    public sealed partial class ConfigurationQualifiers
    {
        public int? MobileCountryCode { get; set; }
        public int? MobileNetworkCarrier { get; set; }
        public string Language { get; set; }
        public string Region { get; set; }
        public int? SmallestWidth { get; set; }
        public int? AvailableWidth { get; set; }
        public int? AvailableHeight { get; set; }
        public ScreenSizes ScreenSize { get; set; }
        public ScreenAspects ScreenAspect { get; set; }
        public ScreenOrientations ScreenOrientation { get; set; }
        public UIModes UIMode { get; set; }
        public NightModes NightMode { get; set; }
        public ScreenPixelDensities ScreenPixelDensity { get; set; }
        public TouchScreenTypes TouchScreenType { get; set; }
        public KeyboardAvailabilities KeyboardAvailability { get; set; }
        public PrimaryTextInputMethods PrimaryTextInputMethod { get; set; }
        public NavigationKeyAvailabilities NavigationKeyAvailability { get; set; }
        public PrimaryNonTouchNavigationMethods PrimaryNonTouchNavigationMethod { get; set; }
        public int? PlatformVersion { get; set; }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        public override string ToString()
        {
            var list = new List<string>();
            if (MobileCountryCode.HasValue)
            {
                list.Add(string.Format("mcc{0}", MobileCountryCode.Value));
            }
            if (MobileNetworkCarrier.HasValue)
            {
                list.Add(string.Format("mnc{0}", MobileNetworkCarrier.Value));                
            }
            if (!string.IsNullOrEmpty(Language))
            {
                list.Add(Language.ToLower());
            }
            if (!string.IsNullOrEmpty(Region))
            {
                list.Add("r" + Region.ToUpper());
            }
            if (SmallestWidth.HasValue)
            {
                list.Add(string.Format("sw{0}dp", SmallestWidth.Value));                                
            }
            if (AvailableWidth.HasValue)
            {
                list.Add(string.Format("w{0}dp", AvailableWidth.Value));
            }
            if (AvailableHeight.HasValue)
            {
                list.Add(string.Format("h{0}dp", AvailableHeight.Value));
            }
            if (ScreenSize != ScreenSizes.Any)
            {
                list.Add(ScreenSizeOptions.GetOption(ScreenSize));
            }
            if (ScreenAspect != ScreenAspects.Any)
            {
                list.Add(ScreenAspectOptions.GetOption(ScreenAspect));
            }
            if (ScreenOrientation!= ScreenOrientations.Any)
            {
                list.Add(ScreenOrientationOptions.GetOption(ScreenOrientation));
            }
            if (UIMode != UIModes.Any)
            {
                list.Add(UIModeOptions.GetOption(UIMode));
            }
            if (NightMode != NightModes.Any)
            {
                list.Add(NightModeOptions.GetOption(NightMode));
            }
            if (ScreenPixelDensity!= ScreenPixelDensities.Any)
            {
                list.Add(ScreenPixelDensityOptions.GetOption(ScreenPixelDensity));
            }
            if (TouchScreenType != TouchScreenTypes.Any)
            {
                list.Add(TouchScreenTypeOptions.GetOption(TouchScreenType));
            }
            if (KeyboardAvailability != KeyboardAvailabilities.Any)
            {
                list.Add(KeyboardAvailabilityOptions.GetOption(KeyboardAvailability));
            }
            if (PrimaryTextInputMethod != PrimaryTextInputMethods.Any)
            {
                list.Add(PrimaryTextInputMethodOptions.GetOption(PrimaryTextInputMethod));
            }
            if (NavigationKeyAvailability!= NavigationKeyAvailabilities.Any)
            {
                list.Add(NavigationKeyAvailabilityOptions.GetOption(NavigationKeyAvailability));
            }
            if (PrimaryNonTouchNavigationMethod!= PrimaryNonTouchNavigationMethods.Any)
            {
                list.Add(PrimaryNonTouchNavigationMethodOptions.GetOption(PrimaryNonTouchNavigationMethod));
            }
            if (PlatformVersion.HasValue)
            {
                list.Add(string.Format("v{0}", PlatformVersion.Value));
            }

            if (list.Count == 0)
                return string.Empty;

            return "-" + string.Join("-", list.ToArray());
        }

        /// <summary>
        /// Gets the alternate resource data from the given filename and put it in a valid order.
        /// </summary>
        public static ConfigurationQualifiers Parse(string resourceFile)
        {
            var result = new ConfigurationQualifiers();
            if (string.IsNullOrEmpty(resourceFile))
                return result;
            var name = GetFileNameWithoutExtension(resourceFile);
            var index = name.IndexOf('-');
            if (index < 0)
            {
                // No alternate resource directory
                return result;
            }

            name = name.Substring(index + 1).ToLower();
            var parts = name.Split('-').ToList();

            // Detect all allowed qualifiers in order
            string option;
            var number = 0;

            // Mobile Country Code
            option = parts.FirstOrDefault(x => IsNumberQualifier(x, "mcc", null, out number));
            if (option != null)
            {
                result.MobileCountryCode = number;
                parts.Remove(option);
            }

            // Mobile Network Carrier
            option = parts.FirstOrDefault(x => IsNumberQualifier(x, "mnc", null, out number));
            if (option != null)
            {
                result.MobileNetworkCarrier = number;
                parts.Remove(option);
            }

            // Country code
            option = parts.FirstOrDefault(x => (x.Length == 2) && CountryCodes.Contains(x));
            if (option != null)
            {
                result.Language = option;
                parts.Remove(option);
            }

            // Region code
            option = parts.FirstOrDefault(x => (x.Length == 3) && (x[0] == 'r') && RegionCodes.Contains(x.Substring(1).ToUpper()));
            if (option != null)
            {
                result.Region = option.Substring(1).ToUpper();
                parts.Remove(option);
            }

            // SmallestWidth
            option = parts.FirstOrDefault(x => IsNumberQualifier(x, "sw", "dp", out number));
            if (option != null)
            {
                result.SmallestWidth = number;
                parts.Remove(option);                
            }

            // Available Width
            option = parts.FirstOrDefault(x => IsNumberQualifier(x, "w", "dp", out number));
            if (option != null)
            {
                result.AvailableWidth = number;
                parts.Remove(option);
            }

            // Available height
            option = parts.FirstOrDefault(x => IsNumberQualifier(x, "h", "dp", out number));
            if (option != null)
            {
                result.AvailableHeight = number;
                parts.Remove(option);
            }

            // Screen size
            option = parts.FirstOrDefault(x => ScreenSizeOptions.Contains(x));
            if (option != null)
            {
                result.ScreenSize = ScreenSizeOptions.GetValue(option);
                parts.Remove(option);
            }

            // Screen aspect
            option = parts.FirstOrDefault(x => ScreenAspectOptions.Contains(x));
            if (option != null)
            {
                result.ScreenAspect = ScreenAspectOptions.GetValue(option);
                parts.Remove(option);
            }

            // Screen orientations
            option = parts.FirstOrDefault(x => ScreenOrientationOptions.Contains(x));
            if (option != null)
            {
                result.ScreenOrientation = ScreenOrientationOptions.GetValue(option);
                parts.Remove(option);
            }

            // UI Modes
            option = parts.FirstOrDefault(x => UIModeOptions.Contains(x));
            if (option != null)
            {
                result.UIMode = UIModeOptions.GetValue(option);
                parts.Remove(option);
            }

            // Night mode
            option = parts.FirstOrDefault(x => NightModeOptions.Contains(x));
            if (option != null)
            {
                result.NightMode = NightModeOptions.GetValue(option);
                parts.Remove(option);
            }

            // Screen pixel density
            option = parts.FirstOrDefault(x => ScreenPixelDensityOptions.Contains(x));
            if (option != null)
            {
                result.ScreenPixelDensity = ScreenPixelDensityOptions.GetValue(option);
                parts.Remove(option);
            }

            // Touch screen type
            option = parts.FirstOrDefault(x => TouchScreenTypeOptions.Contains(x));
            if (option != null)
            {
                result.TouchScreenType = TouchScreenTypeOptions.GetValue(option);
                parts.Remove(option);
            }

            // Keyboard availability
            option = parts.FirstOrDefault(x => KeyboardAvailabilityOptions.Contains(x));
            if (option != null)
            {
                result.KeyboardAvailability = KeyboardAvailabilityOptions.GetValue(option);
                parts.Remove(option);
            }

            // Primary text input method
            option = parts.FirstOrDefault(x => PrimaryTextInputMethodOptions.Contains(x));
            if (option != null)
            {
                result.PrimaryTextInputMethod = PrimaryTextInputMethodOptions.GetValue(option);
                parts.Remove(option);
            }

            // Navigation key availability
            option = parts.FirstOrDefault(x => NavigationKeyAvailabilityOptions.Contains(x));
            if (option != null)
            {
                result.NavigationKeyAvailability = NavigationKeyAvailabilityOptions.GetValue(option);
                parts.Remove(option);
            }

            // Primary non-touch navigation method
            option = parts.FirstOrDefault(x => PrimaryNonTouchNavigationMethodOptions.Contains(x));
            if (option != null)
            {
                result.PrimaryNonTouchNavigationMethod = PrimaryNonTouchNavigationMethodOptions.GetValue(option);
                parts.Remove(option);
            }

            // Platform version
            option = parts.FirstOrDefault(x => IsNumberQualifier(x, "v", null, out number));
            if (option != null)
            {
                result.PlatformVersion = number;
                parts.Remove(option);
            }

            return result;
        }

        /// <summary>
        /// Is the given a qualifier with a prefix, number and optional postfix?
        /// </summary>
        private static bool IsNumberQualifier(string value, string prefix, string postfix, out int numberValue)
        {
            numberValue = 0;
            if (!value.StartsWith(prefix))
                return false;
            value = value.Substring(prefix.Length);
            if (!string.IsNullOrEmpty(postfix))
            {
                // A postfix is needed
                if (!value.EndsWith(postfix))
                    return false;
                // Remove postfix
                value = value.Substring(0, value.Length - postfix.Length);
            }
            return (int.TryParse(value, out numberValue));
        }

        /// <summary>
        /// Remove configuration qualifiers from the given name.
        /// </summary>
        public static string StripQualifiers(string resourceFile, bool removeExtension, bool normalizeName)
        {
            if (normalizeName)
                resourceFile = resourceFile.ToLower();
            var name = GetFileNameWithoutExtension(resourceFile);
            // Strip of alternate resource stuff
            var index = name.IndexOf('-');
            if (index > 0)
                name = name.Substring(0, index);
            name = name.Replace('.', '_');
            if (removeExtension)
                return name;
            var ext = GetExtension(resourceFile);
            return name + ext;
        }

        /// <summary>
        /// Gets the resource filename of the given path.
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if ((name != null) && (name.EndsWith(".9")))
                name = name.Substring(0, name.Length - 2);
            return name;
        }

        /// <summary>
        /// Gets the resource extension of the given path.
        /// </summary>
        public static string GetExtension(string path)
        {
            var ext = Path.GetExtension(path) ?? string.Empty;
            var name = path.Substring(0, path.Length - ext.Length);
            if (name.EndsWith(".9"))
                ext = ".9" + ext;
            return ext;
        }
    }
}
