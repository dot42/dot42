using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Forms;
using Dot42.Shared.UI;

namespace Dot42.ApkSpy
{
    internal static class Program
    {
        private static CompositionContainer compositionContainer;

        /// <summary>
        /// Get the MEF based composition container.
        /// </summary>
        internal static CompositionContainer CompositionContainer
        {
            get
            {
                if (compositionContainer == null)
                {
                    var catalog = new AggregateCatalog();
                    catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
                    //catalog.Catalogs.Add(new DirectoryCatalog(".", "*.Plugin.dll"));

                    compositionContainer = new CompositionContainer(catalog);
                }
                return compositionContainer;
            }
        }

        /// <summary>
        /// Entry point
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                // Load mutex
                AppMutex.EnsureLoaded();

                // Create form
                var form = new MainForm();
                Application.EnableVisualStyles();
                Application.Run(form);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
                return 1;
            }
        }
    }
}
