using System.Collections.Generic;

namespace Dot42.JvmClassLib.Attributes
{
    public sealed class ExceptionsAttribute : Attribute
    {
        internal const string AttributeName = "Exceptions";

        private readonly List<TypeReference> exceptions = new List<TypeReference>();

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }

        /// <summary>
        /// Gets all attributes of this class.
        /// </summary>
        public List<TypeReference> Exceptions { get { return exceptions; } }
    }
}
