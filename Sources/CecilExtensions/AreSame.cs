using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Dot42.CecilExtensions
{
    /// <summary>
    /// Equality extension methods
    /// </summary>
    partial class Extensions
    {
        /// <summary>
        /// Are the given scopes the same?
        /// </summary>
        public static bool AreSame(this IMetadataScope x, IMetadataScope y)
        {
            // Both null?
            if ((x == null) && (y == null)) { return true; }

            // One null, other not null
            if ((x == null) || (y == null)) { return false; }

            var nx = GetName(x);
            var ny = GetName(y);

            return (nx == ny);
        }

        /// <summary>
        /// Gets the normalized name of the given scope
        /// </summary>
        public static string GetName(this IMetadataScope scope)
        {
            if (scope == null) { return string.Empty; }
            switch (scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    return scope.Name;
                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition)scope).Assembly.Name.Name;
                case MetadataScopeType.ModuleReference:
                    return scope.Name;
                default:
                    throw new ArgumentException("Unknown MetadataScopeType " + scope.MetadataScopeType);
            }
        }

        /// <summary>
        /// Are method reference a and b the same or both null?
        /// Note! Declaring type is not taken into account!
        /// </summary>
        public static bool AreSameOrNull(this MethodReference a, MethodReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if ((a == null) && (b == null)) { return true; }
            if ((a == null) || (b == null)) { return false; }
            return a.AreSame(b, genericParamResolver);
        }

        /// <summary>
        /// Are field reference a and b the same?
        /// Note! Declaring type is not taken into account!
        /// </summary>
        public static bool AreSame(this FieldReference a, FieldReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            return (a.Name == b.Name) && AreSame(a.FieldType, b.FieldType, genericParamResolver);
        }

        /// <summary>
        /// Are method reference a and b the same?
        /// Note! Declaring type is not taken into account!
        /// </summary>
        public static bool AreSame(this MethodReference a, MethodReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.Name != b.Name) { return false; }
            if (!AreSame(a.ReturnType, b.ReturnType, genericParamResolver)) { return false; }

            var hasParameters = a.HasParameters;
            if (hasParameters != b.HasParameters) { return false; }

            if (hasParameters)
            {
                if (!AreSame(a.Parameters, b.Parameters, genericParamResolver)) { return false; }
            }

            var hasGParameters = a.HasGenericParameters;
            if (hasGParameters != b.HasGenericParameters) { return false; }
            if (hasGParameters)
            {
                if (a.GenericParameters.Count != b.GenericParameters.Count) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Are method reference a and b the same when not taking into account generic arguments of return and parameter types.
        /// Note! Declaring type is not taken into account!
        /// </summary>
        public static bool AreSameExcludingGenericArguments(this MethodReference a, MethodReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.Name != b.Name) { return false; }
            if (!AreSame(a.ReturnType.GetElementType(), b.ReturnType.GetElementType(), genericParamResolver)) { return false; }

            var hasParameters = a.HasParameters;
            if (hasParameters != b.HasParameters) { return false; }

            if (hasParameters)
            {
                if (!AreSameExcludingGenericArguments(a.Parameters, b.Parameters, genericParamResolver)) { return false; }
            }

            var hasGParameters = a.HasGenericParameters;
            if (hasGParameters != b.HasGenericParameters) { return false; }
            if (hasGParameters)
            {
                if (a.GenericParameters.Count != b.GenericParameters.Count) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Are method reference a and b the same?
        /// Note! Declaring type IS taken into account!
        /// </summary>
        public static bool AreSameIncludingDeclaringType(this MethodReference a, MethodReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            return a.AreSame(b, genericParamResolver) && a.DeclaringType.GetElementType().AreSame(b.DeclaringType.GetElementType(), genericParamResolver);
        }

        /// <summary>
        /// Do method a and b have the same override collection?
        /// </summary>
        public static bool AreSameByOverrides(this MethodDefinition a, MethodDefinition b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            var overridesA = a.Overrides;
            var overridesB = b.Overrides;

            var count = overridesA.Count;
            if (count != overridesB.Count) 
                return false; 
            for (int i = 0; i < count; i++)
            {
                if (!overridesA[i].DeclaringType.AreSame(overridesB[i].DeclaringType, genericParamResolver)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Are the given properties the same?
        /// </summary>
        public static bool AreSame(this PropertyReference a, PropertyReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            var parametersA = a.Parameters;
            var parametersB = b.Parameters;
            if (parametersA.Count != parametersB.Count) 
                return false; 
            if (a.Name != b.Name)  
                return false; 
            if (!a.PropertyType.AreSame(b.PropertyType, genericParamResolver))  
                return false; 
            if (!parametersA.AreSame(parametersB, genericParamResolver)) 
                return false; 
            return true;
        }

        /// <summary>
        /// Are the given properties using the same methods?
        /// </summary>
        /// <returns>True if Getter or Setter are the same</returns>
        public static bool AreSameByMethod(this PropertyDefinition a, PropertyDefinition b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.GetMethod.AreSameOrNull(b.GetMethod, genericParamResolver)) { return true; }
            if (a.SetMethod.AreSameOrNull(b.SetMethod, genericParamResolver)) { return true; }
            return false;
        }

        /// <summary>
        /// Are the given events using the same methods?
        /// </summary>
        /// <returns>True if Getter or Setter are the same</returns>
        public static bool AreSameByMethod(this EventDefinition a, EventDefinition b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.AddMethod.AreSameOrNull(b.AddMethod, genericParamResolver)) { return true; }
            if (a.RemoveMethod.AreSameOrNull(b.RemoveMethod, genericParamResolver)) { return true; }
            if (a.InvokeMethod.AreSameOrNull(b.InvokeMethod, genericParamResolver)) { return true; }
            return false;
        }

        /// <summary>
        /// Are the given events the same?
        /// </summary>
        public static bool AreSame(this EventReference a, EventReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.Name != b.Name)
                return false;
            return (a.EventType.AreSame(b.EventType, genericParamResolver));
        }

        /// <summary>
        /// Are the given parameter lists the same?
        /// </summary>
        public static bool AreSame(this IList<ParameterDefinition> a, IList<ParameterDefinition> b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            var count = a.Count;
            if (count != b.Count) { return false; }
            if (count == 0) { return true; }

            for (int i = 0; i < count; i++)
            {
                if (!AreSame(a[i].ParameterType, b[i].ParameterType, genericParamResolver)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Are the given parameter lists the same when not taking generic arguments into account?
        /// </summary>
        public static bool AreSameExcludingGenericArguments(this IList<ParameterDefinition> a, IList<ParameterDefinition> b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            var count = a.Count;
            if (count != b.Count) { return false; }
            if (count == 0) { return true; }

            for (int i = 0; i < count; i++)
            {
                if (!AreSame(a[i].ParameterType.GetElementType(), b[i].ParameterType.GetElementType(), genericParamResolver)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Is type a the same as type b?
        /// </summary>
        public static bool AreSame(this TypeSpecification a, TypeSpecification b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (!AreSame(a.ElementType, b.ElementType, genericParamResolver))
                return false;

            if (a.IsGenericInstance)
                return AreSame((GenericInstanceType)a, (GenericInstanceType)b, genericParamResolver);

            if (a.IsRequiredModifier || a.IsOptionalModifier)
                return AreSame((IModifierType)a, (IModifierType)b, genericParamResolver);

            if (a.IsArray)
                return AreSame((ArrayType)a, (ArrayType)b);

            return true;
        }

        /// <summary>
        /// Are type a and b the same?
        /// </summary>
        public static bool AreSame(this ArrayType a, ArrayType b)
        {
            if (a.Rank != b.Rank)
                return false;

            // TODO: dimensions

            return true;
        }

        /// <summary>
        /// Are type a and b the same?
        /// </summary>
        public static bool AreSame(this IModifierType a, IModifierType b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            return AreSame(a.ModifierType, b.ModifierType, genericParamResolver);
        }

        /// <summary>
        /// Are type a and b the same?
        /// </summary>
        public static bool AreSame(this GenericInstanceType a, GenericInstanceType b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            var hasArgumentsB = b.HasGenericArguments;
            if (!a.HasGenericArguments)
                return !hasArgumentsB;

            if (!hasArgumentsB)
                return false;

            var argumentsA = a.GenericArguments;
            var argumentsB = b.GenericArguments;
            if (argumentsA.Count != argumentsB.Count)
                return false;

            for (var i = 0; i < argumentsA.Count; i++)
                if (!AreSame(argumentsA[i], argumentsB[i], genericParamResolver))
                    return false;

            return true;
        }

        /// <summary>
        /// Are type a and b the same?
        /// </summary>
        public static bool AreSame(GenericParameter a, GenericParameter b)
        {
            return a.Position == b.Position;
        }

        
        /// <summary>
        /// Are type a and b the same?
        /// </summary>
        public static bool AreSameIncludingScope(this TypeReference a, TypeReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            return a.AreSame(b, genericParamResolver) && a.Scope.AreSame(b.Scope);
        }

        /// <summary>
        /// Are type a and b the same?
        /// </summary>
        public static bool AreSame(this TypeReference a, TypeReference b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.IsGenericParameter && (genericParamResolver != null))
                a = genericParamResolver((GenericParameter) a);
            if (b.IsGenericParameter && (genericParamResolver != null))
                b = genericParamResolver((GenericParameter) b);

            if (a.MetadataType != b.MetadataType)
                return false;

            if (a.IsGenericParameter)
                return AreSame((GenericParameter)a, (GenericParameter)b);

            if (a is TypeSpecification)
                return AreSame((TypeSpecification)a, (TypeSpecification)b, genericParamResolver);

            return a.FullName == b.FullName;
        }

        /// <summary>
        /// Are custom attribute a and b the same?
        /// </summary>
        public static bool AreSame(this CustomAttribute a, CustomAttribute b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            var argumentsA = a.ConstructorArguments;
            var argumentsB = b.ConstructorArguments;
            if (argumentsA.Count != argumentsB.Count) 
                return false; 
            if (!a.Constructor.AreSame(b.Constructor, genericParamResolver)) 
                return false; 
            if (!a.Constructor.DeclaringType.AreSame(b.Constructor.DeclaringType, genericParamResolver)) 
                return false; 
            if (!a.Fields.AreSame(b.Fields, genericParamResolver))  
                return false; 
            if (!a.Properties.AreSame(b.Properties, genericParamResolver))  
                return false; 

            for (int i = 0; i < argumentsA.Count; i++)
            {
                if (!argumentsA[i].AreSame(argumentsB[i], genericParamResolver)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Are arguments lists a and b the same?
        /// </summary>
        public static bool AreSame(this IList<CustomAttributeNamedArgument> a, IList<CustomAttributeNamedArgument> b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (a.Count != b.Count) { return false; }
            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].AreSame(b[i], genericParamResolver)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Are argument a and b the same?
        /// </summary>
        public static bool AreSame(this CustomAttributeNamedArgument a, CustomAttributeNamedArgument b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            return (a.Name == b.Name) && a.Argument.AreSame(b.Argument, genericParamResolver);
        }

        /// <summary>
        /// Are argument a and b the same?
        /// </summary>
        public static bool AreSame(this CustomAttributeArgument a, CustomAttributeArgument b, Func<GenericParameter, TypeReference> genericParamResolver)
        {
            if (!a.Type.AreSame(b.Type, genericParamResolver)) { return false; }
            if (object.Equals(a.Value, b.Value)) { return true; }
            if (a.Type.FullName == "System.Type")
            {
                return ((TypeReference)a.Value).AreSame((TypeReference)b.Value, genericParamResolver);
            }
            return false;
        }
    }

    /// <summary>
    /// How to compare
    /// </summary>
    [Flags]
    public enum EqualityFlags
    {
        None = 0,
        IgnoreScope = 0x01
    }
}
