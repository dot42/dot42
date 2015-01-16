using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.FrameworkBuilder
{
    /// <summary>
    /// Helper used to build interface method implementations
    /// </summary>
    internal class InterfaceMethodBuilder
    {
        private readonly MethodDefinition iMethod;
        private readonly GenericInstanceType genericInterfaceType;
        private MethodDefinition method;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InterfaceMethodBuilder(MethodDefinition iMethod, GenericInstanceType genericInterfaceType)
        {
            this.iMethod = iMethod;
            this.genericInterfaceType = genericInterfaceType;
        }

        /// <summary>
        /// Create the interface method in the given type
        /// </summary>
        public void Build(TypeDefinition declaringType, TargetFramework target)
        {
            // Create method
            var name = iMethod.DeclaringType.FullName + "." + iMethod.Name;
            var attributes = MethodAttributes.Private | MethodAttributes.HideBySig;
            method = new MethodDefinition(name, attributes, target.TypeSystem.Void);
            method.DeclaringType = declaringType;
            foreach (var gp in iMethod.GenericParameters)
            {
                method.GenericParameters.Add(new GenericParameter(gp.Name, method));
            }
            method.ReturnType = ResolveGenericParameters(iMethod.ReturnType, method);
            foreach (var paramDef in iMethod.Parameters)
            {
                method.Parameters.Add(new ParameterDefinition(ResolveGenericParameters(paramDef.ParameterType, method)));
            }
            declaringType.Methods.Add(method);

            // Set override
            method.Overrides.Add(iMethod);

            // Create method body
            if (!method.IsAbstract)
            {
                var body = method.Body = new MethodBody(method);
                var seq = body.GetILProcessor();
                if (method.ReturnType.FullName != "System.Void")
                {
                    if (method.ReturnType.IsPrimitive)
                    {
                        seq.Emit(OpCodes.Ldc_I4_0);
                    }
                    else
                    {
                        seq.Emit(OpCodes.Ldnull);
                    }
                }
                seq.Emit(OpCodes.Ret);
            }
        }

        private TypeReference ResolveGenericParameters(TypeReference typeRef, MethodDefinition method)
        {
            if (typeRef.IsGenericParameter)
            {
                var gp = (GenericParameter) typeRef;
                var ownerGenericParameterType = gp.Owner.GenericParameterType;
                if ((ownerGenericParameterType == GenericParameterType.Type) && (genericInterfaceType != null))
                    return genericInterfaceType.GenericArguments[gp.Position];
                if (ownerGenericParameterType == GenericParameterType.Method)
                    return method.GenericParameters[gp.Position];
                throw new ArgumentException(string.Format("Unknown generic parameter {0} in {1}", typeRef, method));
            }
            if (typeRef.IsGenericInstance)
            {
                var git = (GenericInstanceType) typeRef;
                var clone = new GenericInstanceType(ResolveGenericParameters(git.ElementType, method));
                foreach (var garg in git.GenericArguments)
                {
                    clone.GenericArguments.Add(ResolveGenericParameters(garg, method));
                }
                return clone;
            }
            return typeRef;
        }
    }
}
