using System;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.JvmClassLib.Attributes;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    /// <summary>
    /// Method invocation frame.
    /// This class is used to allocate registers.
    /// </summary>
    internal sealed class AstInvocationFrame : InvocationFrame
    {
        private readonly DexTargetPackage targetPackage;
        private readonly MethodBody body;
        private readonly ArgumentRegisterSpec thisArgument;

        /// <summary>
        /// Create a frame for the given method
        /// </summary>
        internal AstInvocationFrame(DexTargetPackage targetPackage, MethodDefinition method, MethodSource source, MethodBody body) 
        {
            this.targetPackage = targetPackage;
            this.body = body;
            this.body = body;
            Debug.Assert(!body.Registers.Any());

            var prototypeParamOffset = 0;
            var prototype = method.Prototype;

            if (source.IsDotNet)
            {
                var ilMethod = source.ILMethod;

                // Allocate this
                if (!method.IsStatic)
                {
                    thisArgument = (ArgumentRegisterSpec) Allocate(method.Owner, true, RCategory.Argument, ilMethod.Body.ThisParameter);
                    arguments.Add(thisArgument);
                }
                else if (ilMethod.IsAndroidExtension() && !ilMethod.IsStatic)
                {
                    prototypeParamOffset++;
                    var type = ilMethod.DeclaringType.GetReference(targetPackage, source.Method.Module);
                    thisArgument = (ArgumentRegisterSpec) Allocate(type, true, RCategory.Argument, ilMethod.Body.ThisParameter);
                    arguments.Add(thisArgument);
                }
                // Allocate arguments
                var paramCount = ilMethod.Parameters.Count;
                for (var i = 0; i < paramCount; i++)
                {
                    var p = ilMethod.Parameters[i];
                    var type = prototype.Parameters[prototypeParamOffset++].Type;
                    arguments.Add((ArgumentRegisterSpec) Allocate(type, false, RCategory.Argument, p));
                }
            }
            else if (source.IsJava)
            {
                var javaMethod = source.JavaMethod;

                // Allocate this
                var code = javaMethod.Attributes.OfType<CodeAttribute>().First();
                if (!method.IsStatic)
                {
                    thisArgument = (ArgumentRegisterSpec)Allocate(method.Owner, true, RCategory.Argument, code.ThisParameter);
                    arguments.Add(thisArgument);
                }
                // Allocate arguments
                foreach (var p in code.Parameters)
                {
                    var type = prototype.Parameters[prototypeParamOffset++].Type;
                    arguments.Add((ArgumentRegisterSpec)Allocate(type, false, RCategory.Argument, p.Item2));
                }
            }
            else if (source.IsAst)
            {
                // Allocate this
                if (!method.IsStatic)
                {
                    thisArgument = (ArgumentRegisterSpec)Allocate(method.Owner, true, RCategory.Argument, null);
                    arguments.Add(thisArgument);
                }
                // Allocate arguments
                foreach (var p in ((XSyntheticMethodDefinition)source.Method).AstParameters)
                {
                    var type = prototype.Parameters[prototypeParamOffset++].Type;
                    arguments.Add((ArgumentRegisterSpec)Allocate(type, false, RCategory.Argument, p));
                }
            }
            else
            {
                throw new ArgumentException("Unknown source");
            }

            // Add GenericInstanceType parameter (if any)
            if (source.Method.NeedsGenericInstanceTypeParameter)
            {
                var type = prototype.GenericInstanceTypeParameter.Type;
                GenericInstanceTypeArgument = (ArgumentRegisterSpec) Allocate(type, false, RCategory.Argument, null);
                arguments.Add(GenericInstanceTypeArgument);                
            }
            // Add GenericInstanceMethod parameter (if any)
            if (source.Method.NeedsGenericInstanceMethodParameter)
            {
                var type = prototype.GenericInstanceMethodParameter.Type;
                GenericInstanceMethodArgument = (ArgumentRegisterSpec) Allocate(type, false, RCategory.Argument, null);
                arguments.Add(GenericInstanceMethodArgument);
            }
            // Check register count
            var expected = prototype.Parameters.Sum(x => x.Type.IsWide() ? 2 : 1);
            if (!method.IsStatic) expected++;
            if (expected != body.Registers.Count())
            {
                throw new ArgumentException(string.Format("Expected {0} registers, found {1} (in {2})", expected, body.Registers.Count(), method));
            }
        }

        /// <summary>
        /// Gets the register spec used to hold "this".
        /// Can be null.
        /// </summary>
        internal override ArgumentRegisterSpec ThisArgument { get { return thisArgument; } }

        /// <summary>
        /// Gets the register spec used for the given variable.
        /// </summary>
        internal RegisterSpec GetArgument(AstVariable variable)
        {
            if (variable.IsParameter)
            {
                return arguments.First(x => (x.Parameter != null) && x.Parameter.Equals(variable));
            }
            VariableRegisterSpec r;
            if (!variables.TryGetValue(variable, out r))
            {
                var category = variable.PreventOptimizations ? RCategory.VariablePreventOptimization : RCategory.Variable;
                r = (VariableRegisterSpec) Allocate(variable.Type.GetReference(targetPackage), false, category, variable);
                variables.Add(variable, r);
            }
            return r;
        }

        /// <summary>
        /// Allocate a register for the given type.
        /// </summary>
        protected override RegisterSpec Allocate(TypeReference type, bool forceObject, RCategory category, object parameter)
        {
            var isWide = !forceObject && type.IsWide();
            if (isWide)
            {
                var pair = body.AllocateWideRegister(category);
                switch (category)
                {
                    case RCategory.Temp:
                        return new RegisterSpec(pair.Item1, pair.Item2, type);
                    case RCategory.Variable:
                    case RCategory.VariablePreventOptimization:
                        return new VariableRegisterSpec(pair.Item1, pair.Item2, type, (AstVariable)parameter);
                    case RCategory.Argument:
                        return new ArgumentRegisterSpec(pair.Item1, pair.Item2, type, ParameterWrapper.Wrap(parameter));
                    default:
                        throw new ArgumentException("Unknown category " + category);
                }
            }
            var isPrimitive = !forceObject && (type is PrimitiveType);
            var register = body.AllocateRegister(category, isPrimitive ? RType.Value : RType.Object);
            switch (category)
            {
                case RCategory.Temp:
                    return new RegisterSpec(register, null, type);
                case RCategory.Variable:
                case RCategory.VariablePreventOptimization:
                    return new VariableRegisterSpec(register, null, type, (AstVariable) parameter);
                case RCategory.Argument:
                    return new ArgumentRegisterSpec(register, null, type, ParameterWrapper.Wrap(parameter));
                default:
                    throw new ArgumentException("Unknown category " + category);
            }
        }
    }
}
