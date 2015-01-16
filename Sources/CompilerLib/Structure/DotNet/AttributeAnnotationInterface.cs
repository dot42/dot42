using System.Collections.Generic;
using Dot42.DexLib;
using Mono.Cecil;
using FieldDefinition = Mono.Cecil.FieldDefinition;
using MethodDefinition = Dot42.DexLib.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Holds a class definition defining the annotation interface used to store values of a custom attribute type.
    /// </summary>
    internal class AttributeAnnotationInterface
    {
        private readonly ClassDefinition annotationInterfaceClass;
        private readonly Dictionary<FieldDefinition, MethodDefinition> fieldToGetMethodMap = new Dictionary<FieldDefinition, MethodDefinition>();
        private readonly Dictionary<PropertyDefinition, MethodDefinition> propertyToGetMethodMap = new Dictionary<PropertyDefinition, MethodDefinition>();
        private readonly Dictionary<Mono.Cecil.MethodDefinition, AttributeCtorMapping> ctorMap = new Dictionary<Mono.Cecil.MethodDefinition, AttributeCtorMapping>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AttributeAnnotationInterface(ClassDefinition annotationInterfaceClass)
        {
            this.annotationInterfaceClass = annotationInterfaceClass;
        }

        /// <summary>
        /// Gets the annotation interface used to store attribute values.
        /// </summary>
        public ClassDefinition AnnotationInterfaceClass
        {
            get { return annotationInterfaceClass; }
        }

        /// <summary>
        /// Gets a mapping between the .NET (attribute) field names and their annotation get method name.
        /// </summary>
        public Dictionary<FieldDefinition, MethodDefinition> FieldToGetMethodMap { get { return fieldToGetMethodMap; } }

        /// <summary>
        /// Gets a mapping between the .NET (attribute) property names and their annotation get method name.
        /// </summary>
        public Dictionary<PropertyDefinition, MethodDefinition> PropertyToGetMethodMap { get { return propertyToGetMethodMap; } }

        /// <summary>
        /// Gets a mapping between the .NET (attribute) ctor and a static method that builds the attribute from it's annotation interface.
        /// </summary>
        public Dictionary<Mono.Cecil.MethodDefinition, AttributeCtorMapping> CtorMap { get { return ctorMap; } }
    }
}
