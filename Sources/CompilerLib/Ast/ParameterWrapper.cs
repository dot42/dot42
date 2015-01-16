using System;
using System.Diagnostics;
using Dot42.JvmClassLib.Bytecode;
using Mono.Cecil;

namespace Dot42.CompilerLib.Ast
{
    public class ParameterWrapper : IParameter
    {
        private readonly object parameter;
        private readonly string name;

        /// <summary>
        /// Default ctor
        /// </summary>
        private ParameterWrapper(ParameterDefinition parameter)
        {
            this.parameter = parameter;
            name = parameter.Name;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        private ParameterWrapper(LocalVariableReference parameter)
        {
            this.parameter = parameter;
            name = "P" + parameter.Index;
        }

        /// <summary>
        /// Wrap the given parameter which cn be null.
        /// </summary>
        public static IParameter Wrap(object parameter)
        {
            if (parameter == null)
                return null;
            var iParam = parameter as IParameter;
            if (iParam != null)
                return iParam;
            var ilParam = parameter as ParameterDefinition;
            if (ilParam != null)
                return new ParameterWrapper(ilParam);
            var javaParam = parameter as LocalVariableReference;
            if (javaParam != null)
                return new ParameterWrapper(javaParam);
            var astParam = parameter as AstVariable;
            if ((astParam != null) && (astParam.IsParameter))
                return Wrap(astParam.OriginalParameter);
            throw new ArgumentException("Unknown parameter " + parameter);
        }

        //public object OriginalParameter { get { return parameter; } }

        /// <summary>
        /// Gets the name of the parameter.
        /// Can be null
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Is this parameter equal to the given variable?
        /// </summary>
        bool IParameter.Equals(IVariable variable)
        {
            var astVar = (AstVariable) variable;
            return Equals(parameter, astVar.OriginalParameter);
        }
    }
}
