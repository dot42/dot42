using System;

namespace Dot42.DebuggerLib
{
    public static class Extensions
    {
        /// <summary>
        /// Is this a primivite value?
        /// If not it is an object reference.
        /// </summary>
        public static bool IsPrimitive(this Jdwp.Tag tag)
        {
            switch (tag)
            {
                case Jdwp.Tag.Array:
                case Jdwp.Tag.Object:
                case Jdwp.Tag.String:
                case Jdwp.Tag.Thread:
                case Jdwp.Tag.ThreadGroup:
                case Jdwp.Tag.ClassLoader:
                case Jdwp.Tag.ClassObject:
                    return false;
                case Jdwp.Tag.Byte:
                case Jdwp.Tag.Char:
                case Jdwp.Tag.Float:
                case Jdwp.Tag.Double:
                case Jdwp.Tag.Int:
                case Jdwp.Tag.Long:
                case Jdwp.Tag.Short:
                case Jdwp.Tag.Void:
                case Jdwp.Tag.Boolean:
                    return true;
                default:
                    throw new ArgumentException("Unknown tag " + (int) tag);
            }
        }
    }
}
