using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Dot42.LoaderLib.Extensions;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// create a redirecting stub if a DexImport or JavaImport or DexNative 
    /// method implements an interface, and but has the wrong name.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class MethodInterfaceImportedlmpementationsConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 510; } // do this after unique signature converter.
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            public void Convert(ReachableContext reachableContext)
            {
                // Collect all names
                var reachableInterfaces = reachableContext.ReachableTypes.Where(i=>i.IsInterface).ToList();
                var interfaceToImplementingTypes = reachableContext.ReachableTypes
                                                                   .Except(reachableInterfaces)
                                                                   .SelectMany(t => GetInterfaces(t).Distinct(), Tuple.Create)
                                                                   .Where(e=>e.Item2.IsReachable)
                                                                   .ToLookup(e=>e.Item2, e=>e.Item1);

                // TODO: i don't think the we cover all possible cases here yet.
                //       what about methods overriding imported methods?

                foreach (var intf in reachableInterfaces)
                foreach(var iMethod in intf.Methods)
                {
                    // ignore DexImport Interface types. TODO: also cover the reverse: a method implementing 
                    //                                         both a import-interface as well as a .NET interface.
                    if (iMethod.GetDexOrJavaImportAttribute() != null || iMethod.GetDexNameAttribute() != null)
                        continue;
                    
                    var nativeImpls = interfaceToImplementingTypes[intf].Select(t=>Tuple.Create(t, iMethod.GetImplementation(t)))
                                                                        .Where(e => e.Item2 != null 
                                                                                 && (e.Item2.GetDexOrJavaImportAttribute() != null 
                                                                                  || e.Item2.GetDexNameAttribute() != null));
                    foreach (var typeAndImpl in nativeImpls)
                    {
                        var type = typeAndImpl.Item1;
                        var impl = typeAndImpl.Item2;

                        var finalName = GetFinalImplementationName(impl);

                        if (finalName == iMethod.Name)
                            continue;

                        // TODO: automatically create a redirecting stub.
                        
                        DLog.Error(DContext.CompilerILConverter, "Type '{0}' implements interface method '{1}' using imported method '{2}'. This will not work. Create a 'new' or explicit implementation, that redirects to the imported.", type.FullName, iMethod.FullName, impl.FullName);
                    }
                }
            }

            private static string GetFinalImplementationName(MethodDefinition impl)
            {
                string finalName;
                var importAttr = impl.GetDexOrJavaImportAttribute();
                if (importAttr != null)
                {
                    string descriptor, className;
                    importAttr.GetDexOrJavaImportNames(impl, out finalName, out descriptor, out className);
                }
                else
                {
                    var dexNameAttr = impl.GetDexNameAttribute();
                    finalName = dexNameAttr.ConstructorArguments[0].Value.ToString();
                }
                return finalName;
            }

            /// <summary>
            /// Gets all interfaces implemented by the given type.
            /// </summary>
            internal static IEnumerable<TypeReference> GetInterfaces(TypeDefinition type)
            {
                if (type.HasInterfaces)
                {
                    foreach (var intf in type.Interfaces)
                    {
                        yield return intf.Interface;
                        var intfDef = intf.Interface.GetElementType().Resolve();
                        if (intfDef != null)
                        {
                            if (intfDef != intf.Interface)
                            {
                                yield return intfDef;
                            }
                            foreach (var x in GetInterfaces(intfDef))
                            {
                                yield return x;
                            }
                        }
                    }
                }
            }
        }
    }
}