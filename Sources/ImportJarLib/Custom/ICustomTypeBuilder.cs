namespace Dot42.ImportJarLib.Custom
{
    public interface ICustomTypeBuilder
    {
        /// <summary>
        /// Get this object as type builder.
        /// </summary>
        TypeBuilder AsTypeBuilder();

        /// <summary>
        /// Gets the name of the generated type. 
        /// Used for sorting.
        /// </summary>
        string CustomTypeName { get; }
    }
}
