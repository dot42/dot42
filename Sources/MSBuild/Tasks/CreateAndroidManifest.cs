using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dot42.Utility;

using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to create an AndroidManifest.xml file from attributes in the given assembly.
    /// </summary>
    public class CreateAndroidManifest : Dot42CompilerTask
    {
        /// <summary>
        /// Input assembly
        /// </summary>
        public ITaskItem Assembly { get; set; }

        /// <summary>
        /// Manifest resource. There should only be one (or none, in which case we create one automatically).
        /// </summary>
        public ITaskItem[] ManifestResources { get; set; }

        /// <summary>
        /// AppWidgetProvider code files
        /// </summary>
        public ITaskItem[] AppWidgetProviders { get; set; }

        /// <summary>
        /// Folders containing reference assemblies
        /// </summary>
        public ITaskItem[] ReferenceFolders { get; set; }

        /// <summary>
        /// Target SDK version.
        /// Can be formatted as API level or a v1.2.3
        /// </summary>
        public string TargetSdkVersion { get; set; }

        /// <summary>
        /// Output folder
        /// </summary>
        public ITaskItem DstDir { get; set; }

        /// <summary>
        /// Package name to create
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Root namespace as specified in MSBuild
        /// </summary>
        public string RootNamespace { get; set; }

        /// <summary>
        /// Set debuggable
        /// </summary>
        public bool Debuggable { get; set; }

        protected override string[] GenerateArguments()
        {
            if (null != ManifestResources)
            {
                // Check if there is more than 1 manifest file.
                if (ManifestResources.Length > 1)
                {
                    // Add a Build Error for each file, so the user can easily find each one.
                    foreach (ITaskItem item in ManifestResources)
                    {
                        Log.LogError("Error", "D42-0005", string.Empty, item.ItemSpec, 1, 1, 1, 1, "The project should only contain 1 manifest file.");
                    }

                    // Tell the base class that executing this task has failed.
                    return null;
                }

                // There is only 1 manifest. Copy it to the output folder because we don't need to create one.
                if (1 == ManifestResources.Length)
                {
                    File.Copy(ManifestResources[0].ItemSpec, Path.Combine(DstDir.ItemSpec, "AndroidManifest.xml"), true);

                    // Tell the base class that executing this task has finished successful.
                    return new string[0];
                }
            }

            var builder = new List<string>();

            builder.Add(ToolOptions.CreateManifest.AsArg());

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

            if (Debuggable)
            {
                builder.Add(ToolOptions.DebugInfo.AsArg());
            }

            if (DstDir != null)
            {
                builder.Add(ToolOptions.OutputFolder.AsArg());
                builder.Add(DstDir.ItemSpec);
            }

            if (Assembly != null)
            {
                builder.Add(ToolOptions.InputAssembly.AsArg());
                builder.Add(Assembly.ItemSpec);
            }

            if (AppWidgetProviders != null)
            {
                foreach (var x in AppWidgetProviders)
                {
                    builder.Add(ToolOptions.AppWidgetProvider.AsArg());
                    builder.Add(x.ItemSpec);
                }
            }

            if (ReferenceFolders != null)
            {
                foreach (var x in ReferenceFolders.Where(x => x != null))
                {
                    builder.Add(ToolOptions.ReferenceFolder.AsArg());
                    builder.Add(x.ItemSpec);
                }
            }

            if (!string.IsNullOrEmpty(TargetSdkVersion))
            {
                builder.Add(ToolOptions.TargetSdkVersion.AsArg());
                builder.Add(TargetSdkVersion);
            }

            builder.AddTarget();
            return builder.ToArray();
        }
    }
}
