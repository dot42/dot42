using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Forms;
using Dot42.Gui.SamplesTool;
using Dot42.Shared.UI;
using Dot42.Utility;

#if ANDROID
using MainForm = Dot42.Gui.Forms.Android.MainForm;
#elif BB
using Dot42.Utility;
using MainForm = Dot42.Gui.Forms.BlackBerry.MainForm;
#endif

namespace Dot42.Gui
{
    internal static class Program
    {
#if ANDROID
        internal const Targets Target = Targets.Android;
#elif BB
        internal const Targets Target = Targets.BlackBerry;
#endif

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
                var options = new CommandLineOptions(args);
                if (options.ShowHelp)
                {
                    options.Usage();
                    return 1;
                }

                // Load mutex
                AppMutex.EnsureLoaded();                

                // Initialize target
                Locations.Target = Target;

                if (!string.IsNullOrEmpty(options.SamplesFolder))
                {
                    var toolForm = new SamplesToolForm(options.SamplesFolder);
                    Application.EnableVisualStyles();
                    Application.Run(toolForm);
                    return 0;
                }

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
