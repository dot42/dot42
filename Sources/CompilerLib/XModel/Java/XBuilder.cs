using System;
using System.Diagnostics;
using System.Linq;
using Dot42.JvmClassLib;
using Dot42.JvmClassLib.Structures;

namespace Dot42.CompilerLib.XModel.Java
{
    public static partial class XBuilder
    {
        /// <summary>
        /// Convert an Java type reference to an XTypeReference.
        /// </summary>
        public static XTypeReference AsTypeReference(XModule module, TypeReference type, XTypeUsageFlags usageFlags)
        {
            if (type.IsArray)
            {
                var arrayType = (ArrayTypeReference)type;
                return new XArrayType(AsTypeReference(module, arrayType.ElementType, usageFlags));
            }

            if (type.IsBaseType)
            {
                var baseType = (BaseTypeReference)type;
                var ts = module.TypeSystem;
                switch (baseType.Type)
                {
                    case BaseTypes.Boolean:
                        return ts.Bool;
                    case BaseTypes.Byte:
                        return ts.SByte;
                    case BaseTypes.Char:
                        return ts.Char;
                    case BaseTypes.Short:
                        return ts.Short;
                    case BaseTypes.Int:
                        return ts.Int;
                    case BaseTypes.Long:
                        return ts.Long;
                    case BaseTypes.Float:
                        return ts.Float;
                    case BaseTypes.Double:
                        return ts.Double;
                }
                throw new NotImplementedException("Unknown base type " + (int)baseType.Type);
            }

            if (type.IsVoid)
            {
                return module.TypeSystem.Void;
            }

            if (type.IsTypeVariable)
            {
                throw new NotImplementedException("Unknown type " + type);
            }

            {
                var objectType = (ObjectTypeReference) type;
                if ((usageFlags != XTypeUsageFlags.DeclaringType) && (usageFlags != XTypeUsageFlags.BaseType))
                {
                    switch (objectType.ClassName)
                    {
                        case "java/lang/Boolean":
                            return CreateNullableT(module, module.TypeSystem.Bool);
                        case "java/lang/Byte":
                            return CreateNullableT(module, module.TypeSystem.Byte);
                        case "java/lang/Character":
                            return CreateNullableT(module, module.TypeSystem.Char);
                        case "java/lang/Short":
                            return CreateNullableT(module, module.TypeSystem.Short);
                        case "java/lang/Integer":
                            return CreateNullableT(module, module.TypeSystem.Int);
                        case "java/lang/Long":
                            return CreateNullableT(module, module.TypeSystem.Long);
                        case "java/lang/Float":
                            return CreateNullableT(module, module.TypeSystem.Float);
                        case "java/lang/Double":
                            return CreateNullableT(module, module.TypeSystem.Double);
                    }
                }

                var typeRef = new JavaTypeReference(module, objectType, objectType.ClassName);
                return typeRef;
            }
        }

        /// <summary>
        /// Create a System.Nullable&lt;T&gt; type with given T argument.
        /// </summary>
        private static XGenericInstanceType CreateNullableT(XModule module, XTypeReference argument)
        {
            return new XGenericInstanceType(new XTypeReference.SimpleXTypeReference(module, "System", "Nullable`1", null, true, new[] { "T" }), new[] { argument });            
        }

        /// <summary>
        /// Convert an Java type reference to an XTypeReference.
        /// </summary>
        public static XTypeReference AsTypeReference(XModule module, string className, XTypeUsageFlags usageFlags)
        {
            var objectType = new ObjectTypeReference(className, null);
            return AsTypeReference(module, objectType, usageFlags);
        }

        /// <summary>
        /// Convert an Java type reference to an XTypeReference.
        /// </summary>
        public static XTypeReference AsTypeReference(XModule module, ClassFile classFile, XTypeUsageFlags usageFlags)
        {
            if (classFile == null)
                return null;
            var objectType = new ObjectTypeReference(classFile.ClassName, null);
            return AsTypeReference(module, objectType, usageFlags);
        }

        /// <summary>
        /// Convert an Java field reference to an XFieldReference.
        /// </summary>
        public static XFieldReference AsFieldReference(XModule module, ConstantPoolFieldRef field)
        {
            return AsFieldReference(module, field.Name, field.Descriptor, field.DeclaringType, field.ClassName);
        }

        /// <summary>
        /// Convert an Java field reference to an XFieldReference.
        /// </summary>
        public static XFieldReference AsFieldReference(XModule module, string fieldName, string descriptor, TypeReference classNameAsType, string className)
        {
            var declaringType = AsTypeReference(module, classNameAsType, XTypeUsageFlags.DeclaringType);
            return AsFieldReference(module, fieldName, descriptor, declaringType, className);
        }

        /// <summary>
        /// Convert an Java field reference to an XFieldReference.
        /// </summary>
        public static XFieldReference AsFieldReference(XModule module, string fieldName, string descriptor, XTypeReference declaringType, string className)
        {
            var fieldType = AsTypeReference(module, Descriptors.ParseFieldType(descriptor), XTypeUsageFlags.FieldType);
            return new JavaFieldReference(fieldName, fieldType, declaringType, fieldName, descriptor, className);
        }

        /// <summary>
        /// Convert an Java field reference to an XFieldReference.
        /// </summary>
        public static XFieldDefinition AsFieldDefinition(XModule module, FieldDefinition field)
        {
            return AsTypeReference(module, field.DeclaringClass, XTypeUsageFlags.DeclaringType).Resolve().GetByOriginalField(field);
        }

        /// <summary>
        /// Convert an Java method reference to an XMethodReference.
        /// </summary>
        public static XMethodReference AsMethodReference(XModule module, ConstantPoolMethodRef method, bool hasThis)
        {
            return AsMethodReference(module, method.Name, method.Descriptor, method.DeclaringType, method.ClassName, hasThis);
        }

        /// <summary>
        /// Convert an Java method reference to an XMethodReference.
        /// </summary>
        public static XMethodReference AsMethodReference(XModule module, string methodName, string descriptor, TypeReference classNameAsType, string className, bool hasThis)
        {
            var declaringType = AsTypeReference(module, classNameAsType, XTypeUsageFlags.DeclaringType);
            return AsMethodReference(module, methodName, descriptor, declaringType, className, hasThis);
        }

        /// <summary>
        /// Convert an Java method reference to an XMethodReference.
        /// </summary>
        public static XMethodReference AsMethodReference(XModule module, string methodName, string descriptor, XTypeReference declaringType, string className, bool hasThis)
        {
            if (declaringType.IsArray)
            {
                declaringType = module.TypeSystem.Object;
            }
            var _descriptor = Descriptors.ParseMethodDescriptor(descriptor);
            var returnType = AsTypeReference(module, _descriptor.ReturnType, XTypeUsageFlags.ReturnType);
            var parameters = _descriptor.Parameters.Select((x, i) => new JavaParameter(module, "p" + i, x));
            var name = methodName;
            if (name == "<init>") name = ".ctor";
            else if (name == "<clinit>") name = ".cctor";
            return new JavaMethodReference(name, hasThis, returnType, declaringType, parameters, null, methodName, descriptor, className);
        }

        /// <summary>
        /// Convert an Java method reference to an XMethodReference.
        /// </summary>
        public static XMethodDefinition AsMethodDefinition(XModule module, MethodDefinition method)
        {
            return AsTypeReference(module, method.DeclaringClass, XTypeUsageFlags.DeclaringType).Resolve().GetByOriginalMethod(method);
        }
    }
}
