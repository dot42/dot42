namespace Dot42.Utility
{
    /// <summary>
    /// Defines for command line arguments of the various tools
    /// </summary>
    public static class ToolOptions
    {
        /// <summary>
        /// Show help/usage
        /// </summary>
        public static readonly ToolOption Help = new ToolOption("h|?|help");

        /// <summary>
        /// Show activation (when needed)
        /// </summary>
        public static readonly ToolOption Activate = new ToolOption("activate");

        /// <summary>
        /// Input assembly path
        /// </summary>
        public static readonly ToolOption InputAssembly = new ToolOption("a=|assembly=");

        /// <summary>
        /// Fix assembly path
        /// </summary>
        public static readonly ToolOption FixAssembly = new ToolOption("fa=");

        /// <summary>
        /// Find API enhancements assembly path
        /// </summary>
        public static readonly ToolOption FindApiEnhancementsAssembly = new ToolOption("faea=");

        /// <summary>
        /// Uninstall APK
        /// </summary>
        public static readonly ToolOption UninstallAPK = new ToolOption("unapk=");

        /// <summary>
        /// Native code library path
        /// </summary>
        public static readonly ToolOption NativeCodeLibrary = new ToolOption("nclib=");

        /// <summary>
        /// Path of file containing type/member usage information originating from the resources.
        /// </summary>
        public static readonly ToolOption ResourceTypeUsageInformationPath = new ToolOption("rtip=");

        /// <summary>
        /// Generate debug info
        /// </summary>
        public static readonly ToolOption DebugInfo = new ToolOption("d|debug");

        /// <summary>
        /// Generate set next instruction code. This can be only used for debug builds,
        /// an will only generate code for debg assemblies.
        /// </summary>
        public static readonly ToolOption GenerateSetNextInstructionCode = new ToolOption("gensetnextinstr");

        /// <summary>
        /// Input resources.arsc path
        /// </summary>
        public static readonly ToolOption InputResources = new ToolOption("resources=");

        /// <summary>
        /// Input frameworks folder
        /// </summary>
        public static readonly ToolOption InputFrameworksFolder = new ToolOption("frameworksfolder=");

        /// <summary>
        /// Reference assembly
        /// </summary>
        public static readonly ToolOption Reference = new ToolOption("r=|ref=");

        /// <summary>
        /// Folder containing references
        /// </summary>
        public static readonly ToolOption ReferenceFolder = new ToolOption("l=|lib=");

        /// <summary>
        /// Folder used for results
        /// </summary>
        public static readonly ToolOption OutputFolder = new ToolOption("out=");

        /// <summary>
        /// Folder used for intermediate results
        /// </summary>
        public static readonly ToolOption TempFolder = new ToolOption("tmp=");

        /// <summary>
        /// Folder towards generated code is written
        /// </summary>
        public static readonly ToolOption GeneratedCodeFolder = new ToolOption("gcout=");

        /// <summary>
        /// Namespace for generated code 
        /// </summary>
        public static readonly ToolOption GeneratedCodeNamespace = new ToolOption("gcns=");

        /// <summary>
        /// Language for generated code 
        /// </summary>
        public static readonly ToolOption GeneratedCodeLanguage = new ToolOption("gclang=");

        /// <summary>
        /// Folder containing all precompiled and raw resources
        /// </summary>
        public static readonly ToolOption ResourcesFolder = new ToolOption("res=");

        /// <summary>
        /// File towards the used output folder is written
        /// </summary>
        public static readonly ToolOption OutputFolderFile = new ToolOption("outfolderfile=");

        /// <summary>
        /// Path of input manifest
        /// </summary>
        public static readonly ToolOption InputManifest = new ToolOption("m=");

        /// <summary>
        /// Switch to create the android manifest from the assembly.
        /// </summary>
        public static readonly ToolOption CreateManifest = new ToolOption("cm");

        /// <summary>
        /// Switch to compile given resources.
        /// </summary>
        public static readonly ToolOption CompileResources = new ToolOption("cr");

        /// <summary>
        /// Path of input jar file
        /// </summary>
        public static readonly ToolOption InputJar = new ToolOption("j=|jar=");

        /// <summary>
        /// Library name
        /// </summary>
        public static readonly ToolOption LibName = new ToolOption("libname=");

        /// <summary>
        /// Import library as stubs only
        /// </summary>
        public static readonly ToolOption ImportStubs = new ToolOption("stubs");

        /// <summary>
        /// Path of input attrs.xml file
        /// </summary>
        public static readonly ToolOption InputAttrsXml = new ToolOption("attrs=");

        /// <summary>
        /// Path of input source.properties
        /// </summary>
        public static readonly ToolOption InputSourceProperties = new ToolOption("sp=");

        /// <summary>
        /// Path of signing certificate
        /// </summary>
        public static readonly ToolOption CertificatePath = new ToolOption("pfx=");

        /// <summary>
        /// Password of signing certificate
        /// </summary>
        public static readonly ToolOption CertificatePassword = new ToolOption("pwd=");

        /// <summary>
        /// Signing certificate thumbprint
        /// </summary>
        public static readonly ToolOption CertificateThumbprint = new ToolOption("ctp=");

        /// <summary>
        /// Path of free apps key file
        /// </summary>
        public static readonly ToolOption FreeAppsKeyPath = new ToolOption("fak=");

        /// <summary>
        /// Path of input code file
        /// </summary>
        public static readonly ToolOption InputCodeFile = new ToolOption("c=");

        /// <summary>
        /// Path of input map file
        /// </summary>
        public static readonly ToolOption InputMapFile = new ToolOption("map=");

        /// <summary>
        /// Path of output APK/BAR
        /// </summary>
        public static readonly ToolOption OutputPackage = new ToolOption("pkg=");

        /// <summary>
        /// Path of a animation resource
        /// </summary>
        public static readonly ToolOption AnimationResource = new ToolOption("ares=");

        /// <summary>
        /// Path of a drawable resource
        /// </summary>
        public static readonly ToolOption DrawableResource = new ToolOption("dres=");

        /// <summary>
        /// Path of a layout resource
        /// </summary>
        public static readonly ToolOption LayoutResource = new ToolOption("lres=");

        /// <summary>
        /// Path of a menu resource
        /// </summary>
        public static readonly ToolOption MenuResource = new ToolOption("mres=");

        /// <summary>
        /// Path of a values resource
        /// </summary>
        public static readonly ToolOption ValuesResource = new ToolOption("vres=");

        /// <summary>
        /// Path of a xml resource
        /// </summary>
        public static readonly ToolOption XmlResource = new ToolOption("xres=");

        /// <summary>
        /// Path of a raw resource
        /// </summary>
        public static readonly ToolOption RawResource = new ToolOption("rres=");

        /// <summary>
        /// Name of the package to create
        /// </summary>
        public static readonly ToolOption PackageName = new ToolOption("pkgname=");

        /// <summary>
        /// Root namespace used in the assembly.
        /// </summary>
        public static readonly ToolOption RootNamespace = new ToolOption("rootns=");

        /// <summary>
        /// Path of vstemplate to be updated.
        /// </summary>
        public static readonly ToolOption VsTemplatePath = new ToolOption("vstemplate=");

        /// <summary>
        /// Target platform.
        /// </summary>
        public static readonly ToolOption Target = new ToolOption("target=");

        /// <summary>
        /// Folder containing doxygen xml output.
        /// </summary>
        public static readonly ToolOption DoxygenXmlFolder = new ToolOption("doxygenxml=");

        /// <summary>
        /// Folder used for samples
        /// </summary>
        public static readonly ToolOption SamplesFolder = new ToolOption("samplefolder=");

        /// <summary>
        /// Script path
        /// </summary>
        public static readonly ToolOption Script = new ToolOption("script=");

        /// <summary>
        /// Folder containing forward assembly sources
        /// </summary>
        public static readonly ToolOption ForwardAssembliesFolder = new ToolOption("forwardassembliesfolder=");

        /// <summary>
        /// Public key token
        /// </summary>
        public static readonly ToolOption PublicKeyToken = new ToolOption("publickeytoken=");

        /// <summary>
        /// Generate version only
        /// </summary>
        public static readonly ToolOption VersionOnly = new ToolOption("versiononly");

        /// <summary>
        /// Path of an app widget provider code file
        /// </summary>
        public static readonly ToolOption AppWidgetProvider = new ToolOption("awp=");

        /// <summary>
        /// Source files of Dot42 Resource System ID's
        /// </summary>
        public static readonly ToolOption SystemIdSourceFile = new ToolOption("idsource=");

        /// <summary>
        /// Source files of dot42 debugger exceptions
        /// </summary>
        public static readonly ToolOption DebuggerExceptionsSourceFile = new ToolOption("dbgexceptionssource=");

        /// <summary>
        /// Set compilation mode to application
        /// </summary>
        public static readonly ToolOption CompilationModeApplication = new ToolOption("modeapp");

        /// <summary>
        /// Set compilation mode to class library
        /// </summary>
        public static readonly ToolOption CompilationModeClassLibrary = new ToolOption("modecl");

        /// <summary>
        /// Set compilation mode to all
        /// </summary>
        public static readonly ToolOption CompilationModeAll = new ToolOption("modeall");

        /// <summary>
        /// Set compilation target to dex (default)
        /// </summary>
        public static readonly ToolOption CompilationTargetDex = new ToolOption("ctd");

        /// <summary>
        /// Set compilation target to java
        /// </summary>
        public static readonly ToolOption CompilationTargetJava = new ToolOption("ctj");

        /// <summary>
        /// Specify Target SDK version
        /// </summary>
        public static readonly ToolOption TargetSdkVersion = new ToolOption("tsv=");

        /// <summary>
        /// Specify enum type name
        /// </summary>
        public static readonly ToolOption EnumTypeName = new ToolOption("etn=");

        /// <summary>
        /// Exclude a given package from importing
        /// </summary>
        public static readonly ToolOption ExcludePackage = new ToolOption("exclpackage=");

        /// <summary>
        /// Set compiler to WCF Proxy tool
        /// </summary>
        public static readonly ToolOption WcfProxyInputAssembly = new ToolOption("wcfa=");

        /// <summary>
        /// Set path of WCF proxy generated source file
        /// </summary>
        public static readonly ToolOption WcfProxyGeneratedSourcePath = new ToolOption("wcfgcp=");

        /// <summary>
        /// Set path of a blackberry debug token
        /// </summary>
        public static readonly ToolOption DebugToken = new ToolOption("dbgtok=");

        /// <summary>
        /// Check forwarders in assembly
        /// </summary>
        public static readonly ToolOption CheckForwarders = new ToolOption("chkfwa=");

        public static readonly ToolOption EnableCompilerCache = new ToolOption("enable-compiler-cache");

        /// <summary>
        /// Enble usage of 'dx' from Android SDK Tools to compile .jar classes.
        /// </summary>
        public static readonly ToolOption EnableDxJarCompilation = new ToolOption("enable-dx-jar-compilation");
    }
}
