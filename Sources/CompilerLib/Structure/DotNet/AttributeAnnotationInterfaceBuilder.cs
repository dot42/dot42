using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using Mono.Cecil.Rocks;
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

            TypeDefinition currentType = attributeType;

            while (currentType != null && currentType.FullName != typeof(Attribute).FullName)
            {
                // Add field mapping
                foreach (var field in currentType.Fields.Where(x => x.IsReachable && x.IsPublic))
                {
                    string methodName = CreateGetMethodName(NameConverter.GetConvertedName(field), result);
                    MethodDefinition method = new MethodDefinition(@interface, methodName,
                                                        MakePrototype(field.FieldType, targetPackage, compiler.Module));
                    method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                    result.FieldToGetMethodMap.Add(field, method);
                    @interface.Methods.Add(method);
                }

                // Add property mapping
                foreach (var property in currentType.Properties.Where(
                                              x => x.IsReachable && (x.SetMethod != null) 
                                         && x.SetMethod.IsPublic && x.SetMethod.IsReachable))
                {
                    // ignore properties with same name [might be overriden]
                    if (result.PropertyToGetMethodMap.Keys.Any(k => k.Name == property.Name))
                        continue;

                    string methodName = CreateGetMethodName(NameConverter.GetConvertedName(property), result);
                    Mono.Cecil.TypeReference propType = property.PropertyType;

                    MethodDefinition method = new MethodDefinition(@interface, methodName,
                                                        MakePrototype(propType, targetPackage, compiler.Module));
                    method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                    result.PropertyToGetMethodMap.Add(property, method);
                    @interface.Methods.Add(method);
                }

                if (currentType.BaseType == null || currentType.BaseType.IsSystemObject())
                    break;

                currentType = currentType.BaseType.Resolve();
            }
            // Add ctor mapping
            var argIndex = 0;
            foreach (var ctor in attributeType.Methods.Where(x => (x.Name == ".ctor") && x.IsReachable))
            {
                // Add methods for the ctor arguments
                List<Tuple<MethodDefinition, Mono.Cecil.TypeReference>> paramGetMethods = new List<Tuple<MethodDefinition, Mono.Cecil.TypeReference>>();
                foreach (ParameterDefinition p in ctor.Parameters)
                {
                    string methodName = CreateGetMethodName("c" + argIndex++, result);
                    MethodDefinition method = new MethodDefinition(@interface, methodName, MakePrototype(p.ParameterType, targetPackage, compiler.Module));
                    method.AccessFlags = AccessFlags.Public | AccessFlags.Abstract;
                    @interface.Methods.Add(method);
                    paramGetMethods.Add(Tuple.Create(method, p.ParameterType));
                }

                // Add a builder method
                MethodDefinition buildMethod = CreateBuildMethod(sequencePoint, ctor, paramGetMethods, compiler, targetPackage, attributeClass, result);
                result.CtorMap.Add(ctor, new AttributeCtorMapping(buildMethod, paramGetMethods.Select(p=>p.Item1).ToList()));
            }

            // Create default values annotation
            Annotation defAnnotation = CreateDefaultAnnotation(result);
            result.AnnotationInterfaceClass.Annotations.Add(defAnnotation);

            return result;
        }

        private static Prototype MakePrototype(Mono.Cecil.TypeReference propType, DexTargetPackage targetPackage, XModule module)
        {
            // use object arrays.
            var type = new DexLib.ArrayType(module.TypeSystem.Object.GetReference(targetPackage));
            return new Prototype(type);

            // always use arrays.
            //var arrayType = propType.MakeArrayType().GetReference(targetPackage, module);
            //return new Prototype(arrayType);
        }

        /// <summary>
        /// Create a method definition for the builder method that builds a custom attribute from an annotation.
        /// </summary>
        private static MethodDefinition CreateBuildMethod(
            ISourceLocation seqp,
            Mono.Cecil.MethodDefinition ctor,
            List<Tuple<MethodDefinition, Mono.Cecil.TypeReference>> paramGetMethods,
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
            foreach (var p in paramGetMethods)
            {
                Instruction branchIfNotSet; // this can not happen, but lets keep the code below simple.
                
                XModel.XTypeReference xType = XBuilder.AsTypeReference(compiler.Module, p.Item2);

                Register[] valueRegs = CreateLoadValueSequence(seqp, body, xType, annotationReg, p.Item1, compiler, targetPackage, out branchIfNotSet);
                branchIfNotSet.Operand = body.Instructions.Add(seqp, RCode.Nop);

                ctorArgRegs.AddRange(valueRegs);
            }

            // Invoke ctor
            DexLib.MethodReference dctor = ctor.GetReference(targetPackage, compiler.Module);
            body.Instructions.Add(seqp, RCode.Invoke_direct, dctor, new[] { attributeReg }.Concat(ctorArgRegs).ToArray());

            // Get field values
            foreach (var fieldMap in mapping.FieldToGetMethodMap)
            {
                var field = fieldMap.Key;
                XModel.XTypeReference xFieldType = XBuilder.AsTypeReference(compiler.Module, field.FieldType);
                
                MethodDefinition getter = fieldMap.Value;
                Instruction branchIfNotSet;
                
                Register[] valueRegs = CreateLoadValueSequence(seqp, body, xFieldType, annotationReg, getter, compiler, targetPackage, out branchIfNotSet);
              
                var put = body.Instructions.Add(seqp, xFieldType.IPut(), null, valueRegs[0], attributeReg);
                
                mapping.FixOperands.Add(Tuple.Create(put, (MemberReference)field));

                branchIfNotSet.Operand = body.Instructions.Add(seqp, RCode.Nop);
            }

            // Get property values
            foreach (var propertyMap in mapping.PropertyToGetMethodMap)
            {
                PropertyDefinition property = propertyMap.Key;
                XTypeReference xType = XBuilder.AsTypeReference(compiler.Module, property.PropertyType);

                MethodDefinition getter = propertyMap.Value;
                Instruction branchIfNotSet;

                Register[] valueRegs = CreateLoadValueSequence(seqp, body, xType, annotationReg, getter, compiler, targetPackage, out branchIfNotSet);
                
                XModel.XMethodDefinition xSetMethod = XBuilder.AsMethodDefinition(compiler.Module, property.SetMethod);
                
                var set = body.Instructions.Add(seqp, xSetMethod.Invoke(xSetMethod, null), null, new[] { attributeReg }.Concat(valueRegs).ToArray());

                mapping.FixOperands.Add(Tuple.Create(set, (MemberReference)property.SetMethod));

                branchIfNotSet.Operand = body.Instructions.Add(seqp, RCode.Nop);
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
            XTypeReference valueType,
            Register annotationReg,
            MethodDefinition getter,
            AssemblyCompiler compiler,
            DexTargetPackage targetPackage,
            out Instruction branchIfNotSet)
        {
            // NOTE: It would be better if we wouldn't get the values as object arrays
            //       but as arrays of the actual type. 
            //       Apparently though the DexWriter will not write our attributes
            //       if they contain arrays not of type object[]. Therefore the 
            //       conversion code below.
            //       All in all it would be much cleaner if we could emit Ast code here
            //       instead of RL code.
            List<Register> result = new List<Register>();

            // get the array.
            Register regObject = body.AllocateRegister(RCategory.Temp, RType.Object);
            Register regIntVal = body.AllocateRegister(RCategory.Temp, RType.Value);
            
            body.Instructions.Add(seqp, RCode.Invoke_interface, getter, annotationReg);
            body.Instructions.Add(seqp, RCode.Move_result_object, regObject);

            // allocate result, initialize to default value.
            if (valueType.IsWide())
            {
                Tuple<Register, Register> regs = body.AllocateWideRegister(RCategory.Temp);
                body.Instructions.Add(seqp, RCode.Const_wide, 0, regs.Item1);
                result.Add(regs.Item1);
                result.Add(regs.Item2);
            }
            else if (valueType.IsPrimitive)
            {
                Register reg = body.AllocateRegister(RCategory.Temp, RType.Value);
                body.Instructions.Add(seqp, RCode.Const, 0, reg);
                result.Add(reg);
            }
            else // object 
            {
                Register reg = body.AllocateRegister(RCategory.Temp, RType.Object);
                body.Instructions.Add(seqp, RCode.Const, 0, reg);
                result.Add(reg);
            }

            // check if value is unset (array length 0) or null (array length 2)
            body.Instructions.Add(seqp, RCode.Array_length, regIntVal, regObject);
            branchIfNotSet = body.Instructions.Add(seqp, RCode.If_eqz, regIntVal);
            body.Instructions.Add(seqp, RCode.Rsub_int, 1, regIntVal, regIntVal);
            var branchOnNull = body.Instructions.Add(seqp, RCode.If_nez, regIntVal);

            // get the (boxed) value
            body.Instructions.Add(seqp, RCode.Const, 0, regIntVal);

            // convert to target type.
            if (valueType.IsArray)
            {
                Register regTmp = body.AllocateRegister(RCategory.Temp, RType.Object);
                Register regType = body.AllocateRegister(RCategory.Temp, RType.Object);
                
                var helper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName);
                var convertArray = helper.Resolve().Methods.First(p => p.Name == "ConvertArray" && p.Parameters.Count == 2)
                                         .GetReference(targetPackage);
                var underlying = valueType.ElementType.GetReference(targetPackage);

                body.Instructions.Add(seqp, RCode.Aget_object, regTmp, regObject, regIntVal);
                body.Instructions.Add(seqp, RCode.Const_class, underlying, regType);
                body.Instructions.Add(seqp, RCode.Invoke_static, convertArray, regTmp, regType);
                body.Instructions.Add(seqp, RCode.Move_result_object, result[0]);
                body.Instructions.Add(seqp, RCode.Check_cast, valueType.GetReference(targetPackage), result[0]);
            }
            else if (valueType.IsEnum())
            {
                Register regTmp = body.AllocateRegister(RCategory.Temp, RType.Object);
                Register regType = body.AllocateRegister(RCategory.Temp, RType.Object);
                
                var getFromObject = compiler.GetDot42InternalType("Enum").Resolve()
                                            .Methods.Single(p=>p.Name == "GetFromObject")
                                            .GetReference(targetPackage);

                body.Instructions.Add(seqp, RCode.Aget_object, regTmp, regObject, regIntVal);
                body.Instructions.Add(seqp, RCode.Const_class, valueType.GetReference(targetPackage), regType);
                body.Instructions.Add(seqp, RCode.Invoke_static, getFromObject, regType, regTmp);
                body.Instructions.Add(seqp, valueType.MoveResult(), result[0]);
                body.Instructions.Add(seqp, RCode.Check_cast, valueType.GetReference(targetPackage), result[0]);
            }
            else if(!valueType.IsPrimitive)
            {
                body.Instructions.Add(seqp, RCode.Aget_object, result[0], regObject, regIntVal);
                body.Instructions.Add(seqp, RCode.Check_cast, valueType.GetReference(targetPackage), result[0]);
            }
            else
            {
                Register regTmp = body.AllocateRegister(RCategory.Temp, RType.Object);
                // unbox and store
                RCode afterConvert;
                var unbox  = valueType.GetUnboxValueMethod(compiler, targetPackage, out afterConvert);
                body.Instructions.Add(seqp, RCode.Aget_object, regTmp, regObject, regIntVal);
                body.Instructions.Add(seqp, RCode.Invoke_static, unbox, regTmp);
                body.Instructions.Add(seqp, valueType.MoveResult(), result[0]);
                
                if (afterConvert != RCode.Nop)
                {
                    body.Instructions.Add(seqp, afterConvert, result[0], result[0]);
                }
            }

            // nop will be removed at some stage later.
            var nop = body.Instructions.Add(seqp, RCode.Nop);
            branchOnNull.Operand = nop;

            return result.ToArray();
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
            return new object[0];
        }
    }
}
