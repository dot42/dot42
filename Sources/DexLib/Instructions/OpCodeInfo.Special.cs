namespace Dot42.DexLib.Instructions
{
    /// <summary>
    /// <see cref="OpCodeInfo"/> for invoke instructions.
    /// </summary>
    internal sealed class OpCodeInvokeInfo : OpCodeInfo
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public OpCodeInvokeInfo(OpCodes code)
            : base(code)
        {
        }

        /// <summary>
        /// Gets the register usage flags for the register at the given (0 based) index.
        /// </summary>
        public override RegisterFlags GetUsage(int registerIndex)
        {
            return RegisterFlags.Source4Bits;
        }
    }

    /// <summary>
    /// <see cref="OpCodeInfo"/> for invoke_range instructions.
    /// </summary>
    internal sealed class OpCodeInvokeRangeInfo : OpCodeInfo
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public OpCodeInvokeRangeInfo(OpCodes code)
            : base(code)
        {
        }

        /// <summary>
        /// Gets the register usage flags for the register at the given (0 based) index.
        /// </summary>
        public override RegisterFlags GetUsage(int registerIndex)
        {
            return RegisterFlags.Source16Bits;
        }
    }
}
