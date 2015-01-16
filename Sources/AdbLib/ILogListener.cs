namespace Dot42.AdbLib
{
    public interface ILogListener
    {
        /// <summary>
        /// Add received output data.
        /// </summary>
        void AddEntry(LogEntry entry);

        /// <summary>
        /// Should further processing be cancelled?
        /// </summary>
        bool IsCancelled { get; }
    }
}
