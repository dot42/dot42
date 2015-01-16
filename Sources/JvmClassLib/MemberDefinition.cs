using System.Collections.Generic;
using Dot42.JvmClassLib.Attributes;

namespace Dot42.JvmClassLib
{
    public abstract class MemberDefinition : AbstractReference
    {
        private readonly ClassFile cf;
        private readonly string name;
        private readonly List<Attribute> attributes = new List<Attribute>();

        /// <summary>
        /// Default ctor
        /// </summary>
        protected MemberDefinition(ClassFile cf, string name)
        {
            this.cf = cf;
            this.name = name;
        }

        /// <summary>
        /// Gets the class that contains this member.
        /// </summary>
        public ClassFile DeclaringClass { get { return cf; }}

        /// <summary>
        /// Name of this field
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Gets all attributes of this field.
        /// </summary>
        public IEnumerable<Attribute> Attributes { get { return attributes; } }

        /// <summary>
        /// Adds the given attribute to this provider.
        /// </summary>
        protected void Add(Attribute attribute)
        {
            attributes.Add(attribute);
        }
    }
}
