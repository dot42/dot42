namespace Dot42.DeviceLib
{
    public interface IStartActivityListener
    {
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
