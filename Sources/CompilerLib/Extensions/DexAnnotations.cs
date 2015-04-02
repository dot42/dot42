using System.Collections.Generic;
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
        public static void AddGenericMemberAnnotationIfGeneric(this IAnnotationProvider provider, XTypeReference xtype, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            if (!xtype.IsGenericInstance && !xtype.IsGenericParameter)
                return;

            var genericsMemberClass = compiler.GetDot42InternalType("IGenericMember").GetClassReference(targetPackage);
            var annotation = new Annotation {Type = genericsMemberClass, Visibility = AnnotationVisibility.Runtime};

            if (xtype.IsGenericInstance)
            {
                bool handled = false;
                List<object> genericArguments = new List<object>();

                if (xtype.GetElementType().IsNullableT())
                {
                    // privitive and enums are represented by their marker classes. 
                    // no annotation needed.
                    var argument = ((XGenericInstanceType) xtype).GenericArguments[0];
                    if (argument.IsEnum() || argument.IsPrimitive)
                        return;
                    
                    // structs have marker classes.
                    var classRef = xtype.GetReference(targetPackage) as ClassReference;
                    var @class = classRef == null ? null : targetPackage.DexFile.GetClass(classRef.Fullname);
                    if (@class != null && @class.NullableMarkerClass != null)
                    {
                        annotation.Arguments.Add(new AnnotationArgument("GenericInstanceType",
                                                 new object[] {@class.NullableMarkerClass}));
                        handled = true;
                    }
                }
                
                if(!handled)
                {
                    foreach (var x in ((XGenericInstanceType) xtype).GenericArguments)
                    {
                        if (!x.IsGenericParameter)
                        {
                            genericArguments.Add(x.GetReference(targetPackage));
                        }
                        else
                        {
                            var gparm = (XGenericParameter) x;

                            // TODO: if we wanted to annotate methods as well, we should differentiate 
                            //       between generic method arguments and generic type arguments.
                            genericArguments.Add(gparm.Position);
                        }
                    }
                    annotation.Arguments.Add(new AnnotationArgument("GenericArguments", genericArguments.ToArray()));
                }
            }
            else // generic parameter
            {
                var parm = (XGenericParameter)xtype;
                annotation.Arguments.Add(new AnnotationArgument("GenericParameter", parm.Position));

                // TODO: handle parameters of generic parameters.
            }

            provider.Annotations.Add(annotation);
        }

    }
}
