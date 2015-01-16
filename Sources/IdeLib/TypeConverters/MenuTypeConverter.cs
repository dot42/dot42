namespace Dot42.Ide.TypeConverters
{
    /// <summary>
    /// Type converter for android:showAsAction
    /// </summary>
    internal class ShowAsActionTypeConverter : StringTypeConverter
    {
        public ShowAsActionTypeConverter()
            : base("ifRoom", "withText", "never", "always", "collapseActionView", null)
        {
        }
    }

    /// <summary>
    /// Type converter for android:menuCategory
    /// </summary>
    internal class MenuCategoryTypeConverter : StringTypeConverter
    {
        public MenuCategoryTypeConverter()
            : base("container", "system", "secondary", "alternative", null)
        {
        }
    }

    /// <summary>
    /// Type converter for android:checkableBehavior
    /// </summary>
    internal class CheckableBehaviorTypeConverter : StringTypeConverter
    {
        public CheckableBehaviorTypeConverter()
            : base("none", "all", "single", null)
        {
        }
    }
}
