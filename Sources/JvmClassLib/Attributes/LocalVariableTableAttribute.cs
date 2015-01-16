using System.Collections.Generic;

namespace Dot42.JvmClassLib.Attributes
{
    public sealed class LocalVariableTableAttribute : Attribute
    {
        internal const string AttributeName = "LocalVariableTable";

        private readonly List<LocalVariable> variables = new List<LocalVariable>();

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }

        /// <summary>
        /// Gets all variables.
        /// </summary>
        public List<LocalVariable> Variables { get { return variables; } }
    }
}
