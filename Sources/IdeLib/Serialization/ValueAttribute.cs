using System;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Specifies ths the value of a property is stored in the XML element's value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ValueAttribute : Attribute
    {
    }
}
