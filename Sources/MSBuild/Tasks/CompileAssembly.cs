using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Utility;
using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to compile and assembly into a dex file.
    /// </summary>
    public class CompileAssembly : Dot42CompilerTask 
    {
        /// <summary>
        /// Input assembly
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Input resource table
        /// </summary>
        [Required]
        public ITaskItem Resources { get; set; }

        /// <summary>
        /// Folders containing reference assemblies
        /// </summary>
        public ITaskItem[] ReferenceFolders { get; set; }

        /// <summary>
        /// Reference assemblies to use resource compilation
        /// </summary>
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Destination folder
        /// </summary>
        [Required]
        public ITaskItem DstDir { get; set; }

        /// <summary>
        /// Package name to create
        /// </summary>
        [Required]
        public string PackageName { get; set; }

        /// <summary>
        /// Root namespace as specified in MSBuild
        /// </summary>
        [Required]
        public string RootNamespace { get; set; }

        /// <summary>
        /// Add debug info
        /// </summary>
        public bool GenerateDebugInfo { get; set; }

        /// <summary>
        /// Path of Free Apps Key file
        /// </summary>
        public ITaskItem FreeAppsKeyPath { get; set; }

        /// <summary>
        /// The way the assembly is compiled.
        /// </summary>
        /// <value>"app", "library", "all"</value>
        public string CompilationMode { get; set; }

        /// <summary>
        /// Path of resource type usage info file.
        /// </summary>
        [Required]
        public ITaskItem ResourceTypeUsageInformation { get; set; }

        /// <summary>
        /// Generate the 'command line' arguments.
        /// </summary>
        protected override string[] GenerateArguments()
        {
            var builder = new List<string>();

            if (!string.IsNullOrEmpty(CompilationMode))
            {
                switch (CompilationMode.ToLower())
                {
                    case "app":
                        builder.Add(ToolOptions.CompilationModeApplication.AsArg());
                        break;
                    case "library":
                        builder.Add(ToolOptions.CompilationModeClassLibrary.AsArg());
                        break;
                    case "all":
                        builder.Add(ToolOptions.CompilationModeAll.AsArg());
                        break;
                    default:
                        throw new ArgumentException(string.Format("Unknown CompilationMode '{0}'", CompilationMode));
                }
            }

            if (!string.IsNullOrEmpty(PackageName))
            {
                builder.Add(ToolOptions.PackageName.AsArg());
                builder.Add(PackageName);
            }

            if (!string.IsNullOrEmpty(RootNamespace))
            {
                builder.Add(ToolOptions.RootNamespace.AsArg());
                builder.Add(RootNamespace);
            }

            if (DstDir != null)
            {
                builder.Add(ToolOptions.OutputFolder.AsArg());
                builder.Add(Path.GetFullPath(DstDir.ItemSpec));
            }

            if (GenerateDebugInfo)
            {
                builder.Add(ToolOptions.DebugInfo.AsArg());
            }

            if (Assemblies != null)
            {
                foreach (var asm in Assemblies)
                {
                    builder.Add(ToolOptions.InputAssembly.AsArg());
                    builder.Add(Path.GetFullPath(asm.ItemSpec));
                }
            }

            if (Resources != null)
            {
                builder.Add(ToolOptions.InputResources.AsArg());
                builder.Add(Path.GetFullPath(Resources.ItemSpec));
            }

            if (ReferenceFolders != null)
            {
                foreach (var x in ReferenceFolders.GetReferenceFolders())
                {
                    builder.Add(ToolOptions.ReferenceFolder.AsArg());
                    builder.Add(x);
                }
            }

            if (References != null)
            {
                foreach (var x in References.Where(x => (x != null) && File.Exists(x.ItemSpec)))
                {
                    builder.Add(ToolOptions.Reference.AsArg());
                    builder.Add(Path.GetFileNameWithoutExtension(x.ItemSpec));
                }
            }

            if (ResourceTypeUsageInformation != null)
            {
                builder.Add(ToolOptions.ResourceTypeUsageInformationPath.AsArg());
                builder.Add(Path.GetFullPath(ResourceTypeUsageInformation.ItemSpec));
            }

            if (FreeAppsKeyPath != null)
            {
                builder.Add(ToolOptions.FreeAppsKeyPath.AsArg());
                builder.Add(Path.GetFullPath(FreeAppsKeyPath.ItemSpec));
            }

            builder.AddTarget();
            return builder.ToArray();
        }
    }
}
