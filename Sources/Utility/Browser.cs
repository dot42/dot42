using System;
using System.Diagnostics;

namespace Dot42.Utility
{
    public static class Browser
    {
        /// <summary>
        /// Open the given URL in a browser.
        /// </summary>
        public static bool Open(string url)
        {
            try
            {
                Process.Start(url);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
