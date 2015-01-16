namespace Dot42.AdbLib
{
    public interface IShellOutputReceiver
    {
        /// <summary>
        /// Add received output data.
        /// </summary>
        void AddOutput(byte[] data, int offset, int length);

        /// <summary>
        /// Command has finished.
        /// </summary>
        void Completed();

        /// <summary>
        /// Should further processing be cancelled?
        /// </summary>
        bool IsCancelled { get; }
    }
}
