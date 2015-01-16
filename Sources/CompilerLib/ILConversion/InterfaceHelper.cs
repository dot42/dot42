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
        internal static MethodDefinition CreateExplicitStub(MethodDefinition implicitImpl, string name, MethodDefinition iMethod, bool avoidGenericParam)
        {
            // Create method
            var newMethod = new MethodDefinition(name, implicitImpl.Attributes, implicitImpl.ReturnType);
            newMethod.IsVirtual = false;
            newMethod.IsAbstract = false;
            newMethod.IsFinal = true;

            // Clone generic parameters
            foreach (var gp in implicitImpl.GenericParameters)
            {
                newMethod.GenericParameters.Add(new GenericParameter(gp.Name, newMethod));
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
            var targetType = implicitImpl.DeclaringType;
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
            worker.Emit(implicitImpl.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, implicitImpl);
            worker.Emit(OpCodes.Ret);

            // Mark method reachable
            if (implicitImpl.IsReachable)
            {
                newMethod.SetReachable(null);
            }

            return newMethod;
        }
    }
}
