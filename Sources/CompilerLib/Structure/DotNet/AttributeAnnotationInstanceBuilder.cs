using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using FieldDefinition = Mono.Cecil.FieldDefinition;
using MethodDefinition = Dot42.DexLib.MethodDefinition;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build Annotation instances for attributes found in the given attribute provider.
    /// 
    /// Earlier attempts at this would encode the attributes initialization data (i.e.
    /// contructor parameters, field values, property values) as a specially generated
    /// annotation. This makes attributes appear to be similary handled to annotations.
    /// Due to the far greater capabilities of the CLR Attribute system in comparison to
    /// java annotations, a couple of ugly compatibility workarounds were neccessary. 
    /// These would work well under Dalvik. With the advent of Android ART this system 
    /// broke down, since ART appears to handle Annotations more stricly to the java 
    /// standard. Most notably, ART does not allow object[] arrays as annotation values,
    /// and for nested annotations, the exact type has to be given, in contrast to specifying
    /// 'Annotation'.
    /// 
    /// The current code therefore takes a differing approach. An Attribute is now identified
    /// by an annotation specifying the attribute type and a builder method. The builder
    /// method constructs a fully initialized instance of the attribute. For each 
    /// variant of attribute parameters, there is a unique builder method.
    /// 
    /// A question arisies as to where to put the builder method. Logically, a builder method
    /// could be placed in the class bearing the attribute. We want to allow to share 
    /// builder methods that construct the very same instance of an attributes. Therefore,
    /// at the moment, builder methods are placed in the corresponding attribute class.
    /// </summary>
    internal static class AttributeAnnotationInstanceBuilder
    {
        /// <summary>
        /// Initializes a mapping.
        /// </summary>
        public static AttributeAnnotationMapping CreateMapping(
            ISourceLocation sequencePoint,
            AssemblyCompiler compiler,
            DexTargetPackage targetPackage,
            TypeDefinition attributeType,
            ClassDefinition attributeClass)
        {
            return new AttributeAnnotationMapping(attributeType, attributeClass);
        }


        /// <summary>
        /// Create annotations for all included attributes
        /// </summary>
        public static void CreateAttributeAnnotations(AssemblyCompiler compiler, ICustomAttributeProvider attributeProvider,
                                  IAnnotationProvider annotationProvider, DexTargetPackage targetPackage, bool customAttributesOnly = false)
        {
            if (!attributeProvider.HasCustomAttributes)
                return;

            var annotations = new List<Annotation>();
            foreach (var attr in attributeProvider.CustomAttributes)
            {
                var attributeType = attr.AttributeType.Resolve();
                if (!attributeType.HasIgnoreAttribute())
                {
                    CreateAttributeAnnotation(compiler, attr, attributeType, annotations, targetPackage);
                }
            }
            if (annotations.Count > 0)
            {
                // Create 1 IAttributes annotation
                var attrsAnnotation = new Annotation { Visibility = AnnotationVisibility.Runtime };
                attrsAnnotation.Type = compiler.GetDot42InternalType("IAttributes").GetClassReference(targetPackage);
                attrsAnnotation.Arguments.Add(new AnnotationArgument("Attributes", annotations.ToArray()));
                annotationProvider.Annotations.Add(attrsAnnotation);
            }

            if (!customAttributesOnly)
            {
                // Add annotations specified using AnnotationAttribute
                foreach (var attr in attributeProvider.CustomAttributes.Where(IsAnnotationAttribute))
                {
                    var annotationType = (TypeReference) attr.ConstructorArguments[0].Value;
                    var annotationClass = annotationType.GetClassReference(targetPackage, compiler.Module);
                    annotationProvider.Annotations.Add(new Annotation(annotationClass, AnnotationVisibility.Runtime));
                }
            }
        }

        /// <summary>
        /// Create an annotation for the given attribute
        /// </summary>
        private static void CreateAttributeAnnotation(AssemblyCompiler compiler, CustomAttribute attribute, TypeDefinition attributeType,
                                   List<Annotation> annotationList, DexTargetPackage targetPackage)
        {
            // Gets the mapping for the type of attribute
            var mapping = compiler.GetAttributeAnnotationMapping(attributeType);

            MethodDefinition factoryMethod;

            // Note: not multithreading capable. see my comments elsewhere.
            if(mapping.FactoryMethodMap.ContainsKey(attribute))
                factoryMethod = mapping.FactoryMethodMap[attribute];
            else
            {
                // create the factory method.
                factoryMethod = CreateFactoryMethod(compiler, targetPackage, attribute, mapping);
                mapping.FactoryMethodMap[attribute] = factoryMethod;
            }

            // Create attribute annotation
            var attrAnnotation = new Annotation { Visibility = AnnotationVisibility.Runtime };
            attrAnnotation.Type = compiler.GetDot42InternalType("IAttribute").GetClassReference(targetPackage);
            attrAnnotation.Arguments.Add(new AnnotationArgument("AttributeType", attributeType.GetReference(targetPackage, compiler.Module)));
            attrAnnotation.Arguments.Add(new AnnotationArgument("FactoryMethod", factoryMethod.Name));

            // Add annotation
            annotationList.Add(attrAnnotation);
        }

        private static MethodDefinition CreateFactoryMethod(AssemblyCompiler compiler, DexTargetPackage targetPackage, CustomAttribute attribute, AttributeAnnotationMapping mapping)
        {
            var targetClass = mapping.AttributeClass; // is this really the right place for the factory methods?
            ISourceLocation seqp = null;
            var attributeTypeDef = attribute.AttributeType.Resolve();

            // create method
            string methodName = CreateAttributeFactoryMethodName(targetClass); 
            MethodDefinition method = new MethodDefinition(targetClass, methodName, new Prototype(mapping.AttributeClass));
            method.AccessFlags = AccessFlags.Public | AccessFlags.Static | AccessFlags.Synthetic;
            targetClass.Methods.Add(method);

            // create method body
            MethodBody body = new MethodBody(null);
            // Allocate attribute
            Register attributeReg = body.AllocateRegister(RCategory.Temp, RType.Object);
            body.Instructions.Add(seqp, RCode.New_instance, mapping.AttributeClass, attributeReg);

            // collect ctor arguments
            List<Register> ctorArgRegs = new List<Register>() { attributeReg };
            foreach (var p in attribute.ConstructorArguments)
            {
                XTypeReference xType = XBuilder.AsTypeReference(compiler.Module, p.Type);
                Register[] valueRegs = CreateInitializeValueInstructions(seqp, body, xType, p, compiler, targetPackage);
                ctorArgRegs.AddRange(valueRegs);
            }
            // Invoke ctor
            DexLib.MethodReference dctor = attribute.Constructor.GetReference(targetPackage, compiler.Module);
            body.Instructions.Add(seqp, RCode.Invoke_direct, dctor, ctorArgRegs.ToArray());

            // set field values
            foreach (var p in attribute.Fields)
            {
                var field = GetField(attributeTypeDef, p.Name);
                var xField = XBuilder.AsFieldReference(compiler.Module, field);

                Register[] valueRegs = CreateInitializeValueInstructions(seqp, body, xField.FieldType, p.Argument, compiler, targetPackage);
                body.Instructions.Add(seqp, xField.FieldType.IPut(), xField.GetReference(targetPackage),
                                                valueRegs[0], attributeReg);
            }

            // set property values
            foreach (var p in attribute.Properties)
            {
                PropertyDefinition property = GetSettableProperty(attributeTypeDef, p.Name);
                XTypeReference xType = XBuilder.AsTypeReference(compiler.Module, property.PropertyType);

                Register[] valueRegs = CreateInitializeValueInstructions(seqp, body, xType, p.Argument, compiler, targetPackage);
                XMethodDefinition xSetMethod = XBuilder.AsMethodDefinition(compiler.Module, property.SetMethod);
                body.Instructions.Add(seqp, xSetMethod.Invoke(xSetMethod, null), 
                                            xSetMethod.GetReference(targetPackage), 
                                            new[] { attributeReg }.Concat(valueRegs).ToArray());
            }

            // Return attribute
            body.Instructions.Add(seqp, RCode.Return_object, attributeReg);

            // Register method body
            targetPackage.Record(new CompiledMethod() { DexMethod = method, RLBody = body });

            // Return method
            return method;

        }

        private static FieldDefinition GetField(TypeDefinition attributeType, string name)
        {
            TypeDefinition type = attributeType;
            while (type != null)
            {
                var field = type.Fields.FirstOrDefault(f => f.Name == name);
                if (field != null) return field;
                type = type.BaseType.Resolve();

            }
            throw new Exception("field not found on type " + attributeType.FullName + ": " + name);
        }

        /// <summary>
        /// will return the first property with the given name and a public, nonstatic setter.
        /// </summary>
        private static PropertyDefinition GetSettableProperty(TypeDefinition attributeType, string name)
        {
            TypeDefinition type = attributeType;
            while (type != null)
            {
                var prop = type.Properties.FirstOrDefault(f => f.Name == name && f.SetMethod != null && f.SetMethod.IsPublic && !f.SetMethod.IsStatic);
                if (prop != null) return prop;
                type = type.BaseType.Resolve();

            }
            throw new Exception("property not found on type "  + attributeType.FullName + ": " + name);
        }

        /// <summary>
        /// Create code to initialize a value from an attribute.
        /// </summary>
        /// <returns>The register(s) holding the value</returns>
        private static Register[] CreateInitializeValueInstructions(ISourceLocation seqp, MethodBody body, XTypeReference targetType, CustomAttributeArgument value, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            List<Register> result = new List<Register>();

            // allocate result, initialize to default value.
            if (targetType.IsWide())
            {
                Tuple<Register, Register> regs = body.AllocateWideRegister(RCategory.Temp);
                //body.Instructions.Add(seqp, RCode.Const_wide, 0, regs.Item1);
                result.Add(regs.Item1);
                result.Add(regs.Item2);
            }
            else if (targetType.IsPrimitive)
            {
                Register reg = body.AllocateRegister(RCategory.Temp, RType.Value);
                //body.Instructions.Add(seqp, RCode.Const, 0, reg);
                result.Add(reg);
            }
            else // object 
            {
                Register reg = body.AllocateRegister(RCategory.Temp, RType.Object);
                //body.Instructions.Add(seqp, RCode.Const, 0, reg);
                result.Add(reg);
            }

            // load data

            if (value.Value == null) // must be a reference type
            {
                body.Instructions.Add(seqp, RCode.Const, 0, result[0]);
                body.Instructions.Add(seqp, RCode.Check_cast, targetType.GetReference(targetPackage), result[0]);
                return result.ToArray();
            }

            var valueType = XBuilder.AsTypeReference(compiler.Module, value.Type);

            if (value.Value is CustomAttributeArgument)
            {
                // this happens if a type conversion is neccessary
                var nestedValue = (CustomAttributeArgument)value.Value;
                valueType = XBuilder.AsTypeReference(compiler.Module, nestedValue.Type);

                var rOrigValue = CreateInitializeValueInstructions(seqp, body, valueType, nestedValue, compiler, targetPackage);

                if (!nestedValue.Type.IsPrimitive)
                {
                    body.Instructions.Add(seqp, RCode.Move_object, result[0], rOrigValue[0]);
                    body.Instructions.Add(seqp, RCode.Check_cast, targetType.GetReference(targetPackage), result[0]);
                }
                else if(!targetType.IsPrimitive)
                {
                    body.Instructions.Add(seqp, RCode.Invoke_static, valueType.GetBoxValueOfMethod(), rOrigValue);
                    body.Instructions.Add(seqp, RCode.Move_result_object, result[0]);
                    body.Instructions.Add(seqp, RCode.Check_cast, targetType.GetReference(targetPackage), result[0]);
                }
                else
                {
                    throw new Exception(string.Format("type converstion in attribute {0}=>{1} not yet supported", valueType.FullName, targetType.FullName));
                }
            }
            else if (valueType.IsArray)
            {
                var array = (CustomAttributeArgument[])value.Value;
                var elementType = valueType.ElementType;

                Register rIndex = body.AllocateRegister(RCategory.Temp, RType.Value);
                body.Instructions.Add(seqp, RCode.Const, array.Length, rIndex);
                body.Instructions.Add(seqp, RCode.New_array, valueType.GetReference(targetPackage), result[0], rIndex);

                // iterate through each value
                for (int i = 0; i < array.Length; i++)
                {
                    Register rLoaded = CreateInitializeValueInstructions(seqp, body, elementType, array[i], compiler, targetPackage)[0];

                    body.Instructions.Add(seqp, RCode.Const, i, rIndex);
                    body.Instructions.Add(seqp, valueType.APut(), rLoaded, result[0], rIndex);
                }
            }
            else if (targetType.IsEnum())
            {
                var enumClass = (targetType.IsEnum()? targetType:valueType).GetReference(targetPackage) ;

                Register rEnumClass = body.AllocateRegister(RCategory.Temp, RType.Object);
                body.Instructions.Add(seqp, RCode.Const_class, enumClass, rEnumClass);

                long lVal = Convert.ToInt64(value.Value);
                if (lVal <= int.MaxValue && lVal >= int.MinValue)
                {
                    Register regTmp = body.AllocateRegister(RCategory.Temp, RType.Value);    
                    body.Instructions.Add(seqp, RCode.Const, (int)lVal, regTmp);
                    var get = compiler.GetDot42InternalType("Enum").Resolve()
                                      .Methods.Single(p => p.Name == "Get" && p.Parameters.Count == 2 && !p.Parameters[1].ParameterType.IsWide())
                                      .GetReference(targetPackage);
                    body.Instructions.Add(seqp, RCode.Invoke_static, get, rEnumClass, regTmp);
                    body.Instructions.Add(seqp, targetType.MoveResult(), result[0]);
                }
                else
                {
                    var regTmp = body.AllocateWideRegister(RCategory.Temp);
                    body.Instructions.Add(seqp, RCode.Const, (long)lVal, regTmp.Item1);
                    var get = compiler.GetDot42InternalType("Enum").Resolve()
                                      .Methods.Single(p => p.Name == "Get" && p.Parameters.Count == 2 && p.Parameters[1].ParameterType.IsWide())
                                      .GetReference(targetPackage);
                    body.Instructions.Add(seqp, RCode.Invoke_static, get, rEnumClass, regTmp.Item1);
                    body.Instructions.Add(seqp, targetType.MoveResult(), result[0]);
                }
                body.Instructions.Add(seqp, RCode.Check_cast, targetType.GetReference(targetPackage), result[0]);
            }
            else if (valueType.IsSystemString())
            {
                body.Instructions.Add(seqp, RCode.Const_string, (string)value.Value, result[0]);
            }
            else if (valueType.IsSystemType())
            {
                var type = XBuilder.AsTypeReference(compiler.Module, (TypeReference)value.Value);
                // TODO: this might not work with typeof(void) on ART runtime.
                body.Instructions.Add(seqp, RCode.Const_class, type.GetReference(targetPackage), result[0]);
            }
            else if (!valueType.IsPrimitive)
            {
                // can this happen?
                throw new Exception("invalid value type in attribute: " + targetType.FullName);
            }
            else
            {
                if (targetType.IsSystemObject())
                {
                    // can this happen? or is this always handled above?

                    // boxing required.
                    var rUnboxed = CreateInitializeValueInstructions(seqp, body, valueType, value, compiler, targetPackage);
                    body.Instructions.Add(seqp, RCode.Invoke_static, valueType.GetBoxValueOfMethod(), rUnboxed);
                    body.Instructions.Add(seqp, RCode.Move_result_object, result[0]);
                }
                else if(targetType.IsDouble())
                {
                    body.Instructions.Add(seqp, RCode.Const_wide, Convert.ToDouble(value.Value), result[0]);
                }
                else if (targetType.IsWide() && valueType.IsUInt64())
                {
                    body.Instructions.Add(seqp, RCode.Const_wide, (long)Convert.ToUInt64(value.Value), result[0]);
                }
                else if (targetType.IsWide())
                {
                    body.Instructions.Add(seqp, RCode.Const_wide, Convert.ToInt64(value.Value), result[0]);
                }
                else if (targetType.IsFloat())
                {
                    body.Instructions.Add(seqp, RCode.Const, Convert.ToSingle(value.Value), result[0]);
                }
                else 
                {
                    body.Instructions.Add(seqp, RCode.Const, (int)Convert.ToInt64(value.Value), result[0]);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Create a unique name for the build method that builds an attribute from an annotation interface.
        /// </summary>
        private static string CreateAttributeFactoryMethodName(ClassDefinition owner)
        {
            int index = 0;
            while (true)
            {
                string name = "__createInstance" + index++;
                if (owner.Methods.All(x => x.Name != name))
                    return name;
            }
        }

        /// <summary>
        /// Is the given custom attribute of type Dot42.AnnotationAttribute?
        /// </summary>
        private static bool IsAnnotationAttribute(CustomAttribute ca)
        {
            return (ca.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                   (ca.AttributeType.Name == AttributeConstants.AnnotationAttributeName);
        }
    }
}
