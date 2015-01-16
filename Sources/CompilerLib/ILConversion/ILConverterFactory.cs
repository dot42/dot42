namespace Dot42.CompilerLib.ILConversion
{
    public interface ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Create the converter
        /// </summary>
        ILConverter Create();
    }
}
