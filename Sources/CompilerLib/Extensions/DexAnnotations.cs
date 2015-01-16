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
            annotation.Arguments.Add(new AnnotationArgument("class", @class));
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
    }
}
