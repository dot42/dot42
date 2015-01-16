using System;
using System.Collections.Generic;
using System.Linq;

using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;

using Mono.Cecil;

using MethodDefinition = Dot42.DexLib.MethodDefinition;
using TypeReference = Dot42.DexLib.TypeReference;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for an annotation interface that is used to store the values of a custom attribute.
    /// </summary>
    internal static class AttributeAnnotationInterfaceBuilder 
    {
        /// <summary>
        /// Create an annotation interface.
        /// </summary>
        internal static AttributeAnnotationInterface Create(
            ISourceLocation sequencePoint,
            AssemblyCompiler compiler,
            DexTargetPackage targetPackage,
            TypeDefinition attributeType,
            ClassDefinition attributeClass)
        {
            // Create class
            ClassDefinition @interface = new ClassDefinition();
            @interface.Name = CreateAnnotationTypeName(attributeClass);
            @interface.Namespace = attributeClass.Namespace;
            @interface.AccessFlags = AccessFlags.Public | AccessFlags.Abstract | AccessFlags.Interface | AccessFlags.Annotation;
            @interface.Owner = attributeClass;
            attributeClass.InnerClasses.Add(@interface);

            // Set super class
            @interface.SuperClass = new ClassReference("java/lang/Object");

            // Implement Dot42.Internal.IAttribute
            @interface.Interfaces.Add(new ClassReference("java/lang/annotation/Annotation"));

            // Prepare result
            AttributeAnnotationInterface result = new AttributeAnnotationInterface(@interface);

            // Add methods from IAttribute
            XModel.XTypeDefinition baseIntfType = compiler.GetDot42InternalType("IAttribute").Resolve();
            foreach (XModel.XMethodDefinition imethod in baseIntfType.Methods)
            {
                if (imethod.Parameters.Count > 0) 
                    throw new CompilerException(string.Format("Invalid IAttribute method {0}", imethod));
                string methodName = NameConverter.GetConvertedName(imethod);
                TypeReference dfieldType = imethod.ReturnType.GetReference(targetPackage);
                MethodDefinition method = new MethodDefinition(@interface, methodName, new Prototype(dfieldType));
                method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                @interface.Methods.Add(method);                
            }

            // Add field mapping
            foreach (var field in attributeType.Fields.Where(x => x.IsReachable && x.IsPublic))
            {
                string methodName = CreateGetMethodName(NameConverter.GetConvertedName(field), result);
                TypeReference dfieldType = field.FieldType.GetReference(targetPackage, compiler.Module);
                MethodDefinition method = new MethodDefinition(@interface, methodName, new Prototype(dfieldType));
                method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                result.FieldToGetMethodMap.Add(field, method);
                @interface.Methods.Add(method);
            }

            // Add property mapping
            foreach (var property in attributeType.Properties.Where(x => x.IsReachable && (x.SetMethod != null) && (x.SetMethod.IsPublic) && x.SetMethod.IsReachable))
            {
                string methodName = CreateGetMethodName(NameConverter.GetConvertedName(property), result);
                TypeReference dpropType = property.PropertyType.GetReference(targetPackage, compiler.Module);
                MethodDefinition method = new MethodDefinition(@interface, methodName, new Prototype(dpropType));
                method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                result.PropertyToGetMethodMap.Add(property, method);
                @interface.Methods.Add(method);
            }

            // Add ctor mapping
            var argIndex = 0;
            foreach (var ctor in attributeType.Methods.Where(x => (x.Name == ".ctor") && x.IsReachable))
            {
                // Add methods for the ctor arguments
                List<MethodDefinition> paramGetMethods = new List<MethodDefinition>();
                foreach (ParameterDefinition p in ctor.Parameters)
                {
                    string methodName = CreateGetMethodName("c" + argIndex++, result);
                    TypeReference dparamType = p.ParameterType.GetReference(targetPackage, compiler.Module);
                    MethodDefinition method = new MethodDefinition(@interface, methodName, new Prototype(dparamType));
                    method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                    @interface.Methods.Add(method);
                    paramGetMethods.Add(method);
                }

                // Add a builder method
                MethodDefinition buildMethod = CreateBuildMethod(sequencePoint, ctor, paramGetMethods, compiler, targetPackage, attributeClass, result);
                result.CtorMap.Add(ctor, new AttributeCtorMapping(buildMethod, paramGetMethods));
            }

            // Create default values annotation
            Annotation defAnnotation = CreateDefaultAnnotation(result);
            result.AnnotationInterfaceClass.Annotations.Add(defAnnotation);

            return result;
        }

        /// <summary>
        /// Create a method definition for the builder method that builds a custom attribute from an annotation.
        /// </summary>
        private static MethodDefinition CreateBuildMethod(
            ISourceLocation seqp,
            Mono.Cecil.MethodDefinition ctor,
            List<MethodDefinition> paramGetMethods,
            AssemblyCompiler compiler,
            DexTargetPackage targetPackage,
            ClassDefinition attributeClass,
            AttributeAnnotationInterface mapping)
        {
            // Create method definition
            string name = CreateBuildMethodName(attributeClass);
            TypeReference attributeTypeRef = ctor.DeclaringType.GetReference(targetPackage, compiler.Module);
            MethodDefinition method = new MethodDefinition(attributeClass, name, new Prototype(attributeTypeRef, new Parameter(mapping.AnnotationInterfaceClass, "ann")));
            method.AccessFlags = AccessFlags.Public | AccessFlags.Static | AccessFlags.Synthetic;
            attributeClass.Methods.Add(method);

            // Create method body
            MethodBody body = new MethodBody(null);
            Register annotationReg = body.AllocateRegister(RCategory.Argument, RType.Object);
            //body.Instructions.Add(seqp, RCode.Check_cast, mapping.AnnotationInterfaceClass, annotationReg);

            // Allocate attribute
            Register attributeReg = body.AllocateRegister(RCategory.Temp, RType.Object);
            body.Instructions.Add(seqp, RCode.New_instance, attributeClass, attributeReg);

            // Get ctor arguments
            List<Register> ctorArgRegs = new List<Register>();
            foreach (MethodDefinition p in paramGetMethods)
            {
                TypeReference paramType = p.Prototype.ReturnType;
                Register[] valueRegs = CreateLoadValueSequence(seqp, body, paramType, annotationReg, p);
                ctorArgRegs.AddRange(valueRegs);
            }

            // Invoke ctor
            DexLib.MethodReference dctor = ctor.GetReference(targetPackage, compiler.Module);
            body.Instructions.Add(seqp, RCode.Invoke_direct, dctor, new[] { attributeReg }.Concat(ctorArgRegs).ToArray());

            // Get field values
            foreach (var fieldMap in mapping.FieldToGetMethodMap)
            {
                Mono.Cecil.FieldDefinition field = fieldMap.Key;
                MethodDefinition getter = fieldMap.Value;
                Register[] valueRegs = CreateLoadValueSequence(seqp, body, getter.Prototype.ReturnType, annotationReg, getter);
                DexLib.FieldReference dfield = field.GetReference(targetPackage, compiler.Module);
                XModel.XTypeReference xFieldType = XBuilder.AsTypeReference(compiler.Module, field.FieldType);
                body.Instructions.Add(seqp, xFieldType.IPut(), dfield, valueRegs[0], attributeReg);
            }

            // Get property values
            foreach (var propertyMap in mapping.PropertyToGetMethodMap)
            {
                PropertyDefinition property = propertyMap.Key;
                MethodDefinition getter = propertyMap.Value;
                Register[] valueRegs = CreateLoadValueSequence(seqp, body, getter.Prototype.ReturnType, annotationReg, getter);
                DexLib.MethodReference dmethod = property.SetMethod.GetReference(targetPackage, compiler.Module);
                XModel.XMethodDefinition xSetMethod = XBuilder.AsMethodDefinition(compiler.Module, property.SetMethod);
                body.Instructions.Add(seqp, xSetMethod.Invoke(xSetMethod, null), dmethod, new[] { attributeReg }.Concat(valueRegs).ToArray());
            }

            // Return attribute
            body.Instructions.Add(seqp, RCode.Return_object, attributeReg);

            // Register method body
            targetPackage.Record(new CompiledMethod() { DexMethod = method, RLBody = body });

            // Return method
            return method;
        }

        /// <summary>
        /// Create code to load a value from an annotation interface.
        /// </summary>
        /// <returns>The register(s) holding the value</returns>
        private static Register[] CreateLoadValueSequence(
            ISourceLocation seqp,
            MethodBody body,
            TypeReference valueType,
            Register annotationReg,
            MethodDefinition getter)
        {
            if (valueType.IsWide())
            {
                Tuple<Register, Register> regs = body.AllocateWideRegister(RCategory.Temp);
                body.Instructions.Add(seqp, RCode.Invoke_interface, getter, annotationReg);
                body.Instructions.Add(seqp, RCode.Move_result_wide, regs.Item1);
                return new[] { regs.Item1, regs.Item2 };
            }
            if (valueType is PrimitiveType)
            {
                Register reg = body.AllocateRegister(RCategory.Temp, RType.Value);
                body.Instructions.Add(seqp, RCode.Invoke_interface, getter, annotationReg);
                body.Instructions.Add(seqp, RCode.Move_result, reg);
                return new[] { reg };
            }
            else
            {
                Register reg = body.AllocateRegister(RCategory.Temp, RType.Object);
                body.Instructions.Add(seqp, RCode.Invoke_interface, getter, annotationReg);
                body.Instructions.Add(seqp, RCode.Move_result_object, reg);
                return new[] { reg };
            }
        }

        /// <summary>
        /// Create a typename for the annotation interface
        /// </summary>
        private static string CreateAnnotationTypeName(ClassDefinition owner)
        {
            var i = 1;
            while (true)
            {
                string name = owner.Name + "$annotation" + i++;
                if (owner.InnerClasses.Any(x => x.Name == name))
                    continue;
                return name;
            }
        }

        /// <summary>
        /// Create a unique name for the get method that gets the value of the given field/property/ctor argument.
        /// </summary>
        private static string CreateGetMethodName(string memberName, AttributeAnnotationInterface attributeAnnotationInterface)
        {
            string memberNameBase = memberName;
            int index = 0;
            while (true)
            {
                bool unique = attributeAnnotationInterface.FieldToGetMethodMap.Values.All(x => x.Name != memberName) &&
                              attributeAnnotationInterface.PropertyToGetMethodMap.Values.All(x => x.Name !=  memberName);
                if (unique)
                    return memberName;
                memberName = memberNameBase + (index++);
            }
        }

        /// <summary>
        /// Create a unique name for the build method that builds an attribute from an annotation interface.
        /// </summary>
        private static string CreateBuildMethodName(ClassDefinition owner)
        {
            int index = 0;
            while (true)
            {
                string name = "__build" + index++;
                if (owner.Methods.All(x => x.Name != name))
                    return name;
            }
        }

        /// <summary>
        /// Create an annotation with default values.
        /// </summary>
        private static Annotation CreateDefaultAnnotation(AttributeAnnotationInterface mapping)
        {
            // Create annotation
            Annotation annotation = new Annotation { Visibility = AnnotationVisibility.Runtime };
            annotation.Type = mapping.AnnotationInterfaceClass;

            // Add field default values
            foreach (KeyValuePair<Mono.Cecil.FieldDefinition, MethodDefinition> entry in mapping.FieldToGetMethodMap)
            {
                string name = entry.Value.Name;
                Mono.Cecil.TypeReference type = entry.Key.FieldType;
                annotation.Arguments.Add(new AnnotationArgument(name, GetDefaultValue(type)));
            }

            // Add property default values
            foreach (KeyValuePair<PropertyDefinition, MethodDefinition> entry in mapping.PropertyToGetMethodMap)
            {
                string name = entry.Value.Name;
                Mono.Cecil.TypeReference type = entry.Key.PropertyType;
                annotation.Arguments.Add(new AnnotationArgument(name, GetDefaultValue(type)));
            }

            // Add ctor argument default values
            foreach (KeyValuePair<Mono.Cecil.MethodDefinition,AttributeCtorMapping> entry in mapping.CtorMap)
            {
                Mono.Cecil.MethodDefinition ctor = entry.Key;
                for (int i = 0; i < ctor.Parameters.Count; i++)
                {
                    string name = entry.Value.ArgumentGetters[i].Name;
                    Mono.Cecil.TypeReference type = entry.Key.Parameters[i].ParameterType;
                    annotation.Arguments.Add(new AnnotationArgument(name, GetDefaultValue(type)));
                }
            }

            // Wrap it in a dalvik.annotation.AnnotationDefault
            Annotation defAnnotation = new Annotation { Visibility = AnnotationVisibility.System };
            defAnnotation.Type = new ClassReference("dalvik.annotation.AnnotationDefault");
            defAnnotation.Arguments.Add(new AnnotationArgument("value", annotation));
            return defAnnotation;
        }

        /// <summary>
        /// Gets the default value for any value of the given type.
        /// </summary>
        private static object GetDefaultValue(Mono.Cecil.TypeReference type)
        {
            if (type.IsByte()) return (byte)0;
            if (type.IsSByte()) return (sbyte)0;
            if (type.IsBoolean()) return false;
            if (type.IsChar()) return '\0';
            if (type.IsInt16()) return (short)0;
            if (type.IsInt32()) return 0;
            if (type.IsInt64()) return 0L;
            if (type.IsFloat()) return 0.0F;
            if (type.IsDouble()) return 0.0;
            return null;
        }
    }
}
