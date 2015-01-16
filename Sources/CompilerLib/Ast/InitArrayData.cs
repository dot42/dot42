using System;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Operand data for <see cref="AstCode.InitArray"/>.
    /// </summary>
    public sealed class InitArrayData
    {
        private readonly XArrayType arrayType;
        private readonly Array values;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InitArrayData(XArrayType arrayType, Array values)
        {
            this.arrayType = arrayType;
            this.values = values;
        }

        /// <summary>
        /// Type of the array.
        /// </summary>
        public XArrayType ArrayType
        {
            get { return arrayType; }
        }

        /// <summary>
        /// Number of elements in <see cref="Values"/>.
        /// </summary>
        public int Length
        {
            get { return values.Length; }
        }

        /// <summary>
        /// Array data
        /// </summary>
        public Array Values
        {
            get { return values; }
        }
    }
}
