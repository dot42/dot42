namespace Dot42.ResourcesLib
{
    public enum ResourceType
    {
        // Dot42 custom types
        Unknown, // Must be first
        Manifest,
        // Folder types
        Animation,
        Drawable,
        Layout,
        Menu,
        Values,
        Xml,
        Raw,
        // ID types
        Attr,
        Bool,
        Color,
        Dimension,
        Id,
        Integer,
        IntegerArray,
        TypedArray,
        String,
        Style,
        Plural,
        StringArray,

        // Masks
        FirstIdType = Attr,
        LastIdType = StringArray
    }
}
