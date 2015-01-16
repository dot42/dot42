namespace Dot42.Ide.TypeConverters
{
    /// <summary>
    /// Type converter for android bool values
    /// </summary>
    internal class BoolTypeConverter : StringTypeConverter
    {
        public BoolTypeConverter()
            : base("true", "false")
        {
        }
    }
}
