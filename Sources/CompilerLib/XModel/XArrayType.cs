using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Array of a element type
    /// </summary>
    public sealed class XArrayType : XTypeSpecification
    {
        private readonly ReadOnlyCollection<XArrayDimension> dimensions;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XArrayType(XTypeReference elementType, IEnumerable<XArrayDimension> dimensions = null)
            : base(elementType)
        {
            this.dimensions = (dimensions != null) ? dimensions.ToList().AsReadOnly() : null;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public XArrayType(XTypeReference elementType, params XArrayDimension[] dimensions)
            : base(elementType)
        {
            this.dimensions = ((dimensions != null) && (dimensions.Length > 0)) ? dimensions.ToList().AsReadOnly() : null;
        }

        /// <summary>
        /// What kind of reference is this?
        /// </summary>
        public override XTypeReferenceKind Kind { get { return XTypeReferenceKind.ArrayType; } }

        /// <summary>
        /// Gets number of dimensions.
        /// </summary>
        public int Rank
        {
            get { return (dimensions == null) ? 1 : dimensions.Count; }
        }

        /// <summary>
        /// Gets all dimensions
        /// </summary>
        public IEnumerable<XArrayDimension> Dimensions
        {
            get
            {
                if (dimensions != null)
                    return dimensions;
                return new[] { new XArrayDimension() };
            }
        }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public override string GetFullName(bool noGenerics)
        {
            return ElementType.GetFullName(noGenerics) + "[]";
        }

        /// <summary>
        /// Is this an array type?
        /// </summary>
        public override bool IsArray
        {
            get { return true; }
        }
    }
}
