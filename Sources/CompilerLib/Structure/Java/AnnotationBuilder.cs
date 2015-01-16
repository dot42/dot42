using System;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.JvmClassLib.Attributes;
using JAnnotation = Dot42.JvmClassLib.Attributes.Annotation;
using DAnnotation = Dot42.DexLib.Annotation;

namespace Dot42.CompilerLib.Structure.Java
{
    /// <summary>
    /// Build Dex annotations from java annotations
    /// </summary>
    internal static class AnnotationBuilder
    {
        /// <summary>
        /// Build annotations
        /// </summary>
        public static void BuildAnnotations(IAttributeProvider source, IAnnotationProvider annTarget, DexTargetPackage targetPackage, XModule module)
        {
            var attr = source.Attributes.OfType<RuntimeVisibleAnnotationsAttribute>().FirstOrDefault();
            if (attr == null)
                return;

            foreach (var ann in attr.Annotations)
            {
                try
                {
                    annTarget.Annotations.Add(CreateAnnotation(ann, targetPackage, module));
                }
                catch (Exception)
                {
                    Console.WriteLine(string.Format("Cannot convert annotation of type {0} in {1}", ann.AnnotationTypeName, source));
                }
            }
        }

        private static DAnnotation CreateAnnotation(JAnnotation source, DexTargetPackage targetPackage, XModule module)
        {
            // Get annotation type
            var type = source.AnnotationType.GetClassReference(XTypeUsageFlags.AnnotationType, targetPackage, module);

            var arguments = source.ValuePairs.Select(CreateAnnotationArgument).ToArray();

            return new DAnnotation(type, AnnotationVisibility.Runtime, arguments);
        }

        private static AnnotationArgument CreateAnnotationArgument(ElementValuePair pair)
        {
            return pair.Value.Accept(AnnotationArgumentBuilder.Instance, pair.ElementName);
        }

        private sealed class AnnotationArgumentBuilder : IElementValueVisitor<AnnotationArgument, string>
        {
            internal static readonly AnnotationArgumentBuilder Instance = new AnnotationArgumentBuilder();

            public AnnotationArgument Visit(AnnotationElementValue value, string data)
            {
                throw new NotImplementedException();
            }

            public AnnotationArgument Visit(ArrayElementValue value, string data)
            {
                throw new NotImplementedException();
            }

            public AnnotationArgument Visit(ClassElementValue value, string data)
            {
                throw new NotImplementedException();
            }

            public AnnotationArgument Visit(ConstElementValue value, string data)
            {
                return new AnnotationArgument(data, value.Value);
            }

            public AnnotationArgument Visit(EnumConstElementValue value, string data)
            {
                throw new NotImplementedException();
            }
        }
    }
}
