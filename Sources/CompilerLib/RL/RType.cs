namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Type of data containing in a register.
    /// </summary>
    public enum RType
    {
        /// <summary>
        /// 32-bit number value (byte, short, char, int, float)
        /// </summary>
        Value,

        /// <summary>
        /// 64-bit number value (long, double)
        /// </summary>
        Wide,

        /// <summary>
        /// Second part of 64-bit number (long, double)
        /// </summary>
        Wide2,

        /// <summary>
        /// Object reference
        /// </summary>
        Object
    }
}
