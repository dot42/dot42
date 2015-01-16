using System.Linq;

namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Validators for various resource values.
    /// </summary>
    partial class ResourceValidators
    {
        /// <summary>
        /// Validate an android menuCategory.
        /// </summary>
        public static bool ValidateMenuCategoryValue(ref string value, out string errorMessage)
        {
            value = (value ?? string.Empty);
            if (!ResourceConstants.MenuCategoryValues.Contains(value))
            {
                errorMessage = "Menu category value expected";
                return false;
            }
            errorMessage = null;
            return true;
        }
    }
}
