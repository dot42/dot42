using System;
using System.IO;
using Dot42.Utility;

namespace Dot42.ApkBuilder
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
                    folder = Path.Combine(rootFolder, @"RegressionTests\Simple");
                    args = new[] {
                        ToolOptions.InputManifest.CreateArg(Path.Combine(folder, @"Out\AndroidManifest.xml")),
                        ToolOptions.InputCodeFile.CreateArg(Path.Combine(folder, @"Out\Code\classes.dex")),
                        ToolOptions.OutputPackage.CreateArg(Path.Combine(folder, @"Out\Simple.apk")),
                        ToolOptions.CertificateThumbprint.CreateArg("EF53D093F360F8941566B7924BFD11027C88FABB")
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
                var builder = new ApkBuilder(options.ApkFile);
                builder.ManifestPath = options.ManifestFile;
                builder.DexFiles.AddRange(options.DexFiles);
                builder.ResourcesFolder = options.ResourcesFolder;
                builder.PfxFile = options.PfxFile;
                builder.PfxPassword = options.PfxPassword;
                builder.CertificateThumbprint = options.CertificateThumbprint;
                builder.Build();

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
