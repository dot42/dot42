using System.Reflection;

namespace Dot42.ResourcesLib
{
    public static class ResourceConstants
    {
#region Resource values

        /// <summary>
        /// Possible values for booleans
        /// </summary>
        public static readonly string[] BoolValues = new[] { "true", "false" };

        /// <summary>
        /// Possible values for plurals/@quantity
        /// </summary>
        public static readonly string[] PluralsQuantityValues = new[] { "zero", "one", "two", "few", "many", "other" };

#endregion

#region Menu values

        /// <summary>
        /// Possible values for menu/item/@menuCategory, menu/group/@menuCategory
        /// </summary>
        public static readonly string[] MenuCategoryValues = new[] { "container", "system", "secondary", "alternative" };

#endregion

    }
}
