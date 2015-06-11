using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Mono.Cecil;
using ArrayType = Dot42.DexLib.ArrayType;

namespace Dot42.CompilerLib.Structure.DotNet
{
    internal static class AssemblyTypesBuilder
    {
        public static void CreateAssemblyTypes(AssemblyCompiler compiler, DexTargetPackage targetPackage,
                                               IEnumerable<TypeDefinition> reachableTypes)
        {
            var xAssemblyTypes = compiler.GetDot42InternalType("AssemblyTypes");
            var assmDef = (ClassDefinition)xAssemblyTypes.GetClassReference(targetPackage);
            var entryAssembly = assmDef.Fields.First(f => f.Name == "EntryAssembly");

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
                    values.Add(assemblyName);
                    prevAssemblyName = assemblyName;
                }

                try
                {
                    // TODO: with compilationmode=all reachable types contains 
                    //       types that are not to be included in the output.
                    //       (most notable 'Module')
                    //       somehow handle that without an exception.
                    var tRef = type.GetReference(targetPackage, compiler.Module);    
                    values.Add(tRef);
                }
                catch
                {
                }
            }

            var xMethod = xAssemblyTypes.Resolve().Methods.First(f => f.Name == "GetAssemblyTypeList");
            ReplaceMethodBody(xMethod, values, compiler, targetPackage);
        }

        private static void ReplaceMethodBody(XMethodDefinition method, List<object> values, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            // This takes about 6 bytes per type.

            // Maybe we could save some bytes by returning to annotations, but this
            // time unstructured annotations, i.e. an annotation that returns the 
            // same array as generated here.

            var compiledMethod = targetPackage.GetMethod(method);
            
            MethodBody body = new MethodBody(new MethodSource(method, compiledMethod.ILSource));

            var one   = body.AllocateRegister(RCategory.Temp, RType.Value);
            var index = body.AllocateRegister(RCategory.Temp, RType.Value);
            var array = body.AllocateRegister(RCategory.Temp, RType.Object);
            var rval = body.AllocateRegister(RCategory.Temp, RType.Object);


            var arrayType = new ArrayType(compiler.Module.TypeSystem.Object.GetReference(targetPackage)); 
            body.Instructions.Add(new Instruction(RCode.Const, values.Count, new[] { index }));
            body.Instructions.Add(new Instruction(RCode.New_array, arrayType, new[] { array, index }));
            
            body.Instructions.Add(new Instruction(RCode.Const, 1, new[] { one }));
            body.Instructions.Add(new Instruction(RCode.Const, 0, new[] { index }));

            foreach(var val in values)
            {
                if(val is string)
                    body.Instructions.Add(new Instruction(RCode.Const_string, val, new[] { rval }));
                else
                    body.Instructions.Add(new Instruction(RCode.Const_class, val, new[] { rval }));

                body.Instructions.Add(new Instruction(RCode.Aput_object, null, new[] { rval, array, index, }));
                body.Instructions.Add(new Instruction(RCode.Add_int_2addr, null, new[] { index, one }));
            }

            body.Instructions.Add(new Instruction(RCode.Return_object, null, new[] { array }));
            
            compiledMethod.RLBody = body;
        }

    }
}
