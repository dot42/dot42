using Dot42.CompilerLib.Structure.DotNet;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Dex related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Create an android EnclosingClass annotation and attach it to the given provider.
        /// </summary>
        public static void AddEnclosingClassAnnotation(this IAnnotationProvider provider, ClassReference @class)
        {
            var annotation = new Annotation { Type = new ClassReference("dalvik/annotation/EnclosingClass"), Visibility = AnnotationVisibility.System };
            annotation.Arguments.Add(new AnnotationArgument("value", @class));
            provider.Annotations.Add(annotation);
        }

        /// <summary>
        /// Create an android InnerClass annotation and attach it to the given provider.
        /// </summary>
        public static void AddInnerClassAnnotation(this IAnnotationProvider provider, string simpleName, AccessFlags accessFlags)
        {
            var annotation = new Annotation { Type = new ClassReference("dalvik/annotation/InnerClass"), Visibility = AnnotationVisibility.System };
            annotation.Arguments.Add(new AnnotationArgument("name", simpleName));
            annotation.Arguments.Add(new AnnotationArgument("accessFlags", (int)accessFlags));
            provider.Annotations.Add(annotation);
        }

        /// <summary>
        /// Create an android MemberClasses annotation and attach it to the given provider.
        /// </summary>
        public static void AddMemberClassesAnnotation(this IAnnotationProvider provider, ClassReference[] classes)
        {
            var annotation = new Annotation { Type = new ClassReference("dalvik/annotation/MemberClasses"), Visibility = AnnotationVisibility.System };
            annotation.Arguments.Add(new AnnotationArgument("value", classes));
            provider.Annotations.Add(annotation);
        }

        /// <summary>
        /// Create a IGnericDefinition annotation and attaches it to the given provider.
        /// </summary>
        public static void AddGenericDefinitionAnnotationIfGeneric(this IAnnotationProvider provider, XTypeReference xtype, AssemblyCompiler compiler, DexTargetPackage targetPackage, bool forceTypeDefinition=false)
        {
            if (!xtype.IsGenericInstance && !xtype.IsGenericParameter)
                return;

            Annotation annotation = GenericDefinitionAnnotationFactory.CreateAnnotation(xtype, forceTypeDefinition, compiler, targetPackage);

            if(annotation != null)
                provider.Annotations.Add(annotation);
        }

        
    }
}
