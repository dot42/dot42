using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dot42.ApkLib.Resources;
using Dot42.Compiler.Manifest;
using Dot42.Compiler.Resources;
using Dot42.CompilerLib;
using Dot42.CompilerLib.CompilerCache;
using Dot42.CompilerLib.XModel;
using Dot42.ImportJarLib;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Java;
using Dot42.Utility;
using Dot42.WcfTools.ProxyBuilder;
using Mono.Cecil;
using TallComponents.Common.Util;
using NameConverter = Dot42.CompilerLib.NameConverter;
using ResourceType = Dot42.ResourcesLib.ResourceType;

namespace Dot42.Compiler
{
    internal static class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        public static int Main(string[] args)
        {
            try
            {
#if DEBUG
                if (args.Length == 0)
                {
                    var assemblyFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\RegressionTests\HelloWorld\obj\Debug"));
                    var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\RegressionTests\Compiler\obj\Debug"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\RegressionTests\Compiler\obj\Release"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\RegressionTests\ImportJavaLib\obj\Debug"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"D:\Ewout\Documents\Visual Studio 2010\Projects\RegisterTest\obj\Debug"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\RegressionTests\Framework\obj\Debug"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\Samples\SimpleAnimation\obj\Debug"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\Build\setje"));
                    var frameworkFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\Build\Application\Frameworks\v4.4"));
                    //var objFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"S:\SVNWork\Dot42\Samples\Calculator\obj\Debug"));
                    args = new[] {
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "HelloWorld.dll")),
                        ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "CompilerTests.dll")),
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "unittests.dll")),
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "JavaImportTest.dll")),
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "RegisterTest.dll")),
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "FrameworkTests.dll")),
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "Calculator.dll")),
                        //ToolOptions.InputAssembly.CreateArg(Path.Combine(objFolder, "SimpleAnimation.dll")),
                        ToolOptions.InputResources.CreateArg(Path.Combine(objFolder, @"TempRes\resources.arsc")),
                        ToolOptions.OutputFolder.CreateArg(objFolder),
                        ToolOptions.PackageName.CreateArg("com.test.CompilerTests"),
                        ToolOptions.ReferenceFolder.CreateArg(frameworkFolder),
                        //ToolOptions.ReferenceFolder.CreateArg(objFolder),
                        //ToolOptions.CompilationModeAll,
                    };

                    string cmdLine;
                    //cmdLine = @"--pkgname com.dot42.regressiontest.UsingSuportLibrary --rootns UsingSuportLibrary --out S:\SVNWork\Dot42\RegressionTests\UsingSupportLibrary\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\UsingSupportLibrary\obj\Debug\UsingSuportLibrary.dll --resources S:\SVNWork\Dot42\RegressionTests\UsingSupportLibrary\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v2.3.3 --fak S:\SVNWork\Dot42\RegressionTests\UsingSupportLibrary\obj\Debug\UsingSuportLibrary.fak";
                    //cmdLine = @"--pkgname com.dot42.regressiontest.UsingSuportLibrary --rootns UsingSuportLibrary --out S:\SVNWork\Dot42\Samples\Various\Support4Demos\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\Samples\Various\Support4Demos\obj\Debug\Support4Demos.dll --resources S:\SVNWork\Dot42\RegressionTests\UsingSupportLibrary\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v2.3.3 --fak S:\SVNWork\Dot42\RegressionTests\UsingSupportLibrary\obj\Debug\UsingSuportLibrary.fak";
                    //cmdLine = @"--modeapp --pkgname com.dot42.regressiontest.UsingGoogleMobAds --rootns UsingGoogleMobAds --out S:\SVNWork\Dot42\RegressionTests\UsingGoogleMobAds\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\UsingGoogleMobAds\obj\Debug\UsingGoogleMobAds.dll --resources S:\SVNWork\Dot42\RegressionTests\UsingGoogleMobAds\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v2.3.3 --fak S:\SVNWork\Dot42\RegressionTests\UsingGoogleMobAds\obj\Debug\UsingGoogleMobAds.fak";
                    //cmdLine = @"--modeapp --pkgname com.dot42.regressiontest.Performance --rootns Performance --out S:\SVNWork\Dot42\RegressionTests\Performance\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\Performance\obj\Debug\Performance.dll --resources S:\SVNWork\Dot42\RegressionTests\Performance\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.0.3 --fak S:\SVNWork\Dot42\RegressionTests\Performance\obj\Debug\Performance.fak";

                    //cmdLine =@"--pkgname com.SorterenMaar --rootns SorterenMaar --out S:\SVNWork\Dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\SorterenMaar.dll --resources S:\SVNWork\Dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.0.3 --fak S:\SVNWork\Dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\SorterenMaar.fak";
                    //cmdLine = @"--pkgname com.Case656 --rootns Case656 --out S:\SVNWork\Dot42\RegressionTests\Cases\cs656\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\Cases\cs656\obj\Debug\Case656.dll --resources S:\SVNWork\Dot42\RegressionTests\Cases\cs656\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.0.3 --fak S:\SVNWork\Dot42\RegressionTests\Cases\cs662\obj\Debug\Case656.fak";
                    //cmdLine = @"--wcfa=D:\SVNWork\dot42\RegressionTests\WcfClient\RestService\bin\Debug\RestService.dll --wcfgcp=c:\temp\test.cs --lib=D:\SVNWork\Dot42\Build\Application\Frameworks\v4.0.3";

                    //cmdLine = @"--modeapp --pkgname com.dot42parse --rootns dot42parse --out S:\SVNWork\Dot42\RegressionTests\Cases\cs765\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\Cases\cs765\obj\Debug\dot42parse.dll --resources S:\SVNWork\Dot42\RegressionTests\Cases\cs765\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.1 --fak S:\SVNWork\Dot42\RegressionTests\Cases\cs765\obj\Debug\dot42parse.fak";
                    //cmdLine = @"--modeapp --pkgname com.Test1 --rootns Test1 --out S:\SVNWork\Dot42\RegressionTests\Cases\cs760\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\Cases\cs760\obj\Debug\Test1.dll --resources S:\SVNWork\Dot42\RegressionTests\Cases\cs760\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.2 --fak S:\SVNWork\Dot42\RegressionTests\Cases\cs760\obj\Debug\Test1.fak";
                    //cmdLine = @"--modeapp --pkgname com.dot42.examples.googleplusclient --rootns GooglePlusClient --out S:\SVNWork\Dot42\RegressionTests\Cases\cs773\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\Cases\cs773\obj\Debug\GooglePlusClient.dll --resources S:\SVNWork\Dot42\RegressionTests\Cases\cs773\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.1 --fak S:\SVNWork\Dot42\RegressionTests\Cases\cs773\obj\Debug\GooglePlusClient.fak";
                    //cmdLine = @"--modeapp --pkgname com.dot42.regressiontest.ImportJar --rootns ImportJar --out S:\SVNWork\Dot42\RegressionTests\ImportJar\obj\Debug\TempCode\ --d --a S:\SVNWork\Dot42\RegressionTests\ImportJar\obj\Debug\ImportJar.dll --resources S:\SVNWork\Dot42\RegressionTests\ImportJar\obj\Debug\TempRes\resources.arsc --l S:\SVNWork\Dot42\Build\Application\Frameworks\v4.0.3 --fak S:\SVNWork\Dot42\RegressionTests\ImportJar\obj\Debug\ImportJar.fak";

                    //cmdLine = @"--j█D:\Development\Dot42\DummyApp\Case3-gdx-backend-android.jar█--r█Dot42█--r█mscorlib█--r█System█--l█C:\Program Files\dot42\Android\Frameworks\v3.1█--libname█Case3-gdx-backend-android█--gcout█D:\Development\Dot42\DummyApp\obj\Debug\";
                    //cmdLine = @"--j█D:\Development\Dot42\DummyApp\Case4-ormlite-core-4.48.jar█--r█Dot42█--r█mscorlib█--r█System█--l█C:\Program Files\dot42\Android\Frameworks\v3.1█--libname█Case4-ormlite-core-4.48█--gcout█D:\Development\Dot42\DummyApp\obj\Debug\";
                    //cmdLine = @"--j█D:\Development\Dot42\DummyApp\Case7-usb-serial-for-android-v010.jar█--r█Dot42█--r█mscorlib█--r█System█--l█C:\Program Files\dot42\Android\Frameworks\v3.1█--libname█Case7-usb-serial-for-android-v010█--gcout█D:\Development\Dot42\DummyApp\obj\Debug\";
                    //cmdLine = @"--j█D:\Development\Dot42\DummyApp\Case8-ksoap2-android-assembly-3.2.0-jar-with-dependencies.jar█--r█Dot42█--r█mscorlib█--r█System█--l█C:\Program Files\dot42\Android\Frameworks\v3.1█--libname█Case8-ksoap2-android-assembly-3.2.0-jar-with-dependencies█--gcout█D:\Development\Dot42\DummyApp\obj\Debug\";
                    //cmdLine = @"--j█D:\Dot42\dot42\RegressionTests\ImportJar\Libs\gson-2.2.2.jar█--r█Dot42█--r█mscorlib█--r█System█--l█D:\Dot42\dot42\Build\Application\Frameworks\v4.0.3█--libname█gson-2.2.2█--gcout█D:\Dot42\dot42\RegressionTests\ImportJar\obj\Debug\";
                    cmdLine = @"--j█D:\Dot42\dot42\RegressionTests\ImportJar\Libs\Parse-1.3.0.jar█--r█Dot42█--r█mscorlib█--r█System█--l█D:\Dot42\dot42\Build\Application\Frameworks\v4.0.3█--libname█Parse-1.3.0█--gcout█D:\Dot42\dot42\RegressionTests\ImportJar\obj\Debug\";
                    
                    //cmdLine = @"--modeapp█--pkgname█com.SorterenMaar█--rootns█SorterenMaar█--out█D:\Dot42\dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\TempCode\█--d█--a█D:\Dot42\dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\SorterenMaar.dll█--resources█D:\Dot42\dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\TempRes\resources.arsc█--l█C:\Program Files\dot42\Android\Frameworks\v4.0.3█--r█Dot42█--r█mscorlib█--r█System█--rtip█D:\Dot42\dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\ResourceTypeUsage.txt█--fak█D:\Dot42\dot42\RegressionTests\Cases\2013-01-19-Brenda-SorterenMaar\obj\Debug\SorterenMaar.fak";
                    //cmdLine = @"--modeapp█--pkgname█dot42.GitHub.Case13█--rootns█Case13█--out█D:\Dot42\dot42\RegressionTests\Cases GitHub\Case13\obj\Debug\TempCode\\█--d█--a█D:\Dot42\dot42\RegressionTests\Cases GitHub\Case13\obj\Debug\Case13.dll█--resources█D:\Dot42\dot42\RegressionTests\Cases GitHub\Case13\obj\Debug\TempRes\resources.arsc█--l█C:\Program Files\dot42\Android\Frameworks\v4.0.3█--r█Dot42█--r█mscorlib█--r█System█--rtip█D:\Dot42\dot42\RegressionTests\Cases GitHub\Case13\obj\Debug\ResourceTypeUsage.txt█--fak█D:\Dot42\dot42\RegressionTests\Cases GitHub\Case13\obj\Debug\Case13.fak";

                    args = cmdLine.Split('█');
                }
#endif

                // Parse command line options
                var options = new CommandLineOptions(args);
                if (options.ShowHelp)
                {
                    options.Usage();
                    return 2;
                }
                MainCode(options);
                return 0;
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
                Console.WriteLine("Error: {0}", ex.Message);
#if DEBUG
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
#endif
                return 1;
            }
        }

        /// <summary>
        /// Program main code
        /// </summary>
        /// <returns>true on success, false on usage due to invalid options</returns>
        internal static bool MainCode(CommandLineOptions options)
        {
            if (options.ShowHelp)
            {
                return false;
            }

            // Detect target
            var target = Locations.SetTarget(options.Target);

            // Build APK/BAR?
            if (!string.IsNullOrEmpty(options.PackagePath))
            {
#if DEBUG
                //Debugger.Launch();
#endif

                // Build APK first
                var apkPath = options.PackagePath;
                if (target == Targets.BlackBerry)
                {
                    // Put APK in TEMP folder
                    apkPath = Path.GetTempFileName() + ".apk";
                }

                var apkBuilder = new ApkBuilder.ApkBuilder(apkPath);
                apkBuilder.MapFilePath = Path.ChangeExtension(options.PackagePath, ".d42map");
                apkBuilder.ManifestPath = options.ManifestFile;
                apkBuilder.DexFiles.AddRange(options.DexFiles);
                apkBuilder.MapFiles.AddRange(options.MapFiles);
                if (options.Assemblies.Any())
                {
                    apkBuilder.Assemblies.AddRange(options.Assemblies);
                }
                apkBuilder.ResourcesFolder = options.ResourcesFolder;
                apkBuilder.PfxFile = options.PfxFile;
                apkBuilder.PfxPassword = options.PfxPassword;
                apkBuilder.CertificateThumbprint = options.CertificateThumbprint;
                apkBuilder.FreeAppsKeyPath = options.FreeAppsKeyPath;
                apkBuilder.PackageName = options.PackageName;
                apkBuilder.NativeCodeLibraries.AddRange(options.NativeCodeLibs);
                apkBuilder.Build();

                if (target == Targets.BlackBerry)
                {
                    // Now build BAR
                    var barBuilder = new BarBuilder.BarBuilder(options.PackagePath);
                    barBuilder.ApkPath = apkPath;
                    barBuilder.DebugTokenPath = options.DebugToken;
                    barBuilder.Build();                    
                }
            }
            else if (!string.IsNullOrEmpty(options.JarFile)) // Import jar file?
            {
                var module = new XModule();
                var resolver = new AssemblyResolver(options.ReferenceFolders, new AssemblyClassLoader(module.OnClassLoaded), module.OnAssemblyLoaded);
                var jarImporter = new JarImporter(options.JarFile, options.LibName, options.ImportStubsOnly, false/*true*/, options.GeneratedCodeFolder, resolver, options.ExcludedPackages, options.UseAutoExcludedPackages);
                foreach (var path in options.References)
                {
                    jarImporter.ImportAssembly(path);
                }
                jarImporter.ImportAssembliesCompleted();
                jarImporter.Import();
            }
            else if (options.WcfProxyInputAssemblies.Any()) // Generate WCF Proxy
            {
                var resolver = new AssemblyResolver(options.ReferenceFolders, null, null);
                var readerParameters = new ReaderParameters(ReadingMode.Immediate) {
                    AssemblyResolver = resolver,
                    SymbolReaderProvider = new SafeSymbolReaderProvider(),
                    ReadSymbols = true
                };
                var assemblies = options.WcfProxyInputAssemblies.Select(x => AssemblyDefinition.ReadAssembly(x, readerParameters)).ToList();
                var proxyTool = new ProxyBuilderTool(assemblies, options.GeneratedProxySourcePath);
                proxyTool.Build();
            }
            else
            {
                // Create namespace converter
                var nsConverter = new NameConverter(options.PackageName, options.RootNamespace);

                // Create manifest file?
                if (options.CreateManifest)
                {
                    ManifestBuilder.CreateManifest(target, options.Assemblies.First(), options.ReferenceFolders, options.PackageName, nsConverter, 
                        options.DebugInfo, options.AppWidgetProviders, options.TargetSdkVersion, options.OutputFolder);
                }
                else
                {
                    // Code compilation?
                    if (options.Assemblies.Any())
                    {
#if DEBUG
                        //Debugger.Launch();
#endif

                        CompileAssembly(options, nsConverter);
                    }
                }

                // Xml compilation
                if (options.CompileResources)
                {
                    if (!CompileResources(options))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compile an assembly into a dex file.
        /// </summary>
        private static void CompileAssembly(CommandLineOptions options, NameConverter nsConverter)
        {
            // Load resource type usage info file
            var usedTypeNames = LoadResourceTypeUsageInformation(options);

            // Load assemblies
            var assemblies = new List<AssemblyDefinition>();
            var module = new XModule();
            var classLoader = new AssemblyClassLoader(module.OnClassLoaded);
            var resolver = new AssemblyResolver(options.ReferenceFolders, classLoader, module.OnAssemblyLoaded);
            var readerParameters = new ReaderParameters(ReadingMode.Immediate)
            {
                AssemblyResolver = resolver,
                SymbolReaderProvider = new SafeSymbolReaderProvider(),
                ReadSymbols = true
            };
            foreach (var asmPath in options.Assemblies)
            {
                var asm = resolver.Load(asmPath, readerParameters);
                module.OnAssemblyLoaded(asm);
                classLoader.LoadAssembly(asm);
                assemblies.Add(asm);
            }
            // Load references
            var references = new List<AssemblyDefinition>();
            foreach (var refPath in options.References)
            {
                var asm = resolver.Load(refPath, readerParameters);
                module.OnAssemblyLoaded(asm);
                classLoader.LoadAssembly(asm);
                references.Add(asm);
            }

            // Load resources
            Table table;
            using (var stream = new FileStream(options.InputResources, FileMode.Open, FileAccess.Read))
            {
                table = new Table(stream);
            }

            var ccache = options.EnableCompilerCache ? new DexMethodBodyCompilerCache(options.OutputFolder, resolver.GetFileName)
                                                     : new DexMethodBodyCompilerCache();

            // Create compiler
            var compiler = new AssemblyCompiler(options.CompilationMode, assemblies, references, table, nsConverter,
                                                options.DebugInfo, classLoader, resolver.GetFileName, ccache, 
                                                usedTypeNames, module, options.GenerateSetNextInstructionCode);
            compiler.Compile();

            ccache.PrintStatistics();

            compiler.Save(options.OutputFolder, options.FreeAppsKeyPath);

            
        }

        private static HashSet<string> LoadResourceTypeUsageInformation(CommandLineOptions options)
        {
            // Load resource type usage info file
            var path = options.ResourceTypeUsageInformationPath;
            if (string.IsNullOrEmpty(path))
            {
                return new HashSet<string>();
            }
            if (!File.Exists(path))
            {
                Console.WriteLine("Cannot load {0}; File does not exist", path);
                return new HashSet<string>();
            }
            return new HashSet<string>(File.ReadAllLines(options.ResourceTypeUsageInformationPath).Where(x => !string.IsNullOrEmpty(x)));
        }

        /// <summary>
        /// Compile all resources.
        /// </summary>
        private static bool CompileResources(CommandLineOptions options)
        {
            // Locate base.apk
            var baseApkPath = SearchBaseApk(options);

            // Create the compiler
            var compiler = new ResourceCompiler(options.OutputFolder, options.GeneratedCodeFolder, options.TempFolder, options.PackageName, options.RootNamespace, options.GeneratedCodeNamespace, options.GeneratedCodeLanguage, options.ResourceTypeUsageInformationPath);

            // Compile the resources
            return compiler.Compile(options.Resources, options.AppWidgetProviders, options.References, options.ReferenceFolders, baseApkPath);
        }

        /// <summary>
        /// Find base.apk in the reference folders.
        /// </summary>
        private static string SearchBaseApk(CommandLineOptions options)
        {
            var files = options.ReferenceFolders.Select(x => Path.Combine(x, "base.apk"));
            var path = files.FirstOrDefault(File.Exists);
            if (path != null)
                return path;
            throw new ArgumentException("base.apk not found");
        }
    }
}
