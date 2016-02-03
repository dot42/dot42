using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Dot42.DexLib.Metadata;
using Mono.Cecil;

namespace Dot42.CompilerLib.Structure.DotNet
{
    internal static class AssemblyTypesBuilder
    {
        public static void CreateAssemblyTypes(AssemblyCompiler compiler, DexTargetPackage targetPackage,
                                               IEnumerable<TypeDefinition> reachableTypes)
        {
            var xAssemblyTypes = compiler.GetDot42InternalType("AssemblyTypes");
            var assemblyTypes = (ClassDefinition)xAssemblyTypes.GetClassReference(targetPackage);
            var entryAssembly = assemblyTypes.Fields.First(f => f.Name == "EntryAssembly");
            var iAssemblyTypes = compiler.GetDot42InternalType("IAssemblyTypes").GetClassReference(targetPackage);
            
            entryAssembly.Value = compiler.Assemblies.First().Name.Name;

            List<object> values = new List<object>();
            string prevAssemblyName = null;

            foreach (var type in reachableTypes.OrderBy(t => t.Module.Assembly.Name.Name)
                                               .ThenBy(t  => t.Namespace)
                                               .ThenBy(t =>  t.Name))
            {
                var assemblyName = type.module.Assembly.Name.Name;
                if (assemblyName == "dot42")
                {
                    // group all android types into virtual "android" assembly,
                    // so that MvvmCross can find all view-types.
                    // -- is this a hack?
                    if (type.Namespace.StartsWith("Android"))
                        assemblyName = "android";
                    else // ignore other types, these will get the "default" assembly.
                        continue;
                }

                if (prevAssemblyName != assemblyName)
                {
                    values.Add("!" + assemblyName); // we need some identification of assemblies.
                    prevAssemblyName = assemblyName;
                }

                // TODO: With compilationmode=all reachable types contains  <Module>
                //       this should be excluded earlier.
                if (type.FullName == "<Module>")
                    continue;

                var tRef = type.GetReference(targetPackage, compiler.Module) as ClassReference;    
                if(tRef != null) values.Add(tRef.Fullname);
            }

            var anno = new Annotation(iAssemblyTypes, AnnotationVisibility.Runtime,
                                      new AnnotationArgument("AssemblyTypeList", values.ToArray()));
            ((IAnnotationProvider)assemblyTypes).Annotations.Add(anno);
        }
    }
}
