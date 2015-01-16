namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Single dimension of an array
    /// </summary>
    public class XArrayDimension
    {
        private readonly int? lowerBound;
        private readonly int? upperBound;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XArrayDimension() 
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public XArrayDimension(int? lowerBound, int? upperBound)
        {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }

        public int? LowerBound
        {
            get { return lowerBound; }
        }

        public int? UpperBound
        {
            get { return upperBound; }
        }

        public bool IsSized
        {
            get { return lowerBound.HasValue || upperBound.HasValue; }
        }

        public override string ToString()
        {
            return !IsSized ? string.Empty : lowerBound + "..." + upperBound;
        }
    }
}
