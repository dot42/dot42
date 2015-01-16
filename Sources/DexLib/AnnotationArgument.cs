using System;
using System.Collections;
using System.Linq;
using System.Text;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public class AnnotationArgument : IEquatable<AnnotationArgument>
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public AnnotationArgument(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(":");
            if (Value is Array)
            {
                var valueAsArr = (Array) Value;
                var length = valueAsArr.Length;
                if (length == 0)
                {
                    builder.Append("[]");
                }
                else
                {
                    builder.Append("[ ");
                    for (var i = 0; i < length; i++)
                    {
                        if (i > 0)
                            builder.Append(", ");
                        builder.Append(valueAsArr.GetValue(i));
                    }
                    builder.Append(" ]");
                }
            }
            else
            {
                builder.Append(Value);
            }
            return builder.ToString();
        }

        public bool Equals(AnnotationArgument other)
        {
            return Name.Equals(other.Name)
                   && ValueFormat.GetFormat(Value).Equals(ValueFormat.GetFormat(other.Value))
                   &&
                   (((ValueFormat.GetFormat(Value) == ValueFormats.Array) &&
                     ArrayEquals(Value as Array, other.Value as Array)) || object.Equals(Value, other.Value));
        }

        internal static bool ArrayEquals(Array array1, Array array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
                if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                    return false;

            return true;
        }
    }
}