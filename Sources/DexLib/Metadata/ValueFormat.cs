using System;

namespace Dot42.DexLib.Metadata
{
    public static class ValueFormat
    {
        public static ValueFormats GetFormat(object value)
        {
            if (value is byte || value is sbyte)
                return ValueFormats.Byte;
            else if (value is short || value is ushort)
                return ValueFormats.Short;
            else if (value is char)
                return ValueFormats.Char;
            else if (value is int || value is uint)
                return ValueFormats.Int;
            else if (value is long || value is ulong)
                return ValueFormats.Long;
            else if (value is float)
                return ValueFormats.Float;
            else if (value is double)
                return ValueFormats.Double;
            else if (value is bool)
                return ValueFormats.Boolean;
            else if (value is string)
                return ValueFormats.String;
            else if (value is TypeReference)
                return ValueFormats.Type;
            else if (value is FieldReference)
            {
                if (value is FieldDefinition && (value as FieldDefinition).IsEnum)
                    return ValueFormats.Enum;

                return ValueFormats.Field;
            }
            else if (value is MethodReference)
                return ValueFormats.Method;
            else if (value is Array)
                return ValueFormats.Array;
            else if (value is Annotation)
                return ValueFormats.Annotation;
            else if (value == null)
                return ValueFormats.Null;
            else
                throw new ArgumentException("Unexpected format");
        }
    }
}