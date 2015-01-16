using System;
using System.IO;
using Dot42.Utility;

namespace Dot42.TemplateBuilder
{
    /// <summary>
    /// Build APK files
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        public static int Main(string[] args)
        {
#if DEBUG
            for (var i = 0; i < args.Length; i++)
            {
                Console.WriteLine("args[{0}]={1}", i, args[i]);
            }
#endif
            try
            {
#if DEBUG
                if (args.Length == 0)
                {
                    var folder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    var rootFolder = Path.GetFullPath(Path.Combine(folder, @"..\..\..\.."));
                    folder = Path.Combine(rootFolder, @"Sources\VStudio\ProjectTemplates\VStudio10\ApplicationProject");
                    args = new[] {
                        ToolOptions.OutputFolder.CreateArg(Path.Combine(rootFolder, @"Build\Templates\Test")),
                        folder
                    };
                }
#endif

                // Parse command line options
                var options = new CommandLineOptions(args);
                if (options.ShowHelp)
                {
                    options.Usage();
                    return 2;
                }

                // Compile now
                var builder = new TemplateBuilder(options.TemplateFolder);
                builder.Build(options.OutputFolder);

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
