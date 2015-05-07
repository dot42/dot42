using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Custom;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;
using Dot42.Utility;
using ICSharpCode.SharpZipLib.Zip;
using CodeGenerator = Dot42.ImportJarLib.CodeGenerator;

namespace Dot42.FrameworkBuilder
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
                    catalog.Catalogs.Add(new AssemblyCatalog(typeof (Program).Assembly));
                    //catalog.Catalogs.Add(new DirectoryCatalog(".", "*.Plugin.dll"));

                    compositionContainer = new CompositionContainer(catalog);
                }
                return compositionContainer;
            }
        }

        /// <summary>
        /// Entry point
        /// </summary>
        internal static int Main(string[] args)
        {
            try
            {
#if DEBUG
                if (args.Length == 0)
                {
                    var assemblyFolder = Path.GetDirectoryName(typeof (Program).Assembly.Location);
                    var folder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\Sources\Framework\Generated"));
                    var sdkRoot = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\Binaries"));
                    var xmlFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\Android\Docs\xml"));
                    var forwardAssembliesFolder = Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\..\..\Sources\Framework\ForwardAssemblies"));
                    //var sdkRoot = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
                    if (string.IsNullOrEmpty(sdkRoot))
                        throw new ArgumentException("Set ANDROID_SDK_ROOT environment variable");
                    var platformFolder = Path.Combine(sdkRoot, @"platforms\android-15");
                    args = new[] {
                        ToolOptions.InputJar.CreateArg(Path.Combine(platformFolder, "Android.jar")),
                        ToolOptions.InputAttrsXml.CreateArg(Path.Combine(platformFolder, @"data\res\values\attrs.xml")),
                        ToolOptions.InputSourceProperties.CreateArg(Path.Combine(platformFolder, "source.properties")),
                        ToolOptions.OutputFolder.CreateArg(folder),
                        //ToolOptions.DoxygenXmlFolder.CreateArg(xmlFolder),
                        //ToolOptions.PublicKeyToken.CreateArg("0a72796057571e65"),
                        ToolOptions.ForwardAssembliesFolder.CreateArg(forwardAssembliesFolder)
                    };
                }
#endif

                var options = new CommandLineOptions(args);
                if (options.ShowHelp)
                {
                    options.Usage();
                    return 2;
                }

                if (!File.Exists(options.FrameworkJar))
                {
                    throw new ArgumentException(string.Format("Framework jar ({0}) not found.", options.FrameworkJar));
                }

                if (!File.Exists(options.SourceProperties))
                {
                    throw new ArgumentException(string.Format("Source.properties ({0}) not found.", options.SourceProperties));                    
                }
                var sdkPropertiesPath = Path.Combine(Path.GetDirectoryName(options.SourceProperties), "sdk.properties");
                if (!File.Exists(sdkPropertiesPath))
                {
                    throw new ArgumentException(string.Format("sdk.properties ({0}) not found.", sdkPropertiesPath));
                }

                // Load source.properties
                var sourceProperties = new SourceProperties(options.SourceProperties);

                using (var jf = new JarFile(File.Open(options.FrameworkJar, FileMode.Open, FileAccess.Read), AttributeConstants.Dot42Scope, null))
                {
                    // Create output folder
                    var folder = Path.Combine(options.OutputFolder, sourceProperties.PlatformVersion);
                    Directory.CreateDirectory(folder);

                    if (!options.VersionOnly)
                    {
                        // Load Doxygen model
                        var xmlModel = new DocModel();
                        using (Profiler.Profile(x => Console.WriteLine("{0:####} ms for loading Doxygen", x.TotalMilliseconds)))
                        {
                            xmlModel.Load(options.DoxygenXmlFolder);
                        }

                        // Create mscorlib
                        CreateFrameworkAssembly(jf, xmlModel, sourceProperties, folder);

                        // Copy AndroidManifest.xml into the assembly.
                        var manifestStream = jf.GetResource("AndroidManifest.xml");

                        // Copy resources.arsc into the assembly.
                        var resourcesStream = jf.GetResource("resources.arsc");

                        // Load attrs.xml into memory
                        var attrsXml = File.ReadAllBytes(options.AttrsXml);

                        // Load layout.xml into memory
                        var layoutXml = File.ReadAllBytes(Path.Combine(folder, "layout.xml"));

                        // Create base package
                        var basePackagePath = Path.Combine(folder, "base.apk");
                        using (var fileStream = new FileStream(basePackagePath, FileMode.Create, FileAccess.Write))
                        {
                            using (var zipStream = new ZipOutputStream(fileStream) {UseZip64 = UseZip64.Off})
                            {
                                zipStream.SetLevel(9);

                                zipStream.PutNextEntry(new ZipEntry("AndroidManifest.xml")
                                {CompressionMethod = CompressionMethod.Deflated});
                                zipStream.Write(manifestStream, 0, manifestStream.Length);
                                zipStream.CloseEntry();

                                zipStream.PutNextEntry(new ZipEntry("resources.arsc")
                                {CompressionMethod = CompressionMethod.Deflated});
                                zipStream.Write(resourcesStream, 0, resourcesStream.Length);
                                zipStream.CloseEntry();

                                zipStream.PutNextEntry(new ZipEntry(@"attrs.xml")
                                {CompressionMethod = CompressionMethod.Deflated});
                                zipStream.Write(attrsXml, 0, attrsXml.Length);
                                zipStream.CloseEntry();

                                zipStream.PutNextEntry(new ZipEntry(@"layout.xml")
                                {CompressionMethod = CompressionMethod.Deflated});
                                zipStream.Write(layoutXml, 0, layoutXml.Length);
                                zipStream.CloseEntry();
                            }
                        }

                        // Create output folder file
                        if (!string.IsNullOrEmpty(options.OutputFolderFile))
                        {
                            File.WriteAllText(options.OutputFolderFile, folder);
                        }
                    }

                    // Create output version file
                    var version =
                        string.Format(File.ReadAllText(Path.Combine(folder, "..", "..", "Header.txt")), "Version.cs") + Environment.NewLine +
                        string.Format("[assembly: System.Reflection.AssemblyVersion(\"{0}\")]", sourceProperties.AssemblyVersion) + Environment.NewLine +
                        string.Format("[assembly: System.Reflection.AssemblyFileVersion(\"{0}\")]", sourceProperties.AssemblyFileVersion) + Environment.NewLine +
                        string.Format("[assembly: System.Reflection.AssemblyInformationalVersion(\"{0}\")]", sourceProperties.AssemblyInformationalVersion) + Environment.NewLine +
                        "#if !BASELIB" + Environment.NewLine +
                        string.Format("[assembly: System.Runtime.Versioning.TargetFramework(\"Dot42,Version={0}\", FrameworkDisplayName = \"Dot42\")]", sourceProperties.PlatformVersion) + Environment.NewLine +
                        "#endif" + Environment.NewLine;
                    File.WriteAllText(Path.Combine(folder, "Version.cs"), version);

                    if (!options.VersionOnly)
                    {
                        // Load sdk.properties
                        var sdkProperties = new SdkProperties(sdkPropertiesPath);

                        // Create framework ini
                        var frameworkIni = new FrameworkIni(folder);
                        Initialize(frameworkIni, sourceProperties, sdkProperties);
                        frameworkIni.Save();

                        // Create FrameworkList.xml
                        FrameworkListBuilder.Build(folder, options.ForwardAssembliesFolder, options.PublicKeyToken,
                                                   sourceProperties.AssemblyVersion, sourceProperties.PlatformVersion);
                    }
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

        /// <summary>
        /// Create Dot42.dll
        /// </summary>
        private static void CreateFrameworkAssembly(JarFile jf, DocModel xmlModel, SourceProperties sourceProperties, string folder)
        {
            // Initialize all
            MappedTypeBuilder.Initialize(CompositionContainer);

            // Create java type wrappers
            var module = new NetModule(AttributeConstants.Dot42Scope);
            var classLoader = new AssemblyClassLoader(null);
            var target = new TargetFramework(null, classLoader, xmlModel, LogMissingParamNamesType, true, false, Enumerable.Empty<string>());

            List<TypeBuilder> typeBuilders;
            using (Profiler.Profile(x => Console.WriteLine("{0:####} ms for Create()", x.TotalMilliseconds)))
            {
                var classTypeBuilders = jf.ClassNames.SelectMany(n => StandardTypeBuilder.Create(jf.LoadClass(n), target));
                var customTypeBuilder = CompositionContainer.GetExportedValues<ICustomTypeBuilder>()
                                                            .OrderBy(x => x.CustomTypeName)
                                                            .Select(x => x.AsTypeBuilder());

                typeBuilders = classTypeBuilders.Concat(customTypeBuilder)
                                                .OrderBy(x => x.Priority)
                                                .ToList();

                
                                                                           
                typeBuilders.ForEach(x => x.CreateType(null, module, target));
            }

            // Create JavaRef attribute
            //JavaRefAttributeBuilder.Build(asm.MainModule);

            // Implement and finalize types
            using (Profiler.Profile(x => Console.WriteLine("{0:####} ms for Implement()", x.TotalMilliseconds)))
            {
                JarImporter.Implement(typeBuilders, target);
            }

            // Save
            using (Profiler.Profile(x => Console.WriteLine("{0:####} ms for Generate()", x.TotalMilliseconds)))
            {
                CodeGenerator.Generate(folder, module.Types, new List<NetCustomAttribute>(), target, new FrameworkCodeGeneratorContext(), target);
            }

            // Create layout.xml
            var doc = new XDocument(new XElement("layout"));
            typeBuilders.ForEach(x => x.FillLayoutXml(jf, doc.Root));
            doc.Save(Path.Combine(folder, "layout.xml"));

            // create dot42.typemap
            doc = new XDocument(new XElement("typemap"));
            typeBuilders.ForEach(x => x.FillTypemapXml(jf, doc.Root));
                doc.Save(Path.Combine(folder, "dot42.typemap"));
            //using (var s = new FileStream(Path.Combine(folder, "dot42.typemap"), FileMode.Create))
            //    CompressedXml.WriteTo(doc, s, Encoding.UTF8);
        }

        /// <summary>
        /// Setup framework.ini values.
        /// </summary>
        private static void Initialize(FrameworkIni frameworkIni, SourceProperties sourceProperties, SdkProperties sdkProperties)
        {
            frameworkIni.Target = "android-" + sourceProperties.ApiLevel;
            frameworkIni.ApiLevel = int.Parse(sourceProperties.ApiLevel);
            foreach (var key in sdkProperties.Keys.Where(x => !x.Contains(".ant.")))
            {
                frameworkIni[key] = sdkProperties[key];
            }
        }

        private static void LogMissingParamNamesType(string typeName)
        {
            Debug.WriteLine(string.Format("Missing parameter names of {0}", typeName));
        }

        private sealed class FrameworkCodeGeneratorContext : ICodeGeneratorContext
        {
            private static readonly string[] PossibleRoots = new[] { "Other", "Android", "Java" };

            /// <summary>
            /// Add code to the header of a source file.
            /// </summary>
            void ICodeGeneratorContext.CreateSourceFileHeader(TextWriter writer)
            {
                writer.WriteLine("#pragma warning disable 1717"); // Assignment to same variable
            }

            /// <summary>
            /// If true, all methods are generated as extern.
            /// </summary>
            bool ICodeGeneratorContext.GenerateExternalMethods { get { return false; } }

            /// <summary>
            /// If true, will output debug releated comments
            /// </summary>
            bool ICodeGeneratorContext.GenerateDebugComments { get { return true; } }

            /// <summary>
            /// Gets all roots that can come our of <see cref="GetNamespaceRoot"/>
            /// </summary>
            public IEnumerable<string> PossibleNamespaceRoots { get { return PossibleRoots; } }

            /// <summary>
            /// Gets the root part of the namespace.
            /// </summary>
            public string GetNamespaceRoot(NetTypeDefinition type)
            {
                var ns = type.Namespace;
                if (!string.IsNullOrEmpty(ns))
                    return ns;
                return PossibleRoots[0];
            }
        }
    }
}
