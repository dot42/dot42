using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value structure with enum_const_value.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public sealed class EnumConstElementValue : ElementValue
    {
        private readonly ConstantPool cp;
        private readonly int typeNameIndex;
        private readonly int constNameIndex;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal EnumConstElementValue(ConstantPool cp, int typeNameIndex, int constNameIndex)
        {
            this.cp = cp;
            this.typeNameIndex = typeNameIndex;
            this.constNameIndex = constNameIndex;
        }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public override char Tag { get { return 'e'; } }

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
        /// The value of the type_name_index item must be a valid index into the constant_pool table. 
        /// The constant_pool entry at that index must be a CONSTANT_Utf8_info structure (§4.4.7) representing a valid field descriptor (§4.3.2) 
        /// that denotes the internal form of the binary name (§4.2.1) of the type of the enum constant represented by this element_value structure.
        /// </remarks>
        public string TypeName
        {
            get { return ((ConstantPoolUtf8)cp[typeNameIndex]).Value; }
        }

        /// <summary>
        /// Simple name of enum constants
        /// </summary>
        /// <remarks>
        /// The value of the const_name_index item must be a valid index into the constant_pool table.
        /// The constant_pool entry at that index must be a CONSTANT_Utf8_info structure (§4.4.7) representing the simple name of the enum 
        /// constant represented by this element_value structure.
        /// </remarks>
        public string ConstName
        {
            get { return ((ConstantPoolUtf8) cp[constNameIndex]).Value; }
        }

        public override string ToString()
        {
            return string.Format("EnumValue:{0}/{1}", TypeName, ConstName);
        }
    }
}