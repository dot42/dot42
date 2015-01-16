namespace Dot42.CompilerLib
{
    /// <summary>
    /// Specific location in a source file.
    /// </summary>
    public interface ISourceLocation
    {
        /// <summary>
        /// Gets the source document name
        /// </summary>
        string Document { get; }

        /// <summary>
        /// Start line number
        /// </summary>
        int StartLine { get; }

        /// <summary>
        /// Starting column number
        /// </summary>
        int StartColumn { get; }

        /// <summary>
        /// End line number
        /// </summary>
        int EndLine { get; }

        /// <summary>
        /// End column number
        /// </summary>
        int EndColumn { get; }

        /// <summary>
        /// If set, ignore this location
        /// </summary>
        bool IsSpecial { get; }
    }
}
