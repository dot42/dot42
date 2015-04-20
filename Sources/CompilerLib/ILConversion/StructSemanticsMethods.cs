using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Create $CopyFrom and $Clone for reachable struct types.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class StructSemanticsMethods : ILConverterFactory
    {

        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 100; }
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
                var todoTypes = reachableContext.ReachableTypes.Where(StructFields.IsNonNullableStruct).ToList();
                if (todoTypes.Count == 0)
                    return;

                foreach (var type in todoTypes)
                {
                    bool isImmutable = false;// StructFields.IsImmutableStruct(type);

                    if (!isImmutable)
                    {
                        // Create methods
                        var copyFromMethod = CreateCopyFromMethod(reachableContext, type);
                        CreateCloneMethod(reachableContext, type, copyFromMethod);
                    }
                    
                    // TODO: create Equals and GetHashCode methods, if they are not overwritten from object.
                    //       Or, alternatively, implement these method based on reflection in ValueType.
                }
            }

            /// <summary>
            /// Create a CopyFrom method.
            /// </summary>
            private static MethodDefinition CreateCopyFromMethod(ReachableContext reachableContext, TypeDefinition type)
            {
                var typeSystem = type.Module.TypeSystem;
                var method = new MethodDefinition(NameConstants.Struct.CopyFromMethodName, MethodAttributes.Public, type);
                var sourceParam = new ParameterDefinition(type);
                method.Parameters.Add(sourceParam);
                method.DeclaringType = type;

                var body = new MethodBody(method);
                body.InitLocals = true;
                method.Body = body;

                // Prepare code
                var seq = new ILSequence();
                seq.Emit(OpCodes.Nop);

                // Call base CopyFrom
                var baseType = (type.BaseType != null) ? type.BaseType.GetElementType().Resolve() : null;
                if ((baseType != null) && baseType.IsValueType && (baseType.FullName != "System.ValueType"))
                {
                    var baseMethod = new MethodReference(NameConstants.Struct.CopyFromMethodName, baseType, baseType) { HasThis = true };
                    baseMethod.Parameters.Add(new ParameterDefinition(baseType));
                    seq.Emit(OpCodes.Ldarg, sourceParam);
                    seq.Emit(OpCodes.Call, baseMethod);
                }

                // Copy all fields
                foreach (var field in type.Fields.Where(x => !x.IsStatic))
                {
                    TypeDefinition fieldTypeDef;
                    var isStructField = StructFields.IsStructField(field, out fieldTypeDef);

                    // Prepare for stfld
                    seq.Emit(OpCodes.Ldarg, body.ThisParameter);

                    // Load from source
                    seq.Emit(OpCodes.Ldarg, sourceParam);
                    seq.Emit(OpCodes.Ldfld, field);

                    // If struct, create clone
                    if (isStructField)
                    {
                        var cloneMethod = new MethodReference(NameConstants.Struct.CloneMethodName, fieldTypeDef, fieldTypeDef) { HasThis = true };
                        seq.Emit(OpCodes.Call, cloneMethod);
                    }

                    // Save in this
                    seq.Emit(OpCodes.Stfld, field);
                }

                // Return this
                seq.Emit(OpCodes.Ldarg, body.ThisParameter);
                seq.Emit(OpCodes.Ret);

                // Append ret sequence
                seq.AppendTo(body);

                // Update offsets
                body.ComputeOffsets();

                // Add method
                type.Methods.Add(method);
                method.SetReachable(reachableContext);

                return method;
            }        

            /// <summary>
            /// Create a Clone method.
            /// </summary>
            private static void CreateCloneMethod(ReachableContext reachableContext, TypeDefinition type, MethodDefinition copyFromMethod)
            {
                var method = new MethodDefinition(NameConstants.Struct.CloneMethodName, MethodAttributes.Public, type);
                method.DeclaringType = type;

                var body = new MethodBody(method);
                body.InitLocals = true;
                method.Body = body;

                // Prepare code
                var seq = new ILSequence();
                seq.Emit(OpCodes.Nop);

                // Create new instance
                var defaultCtor = CreateDefaultCtorRef(type);
                seq.Emit(OpCodes.Newobj, defaultCtor);

                // Call clone.CopyFrom
                seq.Emit(OpCodes.Dup);
                seq.Emit(OpCodes.Ldarg, body.ThisParameter);
                seq.Emit(OpCodes.Call, copyFromMethod);

                // Return clone
                seq.Emit(OpCodes.Ret);

                // Append ret sequence
                seq.AppendTo(body);

                // Update offsets
                body.ComputeOffsets();

                // Add method
                type.Methods.Add(method);
                method.SetReachable(reachableContext);
            }

            /// <summary>
            /// Find the default ctor for the given type and wrap it into a reference if type has generic parameters.
            /// </summary>
            private static MethodReference CreateDefaultCtorRef(TypeDefinition type)
            {
                // Get the ctor
                var ctor = type.Methods.FirstOrDefault(x => x.IsConstructor && !x.IsStatic && (x.Parameters.Count == 0));
                if (ctor == null)
                    throw new CompilerException(string.Format("Cannot find default ctor for type {0}", type.FullName));

                if (type.HasGenericParameters)
                {
                    // Create a reference to a generic instance
                    var declaringTypeGit = new GenericInstanceType(type);
                    foreach (var gp in type.GenericParameters)
                    {
                        declaringTypeGit.GenericArguments.Add(gp);
                    }
                    var ctorRef = new MethodReference(ctor.Name, ctor.ReturnType, declaringTypeGit);
                    ctorRef.HasThis = ctor.HasThis;
                    ctorRef.ExplicitThis = ctor.ExplicitThis;
                    // it is the default ctor, so no need to copy parameters
                    return ctorRef;
                }
                else
                {
                    // Use ctor directly
                    return ctor;
                }    
            }
        }
    }    
}