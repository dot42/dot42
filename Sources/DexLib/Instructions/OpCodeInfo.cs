namespace Dot42.DexLib.Instructions
{
    /// <summary>
    /// Provide register information about RL code instructions.
    /// </summary>
    public partial class OpCodeInfo
    {
        private readonly OpCodes code;
        private readonly RegisterFlags[] registerFlags;

        /// <summary>
        /// Default ctor
        /// </summary>
        public OpCodeInfo(OpCodes code, params RegisterFlags[] registerFlags)
        {
            this.code = code;
            this.registerFlags = registerFlags;
        }

        /// <summary>
        /// Gets the register usage flags for the register at the given (0 based) index.
        /// </summary>
        public virtual RegisterFlags GetUsage(int registerIndex)
        {
            return registerFlags[registerIndex]; 
        }

        /// <summary>
        /// Gets the instruction code.
        /// </summary>
        public OpCodes Code
        {
            get { return code; }
        }
    }
}
