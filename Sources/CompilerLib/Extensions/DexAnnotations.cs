using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;

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
        /// Create a IGnericDefinition annotation and attaches it to the given provider.
        /// TODO: this might better belong somewhere else.
        /// </summary>
        public static void AddGenericDefinitionAnnotationIfGeneric(this IAnnotationProvider provider, XTypeReference xtype, AssemblyCompiler compiler, DexTargetPackage targetPackage, bool forceTypeDefinition=false)
        {
            if (!xtype.IsGenericInstance && !xtype.IsGenericParameter)
                return;

            Annotation annotation = GetGenericDefinitionAnnotationForType(xtype, forceTypeDefinition, compiler, targetPackage);
            if(annotation != null)
                provider.Annotations.Add(annotation);
        }

        public static Annotation GetGenericDefinitionAnnotationForType(XTypeReference xtype, bool forceTypeDefinition, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var genericsDefAnnotationClass = compiler.GetDot42InternalType(InternalConstants.GenericDefinitionAnnotation)
                .GetClassReference(targetPackage);
            var annotation = new Annotation {Type = genericsDefAnnotationClass, Visibility = AnnotationVisibility.Runtime};

            if (xtype.IsGenericInstance)
            {
                bool handled = false;
                List<object> genericArguments = new List<object>();

                if (xtype.GetElementType().IsNullableT())
                {
                    // privitives and enums are represented by their marker classes. 
                    // no annotation needed.
                    var argument = ((XGenericInstanceType) xtype).GenericArguments[0];
                    if (argument.IsEnum() || argument.IsPrimitive && !forceTypeDefinition)
                    {
                        return null;
                    }
                        
                    // structs have marker classes.
                    var classRef = xtype.GetReference(targetPackage) as ClassReference;
                    var @class = classRef == null ? null : targetPackage.DexFile.GetClass(classRef.Fullname);
                    if (@class != null && @class.NullableMarkerClass != null)
                    {
                        annotation.Arguments.Add(new AnnotationArgument("GenericInstanceType", @class.NullableMarkerClass));
                        handled = true;
                    }
                }

                if (!handled)
                {
                    foreach (var arg in ((XGenericInstanceType) xtype).GenericArguments)
                    {
                        if (arg.IsGenericParameter)
                        {
                            var gparm = (XGenericParameter) arg;

                            // TODO: if we wanted to annotate methods as well, we should differentiate 
                            //       between generic method arguments and generic type arguments.

                            genericArguments.Add(gparm.Position);
                        }
                        else if (arg.IsGenericInstance)
                        {
                            var giparm = GetGenericDefinitionAnnotationForType((XGenericInstanceType) arg, true,compiler, targetPackage);
                            genericArguments.Add(giparm);
                        }
                        else
                        {
                            genericArguments.Add(arg.GetReference(targetPackage));
                        }
                    }
                    annotation.Arguments.Add(new AnnotationArgument("GenericArguments", genericArguments.ToArray()));
                }
            }
            else // generic parameter
            {
                var parm = (XGenericParameter) xtype;
                annotation.Arguments.Add(new AnnotationArgument("GenericParameter", parm.Position));
            }

            if(forceTypeDefinition && annotation.Arguments.All(a => a.Name != "GenericInstanceType"))
                annotation.Arguments.Add(new AnnotationArgument("GenericTypeDefinition", xtype.ElementType.GetReference(targetPackage)));

            return annotation;
        }
    }
}
