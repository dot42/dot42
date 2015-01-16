using System.Collections.Generic;
using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an annotation structure.
    /// CLASS FILE FORMAT 4.7.16
    /// </summary>
    public sealed class Annotation
    {
        private readonly ConstantPool cp;
        private readonly int typeIndex;
        private readonly ElementValuePair[] valuePairs;
        private TypeReference type;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="typeIndex"></param>
        /// <param name="valuePairs"></param>
        internal Annotation(ConstantPool cp, int typeIndex, ElementValuePair[] valuePairs)
        {
            this.cp = cp;
            this.typeIndex = typeIndex;
            this.valuePairs = valuePairs;
        }

        public string AnnotationTypeName
        {
            get { return ((ConstantPoolUtf8) cp[typeIndex]).Value; }
        }

        public TypeReference AnnotationType
        {
            get { return type ?? (type = Parse(AnnotationTypeName)); }
        }

        /// <summary>
        /// Parse the given descriptor into a type reference.
        /// </summary>
        private static TypeReference Parse(string descriptor)
        {
            if (descriptor.StartsWith("["))
                return new ArrayTypeReference(Parse(descriptor.Substring(1)));
            if (descriptor.StartsWith("L") && descriptor.EndsWith(";"))
                return Parse(descriptor.Substring(1, descriptor.Length - 2));
            BaseTypeReference baseType;
            if ((descriptor.Length == 1) && BaseTypeReference.TryGetByCode(descriptor[0], out baseType))
                return baseType;
            return new ObjectTypeReference(descriptor, null);
        }


        public IEnumerable<ElementValuePair> ValuePairs { get { return valuePairs; } }
    }
}
