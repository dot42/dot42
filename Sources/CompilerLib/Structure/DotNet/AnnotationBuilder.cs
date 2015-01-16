using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build Annotation instances for attributes found in the given attribute provider.
    /// </summary>
    internal static class AnnotationBuilder
    {
        /// <summary>
        /// Create annotations for all included attributes
        /// </summary>
        public static void Create(AssemblyCompiler compiler, ICustomAttributeProvider attributeProvider,
                                  IAnnotationProvider annotationProvider, DexTargetPackage targetPackage, bool customAttributesOnly = false)
        {
            if (!attributeProvider.HasCustomAttributes)
                return;
            var annotations = new List<Annotation>();
            foreach (var attr in attributeProvider.CustomAttributes)
            {
                var attributeType = attr.AttributeType.Resolve();
                if (!attributeType.HasIgnoreAttribute())
                {
                    Create(compiler, attr, attributeType, annotations, targetPackage);
                }
            }
            if (annotations.Count > 0)
            {
                // Create 1 IAttributes annotation
                var attrsAnnotation = new Annotation { Visibility = AnnotationVisibility.Runtime };
                attrsAnnotation.Type = compiler.GetDot42InternalType("IAttributes").GetClassReference(targetPackage);
                attrsAnnotation.Arguments.Add(new AnnotationArgument("Attributes", annotations.ToArray()));
                annotationProvider.Annotations.Add(attrsAnnotation);
            }
            if (!customAttributesOnly)
            {
                // Add annotations specified using AnnotationAttribute
                foreach (var attr in attributeProvider.CustomAttributes.Where(IsAnnotationAttribute))
                {
                    var annotationType = (TypeReference) attr.ConstructorArguments[0].Value;
                    var annotationClass = annotationType.GetClassReference(targetPackage, compiler.Module);
                    annotationProvider.Annotations.Add(new Annotation(annotationClass, AnnotationVisibility.Runtime));
                }
            }
        }

        /// <summary>
        /// Create an annotation for the given attribute
        /// </summary>
        private static void Create(AssemblyCompiler compiler, CustomAttribute attribute, TypeDefinition attributeType,
                                   List<Annotation> annotationList, DexTargetPackage targetPackage)
        {
            // Gets the mapping for the type of attribute
            var mapping = compiler.GetAttributeAnnotationType(attributeType);
            var ctorMap = mapping.CtorMap[attribute.Constructor.Resolve()];

            // Create annotation
            var annotation = new Annotation {Visibility = AnnotationVisibility.Runtime};
            annotation.Type = mapping.AnnotationInterfaceClass;

            // Add ctor arguments
            var argIndex = 0;
            foreach (var arg in attribute.ConstructorArguments)
            {
                var name = ctorMap.ArgumentGetters[argIndex].Name;
                annotation.Arguments.Add(CreateAnnotationArgument(name, arg.Type, arg.Value, targetPackage, compiler.Module));
                argIndex++;
            }

            // Add field values
            foreach (var arg in attribute.Fields)
            {
                var entry = mapping.FieldToGetMethodMap.First(x => x.Key.Name == arg.Name);
                var name = entry.Value.Name;
                annotation.Arguments.Add(CreateAnnotationArgument(name, arg.Argument.Type, arg.Argument.Value, targetPackage, compiler.Module));
            }

            // Add property values
            foreach (var arg in attribute.Properties)
            {
                if (mapping.PropertyToGetMethodMap.Keys.Any(x => x.Name == arg.Name))
                {
                    var entry = mapping.PropertyToGetMethodMap.First(x => x.Key.Name == arg.Name);
                    var name = entry.Value.Name;
                    annotation.Arguments.Add(CreateAnnotationArgument(name, arg.Argument.Type, arg.Argument.Value, targetPackage, compiler.Module));
                }
            }

            // Create attribute annotation
            var attrAnnotation = new Annotation { Visibility = AnnotationVisibility.Runtime };
            attrAnnotation.Type = compiler.GetDot42InternalType("IAttribute").GetClassReference(targetPackage);
            attrAnnotation.Arguments.Add(new AnnotationArgument("AttributeBuilder", ctorMap.Builder));
            attrAnnotation.Arguments.Add(new AnnotationArgument("AttributeType", attributeType.GetReference(targetPackage, compiler.Module)));
            attrAnnotation.Arguments.Add(new AnnotationArgument("Annotation", annotation));

            // Add annotation
            annotationList.Add(attrAnnotation);
        }

        /// <summary>
        /// Create an argument for an annotation.
        /// </summary>
        private static AnnotationArgument CreateAnnotationArgument(string name, TypeReference valueType, object value, DexTargetPackage targetPackage, XModule module)
        {
            if (valueType.IsSystemType())
            {
                // Convert .NET type reference to Dex type reference
                value = ((TypeReference) value).GetReference(targetPackage, module);
            }
            return new AnnotationArgument(name, value);
        }

        /// <summary>
        /// Is the given custom attribute of type Dot42.AnnotationAttribute?
        /// </summary>
        private static bool IsAnnotationAttribute(CustomAttribute ca)
        {
            return (ca.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                   (ca.AttributeType.Name == AttributeConstants.AnnotationAttributeName);
        }
    }
}
