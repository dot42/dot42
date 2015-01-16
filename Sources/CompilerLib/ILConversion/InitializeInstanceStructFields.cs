using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Extend instance ctors for types that have reachable instance fields of type struct.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class InitializeInstanceStructFields : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 50; }
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
            /// Extend instance ctors for types that have reachable instance fields of type struct.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                foreach (var type in reachableContext.ReachableTypes)
                {
                    List<FieldDefinition> structFields;
                    if (HasInstanceStructFields(type, out structFields))
                    {
                        InitializeStructFields(reachableContext, type, structFields);
                    }
                }
            }

            /// <summary>
            /// Does the given type have any instance field that is a struct?
            /// </summary>
            private static bool HasInstanceStructFields(TypeDefinition type, out List<FieldDefinition> structFields)
            {
                structFields = null;
                if (type.IsEnum) // Class ctor is already created automatically for enums
                    return false;
                if (type.Name.StartsWith("<PrivateImplementationDetails>"))
                    return false;
                foreach (var field in type.Fields.Where(x => !x.IsStatic && x.IsReachable && StructFields.IsStructField(x)))
                {
                    if (structFields == null)
                        structFields = new List<FieldDefinition>();
                    structFields.Add(field);
                }
                return (structFields != null);
            }

            /// <summary>
            /// Make sure all struct fields are initialized in the ctors that need them.
            /// </summary>
            private static void InitializeStructFields(ReachableContext reachableContext, TypeDefinition type, List<FieldDefinition> structFields)
            {
                List<Tuple<MethodDefinition, HashSet<FieldDefinition>>> ctorsToExtend = null;
                foreach (var ctor in type.Methods.Where(x => x.IsConstructor && !x.IsStatic && x.HasBody))
                {
                    HashSet<FieldDefinition> storedFields;
                    if (!NeedsStructInitialization(ctor, structFields, out storedFields))
                        continue;

                    // Extend this ctor to initialize all struct fields.
                    if (ctorsToExtend == null) 
                        ctorsToExtend = new List<Tuple<MethodDefinition, HashSet<FieldDefinition>>>();
                    ctorsToExtend.Add(Tuple.Create(ctor, storedFields));
                }

                if (ctorsToExtend == null)
                {
                    // No ctors need additional initialization
                    return;
                }

                // Inject initialization code
                foreach (var tuple in ctorsToExtend)
                {
                    var ctor = tuple.Item1;
                    var storedFields = tuple.Item2;
                    InjectInitializationCode(ctor, structFields.Except(storedFields));
                }
            }

            /// <summary>
            /// Does the given ctor need it's struct initialized?
            /// </summary>
            private static bool NeedsStructInitialization(MethodDefinition ctor, List<FieldDefinition> structFields, out HashSet<FieldDefinition> storedFields)
            {
                var body = ctor.Body;
                // Find all store-field instructions
                storedFields = new HashSet<FieldDefinition>();
                foreach (var ins in body.Instructions)
                {
                    var code = ins.OpCode.Code;
                    if (code == Code.Stfld)
                    {
                        var fieldRef = (FieldReference) ins.Operand;
                        var fieldDef = fieldRef.Resolve();
                        if (fieldDef != null)
                        {
                            storedFields.Add(fieldDef);
                        }
                    }
                    else if (code == Code.Call)
                    {
                        var methodRef = (MethodReference) ins.Operand;
                        if (methodRef.Name == ".ctor")
                        {
                            var methodDef = methodRef.Resolve();
                            if ((methodDef != null) && (methodDef.DeclaringType == ctor.DeclaringType))
                            {
                                // ctor calls a "this" ctor
                                return false;
                            }
                        }
                    }
                }

                if (!structFields.Except(storedFields).Any())
                {
                    // All struct fields are stored to.
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Is the given instruction a call to a this or base class ctor.
            /// </summary>
            private static bool IsBaseOrThisCtorCall(MethodDefinition ctor, Instruction ins)
            {
                if (ins.OpCode.Code != Code.Call)
                    return false;
                var methodRef = (MethodReference) ins.Operand;
                if (methodRef.Name != ".ctor")
                    return false;
                var methodDef = methodRef.Resolve();
                if (methodDef == null)
                    return false;
                if (methodDef.DeclaringType == ctor.DeclaringType)
                    return true; // "this(..)" ctor call
                var baseType = ctor.DeclaringType.BaseType.GetElementType();
                if (baseType == null)
                    return false;
                return (baseType.Resolve() == methodDef.DeclaringType);
            }

            /// <summary>
            /// Inject initialization code to the given ctor
            /// </summary>
            private static void InjectInitializationCode(MethodDefinition ctor, IEnumerable<FieldDefinition> structFields)
            {
                // Create sequence
                var initSeq = StructFields.CreateInitializationCode(structFields, false);

                // Find location where to insert
                var body = ctor.Body;
                var baseCtorCall = body.Instructions.FirstOrDefault(x => IsBaseOrThisCtorCall(ctor, x));

                if (baseCtorCall != null)
                {
                    // Insert after call to base/this ctor
                    initSeq.InsertToAfter(baseCtorCall, body);
                }
                else
                {
                    // Insert at start of method
                    initSeq.InsertTo(0, body);
                }
            }
        }
    }    
}