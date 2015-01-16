namespace Dot42.ResourcesLib
{
    /// <summary>
    /// Format of a resource id.
    /// </summary>
    public enum ResourceIdFormat
    {
        /// <summary>
        /// Standard Android XML resource reference format.
        /// @[package:]type/name
        /// </summary>
        AndroidXml,

        /// <summary>
        /// AAPT Android XML resource reference format.
        /// @+type/name
        /// </summary>
        AndroidXmlWithCreate,
    }
}
