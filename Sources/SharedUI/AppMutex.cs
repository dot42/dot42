using System.Threading;

namespace Dot42.Shared.UI
{
    public static class AppMutex
    {
        // Do not change this name, since it is also used in the setup.
        private const string MUTEX_NAME = "C51F2F88-FC6C-4AD3-B036-D0DED2A492E0";
        private static Mutex appMutex;

        /// <summary>
        /// Make sure the mutex used to lock the setup is loaded.
        /// </summary>
        public static void EnsureLoaded()
        {
            // Create mutex
            if (appMutex == null)
            {
                appMutex = new Mutex(false, MUTEX_NAME);
            }
        }
    }
}
