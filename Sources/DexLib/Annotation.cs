using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public class Annotation : IEquatable<Annotation>
    {
        public Annotation()
        {
            Arguments = new List<AnnotationArgument>();
        }

        public Annotation(ClassReference type, AnnotationVisibility visibility, params AnnotationArgument[] arguments)
        {
            Type = type;
            Visibility = visibility;
            Arguments = arguments.ToList();
        }

        public ClassReference Type { get; set; }
        public IList<AnnotationArgument> Arguments { get; private set; }
        public AnnotationVisibility Visibility { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Type);
            builder.Append("(");
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(Arguments[i]);
            }
            builder.Append(")");
            return builder.ToString();
        }

        #region " IEquatable "

        public bool Equals(Annotation other)
        {
            bool result = Type.Equals(other.Type) && Arguments.Count.Equals(other.Arguments.Count);
            if (result)
            {
                for (int i = 0; i < Arguments.Count; i++)
                    result = result && Arguments[i].Equals(other.Arguments[i]);
            }
            return result;
        }

        #endregion

        #region " Object "

        public override bool Equals(object obj)
        {
            if (obj is Annotation)
                return Equals(obj as Annotation);

            return false;
        }

        public override int GetHashCode()
        {
            var builder = new StringBuilder();
            builder.AppendLine(TypeDescriptor.Encode(Type));

            foreach (AnnotationArgument argument in Arguments)
            {
                builder.Append(String.Format("{0}=", argument.Name));
                if (ValueFormat.GetFormat(argument.Value) == ValueFormats.Array)
                {
                    var array = argument.Value as Array;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i > 0)
                            builder.Append(",");
                        builder.Append(array.GetValue(i));
                    }
                }
                else
                    builder.Append(argument.Value);
                builder.AppendLine();
            }

            return builder.ToString().GetHashCode();
        }

        #endregion
    }
}