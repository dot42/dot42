using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using Dot42.CompilerLib.Extensions;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET types.
    /// </summary>
    internal class StandardClassBuilder : ClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public StandardClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority
        {
            get
            {
                var sortPriority = base.Type.UsedInNullableT?-50:0;
                if (HasXType() && XType.Priority > 0)
                    sortPriority += 1;
                return sortPriority;
            }
        }

        public override void CreateAnnotations(DexTargetPackage targetPackage)
        {
            base.CreateAnnotations(targetPackage);

            if (Class == null)
                return;

            bool isBaseTypeGeneric = XType.BaseType != null && (XType.BaseType.IsGenericInstance || XType.BaseType.IsGenericParameter);
            bool containsGenericInterfaces = XType.Interfaces.Any(i => i.IsGenericInstance || i.IsGenericParameter);
            bool needsFieldAnnotation = !Class.IsSynthetic && Class.Fields.Count(p => !p.IsSynthetic && !p.IsStatic) > 1;
            
            bool needsAnnotation = Class.GenericInstanceField != null 
                                || XType.GenericParameters.Count > 0 
                                || isBaseTypeGeneric 
                                || containsGenericInterfaces 
                                || needsFieldAnnotation;

            if (!needsAnnotation) return;

            var annType = Compiler.GetDot42InternalType(InternalConstants.TypeReflectionInfoAnnotation).GetClassReference(targetPackage);
            var annotation = new Annotation(annType, AnnotationVisibility.Runtime);

            if(Class.GenericInstanceField != null)
                annotation.Arguments.Add(new AnnotationArgument(InternalConstants.TypeReflectionInfoGenericArgumentsField, 
                                                                Class.GenericInstanceField.Name));

            if (XType.GenericParameters.Count > 0)
                annotation.Arguments.Add(new AnnotationArgument(InternalConstants.TypeReflectionInfoGenericArgumentCountField, XType.GenericParameters.Count));

            List<Annotation> definitions= new List<Annotation>();

            if (isBaseTypeGeneric)
                definitions.Add(AssemblyCompilerExtensions.GetGenericDefinitionAnnotationForType(
                                XType.BaseType, true, Compiler, targetPackage));

            foreach (var intf in XType.Interfaces)
            {
                if(!intf.IsGenericInstance && !intf.IsGenericParameter)
                    continue;
                definitions.Add(AssemblyCompilerExtensions.GetGenericDefinitionAnnotationForType(
                                                            intf, true, Compiler, targetPackage));
            }

            if(definitions.Count > 0)
                annotation.Arguments.Add(new AnnotationArgument(InternalConstants.TypeReflectionInfoGenericDefinitionsField, definitions.ToArray()));

            if (needsFieldAnnotation)
            {
                annotation.Arguments.Add(new AnnotationArgument(InternalConstants.TypeReflectionInfoFieldsField, 
                                         Class.Fields.Where(p=>!p.IsSynthetic && !p.IsStatic).Select(p=>p.Name).ToArray()));
            }

            Class.Annotations.Add(annotation);
        }
    }
}
