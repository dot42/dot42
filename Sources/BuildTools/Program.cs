using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.AdbLib;
using Dot42.ApkLib;
using Dot42.Utility;

namespace Dot42.BuildTools
{
    internal static class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        internal static int Main(string[] args)
        {
            try
            {
#if DEBUG
                if (args.Length == 0)
                {
                    args = new[] {
                        @"-frameworksfolder=S:\SVNWork\dot42\Build\Application\Frameworks", 
                        @"-out=S:\SVNWork\dot42\build\Docs\ApiDocsFiles"
                    };
                    args = new[] {
                        @"-fa=S:\SVNWork\dot42\Build\Application\Frameworks\v4.0.3\mscorlib.dll"
                    };
                    args = new[]
                    {
                        @"--frameworksfolder=c:\Program Files\dot42\Android\Frameworks",
                        @"--idsource=N:\develop\Projekte\TODO\dot42\dot42\Sources\FrameworkDefinitions\Generated\SystemIdConstants.cs"
                    };
                }
#endif

                var options = new CommandLineOptions(args);
                if (options.ShowHelp)
                {
                    options.Usage();
                    return 2;
                }

                // Set target
                Locations.SetTarget(options.Target);

                if (!string.IsNullOrEmpty(options.TemplatePath))
                {
                    // Update template
                    if (!File.Exists(options.TemplatePath))
                    {
                        throw new ArgumentException(string.Format("Template ({0}) not found.", options.TemplatePath));
                    }

                    Templates.TemplateUpdater.UpdateTemplate(options.TemplatePath, options.Target);
                }

                if (!string.IsNullOrEmpty(options.SystemIdSourcePath))
                {
                    // Generate system id's
                    SystemIds.SystemIdSources.Generate(options.SystemIdSourcePath, options.FrameworksFolder);
                }
                else if (!string.IsNullOrEmpty(options.DebuggerExceptionsSourceFile))
                {
                    Exceptions.ExceptionsSnippetBuilder.Generate(options.DebuggerExceptionsSourceFile, options.FrameworksFolder);
                }
                else if (!string.IsNullOrEmpty(options.FrameworksFolder))
                {
                    // Document frameworks
                    if (!Directory.Exists(options.FrameworksFolder))
                    {
                        throw new ArgumentException(string.Format("Frameworks folder ({0}) not found.", options.FrameworksFolder));                        
                    }
                    var builder = new Documentation.DocumentationBuilder(options.FrameworksFolder, options.OutputFolder);
                    builder.Generate();
                }

                if (!string.IsNullOrEmpty(options.SamplesFolder))
                {
                    // Generate samples uninstall script
                    Samples.UninstallDeleteScript.Generate(options.ScriptPath, options.SamplesFolder);
                }

                if (!string.IsNullOrEmpty(options.InputAssembly) && options.EnumTypeNames.Any())
                {
                    EnumNames.EnumNamesBuilder.Generate(options.InputAssembly, options.OutputFolder, options.EnumTypeNames.ToArray());
                }

                if (!string.IsNullOrEmpty(options.FixAssemblyPath))
                {
                    Corlib.FixCorlib.Fix(options.FixAssemblyPath);
                }

                if (!string.IsNullOrEmpty(options.FindApiEnhancements))
                {
                    ApiEnhancements.FindApiEnhancements.Find(options.FindApiEnhancements);
                }

                if (!string.IsNullOrEmpty(options.UninstallAPK))
                {
                    UninstallAPK(options.UninstallAPK);
                }

                if (!string.IsNullOrEmpty(options.CheckForwardersAssembly))
                {
                    CheckForwarders.CheckForwardAssemblies.Check(options.CheckForwardersAssembly);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
                return 1;
            }
        }

        private static void UninstallAPK(string path)
        {
            var pathSet = new HashSet<string>();
            if (File.Exists(path))
            {
                pathSet.Add(path);
            }
            else if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.apk", SearchOption.AllDirectories))
                {
                    pathSet.Add(file);
                }
            }


            try
            {
                var names = new HashSet<string>();
                foreach (var iterator in pathSet)
                {
                    var apk = new ApkFile(iterator);
                    var name = apk.Manifest.PackageName;
                    names.Add(name);
                }

                foreach (var name in names)
                {
                    UninstallAPKByPackageName(name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to uninstall {0} because {1}", path, ex.Message);
            }
        }

        private static void UninstallAPKByPackageName(string name)
        {
            var log = new StringBuilder();
            try
            {
                Console.Write("Uninstalling {0}", name);
                var adb = new Adb();
                adb.Logger += x => log.AppendLine(x);
                adb.UninstallApk(null, name, Adb.Timeout.UninstallApk);
                Console.WriteLine(" OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(log);
                Console.WriteLine("Failed to uninstall {0} because {1}", name, ex.Message);
            }
        }
    }
}
