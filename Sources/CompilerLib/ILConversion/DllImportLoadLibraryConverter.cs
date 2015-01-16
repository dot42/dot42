using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Create static ctors for types that have dllimport methods.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class DllImportLoadLibraryConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 50; }
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
            /// <summary>
            /// Create static ctors for types that have dllimport methods.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                foreach (var type in reachableContext.ReachableTypes.Where(NeedsClassCtor))
                {
                    var classCtor = EnsureClassCtor(reachableContext, type);
                    AddLoadLibraryCalls(type, classCtor);
                }
            }

            /// <summary>
            /// Does the given type have any pinvoke method?
            /// </summary>
            private static bool NeedsClassCtor(TypeDefinition type)
            {
                return type.Methods.Any(x => x.HasPInvokeInfo);
            }

            /// <summary>
            /// Add load library calls for all imported libs.
            /// </summary>
            private static void AddLoadLibraryCalls(TypeDefinition type, MethodDefinition classCtor)
            {
                var libs = type.Methods.Where(x => x.HasPInvokeInfo).Select(x => x.PInvokeInfo.Module.Name).Distinct().ToList();
                var typeSystem = type.Module.TypeSystem;
                var loadLibMethod = new MethodReference("LoadLibrary", typeSystem.Void, typeSystem.LookupType("Java.Lang", "System"));
                loadLibMethod.Parameters.Add(new ParameterDefinition(typeSystem.String));

                var seq = new ILSequence();
                foreach (var libName in libs)
                {
                    // Add System.LoadLibrary(libName) call
                    seq.Append(new Instruction(OpCodes.Ldstr, libName));
                    seq.Append(new Instruction(OpCodes.Call, loadLibMethod));
                }

                seq.InsertTo(0, classCtor.Body);
            }

            /// <summary>
            /// Ensure there is a class ctor.
            /// </summary>
            private static MethodDefinition EnsureClassCtor(ReachableContext reachableContext, TypeDefinition type)
            {
                var ctor = type.GetClassCtor();
                if (ctor != null)
                    return ctor; // Already exists

                // Create class ctor
                var typeSystem = type.Module.TypeSystem;
                ctor = new MethodDefinition(".cctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static | MethodAttributes.SpecialName, typeSystem.Void);
                ctor.DeclaringType = type;

                var body = new MethodBody(ctor);
                body.InitLocals = true;
                ctor.Body = body;

                // Prepare code
                var seq = new ILSequence();
                seq.Emit(OpCodes.Nop);
                seq.Emit(OpCodes.Ret);

                // Append ret sequence
                seq.AppendTo(body);

                // Update offsets
                body.ComputeOffsets();

                // Add ctor
                type.Methods.Add(ctor);
                ctor.SetReachable(reachableContext);
                return ctor;
            }
        }
    }    
}