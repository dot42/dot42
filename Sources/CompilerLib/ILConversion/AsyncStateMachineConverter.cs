using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Reachable;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Implement IAsyncSetThis on implementations of IAsyncStateMachine
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class AsyncStateMachineConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 20; }
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
            /// Implement IAsyncSetThis where needed.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                foreach (var type in reachableContext.ReachableTypes.Where(IsAsyncStateMachine))
                {
                    ImplementISetThis(type, reachableContext);
                }
            }

            /// <summary>
            /// Does the given type implement an async statemachine?
            /// </summary>
            private static bool IsAsyncStateMachine(TypeDefinition type)
            {
                if (!type.IsValueType)
                    return false;
                if (!type.HasInterfaces)
                    return false;
                return type.Interfaces.Any(x => x.Interface.FullName == "System.Runtime.CompilerServices.IAsyncStateMachine");
            }

            /// <summary>
            /// Implement IAsyncSetThis.
            /// </summary>
            private static void ImplementISetThis(TypeDefinition type, ReachableContext reachableContext)
            {
                var thisField = type.Fields.FirstOrDefault(x => x.Name.StartsWith("<>") && x.Name.EndsWith("__this"));
                if (thisField == null)
                    return;

                // Add interface
                var intfType = type.Module.Import(new TypeReference(InternalConstants.Dot42InternalNamespace, "IAsyncSetThis", type.Module, type.Module.Assembly.Name));
                type.Interfaces.Add(new InterfaceImpl(intfType));

                // Add "SetThis(object)" method
                var method = new MethodDefinition("SetThis", MethodAttributes.Public, type.Module.TypeSystem.Void);
                
                type.Methods.Add(method);
                var valueParam = new ParameterDefinition(type.Module.TypeSystem.Object);
                method.Parameters.Add(valueParam);
                method.Body = new MethodBody(method) { InitLocals = true };
                var seq = new ILSequence();
                seq.Emit(OpCodes.Ldarg_0); // this
                seq.Emit(OpCodes.Ldarg, valueParam);
                seq.Emit(OpCodes.Castclass, thisField.FieldType);
                seq.Emit(OpCodes.Stfld, thisField);
                seq.Emit(OpCodes.Ret);
                seq.AppendTo(method.Body);

                method.SetReachable(reachableContext);
            }
        }
    }    
}