using System.Collections.Generic;
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
        public static Annotation CreateAnnotation(XTypeReference xtype, bool forceTypeDefinition,
            AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var genericsDefAnnotationClass = compiler.GetDot42InternalType(InternalConstants.GenericDefinitionAnnotation)
                .GetClassReference(targetPackage);

            var annotation = new Annotation
            {
                Type = genericsDefAnnotationClass,
                Visibility = AnnotationVisibility.Runtime
            };

            string s = GetDefinition(xtype, forceTypeDefinition, compiler, targetPackage);
            if (string.IsNullOrEmpty(s)) return null;

            annotation.Arguments.Add(new AnnotationArgument("Definition", s));

            return annotation;
        }

        /// <summary>
        /// The syntax is experimental, and neither fully fixed nor officially defined.
        /// </summary>
        private static string GetDefinition(XTypeReference xtype, bool forceTypeDefinition, AssemblyCompiler compiler,
            DexTargetPackage targetPackage)
        {
            StringBuilder s = new StringBuilder();

            // TODO: reorganize, so that the syntax becomes clear.

            bool setGenericInstanceType = false;

            if (xtype.IsGenericInstance)
            {
                bool handled = false;

                if (xtype.GetElementType().IsNullableT())
                {
                    // privitives and enums are represented by their marker classes. 
                    // no annotation needed.
                    var argument = ((XGenericInstanceType) xtype).GenericArguments[0];
                    if (!forceTypeDefinition && (argument.IsEnum() || argument.IsPrimitive))
                    {
                        return "";
                    }

                    // structs have marker classes.
                    var classRef = xtype.GetReference(targetPackage) as ClassReference;
                    var @class = classRef == null ? null : targetPackage.DexFile.GetClass(classRef.Fullname);
                    if (@class != null && @class.NullableMarkerClass != null)
                    {
                        s.Append("@");
                        s.Append(GetClassName(@class.NullableMarkerClass));
                        setGenericInstanceType = true;
                        handled = true;
                    }
                }

                if (!handled)
                {
                    bool isFirst = true;
                    foreach (var arg in ((XGenericInstanceType) xtype).GenericArguments)
                    {
                        if (!isFirst) s.Append(",");
                        isFirst = false;

                        if (arg.IsGenericParameter)
                        {
                            var gparm = (XGenericParameter) arg;

                            // TODO: if we wanted to annotate methods as well, we should differentiate 
                            //       between generic method arguments and generic type arguments.
                            s.Append("!");
                            s.Append(gparm.Position);
                        }
                        else if (arg.IsGenericInstance)
                        {
                            var giparm = GetDefinition((XGenericInstanceType)arg, true, compiler, targetPackage);
                            s.Append("{");
                            s.Append(giparm);
                            s.Append("}");
                        }
                        else
                        {
                            s.Append(GetClassName(arg.GetReference(targetPackage)));
                        }
                    }
                }
            }
            else // generic parameter
            {
                var parm = (XGenericParameter) xtype;
                s.Append("!!");
                s.Append(parm.Position);
            }

            if (forceTypeDefinition && !setGenericInstanceType)
            {
                string def = GetClassName(xtype.ElementType.GetReference(targetPackage));
                s.Insert(0, def + "<");
                s.Append(">");
            }

            return s.ToString();
        }

        private static string GetClassName(TypeReference r)
        {
            if (r.IsPrimitive())
                return r.Descriptor;

            var desc = r.Descriptor;

            // prepare for direct class.ForName usage:
            // remove ';' (and L for non-arrays)
            if (desc.StartsWith("["))
                desc = desc.Substring(0, desc.Length - 1);
            else 
                desc = desc.Substring(1, desc.Length - 2);

            return desc.Replace('/', '.');
        }
    }
}
