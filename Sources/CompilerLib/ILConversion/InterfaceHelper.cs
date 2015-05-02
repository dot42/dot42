using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    internal static class InterfaceHelper
    {
        /// <summary>
        /// Create a new method in the declaring type of the given implicit implementation with the given name.
        /// This method will call the implicit implementation.
        /// </summary>
        internal static MethodDefinition CreateExplicitStub(TypeDefinition targetType, MethodDefinition implicitImpl, string name, MethodDefinition iMethod, bool avoidGenericParam)
        {
            MethodReference implicitImplRef = implicitImpl;
            GenericInstanceMethod implicitGenericInstanceMethod=null;
            // Create method
            var newMethod = new MethodDefinition(name, implicitImpl.Attributes, implicitImpl.ReturnType);
            newMethod.IsVirtual = false;
            newMethod.IsAbstract = false;
            newMethod.IsFinal = true;

            if (implicitImpl.GenericParameters.Count > 0)
            {
                implicitImplRef = implicitGenericInstanceMethod = new GenericInstanceMethod(implicitImpl);
            }

            // Clone generic parameters
            foreach (var gp in implicitImpl.GenericParameters)
            {
                newMethod.GenericParameters.Add(new GenericParameter(gp.Name, newMethod));
                implicitGenericInstanceMethod.GenericArguments.Add(gp);
            }

            // Update according to new context
            var cloner = new TypeCloner(avoidGenericParam, implicitImpl.Module.TypeSystem);
            newMethod.ReturnType = cloner.Get(implicitImpl.ReturnType, newMethod);

            // Clone parameters
            foreach (var p in iMethod.Parameters)
            {
                newMethod.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, cloner.Get(p.ParameterType, newMethod)));
            }

            // Add the method
            targetType.Methods.Add(newMethod);


            // Add override
            newMethod.Overrides.Add(iMethod);

            // Create method body
            var body = new MethodBody(newMethod);
            newMethod.Body = body;
            var worker = body.GetILProcessor();

            // Push this 
            worker.Emit(OpCodes.Ldarg, body.ThisParameter);
            for (var i = 0; i < implicitImpl.Parameters.Count; i++)
            {
                var p = iMethod.Parameters[i];
                var newMethodParam = newMethod.Parameters[i];
                worker.Emit(OpCodes.Ldarg, newMethodParam);
                if (/*avoidGenericParam &&*/ p.ParameterType.ContainsGenericParameter)
                {
                    worker.Emit(OpCodes.Box, implicitImpl.Parameters[i].ParameterType);
                }
            }
            worker.Emit(implicitImpl.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, implicitImplRef);
            worker.Emit(OpCodes.Ret);

            // Mark method reachable
            if (implicitImpl.IsReachable)
            {
                newMethod.SetReachable(null);
            }

            return newMethod;
        }

        public static List<MethodReference> GetReachableMethodReferences(IEnumerable<MethodDefinition> reachableMethods)
        {
            var reachableMethodReferences = reachableMethods.Select(x => x.Body)
                                                            .Where(x => x != null)
                                                            .SelectMany(x => x.Instructions)
                                                            .Where(i => i.Operand is MethodReference)
                                                            .Select(i => ((MethodReference)i.Operand).GetElementMethod())
                                                            .ToList();
            return reachableMethodReferences;
        }

        /// <summary>
        /// Rename the given method and all references to it from code.
        /// </summary>
        public static void Rename(MethodDefinition method, string newName, IEnumerable<MethodReference> reachableMethodReferences)
        {
            var resolver = new GenericsResolver(method.DeclaringType);

            // Rename reference to method
            foreach (var methodRef in reachableMethodReferences)
            {
                if (!ReferenceEquals(methodRef, method) && methodRef.AreSameIncludingDeclaringType(method, resolver.Resolve))
                {
                    methodRef.Name = newName;
                }
            }
            // Rename method itself
            method.SetName(newName);
        }
    }
}
