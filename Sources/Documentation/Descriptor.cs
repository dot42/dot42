using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Dot42.Documentation
{
    public abstract class Descriptor : CecilFormat
    {
        public static string Create(FieldDefinition field)
        {
            return field.Name;
        }

        public static string Create(EventDefinition evt)
        {
            return evt.Name;
        }

        public static string Create(PropertyDefinition prop, bool full)
        {
            var sb = new StringBuilder();
            if (full)
                sb.Append(prop.Name);
            if (prop.HasParameters)
            {
                sb.Append('[');
                sb.Append(string.Join(", ", prop.Parameters.Select(x => Format(x, true))));
                sb.Append(']');
            }
            return sb.ToString();
        }

        public static string Create(MethodDefinition method, bool full)
        {
            var sb = new StringBuilder();
            if (full)
                sb.Append(method.IsConstructor ? method.DeclaringType.Name : method.Name);
            sb.Append(Format(method.GenericParameters));
            sb.Append('(');
            sb.Append(string.Join(", ", method.Parameters.Select(x => Format(x, true))));
            sb.Append(')');
            return sb.ToString();
        }

        public static string Format(ParameterDefinition parameter, bool includeGenericArguments)
        {
            return GetShortTypeName(parameter.ParameterType, true, includeGenericArguments);
        }

        public static string Format(ICollection<GenericParameter> genericParameters)
        {
            if (genericParameters.Count == 0)
                return string.Empty;
            return "<" + string.Join(", ", genericParameters.Select(x => x.Name)) + ">";
        }
    }
}
