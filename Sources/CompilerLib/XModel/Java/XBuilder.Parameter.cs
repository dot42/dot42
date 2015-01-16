using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific parameter.
        /// </summary>
        private sealed class JavaParameter : XParameter
        {
            private readonly XModule module;
            private readonly TypeReference type;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaParameter(XModule module, string name, TypeReference type)
                : base(name)
            {
                this.module = module;
                this.type = type;
            }

            /// <summary>
            /// Type of the parameter
            /// </summary>
            public override XTypeReference ParameterType
            {
                get { return AsTypeReference(module, type, XTypeUsageFlags.ParameterType); }
            }
        }
    }
}
