using System.Collections.Generic;

namespace Dot42.JvmClassLib.Attributes
{
    public sealed class InnerClassesAttribute : Attribute
    {
        internal const string AttributeName = "InnerClasses";

        private readonly List<InnerClass> classes = new List<InnerClass>();

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }

        /// <summary>
        /// Gets all inner classes.
        /// </summary>
        public List<InnerClass> Classes { get { return classes; } }
    }
}
