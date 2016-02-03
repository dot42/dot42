using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL;
using Dot42.DexLib;
using java.time;
using Mono.Cecil;
using MethodDefinition = Dot42.DexLib.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Holds a class definition defining the annotation interface used to store values of a custom attribute type.
    /// </summary>
    internal class AttributeAnnotationMapping
    {
        public TypeDefinition AttributeType { get; set; }
        public ClassDefinition AttributeClass { get; set; }

        private readonly Dictionary<CustomAttribute, MethodDefinition> _factoryMethodMap = new Dictionary<CustomAttribute, MethodDefinition>(new CustomAttributeEqualityComparer());

        public AttributeAnnotationMapping(TypeDefinition attributeType, ClassDefinition attributeClass)
        {
            AttributeType = attributeType;
            AttributeClass = attributeClass;
        }

        /// <summary>
        /// Gets a mapping between the .NET attribute instance a static method that builds an equivalent attribute.
        /// </summary>
        public Dictionary<CustomAttribute, MethodDefinition> FactoryMethodMap { get { return _factoryMethodMap; } }

        private class CustomAttributeEqualityComparer : IEqualityComparer<CustomAttribute>, 
                                                        IEqualityComparer<CustomAttributeArgument>,
                                                        IEqualityComparer<CustomAttributeNamedArgument>,
                                                        IEqualityComparer<object>
        {
            public bool Equals(CustomAttribute x, CustomAttribute y)
            {
                if (x.AttributeType != y.AttributeType)
                    return false;
                if (x.Constructor != y.Constructor)
                    return false;
                if (!Enumerable.SequenceEqual(x.ConstructorArguments, y.ConstructorArguments, this))
                    return false;
                if (!Enumerable.SequenceEqual(x.Fields, y.Fields, this))
                    return false;
                if (!Enumerable.SequenceEqual(x.Properties, y.Properties, this))
                    return false;
                return true;
            }

            public int GetHashCode(CustomAttribute attr)
            {
                int hashCode = attr.AttributeType.GetHashCode();
                hashCode = hashCode*13 + attr.Constructor.FullName.GetHashCode();

                hashCode = attr.ConstructorArguments.Aggregate(hashCode, Hash);
                hashCode = attr.Fields.Aggregate(hashCode, Hash);
                hashCode = attr.Properties.Aggregate(hashCode, Hash);

                return hashCode;
            }

            private static int Hash(int hashCode, CustomAttributeNamedArgument arg)
            {
                hashCode = hashCode * 13 + arg.Name.GetHashCode();
                return Hash(hashCode, arg.Argument);
            }

            private static int Hash(int hashCode, CustomAttributeArgument arg)
            {
                if (arg.Value != null)
                    hashCode = hashCode * 13 + arg.Value.GetHashCode();
                return hashCode * 13 + arg.Type.GetHashCode();
            }

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;

                if (!x.GetType().IsArray)
                {
                    return Equals(x, y);
                }

                // array
                var a1 = (object[])x;
                var a2 = (object[])y;

                return Enumerable.SequenceEqual(a1, a2, this);
            }

            public bool Equals(CustomAttributeArgument x, CustomAttributeArgument y)
            {
                if (x.Type != y.Type) return false;
                return ((IEqualityComparer<object>)this).Equals(x.Value, y.Value);
            }

            public bool Equals(CustomAttributeNamedArgument x, CustomAttributeNamedArgument y)
            {
                if (x.Name != y.Name)
                    return false;
                return Equals(x.Argument, y.Argument);
            }

            public int GetHashCode(CustomAttributeNamedArgument obj)
            {
                return 0;
            }

            public int GetHashCode(object obj)
            {
                return 0;
            }

            public int GetHashCode(CustomAttributeArgument obj)
            {
                return 0;
            }
        }
    }
}
