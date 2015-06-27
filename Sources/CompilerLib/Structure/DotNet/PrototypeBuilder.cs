using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.DexLib.Metadata;
using Dot42.FrameworkDefinitions;
using Dot42.Utility;

namespace Dot42.CompilerLib.Structure.DotNet
{
    internal static class PrototypeBuilder
    {
        /// <summary>
        /// Create a prototype for the given methods signature
        /// </summary>
        internal static Prototype BuildPrototype(AssemblyCompiler compiler, DexTargetPackage targetPackage, ClassDefinition declaringClass, XMethodDefinition method)
        {
            var result = new Prototype();
            result.ReturnType = method.ReturnType.GetReference(targetPackage);
            if (method.IsAndroidExtension && !method.IsStatic)
            {
                // Add "this" parameter
                var dparameter = new Parameter(method.DeclaringType.GetReference(targetPackage), "this");
                result.Parameters.Add(dparameter);
            }
            
            foreach (var p in method.Parameters)
            {
                var dparameter = new Parameter(p.ParameterType.GetReference(targetPackage), p.Name);
                result.Parameters.Add(dparameter);
            }
            
            AddGenericParameters(compiler, targetPackage, method, result);

            result.Freeze();
            return result;
        }

        /// <summary>
        /// Add generic parameters to the prototype based on the given XMethodDefinition
        /// </summary>
        public static void AddGenericParameters(AssemblyCompiler compiler, DexTargetPackage targetPackage, XMethodDefinition method, Prototype result)
        {
            if (method.NeedsGenericInstanceTypeParameter)
            {
                var annType = compiler.GetDot42InternalType(InternalConstants.GenericTypeParameterAnnotation).GetClassReference(targetPackage);
                int numParameters = method.DeclaringType.GenericParameters.Count;
                var buildAsArray = numParameters > InternalConstants.GenericTypeParametersAsArrayThreshold;
                var parameterArray = result.GenericInstanceTypeParameters;
                AddGenericParameterArguments(result, buildAsArray, numParameters, "__$$git", annType, parameterArray);
            }
            if (method.NeedsGenericInstanceMethodParameter)
            {
                // Add GenericInstance parameter
                var annType = compiler.GetDot42InternalType(InternalConstants.GenericMethodParameterAnnotation).GetClassReference(targetPackage);
                int numParameters = method.GenericParameters.Count;
                var buildAsArray = numParameters > InternalConstants.GenericMethodParametersAsArrayThreshold;
                var parameterArray = result.GenericInstanceMethodParameters;
                AddGenericParameterArguments(result, buildAsArray, numParameters, "__$$gim", annType, parameterArray);
            }
        }

        private static void AddGenericParameterArguments(Prototype result, bool buildAsArray, int numParameters, string parameterBaseName, ClassReference annType, IList<Parameter> parameterArray)
        {
            if (buildAsArray)
            {
                // Add GenericInstance parameter (to pass the generic instance array of the declaring type)
                var paramType = FrameworkReferences.ClassArray;
                var dparameter = new Parameter(paramType, parameterBaseName);
                dparameter.Annotations.Add(new Annotation(annType, AnnotationVisibility.Runtime));
                result.Parameters.Add(dparameter);
                parameterArray.Add(dparameter);
            }
            else
            {
                for (int i = 0; i < numParameters; ++i)
                {
                    var dparameter = new Parameter(FrameworkReferences.Class, parameterBaseName + (i + 1));
                    dparameter.Annotations.Add(new Annotation(annType, AnnotationVisibility.Runtime));
                    result.Parameters.Add(dparameter);
                    parameterArray.Add(dparameter);
                }
            }
        }

        /// <summary>
        /// Parse a descriptor containing a single field type
        /// </summary>
        public static TypeReference ParseFieldType(string descriptor)
        {
            var index = 0;
            var result = ParseFieldType(descriptor, ref index);
            if (index != descriptor.Length)
                throw new InvalidDescriptorException(descriptor);
            return result;
        }

        /// <summary>
        /// Parse a descriptor containing a single method signature
        /// </summary>
        public static Prototype ParseMethodSignature(string descriptor)
        {
            var index = 0;
            var result = ParseMethodSignature(descriptor, ref index);
            if (index != descriptor.Length)
                throw new InvalidDescriptorException(descriptor);
            return result;
        }

        /// <summary>
        /// Parse a descriptor containing a single field type
        /// </summary>
        private static TypeReference ParseFieldType(string descriptor, ref int index)
        {
            var code = descriptor[index++];
            if (code == '[')
            {
                // Array
                return new ArrayType(ParseFieldType(descriptor, ref index));
            }

            if (code == 'L')
            {
                // Object class
                var end = descriptor.IndexOf(';', index);
                if (end < 0)
                    throw new InvalidDescriptorException(descriptor);
                var name = descriptor.Substring(index, end - index);
                index = end + 1;
                return new ClassReference(name);
            }

            // Should be base type
            return GetPrimitiveType(code);
        }

        /// <summary>
        /// Parse a descriptor containing a single method signature
        /// </summary>
        private static Prototype ParseMethodSignature(string descriptor, ref int index)
        {
            var code = descriptor[index++];
            if (code != '(')
            {
                throw new InvalidDescriptorException(descriptor);
            }

            var parameters = new List<TypeReference>();
            while (descriptor[index] != ')')
            {
                var type = ParseFieldType(descriptor, ref index);
                parameters.Add(type);
            }

            // Skip ')'
            index++;

            // Return type
            if (descriptor[index] == 'V')
            {
                index++;
                return new Prototype(PrimitiveType.Void, parameters.Select(x => new Parameter(x, null)).ToArray());
            }

            // Non void
            var returnType = ParseFieldType(descriptor, ref index);
            return new Prototype(returnType, parameters.Select(x => new Parameter(x, null)).ToArray());
        }

        /// <summary>
        /// Gets the primitive type for the given code.
        /// </summary>
        private static PrimitiveType GetPrimitiveType(char code)
        {
            var td = (TypeDescriptors) code;
            switch (td)
            {
                case TypeDescriptors.Boolean:
                    return PrimitiveType.Boolean;
                case TypeDescriptors.Byte:
                    return PrimitiveType.Byte;
                case TypeDescriptors.Char:
                    return PrimitiveType.Char;
                case TypeDescriptors.Double:
                    return PrimitiveType.Double;
                case TypeDescriptors.Float:
                    return PrimitiveType.Float;
                case TypeDescriptors.Int:
                    return PrimitiveType.Int;
                case TypeDescriptors.Long:
                    return PrimitiveType.Long;
                case TypeDescriptors.Short:
                    return PrimitiveType.Short;
                case TypeDescriptors.Void:
                    return PrimitiveType.Void;
                default:
                    throw new InvalidDescriptorException(code.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
