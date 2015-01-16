using System;

namespace Dot42.ImportJarLib.Model
{
    public sealed class NetParameterDefinition
    {
        private readonly string name;
        private readonly NetTypeReference type;
        private readonly bool isParams;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetParameterDefinition(string name, NetTypeReference type, bool isParams)
        {
            if (type.IsVoid())
                throw new ArgumentException("type cannot be void");
            this.name = Keywords.IsKeyword(name) ? "@" + name : name;
            this.type = type;
            this.isParams = isParams;
            if (isParams && !(type is NetArrayType))
            {
                throw new ArgumentException("Params parameter must have array type");
            }
        }

        /// <summary>
        /// Parameter type
        /// </summary>
        public NetTypeReference ParameterType
        {
            get { return type; }
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Is this parameter a "params" parameter with variable number of arguments?
        /// </summary>
        public bool IsParams { get { return isParams; } }
    }
}
