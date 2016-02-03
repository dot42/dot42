using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.CompilerLib;
using Dot42.ResourcesLib;
using Dot42.Utility;
using Mono.Options;

namespace Dot42.Compiler
{
    internal class CommandLineOptions
    {
        private readonly OptionSet options;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal CommandLineOptions(string[] args)
        {
            CompilationMode = CompilationMode.Application;
            CompilationTarget = CompilationTarget.Dex;
            Assemblies = new List<string>();
            NativeCodeLibs = new List<string>();
            ReferenceFolders = new List<string>();
            DexFiles = new List<string>();
            MapFiles = new List<string>();
            AppWidgetProviders = new List<string>();
            Resources = new List<Tuple<string, ResourceType>>();
            References = new List<string>();
            ExcludedPackages = new List<string>();
            UseAutoExcludedPackages = true;
            GenerateSetNextInstructionCode = false;
            WcfProxyInputAssemblies = new List<string>();
            options = new OptionSet {
                { ToolOptions.Help, "Show usage", v => ShowHelp = true },
                { ToolOptions.CompilationModeApplication, "Set application compilation mode", v => CompilationMode = CompilationMode.Application },
                { ToolOptions.CompilationModeClassLibrary, "Set class library compilation mode", v => CompilationMode = CompilationMode.ClassLibrary },
                { ToolOptions.CompilationModeAll, "Set all compilation mode", v => CompilationMode = CompilationMode.All },
                { ToolOptions.CompilationTargetDex, "Set compilation target to DEX", v => CompilationTarget = CompilationTarget.Dex },
                { ToolOptions.CompilationTargetJava, "Set compilation target to Java", v => CompilationTarget = CompilationTarget.Java },
                { ToolOptions.PackageName, "Specify package name", v => PackageName = v },
                { ToolOptions.RootNamespace, "Specify root namespace", v => RootNamespace = v },
                { ToolOptions.InputAssembly, "Specify input assembly", v => Assemblies.Add(v) },
                { ToolOptions.DebugInfo, "Generate debug info", v => DebugInfo = true },
                { ToolOptions.GenerateSetNextInstructionCode, "Generate set next instruction code", v => GenerateSetNextInstructionCode = true },
                { ToolOptions.InputResources, "Specify input resources", v => InputResources = v },
                { ToolOptions.CreateManifest, "Create AndroidManifest.xml", v => CreateManifest = true },
                { ToolOptions.CompileResources, "Compile given resources", v => CompileResources = true },
                { ToolOptions.InputManifest, "Specify AndroidManifest.xml", v => { ManifestFile = v; Resources.Add(Tuple.Create(v, ResourceType.Manifest)); } },
                { ToolOptions.AnimationResource, "Specify animation resource", v => Resources.Add(Tuple.Create(v, ResourceType.Animation)) },
                { ToolOptions.DrawableResource, "Specify drawable resource", v => Resources.Add(Tuple.Create(v, ResourceType.Drawable)) },
                { ToolOptions.LayoutResource, "Specify layout resource", v => Resources.Add(Tuple.Create(v, ResourceType.Layout)) },
                { ToolOptions.MenuResource, "Specify menu resource", v => Resources.Add(Tuple.Create(v, ResourceType.Menu)) },
                { ToolOptions.ValuesResource, "Specify values resource", v => Resources.Add(Tuple.Create(v, ResourceType.Values)) },
                { ToolOptions.XmlResource, "Specify xml resource", v => Resources.Add(Tuple.Create(v, ResourceType.Xml)) },
                { ToolOptions.RawResource, "Specify raw resource", v => Resources.Add(Tuple.Create(v, ResourceType.Raw)) },
                { ToolOptions.AppWidgetProvider, "Specify an AppWidgetProvider code file", v => AppWidgetProviders.Add(v) },
                { ToolOptions.ReferenceFolder, "Add reference folder", v => ReferenceFolders.Add(v) },
                { ToolOptions.OutputFolder, "Set output folder", v => OutputFolder = v },
                { ToolOptions.GeneratedCodeNamespace, "Set namespace for generated code", v => GeneratedCodeNamespace = v },
                { ToolOptions.GeneratedCodeFolder, "Set output folder for generated code", v => GeneratedCodeFolder = v },
                { ToolOptions.GeneratedCodeLanguage, "Set language for generated code", v => GeneratedCodeLanguage = v },
                { ToolOptions.TempFolder, "Set temporary folder", v => TempFolder = v },
                { ToolOptions.TargetSdkVersion, "Set target SDK version", v => TargetSdkVersion = v },
                { ToolOptions.NativeCodeLibrary, "Specify native code library", v => NativeCodeLibs.Add(v) },
                { ToolOptions.ResourceTypeUsageInformationPath, "Resource type usage information path", v => ResourceTypeUsageInformationPath = v },
                { ToolOptions.Target, "Set target", v => Target = v },
                { ToolOptions.EnableCompilerCache, "Enable compiler cache", v => EnableCompilerCache = true },
                { ToolOptions.EnableDxJarCompilation, "Use 'dx' from Android SDK Tools to compile .jar files.", v => EnableDxJarCompilation = true },
                // APK Builder
                { ToolOptions.OutputPackage, "Set output package path", v => PackagePath = v },
                { ToolOptions.InputCodeFile, "Add code file (*.dex)", v => DexFiles.Add(v) },
                { ToolOptions.InputMapFile, "Add map file (*.d42map)", v => MapFiles.Add(v) },
                { ToolOptions.ResourcesFolder, "Set resources folder", v => ResourcesFolder = v },
                { ToolOptions.CertificatePath, "Set signing certificate path", v => PfxFile = v },
                { ToolOptions.CertificatePassword, "Set signing certificate password", v => PfxPassword = v },
                { ToolOptions.CertificateThumbprint, "Set signing certificate thumbprint", v => CertificateThumbprint = v },
                { ToolOptions.FreeAppsKeyPath, "Set free apps key path", v => FreeAppsKeyPath = v },
                { ToolOptions.DebugToken, "Set debug token", v => DebugToken = v },
                // JAR Import
                { ToolOptions.InputJar, "JAR file to import", v => JarFile = v },
                { ToolOptions.Reference, "Added reference assembly", v => References.Add(v) },
                { ToolOptions.LibName, "Name of library that is being imported", v => LibName = v },
                { ToolOptions.ImportStubs, "Import library as stubs only", v => ImportStubsOnly = true },
                { ToolOptions.ExcludePackage, "Exclude given package from importing", v => { ExcludedPackages.Add(v); UseAutoExcludedPackages = false; } },
                // WCF Proxy Tool
                { ToolOptions.WcfProxyInputAssembly, "WCF Proxy Input assembly", v => WcfProxyInputAssemblies.Add(v) },
                { ToolOptions.WcfProxyGeneratedSourcePath, "WCF Proxy generated source path", v => GeneratedProxySourcePath = v },
            };
            options.Parse(args);

            var haveAssemblies = Assemblies.Any();
            var haveInputResources = !string.IsNullOrEmpty(InputResources);
            var havePackageName = !string.IsNullOrEmpty(PackageName);
            var haveGeneratedCodeFolder = !string.IsNullOrEmpty(GeneratedCodeFolder);
            var haveGeneratedCodeLanguage = !string.IsNullOrEmpty(GeneratedCodeLanguage);
            var haveTempFolder = !string.IsNullOrEmpty(TempFolder);
            var havePackageFile = !string.IsNullOrEmpty(PackagePath);
            var haveRootNamespace = !string.IsNullOrEmpty(RootNamespace);
            var haveJarFile = !string.IsNullOrEmpty(JarFile);
            var haveWcfProxyInputAssemblies = WcfProxyInputAssemblies.Any();
            var haveWcfProxyGenSourcePath = !string.IsNullOrEmpty(GeneratedProxySourcePath);

            if (havePackageFile)
            {
                ShowHelp |= (string.IsNullOrEmpty(ManifestFile));
                ShowHelp |= (string.IsNullOrEmpty(PackagePath));
                ShowHelp |= (string.IsNullOrEmpty(PfxFile) && string.IsNullOrEmpty(CertificateThumbprint));
                ShowHelp |= string.IsNullOrEmpty(PackageName);
            }
            else if (haveJarFile)
            {
                ShowHelp |= !haveGeneratedCodeFolder;
                ShowHelp |= string.IsNullOrEmpty(LibName);
                ShowHelp |= (ReferenceFolders.Count == 0);
            }
            else if (haveWcfProxyInputAssemblies)
            {
                ShowHelp = !haveWcfProxyGenSourcePath;
                ShowHelp |= (ReferenceFolders.Count == 0);
            }
            else
            {
                ShowHelp |= (!haveAssemblies && !CompileResources && !CreateManifest);
                ShowHelp |= (!CreateManifest) && haveAssemblies && !haveInputResources;
                ShowHelp |= (string.IsNullOrEmpty(OutputFolder));
                ShowHelp |= CompileResources &&
                            ((ReferenceFolders.Count == 0) || !havePackageName || !haveTempFolder || !haveRootNamespace);
                ShowHelp |= CreateManifest && (!haveAssemblies || !havePackageName);
                ShowHelp |= haveGeneratedCodeFolder && !haveGeneratedCodeLanguage;
            }
        }

        public bool ShowHelp { get; private set; }
        public string Target { get; private set; }
        public CompilationMode CompilationMode { get; private set; }
        public CompilationTarget CompilationTarget { get; private set; }
        public bool CreateManifest { get; private set; }
        public bool CompileResources { get; private set; }
        public bool DebugInfo { get; private set; }
        public List<Tuple<string, ResourceType>> Resources { get; private set; }
        public string PackageName { get; private set; }
        public List<string> Assemblies { get; private set; }
        public List<string> NativeCodeLibs { get; private set; }
        public string InputResources { get; private set; }
        public string OutputFolder { get; private set; }
        public string TempFolder { get; private set; }
        public string RootNamespace { get; private set; }
        public string GeneratedCodeNamespace { get; private set; }
        public string GeneratedCodeFolder { get; private set; }
        public string GeneratedCodeLanguage { get; private set; }
        public List<string> ReferenceFolders { get; private set; }
        public List<string> AppWidgetProviders { get; private set; }
        public string TargetSdkVersion { get; private set; }
        public string ResourceTypeUsageInformationPath { get; private set; }
        public bool GenerateSetNextInstructionCode { get; private set; }
        public bool EnableCompilerCache { get; private set; }
        public bool EnableDxJarCompilation { get; set; }

        // APK Builder
        public string ManifestFile { get; private set; }
        public string PackagePath { get; private set; }
        public List<string> DexFiles { get; private set; }
        public List<string> MapFiles { get; private set; }
        public string ResourcesFolder { get; private set; }
        public string PfxFile { get; private set; }
        public string PfxPassword { get; private set; }
        public string CertificateThumbprint { get; private set; }
        public string FreeAppsKeyPath { get; private set; }
        public string DebugToken { get; private set; }

        // JAR import
        public string JarFile { get; private set; }
        public string LibName { get; private set; }
        public bool ImportStubsOnly { get; private set; }
        public List<string> References { get; private set; }
        public bool UseAutoExcludedPackages { get; private set; }
        public List<string> ExcludedPackages { get; private set; }

        // WCF proxy tool
        public string GeneratedProxySourcePath { get; private set; }
        public List<string> WcfProxyInputAssemblies { get; private set; }

        /// <summary>
        /// Show usage
        /// </summary>
        public void Usage()
        {
            options.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// Show usage
        /// </summary>
        public string GetUsage()
        {
            using (var writer = new StringWriter())
            {
                options.WriteOptionDescriptions(writer);
                writer.Flush();
                return writer.ToString();
            }
        }
    }
}
