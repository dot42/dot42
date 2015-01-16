using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    internal static class StructFields
    {
        /// <summary>
        /// Is the type of the given field a struct?
        /// </summary>
        internal static bool IsStructField(FieldDefinition field)
        {
            TypeDefinition typeDef;
            return IsStructField(field, out typeDef);
        }

        /// <summary>
        /// Is the type of the given field a struct?
        /// </summary>
        internal static bool IsStructField(FieldDefinition field, out TypeDefinition typeDef)
        {
            typeDef = null;
            var type = field.FieldType;
            if (type.IsGenericInstance)
                type = type.GetElementType();
            if (type.IsPrimitive)
                return false;
            if (type.IsNullableT())
                return false;
            if (!type.IsDefinitionOrReference())
                return false;
            typeDef = type.Resolve();
            return (typeDef != null) && (typeDef.IsValueType && !typeDef.IsEnum);
        }

        /// <summary>
        /// Ensure there is a class ctor.
        /// </summary>
        internal static ILSequence CreateInitializationCode(IEnumerable<FieldDefinition> structFields, bool isStatic)
        {
            // Prepare code
            var seq = new ILSequence();

            foreach (var field in structFields)
            {
                // Create default instance
                var fieldType = field.FieldType.Resolve();
                if (fieldType == null)
                {
                    throw new CompilerException(string.Format("Cannot resolve field type of field {0}", field.MemberFullName()));
                }
                var ctor = fieldType.Methods.FirstOrDefault(x => x.IsConstructor && !x.IsStatic && (x.Parameters.Count == 0));
                if (ctor == null)
                {
                    throw new CompilerException(string.Format("Cannot find default ctor for type {0}", field.DeclaringType.FullName));
                }
                var ctorRef = field.FieldType.CreateReference(ctor);
                if (isStatic)
                {
                    seq.Emit(OpCodes.Newobj, ctorRef);
                    seq.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    seq.Emit(OpCodes.Ldarg_0); // this
                    seq.Emit(OpCodes.Newobj, ctorRef);
                    seq.Emit(OpCodes.Stfld, field);
                }
            }

            return seq;
        }
    }
}
