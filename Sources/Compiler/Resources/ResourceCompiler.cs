using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dot42.ApkLib;
using Dot42.ApkLib.Resources;
using Dot42.Compiler.Shared;
using Dot42.FrameworkDefinitions;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Java;
using Dot42.ResourcesLib;
using Dot42.Utility;
using Mono.Cecil;
using FileAttributes = System.IO.FileAttributes;
using NameConverter = Dot42.CompilerLib.NameConverter;
using ResourceType = Dot42.ResourcesLib.ResourceType;

namespace Dot42.Compiler.Resources
{
    /// <summary>
    /// Compile resources to binary variants.
    /// </summary>
    internal class ResourceCompiler
    {
        private readonly string baseFolder;
        private readonly string generatedCodeFolder;
        private readonly string tempFolder;
        private readonly string packageName;
        private readonly string generatedCodeNamespace;
        private readonly string generatedCodeLanguage;
        private readonly string resourceTypeUsageInformationPath;
        private DateTime lastModified = DateTime.MinValue;
        private readonly ValuesResourceProcessor valuesResourceProcessor = new ValuesResourceProcessor();
        private readonly LayoutResourceProcessor layoutProcessor;

        private const string SystemIdsFileName = "dot42_system_ids.xml";

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ResourceCompiler(string baseFolder, string generatedCodeFolder, string tempFolder, string packageName, string rootNamespace, string generatedCodeNamespace, string generatedCodeLanguage, string resourceTypeUsageInformationPath)
        {
            this.baseFolder = baseFolder;
            this.generatedCodeFolder = generatedCodeFolder;
            this.tempFolder = tempFolder;
            this.packageName = packageName;
            this.generatedCodeNamespace = generatedCodeNamespace;
            this.generatedCodeLanguage = generatedCodeLanguage;
            this.resourceTypeUsageInformationPath = resourceTypeUsageInformationPath;
            var nameConverter = new NameConverter(packageName, rootNamespace);
            layoutProcessor = new LayoutResourceProcessor(nameConverter);
        }

        /// <summary>
        /// Compile the given resources into a suitable output folder.
        /// </summary>
        internal bool Compile(List<Tuple<string, ResourceType>> resources, List<string> appWidgetProviderCodeFiles, List<string> references, List<string> referenceFolders, string baseApkPath)
        {
            // Remove temp folder
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            // Ensure temp\res folder exists
            var resFolder = Path.Combine(tempFolder, "res");
            Directory.CreateDirectory(resFolder);
            var tempApkPath = Path.Combine(tempFolder, "temp.apk");

            // Compile each resource
            foreach (var resource in resources.Where(x => x.Item2 != ResourceType.Manifest))
            {
                CopyResourceToFolder(tempFolder, resource.Item1, resource.Item2);
            }

            // Extract resources out of library project references
            var resolver = new AssemblyResolver(referenceFolders, null, null);
            foreach (var path in references)
            {
                ExtractLibraryProjectResources(resFolder, path, resolver);
            }

            // Select manifest path
            var manifestPath = resources.Where(x => x.Item2 == ResourceType.Manifest).Select(x => x.Item1).FirstOrDefault();

            // Ensure files exists for the appwidgetproviders
            CreateAppWidgetProviderFiles(tempFolder, manifestPath, appWidgetProviderCodeFiles);

            // Create system ID's resource
            CreateSystemIdsResource(tempFolder);

            // Run aapt
            var args = new[] {
                                 "p",
                                 "-f",
                                 "-S",
                                 resFolder,
                                 "-M",
                                 GetManifestPath(tempFolder, manifestPath, packageName),
                                 "-I",
                                 baseApkPath,
                                 "-F",
                                 tempApkPath
                             };
            var runner = new ProcessRunner(Locations.Aapt, args);
            var exitCode = runner.Run();
            if (exitCode != 0)
            {
                ProcessAaptErrors(runner.Output, tempFolder, resources);
                return false;
            }

            // Unpack compiled resources to base folder.
            Table resourceTable = null;
            using (var apk = new ApkFile(tempApkPath))
            {
                foreach (var name in apk.FileNames)
                {
                    // Extract
                    var data = apk.Load(name);

                    // Save
                    var path = Path.Combine(baseFolder, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, data);

                    // Is resource table? yes -> load it
                    if (name == "resources.arsc")
                    {
                        resourceTable = new Table(new MemoryStream(data));
                    }
                }
            }

            // Create R.cs
            if (!string.IsNullOrEmpty(generatedCodeFolder))
            {
                var codeBuilder = new CreateResourceIdsCode(resourceTable, resources, valuesResourceProcessor.StyleableDeclarations);
                codeBuilder.Generate(generatedCodeFolder, packageName, generatedCodeNamespace, generatedCodeLanguage, lastModified);
            }

            // Create resource type usage info file
            if (!string.IsNullOrEmpty(resourceTypeUsageInformationPath))
            {
                // Ensure folder exists
                var folder = Path.GetDirectoryName(resourceTypeUsageInformationPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Create file
                File.WriteAllLines(resourceTypeUsageInformationPath, layoutProcessor.CustomClassNames);                
            }

            return true;
        }

        /// <summary>
        /// Process the output of AAPT to display property errors in VisualStudio.
        /// </summary>
        private static void ProcessAaptErrors(string output, string tempFolder, List<Tuple<string, ResourceType>> resources)
        {
#if DEBUG
            //Debugger.Launch();
#endif

            const string errorPrefix = "Error: ";
            const string errorLevel = "error";

            var lines = output.Split(new[] { '\n', '\r' });
            var paths = resources.Select(x => Tuple.Create(x.Item1, ResourceExtensions.GetNormalizedResourcePath(tempFolder, x.Item1, x.Item2))).ToList();

            foreach (var line in lines)
            {
                var parts = SplitAaptErrorLine(line.Trim(), 4).ToArray();
                int lineNr;
                if ((parts.Length < 4) || !int.TryParse(parts[1], out lineNr))
                {
                    if (line.Length > 0)
                        Console.WriteLine(line);
                    continue;
                }

                // src:line:severity:remaining
                var msg = parts[3].Trim();
                if (msg.StartsWith(errorPrefix))
                    msg = msg.Substring(errorPrefix.Length);
                var url = parts[0];
                var level = parts[2].Trim();

                var pathEntry = paths.FirstOrDefault(x => x.Item2 == url);
                url = (pathEntry != null) ? pathEntry.Item1 : url;

                switch (level.ToLowerInvariant())
                {
                    case errorLevel:
                        DLog.Error(DContext.ResourceCompilerAaptError, url, 0, lineNr, msg);
                        break;
                    default:
                        DLog.Warning(DContext.ResourceCompilerAaptError, url, 0, lineNr, msg);
                        break;
                }

                //Console.WriteLine(line); // DEBUG PURPOSES
            }
        }

        private static IEnumerable<string> SplitAaptErrorLine(string line, int maxParts)
        {
            if (string.IsNullOrEmpty(line))
                yield break;
            var index = 0;
            maxParts--;
            while ((index < line.Length) && (maxParts > 0))
            {
                var split = false;
                if (line[index] == ':')
                {
                    // Potentionally split
                    split = true;
                    if (index + 1 < line.Length)
                    {
                        switch (line[index + 1])
                        {
                            case '\\':
                            case '/':
                                split = false;
                                break;
                        }
                    }
                }

                if (split)
                {
                    yield return line.Substring(0, index);
                    line = line.Substring(index + 1);
                    index = 0;
                    maxParts--;
                }
                else
                {
                    index++;
                }
            }
            yield return line;
        }

        /// <summary>
        /// If the given manifest path is not empty, return it.
        /// If not, create a temp manifest and return it's path.
        /// </summary>
        private string GetManifestPath(string tempFolder, string inputManifestPath, string packageName)
        {
            if (!string.IsNullOrEmpty(inputManifestPath))
                return inputManifestPath;

            var tmpManifest = string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"{0}\"/>", packageName);
            var path = Path.Combine(tempFolder, "AndroidManifest.xml");
            Directory.CreateDirectory(tempFolder);
            File.WriteAllText(path, tmpManifest);
            lastModified = DateTime.Now;
            return path;
        }

        /// <summary>
        /// If the given manifest path is not empty, return it.
        /// If not, create a temp manifest and return it's path.
        /// </summary>
        internal static void CreateSystemIdsResource(string tempFolder)
        {
            var path = Path.Combine(Path.Combine(tempFolder, @"res\values"), SystemIdsFileName);
            if (File.Exists(path))
                return;
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var root = new XElement("resources");
            var doc = new XDocument(root);

            foreach (var id in SystemIdConstants.Ids)
            {
                root.Add(new XElement("item",
                    new XAttribute("type", "id"),
                    new XAttribute("name", id)));
            }

            doc.Save(path);
        }

        /// <summary>
        /// Create temporary files for the given app widgets if they do not yet exist.
        /// </summary>
        internal static void CreateAppWidgetProviderFiles(string tempFolder, string inputManifestPath, List<string> appWidgetProviderCodeFiles)
        {
            var sourceFolder = (inputManifestPath != null) ? Path.GetDirectoryName(Path.GetFullPath(inputManifestPath)) : null;
            for (var index = 0; index < appWidgetProviderCodeFiles.Count; index++)
            {
                var resourceName = AppWidgetProviderResource.GetResourceName(index);
                var sourcePath = (sourceFolder != null) ? Path.Combine(sourceFolder, Path.Combine(@"res\xml", resourceName + ".xml")) : null;
                var path = Path.Combine(Path.Combine(tempFolder, @"res\xml"), resourceName + ".xml");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if ((sourcePath != null) && File.Exists(sourcePath))
                {
                    // Copy source to path
                    File.Copy(sourcePath, path, true);
                }
                else
                {
                    // Generate temp appwidgetprovider file.
                    const string tmpContent = "<appwidget-provider xmlns:android=\"http://schemas.android.com/apk/res/android\"></appwidget-provider>";
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, tmpContent);
                    }
                }
            }
        }

        /// <summary>
        /// Copy the given resource file into a given base folder.
        /// </summary>
        private void CopyResourceToFolder(string folder, string resourceFile, ResourceType resourceType)
        {
            var outputPath = ResourceExtensions.GetNormalizedResourcePath(folder, resourceFile, resourceType);
            var outputFolder = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Collect last modification date
            var modified = File.GetLastWriteTime(resourceFile);
            if (modified > lastModified) lastModified = modified;

            // Copy the resource file
            File.Copy(resourceFile, outputPath, true);  

            // Make sure file is not readonly.
            File.SetAttributes(outputPath, FileAttributes.Normal);

            // Post process
            ProcessResource(outputPath, resourceType);
        }

        /// <summary>
        /// Post process the given resource path.
        /// </summary>
        private void ProcessResource(string outputPath, ResourceType resourceType)
        {
            // Process contents if needed
            switch (resourceType)
            {
                case ResourceType.Layout:
                    layoutProcessor.Process(outputPath, true);
                    break;
                case ResourceType.Values:
                    valuesResourceProcessor.Process(outputPath, true);
                    break;
            }

            // Set the timestamp
            File.SetLastWriteTime(outputPath, DateTime.Now);            
        }

        /// <summary>
        /// Extract resources out of library project references
        /// </summary>
        private void ExtractLibraryProjectResources(string folder, string referencePath, AssemblyResolver resolver)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            var parameters = new ReaderParameters { AssemblyResolver = resolver };
            var assembly = resolver.Load(referencePath, parameters);

            // Go over all LibraryProjectReference attributes.
            foreach (var attr in assembly.CustomAttributes.Where(x => (x.AttributeType.Name == AttributeConstants.LibraryProjectReferenceAttributeName) && (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace)))
            {
                var libPackageName = (string)attr.ConstructorArguments[0].Value;
                var prefix = libPackageName + ".res.";

                foreach (var resource in assembly.MainModule.Resources.OfType<EmbeddedResource>().Where(x => x.Name.StartsWith(prefix)))
                {
                    var name = resource.Name.Substring(prefix.Length);
                    var parts = name.Split(new[] { '.' }, 2);
                    if (parts.Length == 2)
                    {
                        // Export resource to disk
                        var targetPath = Path.Combine(Path.Combine(folder, parts[0]), parts[1]);
                        var outputFolder = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(outputFolder))
                            Directory.CreateDirectory(outputFolder);
                        File.WriteAllBytes(targetPath, resource.GetResourceData());

                        // Post process
                        var xmlName = parts[0].Split(new[] { '-' }, 2)[0];
                        var resourceType = ResourceExtensions.GetResourceTypeFromXmlName(xmlName);
                        ProcessResource(targetPath, resourceType);
                    }
                }
            }
        }
    }
}
