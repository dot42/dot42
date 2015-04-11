using System.Collections.Generic;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Mono.Cecil;

namespace Dot42.CompilerLib.Structure.DotNet
{
    internal static class AssemblyTypesBuilder
    {
        public static void CreateAssemblyTypesAnnotations(AssemblyCompiler compiler, DexTargetPackage targetPackage,
                                                          IEnumerable<TypeDefinition> reachableTypes)
        {
            var assemblyTypes  = compiler.GetDot42InternalType("AssemblyTypes").GetClassReference(targetPackage);
            var iAssemblyTypes = compiler.GetDot42InternalType("IAssemblyTypes").GetClassReference(targetPackage);
            var iAssemblyType  = compiler.GetDot42InternalType("IAssemblyType").GetClassReference(targetPackage);

            var types = new List<Annotation>();

            foreach (var type in reachableTypes)
            {
                var assemblyName = type.module.Assembly.Name.Name;
                if (assemblyName == "dot42")
                {
                    // group all android types into virtual "android" assembly,
                    // so that MvvmCross can find all view-types.
                    // -- is this a hack?
                    if (type.Namespace.StartsWith("Android"))
                        assemblyName = "android";
                    else // ignore other types.
                        continue;
                }
                var a = new Annotation(iAssemblyType, AnnotationVisibility.Runtime,
                    new AnnotationArgument("AssemblyName", assemblyName),
                    new AnnotationArgument("Type", type.GetReference(targetPackage, compiler.Module)));
                types.Add(a);
            }

            var anno = new Annotation(iAssemblyTypes, AnnotationVisibility.Runtime,
                                      new AnnotationArgument("Types", types.ToArray()));

            ((IAnnotationProvider)assemblyTypes).Annotations.Add(anno);
        }

    }
}
