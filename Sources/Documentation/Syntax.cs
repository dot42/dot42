using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Dot42.Documentation
{
    public abstract class Syntax : CecilFormat
    {
        public static string Create(FieldDefinition field)
        {
            var sb = new StringBuilder();
            if (field.IsPublic) sb.Append("public ");
            else if (field.IsAssembly) sb.Append("internal ");
            else if (field.IsFamily) sb.Append("protected ");
            else if (field.IsFamilyOrAssembly || field.IsFamilyAndAssembly) sb.Append("protected internal ");
            else sb.Append("private ");

            if (field.IsLiteral) sb.Append("const ");
            else
            {
                if (field.IsStatic) sb.Append("static ");
                if (field.IsInitOnly) sb.Append("readonly ");
            }

            sb.Append(GetShortTypeName(field.FieldType));
            sb.Append(' ');
            sb.Append(field.Name);
            sb.Append(';');
            return sb.ToString();
        }

        public static string Create(EventDefinition evt)
        {
            var sb = new StringBuilder();
            var method = evt.InvokeMethod ?? evt.AddMethod ?? evt.RemoveMethod;
            if (method.IsPublic) sb.Append("public ");
            else if (method.IsAssembly) sb.Append("internal ");
            else if (method.IsFamily) sb.Append("protected ");
            else if (method.IsFamilyOrAssembly) sb.Append("protected internal ");
            else sb.Append("private ");

            if (method.IsStatic) sb.Append("static ");

            sb.Append(GetShortTypeName(evt.EventType));
            sb.Append(' ');
            sb.Append(evt.Name);
            sb.Append(';');
            return sb.ToString();
        }

        public static string Create(PropertyDefinition prop)
        {
            var sb = new StringBuilder();
            var getter = prop.GetMethod;
            var setter = prop.SetMethod;
            var method = getter ?? setter;
            if (method.IsPublic) sb.Append("public ");
            else if (method.IsAssembly) sb.Append("internal ");
            else if (method.IsFamily) sb.Append("protected ");
            else if (method.IsFamilyOrAssembly) sb.Append("protected internal ");
            else sb.Append("private ");

            if (method.IsStatic)
                sb.Append("static ");
            else if (method.IsAbstract)
                sb.Append("abstract ");
            else if (method.IsVirtual)
                sb.Append("virtual ");

            sb.Append(GetShortTypeName(prop.PropertyType));
            sb.Append(' ');
            sb.Append(prop.Name);
            if (prop.HasParameters)
            {
                sb.Append('[');
                sb.Append(string.Join(", ", prop.Parameters.Select(Format)));
                sb.Append(']');
            }
            sb.Append(" { ");
            if (getter != null)
                sb.Append("get; ");
            if (setter != null)
                sb.Append("set; ");
            sb.Append("};");
            return sb.ToString();
        }

        public static string Create(MethodDefinition method)
        {
            var sb = new StringBuilder();
            if (method.IsPublic) sb.Append("public ");
            else if (method.IsAssembly) sb.Append("internal ");
            else if (method.IsFamily) sb.Append("protected ");
            else if (method.IsFamilyOrAssembly) sb.Append("protected internal ");
            else sb.Append("private ");

            if (method.IsStatic)
                sb.Append("static ");
            else if (method.IsAbstract)
                sb.Append("abstract ");
            else if (method.IsVirtual)
                sb.Append("virtual ");

            if (!method.IsConstructor)
                sb.Append(GetShortTypeName(method.ReturnType));
            sb.Append(' ');
            sb.Append(method.IsConstructor ? method.DeclaringType.Name : method.Name);
            sb.Append(Format(method.GenericParameters));
            sb.Append('(');
            sb.Append(string.Join(", ", method.Parameters.Select(Format)));
            sb.Append(')');
            sb.Append(';');
            return sb.ToString();
        }

        private static string Format(ParameterDefinition parameter)
        {
            return string.Format("{0} {1}", GetShortTypeName(parameter.ParameterType), parameter.Name);
        }

        private static string Format(ICollection<GenericParameter> genericParameters)
        {
            if (genericParameters.Count == 0)
                return string.Empty;
            return "<" + string.Join(", ", genericParameters.Select(x => x.Name)) + ">";
        }
    }
}
