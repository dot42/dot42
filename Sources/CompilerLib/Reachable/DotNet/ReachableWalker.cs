using System;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using Dot42.Utility;
using Mono.Cecil;
using Mono.Cecil.Cil;
using AstCodeUtil = Dot42.CompilerLib.IL2Ast.AstCodeUtil;
using ExceptionHandler = Mono.Cecil.Cil.ExceptionHandler;
using FieldDefinition = Mono.Cecil.FieldDefinition;
using MethodDefinition = Mono.Cecil.MethodDefinition;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.CompilerLib.Reachable.DotNet
{
    /// <summary>
    /// Helper used to find all reachable types.
    /// </summary>
    internal sealed class ReachableWalker 
    {
        /// <summary>
        /// Mark all children of the given assembly
        /// </summary>
        internal static void Walk(ReachableContext context, AssemblyDefinition assembly)
        {
            Walk(context, (ICustomAttributeProvider) assembly);
            foreach (ModuleDefinition mod in assembly.Modules)
            {
                Walk(context, (ICustomAttributeProvider) mod);
            }
        }

        /// <summary>
        /// Mark all children of the given member
        /// </summary>
        internal static void Walk(ReachableContext context, MemberReference member)
        {
            // Mark declaring type
            member.DeclaringType.MarkReachable(context);

            TypeReference typeRef;
            MethodReference methodRef;
            EventReference eventRef;
            FieldReference fieldRef;
            PropertyReference propertyRef;

            if ((typeRef = member as TypeReference) != null)
            {
                Walk(context, typeRef);
            }
            else if ((methodRef = member as MethodReference) != null)
            {
                Walk(context, methodRef);
            }
            else if ((eventRef = member as EventReference) != null)
            {
                Walk(context, eventRef);
            }
            else if ((fieldRef = member as FieldReference) != null)
            {
                Walk(context, fieldRef);
            }
            else if ((propertyRef = member as PropertyReference) != null)
            {
                Walk(context, propertyRef);
            }
        }

        /// <summary>
        /// Mark all base types and externally visible members reachable
        /// </summary>
        private static void Walk(ReachableContext context, TypeReference type)
        {
            // Generic parameters
            Walk(context, (IGenericParameterProvider)type);

            TypeDefinition typeDef;
            TypeSpecification typeSpec;
            GenericParameter genericParam;
            if ((typeDef = type as TypeDefinition) != null)
            {
                var isUsedInSerialization = typeDef.IsUsedInSerialization && !type.IsPrimitive && !typeDef.IsEnum 
                                            && !type.Namespace.StartsWith("System");

                // Mark base type reachable
                typeDef.BaseType.MarkReachable(context, isUsedInSerialization);

                // Mark declaring type reachable
                typeDef.DeclaringType.MarkReachable(context);

                // Mark implemented interfaces reachable
                if (typeDef.HasInterfaces)
                {
                    foreach (var intf in typeDef.Interfaces.Select(x => x.InterfaceType))
                    {
                        intf.MarkReachable(context);
                    }
                }

                // If this is an attribute, include related types
                if (typeDef.IsAttribute())
                {
                    GetDot42InternalType(context, "IAttribute").MarkReachable(context);
                    GetDot42InternalType(context, "IAttributes").MarkReachable(context);
                }
                else if (typeDef.IsEnum)
                {
                    var boxingType = GetDot42InternalType(context, "Boxing");
                    boxingType.Methods.Where(x => (x.Name == "UnboxInteger" || x.Name == "UnboxLong")).ForEach(x => x.MarkReachable(context));
                }

                // Default & class ctor
                typeDef.FindDefaultCtor().MarkReachable(context);
                typeDef.GetClassCtor().MarkReachable(context);

                // Visit externally visible members
                if (typeDef.HasEvents)
                {
                    foreach (var evt in typeDef.Events)
                    {
                        if ((!evt.IsReachable) && context.Include(evt))
                        {
                            evt.MarkReachable(context); 
                        }
                    }
                }
                if (typeDef.HasFields)
                {
                    foreach (var field in typeDef.Fields)
                    {
                        // only public fields, so we don't pull any compiler generated stuff.
                        var isSerializable = isUsedInSerialization && !field.IsStatic && field.IsPublic;

                        if ((!field.IsReachable && context.Include(field)) || (!field.IsUsedInSerialization && isSerializable))
                        {                                                               
                            field.MarkReachable(context, isUsedInSerialization); 
                        }
                    }
                }
                if (typeDef.HasMethods)
                {
                    foreach (var method in typeDef.Methods)
                    {
                        if ((!method.IsReachable) && context.Include(method))
                        {
                            method.MarkReachable(context); 
                        }
                    }
                }
                if (typeDef.HasProperties)
                {
                    foreach (var prop in typeDef.Properties)
                    {
                        var isSerializable = isUsedInSerialization && prop.HasThis;
                        if ((!prop.IsReachable && context.Include(prop) || (!prop.IsUsedInSerialization && isSerializable)))
                        {
                            prop.MarkReachable(context, isUsedInSerialization); 
                        }
                    }
                }

                // Custom attributes
                Walk(context, (ICustomAttributeProvider)typeDef);

                // Walk imported java classes
                CustomAttribute javaImportAttr;
                if ((javaImportAttr = typeDef.GetJavaImportAttribute()) != null)
                {
                    var javaClassName = (string)javaImportAttr.ConstructorArguments[0].Value;
                    ClassFile javaClass;
                    if (context.TryLoadClass(javaClassName, out javaClass))
                    {
                        javaClass.MarkReachable(context);
                    }
                }

                // Dex imported interfaces should have all their methods marked reachable.
                if (typeDef.IsInterface && typeDef.HasDexImportAttribute())
                {
                    typeDef.Methods.ForEach(x => x.MarkReachable(context));
                }

                // Record in context 
                context.RecordReachableType(typeDef);
            }
            else if ((typeSpec = type as TypeSpecification) != null)
            {
                // Element
                typeSpec.ElementType.MarkReachable(context, typeSpec.IsUsedInSerialization);

                // Generic instance
                GenericInstanceType git;
                FunctionPointerType fpt;
                RequiredModifierType reqModType;
                OptionalModifierType optModType;
                if ((git = typeSpec as GenericInstanceType) != null)
                {
                    if (git.ElementType.IsNullableT())
                    {
                        var typeofT = git.GenericArguments[0].Resolve(context);
                        if (typeofT != null && !typeofT.UsedInNullableT)
                        {
                            typeofT.UsedInNullableT = true;
                            DLog.Debug(DContext.CompilerAssemblyResolver, "found System.Nullable<{0}>", typeofT.FullName);
                        }
                    }
                    Walk(context, (IGenericInstance)git);
                }
                else if ((fpt = typeSpec as FunctionPointerType) != null)
                {
                    Walk(context, fpt.ReturnType);
                }
                else if ((reqModType = typeSpec as RequiredModifierType) != null)
                {
                    reqModType.ModifierType.MarkReachable(context);
                }
                else if ((optModType = typeSpec as OptionalModifierType) != null)
                {
                    optModType.ModifierType.MarkReachable(context);
                }
            }
            else if ((genericParam = type as GenericParameter) != null)
            {
                genericParam.IsSerializedParameter = genericParam.HasSerializedParameterAttribute();

                // Owner
                var owner = (MemberReference)genericParam.Owner;
                owner.MarkReachable(context);

                // Constraints
                if (genericParam.HasConstraints)
                {
                    foreach (TypeReference constraint in genericParam.Constraints)
                    {
                        constraint.MarkReachable(context);
                    }
                }
            }
            else
            {
                // Try to resolve
                type.Resolve(context).MarkReachable(context, type.IsUsedInSerialization);
            }
        }

        /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, EventReference evt)
        {
            // Type
            evt.EventType.MarkReachable(context);

            var evtDef = evt as EventDefinition;
            if (evtDef != null)
            {
                evtDef.AddMethod.MarkReachable(context);
                evtDef.RemoveMethod.MarkReachable(context);
                evtDef.InvokeMethod.MarkReachable(context);

                // Custom attributes
                Walk(context, (ICustomAttributeProvider)evtDef);
            }
            else
            {
                // Try to resolve
                evt.Resolve(context).MarkReachable(context);
            }
        }

        /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, FieldReference field)
        {
            // Field type
            field.FieldType.MarkReachable(context, field.IsUsedInSerialization);

            var fieldDef = field as FieldDefinition;
            if (fieldDef != null)
            {
                // Custom attributes
                Walk(context, (ICustomAttributeProvider)fieldDef);

                // Walk imported java classes
                CustomAttribute javaImportAttr;
                if ((javaImportAttr = fieldDef.GetJavaImportAttribute()) != null)
                {
                    string className;
                    string memberName;
                    string descriptor;
                    javaImportAttr.GetDexOrJavaImportNames(fieldDef, out memberName, out descriptor, out className);
                    ClassFile javaClass;
                    if (context.TryLoadClass(className, out javaClass))
                    {
                        var javaField = javaClass.Fields.FirstOrDefault(x => (x.Name == memberName) && (x.Descriptor == descriptor));
                        javaClass.MarkReachable(context);
                        javaField.MarkReachable(context);
                    }
                }
            }
            else
            {
                // Try to resolve
                field.Resolve(context).MarkReachable(context, field.IsUsedInSerialization);
            }
        }

        /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, MethodReference method)
        {
            method.ReturnType.MarkReachable(context);

            // All parameters
            if (method.HasParameters)
            {
                foreach (ParameterDefinition param in method.Parameters)
                {
                    Walk(context, (ParameterReference)param);
                }
            }

            // Generic parameters
            Walk(context, (IGenericParameterProvider)method);

            // Method definition?
            MethodDefinition methodDef;
            MethodSpecification methodSpec;
            if ((methodDef = method as MethodDefinition) != null)
            {
                // Code
                Walk(context, methodDef.Body);

                // Overrides
                foreach (MethodReference methodRef in methodDef.Overrides)
                {
                    methodRef.MarkReachable(context);
                }

                // Base methods
                if (methodDef.IsVirtual)
                {
                    MethodDefinition baseMethod;
                    if ((baseMethod = methodDef.GetBaseMethod()) != null)
                    {
                        if (context.Contains(baseMethod.DeclaringType))
                        {
                            baseMethod.MarkReachable(context);
                        }
                    }
                }

                // Custom attributes
                Walk(context, (ICustomAttributeProvider)methodDef);

                // Walk imported java classes
                CustomAttribute javaImportAttr;
                if ((javaImportAttr = methodDef.GetJavaImportAttribute()) != null)
                {
                    string className;
                    string memberName;
                    string descriptor;
                    javaImportAttr.GetDexOrJavaImportNames(methodDef, out memberName, out descriptor, out className);
                    ClassFile javaClass;
                    if (context.TryLoadClass(className, out javaClass))
                    {
                        var javaMethod = javaClass.Methods.FirstOrDefault(x => (x.Name == memberName) && (x.Descriptor == descriptor));
                        javaClass.MarkReachable(context);
                        javaMethod.MarkReachable(context);
                    }
                }

                // If this method is a property accessor, include the property also
                if (methodDef.SemanticsAttributes.HasFlag(MethodSemanticsAttributes.Getter))
                {
                    var prop = methodDef.DeclaringType.Properties.FirstOrDefault(x => x.GetMethod == methodDef);
                    prop.MarkReachable(context);
                }
                if (methodDef.SemanticsAttributes.HasFlag(MethodSemanticsAttributes.Setter))
                {
                    var prop = methodDef.DeclaringType.Properties.FirstOrDefault(x => x.SetMethod == methodDef);
                    prop.MarkReachable(context);
                }
            }
            else if ((methodSpec = method as MethodSpecification) != null)
            {
                // Method
                var elementRef = methodSpec.ElementMethod;
                elementRef.MarkReachable(context);

                // Generic arguments
                var gim = methodSpec as GenericInstanceMethod;
                if (gim != null)
                {
                    var elementDef = elementRef.Resolve();

                    for (int i = 0; i < gim.GenericArguments.Count; ++i)
                    {
                        bool isSerialized = elementDef.GenericParameters[i].IsSerializedParameter;
                        gim.GenericArguments[i].MarkReachable(context, isSerialized);
                    }
                }
            }
            else
            {
                // Try to resolve
                method.Resolve(context).MarkReachable(context);
            }
        }

                /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, MethodBody body)
        {
            if (body != null)
            {
                // Exception handlers
                if (body.HasExceptionHandlers)
                {
                    foreach (ExceptionHandler handler in body.ExceptionHandlers)
                    {
                        handler.CatchType.MarkReachable(context);
                    }
                }

                // Local variables
                if (body.HasVariables)
                {
                    foreach (VariableDefinition var in body.Variables)
                    {
                        var.VariableType.MarkReachable(context);
                    }
                }

                // Instructions
                if (body.Instructions.Count > 0)
                {
                    Instruction ins = body.Instructions[0];
                    while (ins != null)
                    {
                        object operand = ins.Operand;
                        if (operand != null)
                        {
                            MemberReference memberRef;
                            ParameterReference paramRef;
                            VariableReference varRef;
                            
                            var methodRef = operand as MethodReference;

                            if ((memberRef = operand as MemberReference) != null)
                            {
                                if (!ExcludeFromWalking(context, ins, memberRef))
                                {
                                    memberRef.MarkReachable(context);
                                }
                            }
                            else if ((paramRef = operand as ParameterReference) != null)
                            {
                                Walk(context, paramRef);
                            }
                            else if ((varRef = operand as VariableReference) != null)
                            {
                                varRef.VariableType.MarkReachable(context);
                            }

                            
                            if (methodRef != null)
                            {
                                foreach (var p in methodRef.Parameters)
                                {
                                    if (p.ParameterType.IsGenericParameterArray())
                                    {
                                        var boxingType = GetDot42InternalType(context, "Boxing");
                                        boxingType.Methods.Where(x => x.Name == "Box").ForEach(x => x.MarkReachable(context));
                                        boxingType.Methods.Where(x => x.Name == "UnboxTo").ForEach(x => x.MarkReachable(context));
                                        break;
                                    }
                                }
                                if (methodRef.ReturnType.IsGenericParameterArray())
                                {
                                    var boxingType = GetDot42InternalType(context, "Boxing");
                                    boxingType.Methods.Where(x => x.Name.StartsWith("Unbox") && (x.Parameters.Count == 1)).ForEach(x => x.MarkReachable(context));                                    
                                }

                                if (methodRef.DeclaringType.IsSystemThreadingInterlocked())
                                {
                                    var args = GetCallArguments(body, ins);
                                    var field = args[0].Operand as FieldReference;
                                    if (field != null)
                                    {
                                        field.Resolve().IsUsedInInterlocked = true;
                                    }
                                }

                                if (context.HasSerializedParameters(methodRef))
                                {
                                    var method = methodRef.Resolve();
                                    var args = GetCallArguments(body, ins);

                                    for (int i = 0; i < method.Parameters.Count; i++)
                                    {
                                        // ReSharper disable once PossibleInvalidOperationException
                                        if (!method.Parameters[i].IsSerializedParameter.Value)
                                            continue;

                                        MarkReachableForSerialization(context, args[i], body);
                                    }
                                }
                            }
                        }

                        // Handle special opcodes
                        switch (ins.OpCode.Code)
                        {
                            case Mono.Cecil.Cil.Code.Add_Ovf:
                            case Mono.Cecil.Cil.Code.Add_Ovf_Un:
                                GetDot42InternalType(context, "Checked").Methods.Where(x => x.Name == "Add").ForEach(x => x.MarkReachable(context));
                                break;
                            case Mono.Cecil.Cil.Code.Sub_Ovf:
                            case Mono.Cecil.Cil.Code.Sub_Ovf_Un:
                                GetDot42InternalType(context, "Checked").Methods.Where(x => x.Name == "Sub").ForEach(x => x.MarkReachable(context));
                                break;
                            case Mono.Cecil.Cil.Code.Mul_Ovf:
                            case Mono.Cecil.Cil.Code.Mul_Ovf_Un:
                                GetDot42InternalType(context, "Checked").Methods.Where(x => x.Name == "Mul").ForEach(x => x.MarkReachable(context));
                                break;
                            case Code.Unbox:
                            case Code.Unbox_Any:
                                GetDot42InternalType(context, "Boxing").Methods.Where(x => (x.Name.StartsWith("Unbox"))).ForEach(x => x.MarkReachable(context));
                                break;
                        }

                        ins = ins.Next;
                    }
                }
                //Debug.WriteLine(string.Format("Walk body took: {0}ms", sw.ElapsedMilliseconds));
            }
        }

        private static void MarkReachableForSerialization(ReachableContext context, Instruction arg, MethodBody body)
        {
            object operand = arg.Operand;
            AstCode astCode = (AstCode) arg.OpCode.Code;
            AstCodeUtil.ExpandMacro(ref astCode, ref operand, body);


            FieldReference fieldRef;
            MethodReference methodRef;
            TypeReference typeRef;
            VariableReference varRef;
            ParameterReference paramRef;

            if ((fieldRef = operand as FieldReference) != null)
            {
                fieldRef.FieldType.MarkReachable(context, true);
            }
            else if ((methodRef = operand as MethodReference) != null)
            {
                if (arg.OpCode == OpCodes.Newobj)
                    methodRef.DeclaringType.MarkReachable(context, true);
                else
                    methodRef.ReturnType.MarkReachable(context, true);
            }
            else if ((typeRef = operand as TypeReference) != null)
            {
                typeRef.SetReachable(context, true);
            }
            else if ((paramRef = operand as ParameterReference) != null)
            {
                paramRef.ParameterType.MarkReachable(context, true);
            }
            else if ((varRef = operand as VariableReference) != null)
            {
                varRef.VariableType.MarkReachable(context, true);
            }
            // don't know what to do!
        }

        /// <summary>
        /// Should the given member reference in the given instruction be excluded from being walked?
        /// </summary>
        private static bool ExcludeFromWalking(ReachableContext context, Instruction ins, MemberReference memberReference)
        {
            // Is the given member reference a reference to an array initializer field?
            MethodReference method;
            if (ins.OpCode == OpCodes.Ldtoken)
            {
                var fieldRef = memberReference as FieldReference;
                if (fieldRef == null)
                    return false;
                var nextIns = ins.Next;
                if (nextIns == null)
                    return false;
                if (nextIns.OpCode != OpCodes.Call)
                    return false;
                method = nextIns.Operand as MethodReference;
                if (method == null)
                    return false;
                return (method.Name == "InitializeArray") &&
                       (method.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers");
            }
            method = memberReference as MethodReference;
            if (method != null)
            {
                var methodDef = method.Resolve(context);
                if ((methodDef != null) && methodDef.HasDexNativeAttribute())
                {
                    return true;
                }
            }
            var field = memberReference as FieldReference;
            if (field != null)
            {
                var fieldDef = field.Resolve(context);
                if ((fieldDef != null) && fieldDef.HasResourceIdAttribute())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, PropertyReference prop)
        {
            prop.PropertyType.MarkReachable(context, prop.IsUsedInSerialization);

            // Parameters
            if (prop.Parameters.Count > 0)
            {
                foreach (ParameterDefinition param in prop.Parameters)
                {
                    Walk(context, (ParameterReference)param);
                }
            }

            var propDef = prop as PropertyDefinition;
            if (propDef != null)
            {
                // DO NOT AUTOMATICALLY MAKE GET and SET REACHABLE.
                // (but do so for attributes, so that they can 
                //  be properly stored and loaded)
                if (prop.DeclaringType.Resolve().IsAttribute())
                {
                    propDef.GetMethod.MarkReachable(context);
                    propDef.SetMethod.MarkReachable(context);
                }
                else if (prop.IsUsedInSerialization || propDef.IsUsedInSerialization)
                {
                    propDef.GetMethod.MarkReachable(context);
                    propDef.SetMethod.MarkReachable(context);
                }
                // Custom attributes
                Walk(context, (ICustomAttributeProvider)propDef);
            }
            else
            {
                // Try to resolve
                prop.Resolve(context).MarkReachable(context, prop.IsUsedInSerialization);
            }
        }

        /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, ParameterReference param)
        {
            param.ParameterType.MarkReachable(context);

            var paramDef = param as ParameterDefinition;
            if (paramDef != null)
            {
                var method = paramDef.Method as MemberReference;
                if (method != null)
                {
                    method.MarkReachable(context);
                }
                Walk(context, (ICustomAttributeProvider)paramDef);
            }
        }

        /// <summary>
        /// Mark all reachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, ICustomAttributeProvider provider)
        {
            if (provider.HasCustomAttributes)
            {
                foreach (var attr in provider.CustomAttributes)
                {
                    if (!attr.AttributeType.Resolve().HasIgnoreAttribute())
                    {
                        Walk(context, attr);
                    }
                }
            }
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, CustomAttribute attr)
        {
            // Attribute ctor
            MethodReference ctor = attr.Constructor;
            ctor.MarkReachable(context);
            TypeReference declType = ctor.DeclaringType;

            // Try to resolve attribute
            // Ctor parameters
            if (ctor.HasParameters)
            {
                int paramCount = ctor.Parameters.Count;
                for (int i = 0; i < paramCount; i++)
                {
                    if (ctor.Parameters[i].ParameterType.FullName == "System.Type")
                    {
                        var type = (TypeReference)attr.ConstructorArguments[i].Value;
                        type.MarkReachable(context);
                    }
                }
            }

            // Fields
            foreach (var arg in attr.Fields)
            {
                arg.Argument.Type.MarkReachable(context);
                declType.MarkFieldsReachable(arg.Name, context);
                if (arg.Argument.Type.FullName == "System.Type")
                {
                    var type = (TypeReference)arg.Argument.Value;
                    type.MarkReachable(context);
                }
            }

            // Properties
            foreach (var arg in attr.Properties)
            {
                arg.Argument.Type.MarkReachable(context);
                declType.MarkPropertiesReachable(arg.Name, context);
                if (arg.Argument.Type.FullName == "System.Type")
                {
                    var type = (TypeReference)arg.Argument.Value;
                    type.MarkReachable(context);
                }
            }
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, IGenericParameterProvider provider)
        {
            if (provider.HasGenericParameters)
            {
                // Include types needed for generics
                GetDot42InternalType(context, InternalConstants.TypeHelperName).MarkReachable(context);
                var providerAsType = provider as TypeDefinition;
                if ((providerAsType != null) && !providerAsType.IsStatic())
                {
                    GetDot42InternalType(context, InternalConstants.TypeReflectionInfoAnnotation).MarkReachable(context);
                    GetDot42InternalType(context, InternalConstants.GenericTypeParameterAnnotation).MarkReachable(context);
                    GetDot42InternalType(context, InternalConstants.GenericDefinitionAnnotation).MarkReachable(context);
                }
                if (provider is MethodDefinition)
                {
                    GetDot42InternalType(context, InternalConstants.GenericMethodParameterAnnotation).MarkReachable(context);
                    GetDot42InternalType(context, InternalConstants.GenericDefinitionAnnotation).MarkReachable(context);
                }

                // Mark parameters
                foreach (GenericParameter param in provider.GenericParameters)
                {
                    param.MarkReachable(context);
                }
            }
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, IGenericInstance instance)
        {
            if (instance.HasGenericArguments)
            {
                foreach (TypeReference typeRef in instance.GenericArguments)
                {
                    typeRef.MarkReachable(context);
                }
            }
        }

        /// <summary>
        /// Gets the type definition for a Dot42.Internal.x type where x is the given type name.
        /// </summary>
        private static TypeDefinition GetDot42InternalType(ReachableContext context, string typeName)
        {
            var typeRef = context.Compiler.GetDot42InternalType(typeName);
            return (TypeDefinition) typeRef.Resolve().OriginalTypeDefinition;
        }

        private static TypeDefinition GetDot42InternalType(ReachableContext context, string @namespace, string typeName)
        {
            var typeRef = context.Compiler.GetDot42InternalType(@namespace, typeName);
            return (TypeDefinition)typeRef.Resolve().OriginalTypeDefinition;
        }

        private static Instruction[] GetCallArguments(MethodBody body, Instruction callInstruction)
        {
            var ilseq = new ILSequence();
            foreach (var i in body.Instructions)
            {
                ilseq.Append(i);
                if (i == callInstruction)
                    break;
            }
            Instruction[] args = callInstruction.GetCallArguments(ilseq, false);
            return args;
        }
    }
}
