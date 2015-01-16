using System.Collections.Generic;
using Dot42.Utility;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Field, method and class signatures
    /// </summary>
    public static class Signatures
    {
        /// <summary>
        /// Parse a signature containing a single field type
        /// </summary>
        public static TypeReference ParseFieldTypeSignature(string signature)
        {
            var index = 0;
            var result = ParseFieldTypeSignature(signature, ref index);
            if (index != signature.Length)
                throw new InvalidDescriptorException(signature);
            return result;
        }

        /// <summary>
        /// Parse a signature containing a single ClassSignature
        /// </summary>
        public static ClassSignature ParseClassSignature(string signature)
        {
            var index = 0;
            var result = ParseClassSignature(signature, ref index);
            if (index != signature.Length)
                throw new InvalidDescriptorException(signature);
            return result;
        }

        /// <summary>
        /// Parse a signature containing a single method signature
        /// </summary>
        public static MethodSignature ParseMethodSignature(string signature)
        {
            var index = 0;
            var result = ParseMethodSignature(signature, ref index);
            if (index != signature.Length)
                throw new InvalidDescriptorException(signature);
            return result;
        }

        /// <summary>
        /// Parse a signature containing a single field type
        /// </summary>
        private static TypeReference TryParseFieldTypeSignature(string signature, ref int index)
        {
            var code = signature[index];
            switch (code)
            {
                case '[':
                case 'L':
                case 'T':
                    return ParseFieldTypeSignature(signature, ref index);
            }
            return null;
        }

        /// <summary>
        /// Parse a signature containing a single field type
        /// </summary>
        private static TypeReference ParseFieldTypeSignature(string signature, ref int index)
        {
            var code = signature[index];
            if (code == '[')
            {
                // ArrayTypeSignature
                index++;
                return new ArrayTypeReference(ParseTypeSignature(signature, ref index));
            }

            if (code == 'L')
            {
                // ClassTypeSignature
                return ParseClassTypeSignature(signature, ref index);
            }

            if (code == 'T')
            {
                // TypeVariableSignature
                return ParseTypeVariableSignature(signature, ref index);
            }

            throw new InvalidDescriptorException(signature);
        }

        /// <summary>
        /// Parse a signature containing a single ThrowsSignature
        /// </summary>
        private static TypeReference ParseThrowsSignature(string signature, ref int index)
        {
            var code = signature[index];
            if (code == 'L')
            {
                // ClassTypeSignature
                return ParseClassTypeSignature(signature, ref index);
            }

            if (code == 'T')
            {
                // TypeVariableSignature
                return ParseTypeVariableSignature(signature, ref index);
            }

            throw new InvalidDescriptorException(signature);
        }

        /// <summary>
        /// Parse a signature containing a single TypeSignature.
        /// </summary>
        private static TypeReference ParseTypeSignature(string signature, ref int index)
        {
            var code = signature[index];

            // Try base type
            BaseTypeReference type;
            if (BaseTypeReference.TryGetByCode(code, out type))
            {
                index++;
                return type;
            }

            return ParseFieldTypeSignature(signature, ref index);
        }

        /// <summary>
        /// Parse a signature containing a single TypeVariableSignature.
        /// </summary>
        private static TypeReference ParseTypeVariableSignature(string signature, ref int index)
        {
            var code = signature[index++];
            if (code != 'T')
            {
                throw new InvalidSignatureException(signature);
            }

            // Object class
            var end = signature.IndexOf(';', index);
            if (end < 0)
                throw new InvalidDescriptorException(signature);
            var name = signature.Substring(index, end - index);
            index = end + 1;
            return new TypeVariableReference(name);
        }

        /// <summary>
        /// Parse a signature containing a single ClassTypeSignature.
        /// </summary>
        private static TypeReference TryParseClassTypeSignature(string signature, ref int index)
        {
            return ((index < signature.Length) && (signature[index] == 'L')) ? ParseClassTypeSignature(signature, ref index) : null;
        }

        /// <summary>
        /// Parse a signature containing a single ClassTypeSignature.
        /// </summary>
        private static TypeReference ParseClassTypeSignature(string signature, ref int index)
        {
            var code = signature[index++];
            if (code != 'L')
            {
                throw new InvalidSignatureException(signature);
            }

            // Object class
            var end = signature.IndexOfAny(new[] { '.', '<', ';' }, index);
            if (end < 0)
                throw new InvalidDescriptorException(signature);
            var name = signature.Substring(index, end - index);
            index = end;

            List<TypeArgument> arguments = null;
            if (signature[index] == '<')
            {
                // Add type arguments
                arguments = ParseTypeArguments(signature, ref index);
            }

            var result = new ObjectTypeReference(name, arguments);

            // Parse suffixes
            while (signature[index] == '.')
            {
                index++;
                end = signature.IndexOfAny(new[] { '.', '<', ';' }, index);
                if (end < 0)
                    throw new InvalidDescriptorException(signature);
                var identifier = signature.Substring(index, end - index);
                index = end;

                arguments = null;
                if (signature[index] == '<')
                {
                    arguments = ParseTypeArguments(signature, ref index);
                }
                name = name + "$" + identifier;
                result = new ObjectTypeReference(name, arguments, result);
            }

            // ';' expected
            if (signature[index] != ';')
                throw new InvalidSignatureException(signature);
            index++;

            return result;
        }

        /// <summary>
        /// Parse a signature containing a single ClassTypeSignature.
        /// </summary>
        private static List<TypeArgument> ParseTypeArguments(string signature, ref int index)
        {
            var code = signature[index++];
            if (code != '<')
                throw new InvalidSignatureException(signature);

            var list = new List<TypeArgument>();
            while (true)
            {
                list.Add(ParseTypeArgument(signature, ref index));
                if (signature[index] == '>')
                {
                    // End of type arguments
                    index++;
                    return list;
                }
            }
        }

        /// <summary>
        /// Parse a signature containing a single ClassTypeSignature.
        /// </summary>
        private static TypeArgument ParseTypeArgument(string signature, ref int index)
        {
            var code = signature[index];
            if (code == '*')
            {
                // Any
                index++;
                return TypeArgument.Any;
            }

            var wildcard = TypeArgumentWildcard.None;
            if (code == '+')
            {
                index++;
                wildcard = TypeArgumentWildcard.Plus;
            }
            else if (code == '-')
            {
                index++;
                wildcard = TypeArgumentWildcard.Minus;
            }

            var element = ParseFieldTypeSignature(signature, ref index);
            return new TypeArgument(wildcard, element);
        }

        /// <summary>
        /// Parse a signature containing a single FormalTypeParameters.
        /// </summary>
        private static List<TypeParameter> TryParseFormalTypeParameters(string signature, ref int index)
        {
            return (signature[index] == '<') ? ParseFormalTypeParameters(signature, ref index) : null;
        }

        /// <summary>
        /// Parse a signature containing a single FormalTypeParameters.
        /// </summary>
        private static List<TypeParameter> ParseFormalTypeParameters(string signature, ref int index)
        {
            var code = signature[index++];
            if (code != '<')
                throw new InvalidSignatureException(signature);

            var list = new List<TypeParameter>();
            while (true)
            {
                // FormalTypeParameter
                var end = signature.IndexOf(':', index);
                if (end < 0)
                    throw new InvalidSignatureException(signature);
                var identifier = signature.Substring(index, end - index);
                index = end + 1;

                // ClassBound
                var classBound = TryParseFieldTypeSignature(signature, ref index);
                
                // Interface bound
                var interfaceBound = new List<TypeReference>();
                while (signature[index] == ':')
                {
                    index++;
                    interfaceBound.Add(ParseFieldTypeSignature(signature, ref index));
                }

                list.Add(new TypeParameter(identifier, classBound, interfaceBound));
                if (signature[index] == '>')
                {
                    // End of type parameters
                    index++;
                    return list;
                }
            }
        }

        /// <summary>
        /// Parse a signature containing a single ClassSignature
        /// </summary>
        private static ClassSignature ParseClassSignature(string signature, ref int index)
        {
            var parameters = TryParseFormalTypeParameters(signature, ref index) ?? new List<TypeParameter>();
            var superClass = ParseClassTypeSignature(signature, ref index);

            var interfaces = new List<TypeReference>();
            while (true)
            {
                var sig = TryParseClassTypeSignature(signature, ref index);
                if (sig == null)
                    break;
                interfaces.Add(sig);
            }
            return new ClassSignature(signature, parameters, superClass, interfaces);
        }

        /// <summary>
        /// Parse a signature containing a single method signature
        /// </summary>
        private static MethodSignature ParseMethodSignature(string signature, ref int index)
        {
            // FormatTypeParameters (opt)
            var typeParameters = TryParseFormalTypeParameters(signature, ref index) ?? new List<TypeParameter>();

            // '(' TypeSignature ')'
            var code = signature[index++];
            if (code != '(')
            {
                throw new InvalidDescriptorException(signature);
            }

            var parameters = new List<TypeReference>();
            while (signature[index] != ')')
            {
                var type = ParseTypeSignature(signature, ref index);
                parameters.Add(type);
            }

            // Skip ')'
            index++;

            // Return type
            TypeReference returnType;
            if (signature[index] == 'V')
            {
                index++;
                returnType = VoidTypeReference.Instance;
            }
            else
            {
                returnType = ParseTypeSignature(signature, ref index);
            }

            var throws = new List<TypeReference>();
            while ((index < signature.Length) && (signature[index] == '^'))
            {
                throws.Add(ParseThrowsSignature(signature, ref index));
            }
            return new MethodSignature(signature, typeParameters, returnType, parameters, throws);
        }
    }
}
