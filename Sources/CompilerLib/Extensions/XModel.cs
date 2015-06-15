using System;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.JvmClassLib;
using ArrayType = Dot42.DexLib.ArrayType;
using ByReferenceType = Dot42.DexLib.ByReferenceType;
using FieldReference = Dot42.DexLib.FieldReference;
using MethodReference = Dot42.DexLib.MethodReference;
using TypeReference = Dot42.DexLib.TypeReference;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Dex related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets a Dex field reference for the given field reference.
        /// </summary>
        internal static FieldReference GetReference(this XFieldReference field, DexTargetPackage targetPackage)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            // Resolve the field to a field definition
            XFieldDefinition fieldDef;
            if (field.TryResolve(out fieldDef))
            {

                string className;
                string memberName;
                string descriptor;
                if (fieldDef.TryGetDexImportNames(out memberName, out descriptor, out className))
                {
                    var prototype = PrototypeBuilder.ParseFieldType(descriptor);
                    return new FieldReference(new ClassReference(className), memberName, prototype);
                }
                if (fieldDef.TryGetJavaImportNames(out memberName, out descriptor, out className))
                {
                    var prototype = PrototypeBuilder.ParseFieldType(descriptor);
                    return new FieldReference(new ClassReference(className), memberName, prototype);
                }

                // Field is in the assembly itself
                // Use the mapping
                return targetPackage.NameConverter.GetField(fieldDef);
            }

            var javaField = field as XModel.Java.XBuilder.JavaFieldReference;
            if (javaField != null)
            {
                var prototype = PrototypeBuilder.ParseFieldType(javaField.JavaDecriptor);
                return new FieldReference(new ClassReference(javaField.JavaClassName), javaField.JavaName, prototype);
            }

            throw new ResolveException(string.Format("Field {0} not found", field.FullName));
        }

        /// <summary>
        /// Gets a Dex method reference for the given type reference.
        /// </summary>
        internal static MethodReference GetReference(this XMethodReference method, DexTargetPackage targetPackage)
        {
            if (method == null)
                throw new ArgumentNullException("method");

//#if DEBUG
//            if (method.DeclaringType.IsArray)
//            {
//                Debugger.Launch();
//            }
//#endif

            // Resolve the type to a method definition
            XMethodDefinition methodDef;
            if (method.TryResolve(out methodDef))
            {

                string className;
                string memberName;
                string descriptor;
                if (methodDef.TryGetDexImportNames(out memberName, out descriptor, out className))
                {
                    var prototype = PrototypeBuilder.ParseMethodSignature(descriptor);
                    return new MethodReference(GetDeclaringTypeReference(className), memberName, prototype);
                }

                // Method is in the assembly itself
                // Use the mapping
                return targetPackage.NameConverter.GetMethod(methodDef);
            }

            var javaMethod = method as XModel.Java.XBuilder.JavaMethodReference;
            if (javaMethod != null)
            {
                var prototype = PrototypeBuilder.ParseMethodSignature(javaMethod.JavaDecriptor);
                return new MethodReference(GetDeclaringTypeReference(javaMethod.JavaClassName), javaMethod.JavaName, prototype);
            }

            // Return reference to a method we do not know yet
            throw new ResolveException(string.Format("Method {0} not found", method.FullName));
        }

        /// <summary>
        /// Gets a class reference that is to be used as declaring type.
        /// </summary>
        private static ClassReference GetDeclaringTypeReference(string className)
        {
            if (className.StartsWith("["))
            {
                className = "java/lang/Object";
            }
            return new ClassReference(className);
        }

        /// <summary>
        /// Gets a class reference for the given type reference.
        /// </summary>
        internal static TypeReference GetReference(this XTypeReference type, DexTargetPackage targetPackage)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            type = type.GetWithoutModifiers();

            // Handle array's
            if (type.IsArray)
            {
                var arrType = (XArrayType)type;
                var dimensions = arrType.Dimensions.Count() - 1;
                var dArrayType = new ArrayType(GetReference(type.ElementType, targetPackage));
                while (dimensions > 0)
                {
                    dArrayType = new ArrayType(dArrayType);
                    dimensions--;
                }
                return dArrayType;
            }

            // Handle generic parameters
            if (type.IsGenericParameter || (type.IsByReference && type.ElementType.IsGenericParameter))
            {
                if (type.IsByReference) // this should be possible as well, but would need some more code at some other places.
                    return new ByReferenceType(new ClassReference("java/lang/Object"));

                var gp = (XGenericParameter) type;
                if (gp.AllowConstraintAsTypeReference())
                    return gp.Constraints[0].GetReference(targetPackage);

                return new ClassReference("java/lang/Object");
            }

            // Handle out/ref types
            if (type.IsByReference)
            {
                var byRefType = (XByReferenceType)type;
                return new ByReferenceType(GetReference(byRefType.ElementType, targetPackage));
            }

            // Handle Nullable<T>
            if (type.IsGenericInstance)
            {
                var git = (XGenericInstanceType)type;
                if (git.ElementType.IsNullableT())
                {
                    var arg = git.GenericArguments[0];
                    if (arg.IsBoolean()) return new ClassReference("java/lang/Boolean");
                    if (arg.IsByte() || arg.IsSByte()) return new ClassReference("java/lang/Byte");
                    if (arg.IsChar()) return new ClassReference("java/lang/Character");
                    if (arg.IsInt16() || arg.IsUInt16()) return new ClassReference("java/lang/Short");
                    if (arg.IsInt32() || arg.IsUInt32()) return new ClassReference("java/lang/Integer");
                    if (arg.IsInt64() || arg.IsUInt64()) return new ClassReference("java/lang/Long");
                    if (arg.IsDouble()) return new ClassReference("java/lang/Double");
                    if (arg.IsFloat()) return new ClassReference("java/lang/Float");

                    
                    var typeofT = git.GenericArguments[0];

                    if(typeofT.IsGenericParameter) // use object.
                        return new ClassReference("java/lang/Object");

                    XTypeDefinition typeofTDef;
                    if (!typeofT.TryResolve(out typeofTDef))
                        throw new XResolutionException(typeofT);

                    var className = targetPackage.NameConverter.GetConvertedFullName(typeofTDef);
                    var classDef = targetPackage.DexFile.GetClass(className);
                    
                    // Use nullable base class of T, if enum.
                    if (classDef.IsEnum) 
                        return classDef.SuperClass;

                    // I like the base class concept for enums. unfortunately it seems to be 
                    // impossible for structs and/or might have performance implications.
                    // Just return the type for structs.
                    return classDef; 
                }
            }

            var primType = GetPrimitiveType(type);
            if (primType != null)
                return primType;

            // Resolve the type to a type definition
            XTypeDefinition typeDef;
            if (type.GetElementType().TryResolve(out typeDef))
            {

                // Handle primitive types
                primType = GetPrimitiveType(typeDef);
                if (primType != null)
                    return primType;

                string className;
                if (typeDef.TryGetDexImportNames(out className))
                {
                    // type is a framework type
                    return new ClassReference(className);
                }

                // Handle enums
                /* Enums Are Normal classes now
                if (typeDef.IsEnum)
                {
                    // Convert to primitive type
                    //return typeDef.GetEnumUnderlyingType().GetReference(target, nsConverter);
                }*/

                // Handle nested types of java types
                string convertedFullName;
                if (typeDef.IsNested && typeDef.DeclaringType.HasDexImportAttribute())
                {
                    // Nested type that is not imported, but it's declaring type is imported.
                    convertedFullName = targetPackage.NameConverter.GetConvertedFullName(typeDef.DeclaringType) + "_" +
                                        NameConverter.GetConvertedName(typeDef);
                }
                else if (typeDef.TryGetJavaImportNames(out className))
                {
                    convertedFullName = className.Replace('/', '.');
                }
                else
                {
                    convertedFullName = targetPackage.NameConverter.GetConvertedFullName(typeDef);
                }

                // type is in the assembly itself
                var result = targetPackage.DexFile.GetClass(convertedFullName);
                if (result == null)
                {
                    throw new ArgumentException(string.Format("Cannot find type {0}", convertedFullName));
                }
                return result;
            }

            var javaType = type as XModel.Java.XBuilder.JavaTypeReference;
            if (javaType != null)
            {
                return new ClassReference(javaType.JavaClassName);
            }

            throw new ResolveException(string.Format("Type {0} not found", type.FullName));
        }

        /// <summary>
        /// Gets a class reference for the given type reference.
        /// </summary>
        internal static ClassReference GetClassReference(this XTypeReference type, DexTargetPackage targetPackage)
        {
            var classRef = type.GetReference(targetPackage) as ClassReference;
            if (classRef == null)
                throw new ArgumentException(string.Format("type {0} is not a class reference", type.FullName));
            return classRef;
        }

        /// <summary>
        /// Gets a primitive type or null if the given type is not primitive.
        /// </summary>
        private static PrimitiveType GetPrimitiveType(XTypeReference type)
        {
            // Handle primitive types
            switch (type.Kind)
            {
                case XTypeReferenceKind.Bool:
                    return PrimitiveType.Boolean;
                case XTypeReferenceKind.Char:
                    return PrimitiveType.Char;
                case XTypeReferenceKind.Float:
                    return PrimitiveType.Float;
                case XTypeReferenceKind.Double:
                    return PrimitiveType.Double;
                case XTypeReferenceKind.SByte:
                case XTypeReferenceKind.Byte:
                    return PrimitiveType.Byte;
                case XTypeReferenceKind.Short:
                case XTypeReferenceKind.UShort:
                    return PrimitiveType.Short;
                case XTypeReferenceKind.Int:
                case XTypeReferenceKind.UInt:
                    return PrimitiveType.Int;
                case XTypeReferenceKind.Long:
                case XTypeReferenceKind.ULong:
                    return PrimitiveType.Long;
                case XTypeReferenceKind.Void:
                    return PrimitiveType.Void;
                case XTypeReferenceKind.IntPtr:
                case XTypeReferenceKind.UIntPtr:
                    return PrimitiveType.Int; // Is this correct?
            }

            return null;
        }
    }
}
