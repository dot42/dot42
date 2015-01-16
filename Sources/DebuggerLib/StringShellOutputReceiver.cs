using System.Text;
using System.Threading;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Collect shell output in a string.
    /// </summary>
    public class StringShellOutputReceiver : IShellOutputReceiver
    {
        private readonly StringBuilder sb = new StringBuilder();
        private bool done;
        private readonly object waitLock = new object();
        private bool isCancelled;

        /// <summary>
        /// Add received output data.
        /// </summary>
        public void AddOutput(byte[] data, int offset, int length)
        {
            sb.Append(AdbRequest.DefaultEncoding.GetString(data, offset, length));
        }

        /// <summary>
        /// Command has finished.
        /// </summary>
        public void Completed()
        {
            lock (waitLock)
            {
                done = true;
                Monitor.PulseAll(waitLock);
            }
        }

        /// <summary>
        /// Should further processing be cancelled?
        /// </summary>
        public bool IsCancelled
        {
            get { return isCancelled; }
            set { isCancelled = value; if (value) Completed(); }
        }

        /// <summary>
        /// Block the current thread until we're done.
        /// </summary>
        public void WaitUntilCompleted()
        {
            lock (waitLock)
            {
                while (!done)
                {
                    Monitor.Wait(waitLock);
                }
            }
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
