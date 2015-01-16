// Configuration related enums

using System;

namespace Dot42.ApkLib.Resources
{
    public enum Orientation
    {
        ANY = 0x0000,
        PORT = 0x0001,
        LAND = 0x0002,
        SQUARE = 0x0003,
    }

    public enum Touchscreen
    {
        ANY = 0x0000,
        NOTOUCH = 0x0001,
        STYLUS = 0x0002,
        FINGER = 0x0003,
    }

    public enum Density
    {
        DEFAULT = 0,
        LOW = 120,
        MEDIUM = 160,
        TV = 213,
        HIGH = 240,
        NONE = 0xffff,
    }

    public enum Keyboard
    {
        ANY = 0x0000,
        NOKEYS = 0x0001,
        QWERTY = 0x0002,
        _12KEY = 0x0003,
    }

    public enum Navigation
    {
        ANY = 0x0000,
        NONAV = 0x0001,
        DPAD = 0x0002,
        TRACKBALL = 0x0003,
        WHEEL = 0x0004,
    }

    public enum KeysHidden
    {
        ANY = 0x0000,
        NO = 0x0001,
        YES = 0x0002,
        SOFT = 0x0003,
    }

    public enum NavHidden
    {
        ANY = 0x0000,
        NO = 0x0001,
        YES = 0x0002,
    }

    public enum ScreenSize
    {
        ANY = 0x00,
        SMALL = 0x01,
        NORMAL = 0x02,
        LARGE = 0x03,
        XLARGE = 0x04,
    }

    public enum ScreenLong
    {
        ANY = 0x00,
        NO = 0x1,
        YES = 0x2,
    }

    public enum UiModeType
    {
        ANY = 0x00,
        NORMAL = 0x01,
        DESK = 0x02,
        CAR = 0x03,
        TELEVISION = 0x04,
    }

    public enum UiModeNight
    {
        ANY = 0x00,
        NO = 0x1,
        YES = 0x2,
    }

    /// <summary>
    /// Special values for Res_map.'name' when defining attribute resources.
    /// </summary>    
    public enum AttributeResourceTypes
    {
        // This entry holds the attribute's type code.
        ATTR_TYPE = 0x01000000 | 0,

        // For integral attributes, this is the minimum value it can hold.
        ATTR_MIN = 0x01000000 | 1,

        // For integral attributes, this is the maximum value it can hold.
        ATTR_MAX = 0x01000000 | 2,

        // Localization of this resource is can be encouraged or required with
        // an aapt flag if this is set
        ATTR_L10N = 0x01000000 | 3,

        // for plural support, see android.content.res.PluralRules#attrForQuantity(int)
        ATTR_OTHER = 0x01000000 | 4,
        ATTR_ZERO = 0x01000000 | 5,
        ATTR_ONE = 0x01000000 | 6,
        ATTR_TWO = 0x01000000 | 7,
        ATTR_FEW = 0x01000000 | 8,
        ATTR_MANY = 0x01000000 | 9,
    }

    /// <summary>
    /// Bit mask of allowed types, for use with ATTR_TYPE.
    /// </summary>
    [Flags]
    public enum AttributeTypes
    {
        // No type has been defined for this attribute, use generic
        // type handling.  The low 16 bits are for types that can be
        // handled generically; the upper 16 require additional information
        // in the bag so can not be handled generically for TYPE_ANY.
        TYPE_ANY = 0x0000FFFF,

        // Attribute holds a references to another resource.
        TYPE_REFERENCE = 1 << 0,

        // Attribute holds a generic string.
        TYPE_STRING = 1 << 1,

        // Attribute holds an integer value.  ATTR_MIN and ATTR_MIN can
        // optionally specify a constrained range of possible integer values.
        TYPE_INTEGER = 1 << 2,

        // Attribute holds a boolean integer.
        TYPE_BOOLEAN = 1 << 3,

        // Attribute holds a color value.
        TYPE_COLOR = 1 << 4,

        // Attribute holds a floating point value.
        TYPE_FLOAT = 1 << 5,

        // Attribute holds a dimension value, such as "20px".
        TYPE_DIMENSION = 1 << 6,

        // Attribute holds a fraction value, such as "20%".
        TYPE_FRACTION = 1 << 7,

        // Attribute holds an enumeration.  The enumeration values are
        // supplied as additional entries in the map.
        TYPE_ENUM = 1 << 16,

        // Attribute holds a bitmaks of flags.  The flag bit values are
        // supplied as additional entries in the map.
        TYPE_FLAGS = 1 << 17
    }


    /// <summary>
    /// Enum of localization modes, for use with ATTR_L10N.
    /// </summary>
    public enum LocalizationModes
    {
        L10N_NOT_REQUIRED = 0,
        L10N_SUGGESTED = 1
    }

}
