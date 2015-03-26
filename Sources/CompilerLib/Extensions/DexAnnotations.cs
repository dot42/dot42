using System;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.Utility;

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


        /// <summary>
        /// Create an INullableT annotation and attach it to the given provider.
        /// </summary>
        public static void AddNullableTAnnotation(this IAnnotationProvider provider, ClassReference type)
        {
            var annotation = new Annotation { Type = new ClassReference("dot42/Internal/INullableT"), Visibility = AnnotationVisibility.Runtime };
            annotation.Arguments.Add(new AnnotationArgument("Type", type));
            provider.Annotations.Add(annotation);
        }

        /// <summary>
        /// Create an INullableT annotation and attach it to the given provider.
        /// </summary>
        public static void AddNullableTAnnotationIfNullableT(this IAnnotationProvider provider, XTypeReference xtype, DexTargetPackage targetPackage)
        {
            //if (xtype.IsNullableT())
            //    return;
            if (!xtype.GetElementType().IsNullableT())
                return;

            var classRef = xtype.GetReference(targetPackage) as ClassReference;
            
            if(classRef == null)
                DLog.Warning(DContext.CompilerCodeGenerator, "Warning: Element {0} has no class refrence. Not creating INullableT annotation.", xtype);                
            
            var @class = classRef == null? null : targetPackage.DexFile.GetClass(classRef.Fullname);

            if (@class == null || @class.NullableMarkerClass == null)
                return;

            var annotation = new Annotation { Type = new ClassReference("dot42/Internal/INullableT"), Visibility = AnnotationVisibility.Runtime };
            annotation.Arguments.Add(new AnnotationArgument("Type", @class.NullableMarkerClass));
            provider.Annotations.Add(annotation);
        }

    }
}
