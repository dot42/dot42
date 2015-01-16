using System.Collections.Generic;
using Dot42.Utility;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Field and method descriptors
    /// </summary>
    public static class Descriptors
    {
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
        public static MethodDescriptor ParseMethodDescriptor(string descriptor)
        {
            var index = 0;
            var result = ParseMethodDescriptor(descriptor, ref index);
            if (index != descriptor.Length)
                throw new InvalidDescriptorException(descriptor);
            return result;
        }

        /// <summary>
        /// Parse a class description (L...;)
        /// </summary>
        public static TypeReference ParseClassType(string descriptor)
        {
            return ParseFieldType(descriptor);
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
                return new ArrayTypeReference(ParseFieldType(descriptor, ref index));
            }

            if (code == 'L')
            {
                // Object class
                var end = descriptor.IndexOf(';', index);
                if (end < 0)
                    throw new InvalidDescriptorException(descriptor);
                var name = descriptor.Substring(index, end - index);
                index = end + 1;
                return new ObjectTypeReference(name, null);
            }

            // Should be base type
            BaseTypeReference type;
            if (BaseTypeReference.TryGetByCode(code, out type))
            {
                return type;
            }

            throw new InvalidDescriptorException(descriptor);
        }

        /// <summary>
        /// Parse a descriptor containing a single method signature
        /// </summary>
        private static MethodDescriptor ParseMethodDescriptor(string descriptor, ref int index)
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
                return new MethodDescriptor(VoidTypeReference.Instance, parameters);
            }

            // Non void
            var returnType = ParseFieldType(descriptor, ref index);
            return new MethodDescriptor(returnType, parameters);
        }

        /// <summary>
        /// Return only the parameters part of the given method descriptor.
        /// </summary>
        public static string StripMethodReturnType(string descriptor)
        {
            if (descriptor[0] != '(')
                throw new InvalidDescriptorException(descriptor);
            var index = descriptor.IndexOf(')');
            if (index < 0)
                throw new InvalidDescriptorException(descriptor);
            return descriptor.Substring(0, index + 1);
        }
    }
}
