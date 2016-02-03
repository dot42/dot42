using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Create default ctors for reachable struct types.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class StructDefaultCtorsAndDefaultField : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 40; }
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
            /// Create default ctors for reachable structs.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                // Collect all type
                var todoTypes = reachableContext.ReachableTypes.Where(x => x.IsValueType && !x.IsPrimitive && !x.IsEnum).ToList();
                if (todoTypes.Count == 0)
                    return;

                foreach (var type in todoTypes)
                {
                    if(!HasDefaultCtor(type))
                        CreateDefaultCtor(reachableContext, type);

                    if (StructFields.IsImmutableStruct(type) && !type.HasGenericParameters)
                        CreateDefaultField(reachableContext, type);
                }
            }

            /// <summary>
            /// Does the given type have a default ctor.
            /// </summary>
            private static bool HasDefaultCtor(TypeDefinition type)
            {
                return type.Methods.Any(x => x.IsConstructor && !x.IsStatic && (x.Parameters.Count == 0));
            }

            /// <summary>
            /// Create the Ctor
            /// </summary>
            private static void CreateDefaultCtor(ReachableContext reachableContext, TypeDefinition type)
            {
                var typeSystem = type.Module.TypeSystem;
                var ctor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeSystem.Void);
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

            private void CreateDefaultField(ReachableContext reachableContext, TypeDefinition type)
            {
                // create the field
                var field = new FieldDefinition(NameConstants.Struct.DefaultFieldName, FieldAttributes.InitOnly | FieldAttributes.Static | FieldAttributes.Public, type);
                type.Fields.Add(field);
                field.SetReachable(reachableContext, true);
            }
        }
    }    
}