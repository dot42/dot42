using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dot42.ApkLib;
using Dot42.CompilerLib.XModel;
using Dot42.LoaderLib.DotNet;
using Dot42.ResourcesLib;
using Dot42.Utility;
using Mono.Cecil;
using NameConverter = Dot42.CompilerLib.NameConverter;
using ResourceType = Dot42.ResourcesLib.ResourceType;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    internal sealed partial class ManifestBuilder
    {
        private const string Namespace = AndroidConstants.AndroidNamespace;
        private const string PackageAttribute = "PackageAttribute";
        private static readonly EnumFormatter installLocationsOptions = new EnumFormatter(
            Tuple.Create(0, "internalOnly"),
            Tuple.Create(1, "auto"),
            Tuple.Create(2, "preferExternal")
            );

        private readonly AssemblyDefinition assembly;
        private readonly string packageName;
        private readonly NameConverter nsConverter;
        private readonly bool debuggable;
        private readonly List<string> appWidgetProviderCodeFiles;
        private readonly string targetSdkVersion;
        private readonly XModule module;

        /// <summary>
        /// Default ctor
        /// </summary>
        private ManifestBuilder(string assemblyFile, IEnumerable<string> referenceFolders, string packageName, NameConverter nsConverter, bool debuggable, List<string> appWidgetProviderCodeFiles, string targetSdkVersion)
        {
#if DEBUG
            //Debugger.Launch();
#endif

            module = new XModule();
            this.packageName = packageName;
            this.nsConverter = nsConverter;
            this.debuggable = debuggable;
            this.appWidgetProviderCodeFiles = appWidgetProviderCodeFiles;
            this.targetSdkVersion = targetSdkVersion;

            // Load assembly file
            if (!File.Exists(assemblyFile))
                throw new ArgumentException(string.Format("Assembly {0} not found", assemblyFile));

            var assemblyResolver = new AssemblyResolver(referenceFolders, null, module.OnAssemblyLoaded); 
            var parameters = new ReaderParameters { AssemblyResolver = assemblyResolver};
            assembly = assemblyResolver.Load(assemblyFile, parameters);
        }

        /// <summary>
        /// Compile the given XML file to a binary XML file in the given output folder.
        /// </summary>
        public static void CreateManifest(Targets target, string assemblyFile, IEnumerable<string> referenceFolders, string packageName, NameConverter nsConverter, bool debuggable, List<string> appWidgetProviderCodeFiles, string targetSdkVersion, string outputFolder)
        {
            var builder = new ManifestBuilder(assemblyFile, referenceFolders, packageName, nsConverter, debuggable, appWidgetProviderCodeFiles, targetSdkVersion);
            builder.CreateManifest(target, outputFolder);
        }

        /// <summary>
        /// Compile the given XML file to a binary XML file in the given output folder.
        /// </summary>
        private void CreateManifest(Targets target, string outputFolder)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            var pkgAttributes = assembly.GetAttributes(PackageAttribute).ToList();
            if (pkgAttributes.Count > 1)
                throw new ArgumentException("Multiple Package attributes found");
            var pkgAttr = pkgAttributes.FirstOrDefault();
            
            // Create xml root
            var doc = new XDocument();
            var manifest = new XElement("manifest");
            manifest.Add(new XAttribute(XNamespace.Xmlns + "android", XNamespace.Get(Namespace)));
            doc.Add(manifest);

            // Create uses-sdk
            CreateUsesSdk(manifest, pkgAttr);

            // Set attributes
            manifest.Add(new XAttribute(XName.Get("package"), packageName));
            var version = assembly.Name.Version;
            var versionCode = (pkgAttr != null) ? pkgAttr.GetValue("VersionCode", version.Major) : version.Major;
            manifest.Add(new XAttribute(XName.Get("versionCode", Namespace), versionCode));
            var versionName = (pkgAttr != null) ? pkgAttr.GetValue("VersionName", version.ToString()) : version.ToString();
            manifest.Add(new XAttribute(XName.Get("versionName", Namespace), versionName));
            // Set additional attributes
            if (pkgAttr != null)
            {
                manifest.AddAttrIfNotEmpty("sharedUserId", Namespace, pkgAttr.GetValue<string>("SharedUserId"));
                manifest.AddAttrIfNotEmpty("sharedUserLabel", Namespace, pkgAttr.GetValue<string>("SharedUserLabel"), FormatString);
                manifest.AddAttrIfNotDefault("installLocation", Namespace, pkgAttr.GetValue<int>("InstallLocation"), 0, installLocationsOptions.Format);
            }

            // Create child elements
            CreateApplication(target, manifest, outputFolder);
            CreateInstrumentation(manifest);
            CreatePermission(manifest);
            CreatePermissionGroup(manifest);
            CreateSupportsScreens(manifest);
            CreateUsesFeature(manifest);
            CreateUsesPermission(manifest);

            // Save
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            doc.Save(Path.Combine(outputFolder, "AndroidManifest.xml"));
        }

        /// <summary>
        /// Format a style resource id.
        /// </summary>
        private static string FormatStyle(string id)
        {
            return FormatResourceId(id, ResourceType.Style);
        }

        /// <summary>
        /// Format a string resource id.
        /// </summary>
        private static string FormatString(string id)
        {
            return FormatResourceId(id, ResourceType.String);
        }

        /// <summary>
        /// Format a drawable resource id.
        /// </summary>
        private static string FormatStringOrLiteral(string id)
        {
            if (id.Trim().StartsWith("@"))
            {
                ResourceId resId;
                if (ResourceId.TryParse(id, ResourceType.String, out resId))
                    return resId.ToString(ResourceIdFormat.AndroidXml);
            }
            return id;
        }

        /// <summary>
        /// Format a drawable resource id.
        /// </summary>
        private static string FormatDrawable(string id)
        {
            return FormatResourceId(id, ResourceType.Drawable);
        }

        /// <summary>
        /// Format a layout resource id.
        /// </summary>
        private static string FormatLayout(string id)
        {
            return FormatResourceId(id, ResourceType.Layout);
        }

        /// <summary>
        /// Format a drawable resource id.
        /// </summary>
        internal static string FormatResourceId(string id, ResourceType defaultType)
        {
            ResourceId resId;
            if (ResourceId.TryParse(id, defaultType, out resId))
                return resId.ToString(ResourceIdFormat.AndroidXml);
            throw new ArgumentException(string.Format("Unsupported resource id '{0}'", id));
        }

        /// <summary>
        /// Gets a formatted full classname for the given type.
        /// </summary>
        private string FormatClassName(XTypeDefinition typeDef)
        {
            string className;
            if (typeDef.TryGetDexImportNames(out className))
                return FormatImportedClassName(className);
            if (typeDef.TryGetJavaImportNames(out className))
                return FormatImportedClassName(className);
            return nsConverter.GetConvertedFullName(typeDef);
        }

        /// <summary>
        /// Gets a formatted full classname for the given imported class name.
        /// </summary>
        private static string FormatImportedClassName(string className)
        {
            return className.Replace('/', '.');
        }
    }
}
