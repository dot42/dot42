using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to import library projects into a project.
    /// </summary>
    public class ImportLibraryProjects : Task
    {
        /// <summary>
        /// Get the items to import
        /// </summary>
        [Required]
        public ITaskItem[] LibraryProjects { get; set; }

        /// <summary>
        /// Temporary folder
        /// </summary>
        [Required]
        public ITaskItem TempFolder { get; set; }

        /// <summary>
        /// Embedded resource items
        /// </summary>
        [Output]
        public ITaskItem[] OutputResources { get; set; }

        /// <summary>
        /// Jar reference items
        /// </summary>
        [Output]
        public ITaskItem[] JarReferences { get; set; }

        /// <summary>
        /// Compile items
        /// </summary>
        [Output]
        public ITaskItem[] CompileItems { get; set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ImportLibraryProjects()
        {
            Utils.InitializeLocations();
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
#if DEBUG
            //Debugger.Launch();
#endif
            var embeddedResources = new List<ITaskItem>();
            var jarReferences = new List<ITaskItem>();
            var compileItems = new List<ITaskItem>();
            foreach (var item in LibraryProjects)
            {
                ProcessLibraryProject(item.ItemSpec, embeddedResources, jarReferences, compileItems, TempFolder.ItemSpec);
            }

            OutputResources = embeddedResources.ToArray();
            JarReferences = jarReferences.ToArray();
            CompileItems = compileItems.ToArray();

            return true;
        }

        /// <summary>
        /// Process the library project in the given folder.
        /// </summary>
        private static void ProcessLibraryProject(string folder, List<ITaskItem> embeddedResources, List<ITaskItem> jarReferences, List<ITaskItem> compileItems, string tempFolder)
        {
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string packageName;
            ReadAndroidManifest(folder, out packageName);

            // Read res folder
            var rootFolder = Path.GetFullPath(Path.Combine(folder, "res"));
            if (Directory.Exists(rootFolder))
            {
                var resFiles = Directory.EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories);
                foreach (var iterator in resFiles)
                {
                    var path = Path.GetFullPath(iterator);
                    var item = new TaskItem(path);
                    var relPath = MakeRelPath(path, rootFolder);
                    item.SetMetadata("LogicalName", packageName + ".res." + relPath.Replace(Path.DirectorySeparatorChar, '.'));
                    embeddedResources.Add(item);
                }
            }

            // Read bin/classes folder
            rootFolder = Path.GetFullPath(Path.Combine(folder, @"_bin\classes"));
            if (!Directory.Exists(rootFolder))
                rootFolder = Path.GetFullPath(Path.Combine(folder, @"bin\classes"));
            if (Directory.Exists(rootFolder))
            {
                var jarPath = Path.Combine(tempFolder, packageName + ".Classes.jar");
                var classFiles = Directory.EnumerateFiles(rootFolder, "*.class", SearchOption.AllDirectories).ToList();
                var maxModTime = GetMaxWriteTime(classFiles);

                if (!File.Exists(jarPath) || (File.GetLastWriteTime(jarPath) < maxModTime))
                {
                    using (var zipStream = new ZipOutputStream(File.Create(jarPath)))
                    {
                        var buffer = new byte[4096];
                        foreach (var iterator in classFiles)
                        {
                            var path = Path.GetFullPath(iterator);
                            var relPath = MakeRelPath(path, rootFolder);
                            var entry = new ZipEntry(relPath.Replace('\\', '/'));
                            zipStream.PutNextEntry(entry);

                            using (var fs = File.OpenRead(path))
                            {
                                StreamUtils.Copy(fs, zipStream, buffer);
                            }
                        }
                    }
                    File.SetLastWriteTime(jarPath, maxModTime);
                }

                var item = new TaskItem(jarPath);
                item.SetMetadata("ImportCode", "yes");
                jarReferences.Add(item);
            }

            // Create PackageName.cs
            {
                var sourcePath = Path.Combine(tempFolder, packageName + ".cs");
                File.WriteAllText(sourcePath, string.Format("[assembly: Dot42.LibraryProjectReference(\"{0}\")]", packageName));
                var item = new TaskItem(sourcePath);
                compileItems.Add(item);
            }
        }

        /// <summary>
        /// Read the manifest file and retrieve the package name.
        /// </summary>
        private static void ReadAndroidManifest(string folder, out string packageName)
        {
            var path = Path.Combine(folder, "AndroidManifest.xml");
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("Cannot find AndroidManifest.xml file in {0}", folder));

            var doc = XDocument.Load(path);
            var packageAttr = doc.Root.Attribute("package");
            if (packageAttr == null)
                throw new ArgumentException(string.Format("Cannot find package attribute in AndroidManifest.xml in {0}", folder));
            packageName = packageAttr.Value;
        }

        /// <summary>
        /// Create a path relative to the given root folder.
        /// </summary>
        private static string MakeRelPath(string path, string rootFolder)
        {
            path = Path.GetFullPath(path);
            rootFolder = Path.GetFullPath(rootFolder);

            if (!path.StartsWith(rootFolder))
                return path;

            var relPath = path.Substring(rootFolder.Length);
            if ((relPath.Length > 0) && (relPath[0] == Path.DirectorySeparatorChar))
                relPath = relPath.Substring(1);
            return relPath;
        }

        /// <summary>
        /// Gets the latest modification time of the given files.
        /// </summary>
        private static DateTime GetMaxWriteTime(IEnumerable<string> files)
        {
            var result = DateTime.MinValue;
            foreach (var path in files)
            {
                var modTime = File.GetLastWriteTime(path);
                if (modTime > result)
                    result = modTime;
            }
            return result;
        }
    }
}
