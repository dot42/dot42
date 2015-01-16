using System.Linq;

namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Validators for various resource values.
    /// </summary>
    public static partial class ResourceValidators
    {
        /// <summary>
        /// Validate an android resource id name.
        /// The name is the local part of the ID.
        /// </summary>
        public static bool ValidateIdNameValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty);
            if (string.IsNullOrEmpty(value))
            {
                value = "id_x";
                errorMessage = "Name cannot be empty";
                return false;
            }
            // TODO
            /*if (!AndroidConstants.BoolValues.Contains(value))
            {
                errorMessage = "'true' or 'false' expected";
                return false;
            }*/
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validate a full android resource id.
        /// </summary>
        public static bool ValidateFullIdValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty);
            // TODO
            /*if (!AndroidConstants.BoolValues.Contains(value))
            {
                errorMessage = "'true' or 'false' expected";
                return false;
            }*/
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validate an android bool value
        /// </summary>
        public static bool ValidateBoolValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty).ToLowerInvariant();
            if (!ResourceConstants.BoolValues.Contains(value))
            {
                errorMessage = "'true' or 'false' expected";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validate an android color value
        /// </summary>
        public static bool ValidateColorValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(value))
            {
                value = "#123";
                errorMessage = "Valid color value expected";
                return false;
            }
            if (value[0] != '#')
            {
                errorMessage = "Color value must start with '#'";
                return false;                
            }
            for (var i = 1; i < value.Length; i++)
            {
                if (!IsHexDigit(value[i]))
                {
                    errorMessage = "Color value must contain hex digits";
                    return false;
                }
            }
            switch (value.Length)
            {
                case 4:
                case 5:
                case 7:
                case 9:
                    break;
                default:
                    errorMessage = "Color value has invalid length";
                    return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validate an android integer value
        /// </summary>
        public static bool ValidateIntegerValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty).ToLowerInvariant();
            int x;
            if (!int.TryParse(value, out x))
            {
                errorMessage = "Integer expected";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validate an android plurals/@quantity value
        /// </summary>
        public static bool ValidatePluralsQuantityValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty).ToLowerInvariant();
            if (!ResourceConstants.PluralsQuantityValues.Contains(value))
            {
                errorMessage = "Quantity expected";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Is the given character a valid hex digit (0..9, a..f, A..F)
        /// </summary>
        private static bool IsHexDigit(char ch)
        {
            if (char.IsDigit(ch))
                return true;
            return ((ch >= 'a') && (ch <= 'f')) || ((ch >= 'A') && (ch <= 'F'));
        }
    }
}
