using System;
using System.Diagnostics;
using System.Windows.Forms;
using Dot42.Gui.Properties;

namespace Dot42.Gui.Controls
{
    internal static class CommonActions
    {
        /// <summary>
        /// Open given url.
        /// </summary>
        internal static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                var msg = String.Format(Resources.CannotOpenUrlBecauseX, ex.Message);
                MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
