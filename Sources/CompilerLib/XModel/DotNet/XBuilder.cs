using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Dot42.CompilerLib.XModel.DotNet
{
    public static partial class XBuilder
    {
        /// <summary>
        /// Convert an IL type reference to an XTypeDefinition.
        /// </summary>
        public static XTypeDefinition AsTypeDefinition(XModule module, TypeDefinition type)
        {
            if (type == null)
                return null;
            var xtypeRef = AsTypeReference(module, type);
            XTypeDefinition xtypeDef;
            if (!xtypeRef.TryResolve(out xtypeDef))
                throw new XResolutionException(xtypeRef);
            return xtypeDef;
        }

        /// <summary>
        /// Convert an IL type reference to an XTypeReference.
        /// </summary>
        public static XTypeReference AsTypeReference(XModule module, TypeReference type)
        {
            if (type == null)
                return null;

            var cache = module.GetCache<TypeReferenceCache>();
            XTypeReference result;
            if (cache.TryGetValue(type, out result))
                return result;

            result = CreateTypeReference(module, type);
            cache[type] = result;
            return result;
        }

        /// <summary>
        /// Convert an IL field reference to an XFieldReference.
        /// </summary>
        public static XFieldReference AsFieldReference(XModule module, FieldReference field)
        {
            var cache = module.GetCache<FieldReferenceCache>();
            XFieldReference result;
            if (cache.TryGetValue(field, out result))
                return result;

            result = CreateFieldReference(module, field);
            cache[field] = result;
            return result;
        }

        /// <summary>
        /// Convert an IL field definition to an XFieldDefinition.
        /// </summary>
        public static XFieldDefinition AsFieldDefinition(XModule module, FieldDefinition field)
        {
            return AsTypeReference(module, field.DeclaringType).Resolve().GetByOriginalField(field);
        }

        /// <summary>
        /// Convert an IL method reference to an XMethodReference.
        /// </summary>
        public static XMethodReference AsMethodReference(XModule module, MethodReference method)
        {
            var cache = module.GetCache<MethodReferenceCache>();
            XMethodReference result;
            if (cache.TryGetValue(method, out result))
                return result;

            result = CreateMethodReference(module, method);
            cache[method] = result;
            return result;
        }

        /// <summary>
        /// Convert an IL method definition to an XMethodDefinition.
        /// </summary>
        public static XMethodDefinition AsMethodDefinition(XModule module, MethodDefinition method)
        {
            return AsTypeReference(module, method.DeclaringType).Resolve().GetByOriginalMethod(method);
        }

        /// <summary>
        /// Convert an IL type reference to an XTypeReference.
        /// </summary>
        private static XTypeReference CreateTypeReference(XModule module, TypeReference type)
        {
            if (type == null)
                return null;

            if (type.IsArray)
            {
                var arrayType = (ArrayType)type;
                var dimensions = arrayType.Dimensions.Select(x => new XArrayDimension(x.LowerBound, x.UpperBound));
                return new XArrayType(AsTypeReference(module, arrayType.ElementType), dimensions);
            }

            // Handle known primitive types
            var ts = module.TypeSystem;
            switch (type.MetadataType)
            {
                case MetadataType.Boolean:
                    return ts.Bool;
                case MetadataType.Byte:
                    return ts.Byte;
                case MetadataType.SByte:
                    return ts.SByte;
                case MetadataType.Char:
                    return ts.Char;
                case MetadataType.Int16:
                    return ts.Short;
                case MetadataType.UInt16:
                    return ts.UShort;
                case MetadataType.Int32:
                    return ts.Int;
                case MetadataType.UInt32:
                    return ts.UInt;
                case MetadataType.Int64:
                    return ts.Long;
                case MetadataType.UInt64:
                    return ts.ULong;
                case MetadataType.Single:
                    return ts.Float;
                case MetadataType.Double:
                    return ts.Double;
                case MetadataType.Void:
                    return ts.Void;
                case MetadataType.IntPtr:
                    return ts.IntPtr;
                case MetadataType.UIntPtr:
                    return ts.UIntPtr;
                case MetadataType.Object:
                    return ts.Object;
                case MetadataType.String:
                    return ts.String;
            }

            if (type.IsByReference)
            {
                var byReferenceType = (ByReferenceType)type;
                return new XByReferenceType(AsTypeReference(module, byReferenceType.ElementType));
            }

            if (type.IsGenericParameter)
            {
                var gp = (GenericParameter)type;
                return new ILGenericParameter(module, gp);
            }

            if (type.IsGenericInstance)
            {
                var git = (GenericInstanceType)type;
                var genericArguments = git.GenericArguments.Select(x => AsTypeReference(module, x));
                return new XGenericInstanceType(AsTypeReference(module, git.ElementType), genericArguments);
            }

            if (type.IsPointer)
            {
                var pointerType = (PointerType)type;
                return new XPointerType(AsTypeReference(module, pointerType.ElementType));
            }

            if (type.IsOptionalModifier)
            {
                var modType = (OptionalModifierType)type;
                return new XOptionalModifierType(AsTypeReference(module, modType.ModifierType), AsTypeReference(module, modType.ElementType));
            }

            if (type.IsRequiredModifier)
            {
                var modType = (RequiredModifierType)type;
                return new XRequiredModifierType(AsTypeReference(module, modType.ModifierType), AsTypeReference(module, modType.ElementType));
            }

            if (type is TypeSpecification)
            {
                throw new NotImplementedException("Unknown type " + type);
            }

            if (type.IsDefinition)
            {
                XTypeDefinition typeDef;
                if (module.TryGetType(type.FullName, out typeDef))
                {
                    return typeDef; // Add extra resolve since some type definitions resolve to others.
                }
            }

            {
                var declaringType = (type.DeclaringType != null) ? AsTypeReference(module, type.DeclaringType) : null;
                var genericParameterNames = type.GenericParameters.Select(x => x.Name);
                return new XTypeReference.SimpleXTypeReference(module, type.Namespace, type.Name, declaringType, type.IsValueType, genericParameterNames);
            }
        }

        /// <summary>
        /// Convert an IL field reference to an XFieldReference.
        /// </summary>
        private static XFieldReference CreateFieldReference(XModule module, FieldReference field)
        {
            var declaringType = AsTypeReference(module, field.DeclaringType);

            if (field.IsDefinition)
            {
                var fieldDef = declaringType.Resolve().GetByOriginalField(field);
                return fieldDef.Resolve(); // Add extra resolve since some field definitions resolve to others.
            }

            var fieldType = AsTypeReference(module, field.FieldType);
            return new XFieldReference.Simple(field.Name, fieldType, declaringType);
        }

        /// <summary>
        /// Convert an IL method reference to an XMethodReference.
        /// </summary>
        private static XMethodReference CreateMethodReference(XModule module, MethodReference method)
        {
            if (method.IsGenericInstance)
            {
                var gim = (GenericInstanceMethod)method;
                var elementMethod = AsMethodReference(module, gim.ElementMethod);
                return new XGenericInstanceMethod(elementMethod, gim.GenericArguments.Select(x => AsTypeReference(module, x)));
            }

            var declaringType = AsTypeReference(module, method.DeclaringType);

            if (method.IsDefinition)
            {
                var methodDef = declaringType.Resolve().GetByOriginalMethod(method);
                return methodDef.Resolve(); // Add extra resolve since some method definitions resolve to others.
            }
            return new ILMethodReference(declaringType, method);
        }

        /// <summary>
        /// Create a descriptor for comparing the given method without generics.
        /// </summary>
        private static string CreateNoGenericsDescriptor(XTypeReference type)
        {
            if (type.IsArray)
            {
                return CreateNoGenericsDescriptor(((XArrayType)type).ElementType) + "[]";
            }
            XTypeDefinition typeDef;
            return type.TryResolve(out typeDef) ? typeDef.GetFullName(true) : type.GetFullName(true);
        }

        /// <summary>
        /// Create a postfix for the name of the given method to prevent method-name
        /// clashes. 
        /// Note: I believe that the current approch is to fragile. Methods 
        /// should be renamed based on actual clashing, depending on overrides 
        /// and new slots and such.
        /// this miethod would revert to its orginal CreateSignPrefix() to make it
        /// (with return value handling) to make it clear that it has specific 
        ///  parameter handling. 
        /// This would also be the place to warn the user that two constructor 
        /// overloads clash and can not be resolved. [or - possibly - generating a 
        /// static factory method calling a constructor with an extra marker 
        /// parameter - or just adding this marker parameter to all call sites.]
        /// </summary>
        private static string CreateSignAndGenericsPostfix(MethodReference method)
        {
            var declaringType = method.DeclaringType;

            if (declaringType.IsArray)
                return string.Empty;
            if ((method.Name == ".ctor") || (method.Name == ".cctor"))
                return string.Empty;

            var declType = declaringType.GetElementType().Resolve();
            if ((declType != null) && (declType.IsDelegate()))
                return string.Empty;

            bool isNullableT = declaringType.FullName.StartsWith("System.Nullable`1");

            // do not generics-rename getters or setters.
            // no not generics-rename interface methods or virtual methods (better would be: to rename these as well, but my code wouldn't work)
            
            var methodDef = method.Resolve();

            bool processGenerics = !isNullableT && !methodDef.IsGetter && !methodDef.IsSetter
                                    && !declaringType.Resolve().IsInterface 
                                    && !methodDef.IsVirtual && methodDef.Overrides.Count == 0;

            bool needsGenericPostfix = false;
            StringBuilder genericPostfix = new StringBuilder();

            if (processGenerics)
            {
                if (method.ReturnType != null && method.ReturnType.IsGenericParameter)
                {
                    genericPostfix.Append("T");
                    //needsGenericPostfix = true;  // don't mark based on return type only.
                }
                else
                    genericPostfix.Append("_");

                if (method.HasGenericParameters)
                {
                    needsGenericPostfix = true;
                    foreach (var e in method.GenericParameters)
                        genericPostfix.Append("T");
                }
            }

            var needsPostfix = false;
            var paramPostfix = new StringBuilder();
            foreach (var p in method.Parameters)
            {
                var typeChar = GetParameterPostfixIfRequired(p.ParameterType, processGenerics, ref needsPostfix);
                paramPostfix.Append(typeChar);
            }

            if (method.Name.StartsWith("op_Explicit", StringComparison.OrdinalIgnoreCase))
            {
                if(paramPostfix.Length > 0) paramPostfix.Append("$");
                var typeChar = GetParameterPostfixIfRequired(method.ReturnType, processGenerics, ref needsPostfix);
                paramPostfix.Append(typeChar);
            }

            if (methodDef.IsNewSlot)
            {
                // TODO: rename new methods so that they don't accidentially override a base method.
            }

            if (needsPostfix || needsGenericPostfix)
            {
                if (!needsGenericPostfix)
                    return "$$" + paramPostfix;
                if (!needsPostfix)
                    return "$$" + genericPostfix;
                return "$$$" + genericPostfix + "$" + paramPostfix;
            }

            return string.Empty;
        }

        private static string GetPostfixFromName(string name)
        {
            int idx = name.IndexOf("$$", StringComparison.Ordinal);
            if (idx == -1) return "";
            return name.Substring(idx);
        }

        private static char GetParameterPostfixIfRequired(TypeReference paramType, bool processGenerics, ref bool needsPostfix)
        {
            TypeReference type;

            if (processGenerics && paramType.IsGenericParameter)
            {
                needsPostfix = true;
                return 'T';
            }

            var isNullableInstance = paramType.FullName.StartsWith("System.Nullable`1<") && paramType.IsGenericInstance;
            if (isNullableInstance)
            {
                type = ((GenericInstanceType) paramType).GenericArguments[0];
            }
            else
            {
                type = paramType.GetElementType();
            }

            var typeChar = '_';

            if (type.IsPrimitive)
            {
                switch (type.MetadataType)
                {
                    case MetadataType.SByte:
                        typeChar = 'B';
                        needsPostfix = true;
                        break;
                    case MetadataType.UInt16:
                        typeChar = 'S';
                        needsPostfix = true;
                        break;
                    case MetadataType.UInt32:
                        typeChar = 'I';
                        needsPostfix = true;
                        break;
                    case MetadataType.UInt64:
                        typeChar = 'J';
                        needsPostfix = true;
                        break;
                }
            }
            else if (isNullableInstance)
            {
                typeChar = 'X';
                needsPostfix = true;
            }
            return typeChar;
        }

        private class TypeReferenceCache : Dictionary<TypeReference, XTypeReference> { }
        private class FieldReferenceCache : Dictionary<FieldReference, XFieldReference> { }
        private class MethodReferenceCache : Dictionary<MethodReference, XMethodReference> { }
    }
}
