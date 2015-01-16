using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Dot42.Utility;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to import JAR file(s) to source.
    /// </summary>
    public class ImportJars : Dot42CompilerTask
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ImportJars()
        {
            ImportAsStubs = true;
        }

        /// <summary>
        /// The jar files to import
        /// </summary>
        public ITaskItem[] JarFiles { get; set; }

        /// <summary>
        /// The generated source files
        /// </summary>
        [Output]
        public ITaskItem[] GeneratedSourceFiles { get; set; }

        /// <summary>
        /// Reference assemblies to use in import
        /// </summary>
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Folders containing reference assemblies
        /// </summary>
        public ITaskItem[] ReferenceFolders { get; set; }

        /// <summary>
        /// Generated code folder
        /// </summary>
        public ITaskItem OutputFolder { get; set; }

        /// <summary>
        /// Name of the library to import.
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// Import java classas as stub only.
        /// </summary>
        [DefaultValue(true)]
        public bool ImportAsStubs { get; set; }

        private ITaskItem currentJarFile;

        protected override string[] GenerateArguments()
        {
            var builder = new List<string>();

            builder.Add(ToolOptions.InputJar.AsArg());
            builder.Add(Path.GetFullPath(currentJarFile.ItemSpec));

            if (References != null)
            {
                foreach (var x in References.Where(x => (x != null) && File.Exists(x.ItemSpec)))
                {
                    builder.Add(ToolOptions.Reference.AsArg());
                    builder.Add(Path.GetFileNameWithoutExtension(x.ItemSpec));
                }
            }

            if (ReferenceFolders != null)
            {
                foreach (var x in ReferenceFolders.GetReferenceFolders())
                {
                    builder.Add(ToolOptions.ReferenceFolder.AsArg());
                    builder.Add(x);
                }
            }

            var libName = currentJarFile.GetMetadata("LibraryName") ?? LibraryName;
            if (libName != null)
            {
                builder.Add(ToolOptions.LibName.AsArg());
                builder.Add(libName);
            }

            var excludedPackages = currentJarFile.GetMetadata("ExcludedPackages");
            if (!string.IsNullOrEmpty(excludedPackages))
            {
                foreach (var pkg in excludedPackages.Split(':'))
                {
                    builder.Add(ToolOptions.ExcludePackage.AsArg());
                    builder.Add(pkg);
                }
            }

            if (ImportAsStubs)
            {
                builder.Add(ToolOptions.ImportStubs.AsArg());
            }

            if (OutputFolder != null)
            {
                builder.Add(ToolOptions.GeneratedCodeFolder.AsArg());
                builder.Add(Path.GetFullPath(OutputFolder.ItemSpec));
            }

            builder.AddTarget();
            return builder.ToArray();
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            var generatedSources = new List<ITaskItem>();
            if (JarFiles != null)
            {
                foreach (var item in JarFiles.Where(x => x != null))
                {
                    currentJarFile = item;
                    var rc = base.Execute();
                    if (!rc) return false;

                    var outputFolder = (OutputFolder != null) ? OutputFolder.ItemSpec : ".";
                    var sourcePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(item.ItemSpec));
                    var sourceItem = new TaskItem(sourcePath);
                    generatedSources.Add(sourceItem);

                }
            }
            GeneratedSourceFiles = generatedSources.ToArray();
            return true;
        }
    }
}
