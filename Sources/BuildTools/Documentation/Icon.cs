using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal abstract class Icon : CecilFormat
    {
        public static string Create(FieldDefinition field)
        {
            var declaringType = field.DeclaringType;
            if (declaringType.IsEnum)
            {
                return "enumvalue";
            }

            if (field.IsPublic) return "field";
            if (field.IsAssembly) return "field internal";
            if (field.IsFamily) return "field protected";
            if (field.IsFamilyOrAssembly || field.IsFamilyAndAssembly) return "field protected_internal";
            return "field private";
        }

        public static string Create(EventDefinition evt)
        {
            var method = evt.InvokeMethod ?? evt.AddMethod ?? evt.RemoveMethod;
            if (method.IsPublic) return "event";
            if (method.IsAssembly) return "event internal";
            if (method.IsFamily) return "event protected";
            if (method.IsFamilyOrAssembly) return "event protected_internal";
            return "event private";
        }

        public static string Create(PropertyDefinition prop)
        {
            var getter = prop.GetMethod;
            var setter = prop.SetMethod;
            var method = getter ?? setter;
            if (method.IsPublic) return "property";
            if (method.IsAssembly) return "property internal";
            if (method.IsFamily) return "property protected";
            if (method.IsFamilyOrAssembly) return "property protected_internal";
            return "property private";
        }

        public static string Create(MethodDefinition method)
        {
            var prefix = method.IsConstructor ? "ctor" : "method";
            if (method.IsPublic) return prefix;
            if (method.IsAssembly) return prefix + " internal";
            if (method.IsFamily) return prefix + " protected";
            if (method.IsFamilyOrAssembly) return prefix + " protected_internal";
            return prefix + " private";
        }

        public static string Create(TypeDefinition type)
        {
            if (type.IsEnum) return "enum";
            if (type.IsInterface) return "interface";
            return "type";
        }
    }
}
