using System;
using System.Windows.Forms;
using Dot42.Shared.UI;

namespace Dot42.CertificateUtil
{
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            // Load mutex
            AppMutex.EnsureLoaded();

            // Load app
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
