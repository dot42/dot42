using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value structure with class_info_index.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public sealed class ClassElementValue : ElementValue
    {
        private readonly ConstantPool cp;
        private readonly int classInfoIndex;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ClassElementValue(ConstantPool cp, int classInfoIndex)
        {
            this.cp = cp;
            this.classInfoIndex = classInfoIndex;
        }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public override char Tag { get { return 'c'; } }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(IElementValueVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the enum type name
        /// </summary>
        /// <remarks>
        /// The class_info_index item must be a valid index into the constant_pool table. The constant_pool entry at that index must be a 
        /// CONSTANT_Utf8_info (§4.4.7) structure representing the return descriptor (§4.3.3) of the type that is reified by the class represented
        /// by this element_value structure.
        /// </remarks>
        public string ClassName
        {
            get { return ((ConstantPoolUtf8)cp[classInfoIndex]).Value; }
        }

        public override string ToString()
        {
            return string.Format("Class:{0}", ClassName);
        }
    }
}