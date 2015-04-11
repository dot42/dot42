using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    [Export(typeof (ILConverterFactory))]
    internal class MethodGenericClassOverrideConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 35; }
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
            private ReachableContext reachableContext;
            private List<MethodDefinition> reachableMethods;

            /// <summary>
            /// Add bridge methods when a method with a non-generic parameter overrides a method with a generic parameter.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                this.reachableContext = reachableContext;

                // Initialize some sets
                reachableMethods = reachableContext.ReachableTypes.Where(x => !x.HasDexImportAttribute()).SelectMany(x => x.Methods).Where(m => m.IsReachable).OrderBy(x => x.FullName).ToList();

                // Do we need to convert anything?
                foreach (var method in reachableMethods)
                {
                    MethodDefinition baseMethod;
                    if (!NeedsBridge(method, out baseMethod))
                        continue;

                    // Add bridge
                    var bridge = AddBridge(method, baseMethod);

                    // Remove bridge if is has an exact duplicate
                    var duplicate = method.DeclaringType.Methods.FirstOrDefault(x => (x != bridge) && x.AreSame(bridge, null));
                    if (duplicate != null)
                    {
                        bridge.DeclaringType.Methods.Remove(bridge);
                    }
                }
            }

            /// <summary>
            /// Does the given method require a bridge method?
            /// </summary>
            private static bool NeedsBridge(MethodDefinition method, out MethodDefinition baseMethod)
            {
                baseMethod = null;
                if (method.IsStatic || !method.IsVirtual)
                    return false;
                baseMethod = method.GetBaseMethod();
                if ((baseMethod == null) || (!baseMethod.ContainsGenericParameter))
                    return false;

                var paramCount = method.Parameters.Count;
                for (var i = 0; i < paramCount; i++)
                {
                    var baseP = baseMethod.Parameters[i];
                    if (!baseP.ParameterType.ContainsGenericParameter)
                        continue;
                    var p = method.Parameters[i];
                    if (!p.ParameterType.ContainsGenericParameter)
                        return true;
                }

                return false;
            }

            /// <summary>
            /// Add a method that has the same signature as basemethod and calls method.
            /// </summary>
            private MethodDefinition AddBridge(MethodDefinition method, MethodDefinition baseMethod)
            {
                var bridge = new MethodDefinition(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType) { HasThis = true };

                var cloner = new TypeCloner(true, method.Module.TypeSystem);
                bridge.ReturnType = cloner.Get(baseMethod.ReturnType, bridge);
                bridge.IsAbstract = false;

                // Clone parameters
                foreach (var p in baseMethod.Parameters)
                {
                    bridge.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, cloner.Get(p.ParameterType, bridge)));
                }

                // Create body
                var body = new MethodBody(bridge);
                bridge.Body = body;

                // Create code
                var seq = new ILSequence();
                // this
                seq.Emit(OpCodes.Ldarg_0);
                // parameters
                for (var i = 0; i < bridge.Parameters.Count; i++)
                {
                    var p = bridge.Parameters[i];
                    seq.Emit(OpCodes.Ldarg, p);
                    if (baseMethod.Parameters[i].ParameterType.ContainsGenericParameter)
                    {
                        seq.Emit(OpCodes.Unbox, method.Parameters[i].ParameterType);
                    }
                }
                // Call actual method
                seq.Emit(OpCodes.Call, method);
                // Return
                seq.Emit(OpCodes.Ret);

                // Add code to body
                seq.AppendTo(body);
                body.ComputeOffsets();

                // add overrides, so that we can find the method later
                bridge.Overrides.Add(baseMethod);

                // Add to class
                method.DeclaringType.Methods.Add(bridge);
                bridge.SetReachable(reachableContext);

                return bridge;
            }
        }
    }
}