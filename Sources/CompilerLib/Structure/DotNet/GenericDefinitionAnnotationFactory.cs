using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.Structure.DotNet
{
    public class GenericDefinitionAnnotationFactory
    {
        public static Annotation CreateAnnotation(XTypeReference xtype, bool forceTypeDefinition, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var genericsDefAnnotationClass = compiler.GetDot42InternalType(InternalConstants.GenericDefinitionAnnotation)
                                                     .GetClassReference(targetPackage);
            var genericsArgAnnotationClass = compiler.GetDot42InternalType(InternalConstants.GenericArgumentAnnotation)
                                                     .GetClassReference(targetPackage);

            var annotation = new Annotation { Type = genericsDefAnnotationClass, Visibility = AnnotationVisibility.Runtime };

            if (xtype.IsGenericInstance)
            {
                bool handled = false;
                List<Annotation> genericArguments = new List<Annotation>();

                if (xtype.GetElementType().IsNullableT())
                {
                    // privitives and enums are represented by their marker classes. 
                    // no annotation needed.
                    var argument = ((XGenericInstanceType)xtype).GenericArguments[0];
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
                    foreach (var arg in ((XGenericInstanceType)xtype).GenericArguments)
                    {
                        var argAnn = new Annotation { Type = genericsArgAnnotationClass, Visibility = AnnotationVisibility.Runtime };
                        if (arg.IsGenericParameter)
                        {
                            var gparm = (XGenericParameter)arg;

                            // TODO: if we wanted to annotate methods as well, we should differentiate 
                            //       between generic method arguments and generic type arguments.
                            argAnn.Arguments.Add(new AnnotationArgument("ContainingTypeArgumentIndex", gparm.Position));
                        }
                        else if (arg.IsGenericInstance)
                        {
                            var giparm = CreateAnnotation((XGenericInstanceType)arg, true, compiler, targetPackage);
                            argAnn.Arguments.Add(new AnnotationArgument("NestedType", giparm));
                        }
                        else
                        {
                            argAnn.Arguments.Add(new AnnotationArgument("FixedType", arg.GetReference(targetPackage)));
                        }
                        genericArguments.Add(argAnn);
                    }
                    annotation.Arguments.Add(new AnnotationArgument("GenericArguments", genericArguments.ToArray()));
                }
            }
            else // generic parameter
            {
                var parm = (XGenericParameter)xtype;
                annotation.Arguments.Add(new AnnotationArgument("GenericParameter", parm.Position));
            }

            if (forceTypeDefinition && annotation.Arguments.All(a => a.Name != "GenericInstanceType"))
                annotation.Arguments.Add(new AnnotationArgument("GenericTypeDefinition", xtype.ElementType.GetReference(targetPackage)));

            return annotation;
        }
    }
}
