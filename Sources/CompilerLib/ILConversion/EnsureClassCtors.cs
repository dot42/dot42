using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Create static ctors for types that have reachable static enum and/or struct fields.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class EnsureClassCtors : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 45; }
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            /// <summary>
            /// Create static ctors for types that have reachable static enum and/or struct fields.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                foreach (var type in reachableContext.ReachableTypes.Where(NeedsClassCtor))
                {
                    EnsureClassCtor(reachableContext, type);
                }
            }

            /// <summary>
            /// Does the given type have any static field that is an enum?
            /// </summary>
            private static bool NeedsClassCtor(TypeDefinition type)
            {
                if (type.IsEnum) // Class ctor is already created automatically for enums
                    return false;
                return type.Fields.Any(x => x.IsStatic && IsEnumOrStruct(x.FieldType));
            }

            /// <summary>
            /// Is the given type an enum or struct?
            /// </summary>
            private static bool IsEnumOrStruct(TypeReference type)
            {
                if (type.IsPrimitive)
                    return false;
                if (!type.IsDefinitionOrReference())
                    return false;
                var typeDef = type.Resolve();
                return (typeDef != null) && (typeDef.IsEnum || typeDef.IsValueType);
            }

            /// <summary>
            /// Ensure there is a class ctor.
            /// </summary>
            private static void EnsureClassCtor(ReachableContext reachableContext, TypeDefinition type)
            {
                var ctor = type.GetClassCtor();
                if (ctor != null)
                    return; // Already exists

                // Create class ctor
                var typeSystem = type.Module.TypeSystem;
                ctor = new MethodDefinition(".cctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static | MethodAttributes.SpecialName, typeSystem.Void);
                ctor.DeclaringType = type;

                var body = new MethodBody(ctor);
                body.InitLocals = true;
                ctor.Body = body;

                // Prepare code
                var seq = new ILSequence();
                seq.Emit(OpCodes.Nop);
                seq.Emit(OpCodes.Ret);

                // Append ret sequence
                seq.AppendTo(body);

                // Update offsets
                body.ComputeOffsets();

                // Add ctor
                type.Methods.Add(ctor);
                ctor.SetReachable(reachableContext);
            }
        }
    }    
}