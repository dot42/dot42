using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Utility;
using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to compile resources to binary resources.
    /// </summary>
    public class CompileResources : Dot42CompilerTask, IResourcesTask
    {
        /// <summary>
        /// Manifest
        /// </summary>
        public ITaskItem AndroidManifest { get; set; }

        /// <summary>
        /// Animation resources
        /// </summary>
        public ITaskItem[] AnimationResources { get; set; }

        /// <summary>
        /// Drawable resources
        /// </summary>
        public ITaskItem[] DrawableResources { get; set; }

        /// <summary>
        /// Menu resources
        /// </summary>
        public ITaskItem[] MenuResources { get; set; }

        /// <summary>
        /// Layout resources
        /// </summary>
        public ITaskItem[] LayoutResources { get; set; }

        /// <summary>
        /// Values resources
        /// </summary>
        public ITaskItem[] ValuesResources { get; set; }

        /// <summary>
        /// Xml resources
        /// </summary>
        public ITaskItem[] XmlResources { get; set; }

        /// <summary>
        /// Raw resources
        /// </summary>
        public ITaskItem[] RawResources { get; set; }

        /// <summary>
        /// AppWidgetProvider code files
        /// </summary>
        public ITaskItem[] AppWidgetProviders { get; set; }

        /// <summary>
        /// Reference folders 
        /// </summary>
        public ITaskItem[] ReferenceFolders { get; set; }

        /// <summary>
        /// Reference assemblies to use resource compilation
        /// </summary>
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Destination folder
        /// </summary>
        public ITaskItem DstDir { get; set; }

        /// <summary>
        /// Destination folder for generated code
        /// </summary>
        public ITaskItem GeneratedCodeFolder { get; set; }

        /// <summary>
        /// Namespace for generated code
        /// </summary>
        public string GeneratedCodeNamespace { get; set; }

        /// <summary>
        /// Language for generated code
        /// </summary>
        public string GeneratedCodeLanguage { get; set; }

        /// <summary>
        /// Package name to create
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Root namespace as specified in MSBuild
        /// </summary>
        public string RootNamespace { get; set; }

        /// <summary>
        /// Temporary folder for resource files
        /// </summary>
        public ITaskItem TempFolder { get; set; }

        /// <summary>
        /// Path of resource type usage info file.
        /// </summary>
        [Required]
        public ITaskItem ResourceTypeUsageInformation { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string[] GenerateArguments()
        {
            var builder = new List<string>();

            builder.Add(ToolOptions.CompileResources.AsArg());

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

            if (GeneratedCodeFolder != null)
            {
                builder.Add(ToolOptions.GeneratedCodeFolder.AsArg());
                builder.Add(Path.GetFullPath(GeneratedCodeFolder.ItemSpec));
            }

            if (TempFolder != null)
            {
                builder.Add(ToolOptions.TempFolder.AsArg());
                builder.Add(Path.GetFullPath(TempFolder.ItemSpec));
            }

            if (ResourceTypeUsageInformation != null)
            {
                builder.Add(ToolOptions.ResourceTypeUsageInformationPath.AsArg());
                builder.Add(Path.GetFullPath(ResourceTypeUsageInformation.ItemSpec));                
            }

            if (!string.IsNullOrEmpty(GeneratedCodeNamespace))
            {
                builder.Add(ToolOptions.GeneratedCodeNamespace.AsArg());
                builder.Add(GeneratedCodeNamespace);
            }

            if (!string.IsNullOrEmpty(GeneratedCodeLanguage))
            {
                builder.Add(ToolOptions.GeneratedCodeLanguage.AsArg());
                builder.Add(GeneratedCodeLanguage);
            }

            if (AndroidManifest != null)
            {
                builder.Add(ToolOptions.InputManifest.AsArg());
                builder.Add(Path.GetFullPath(AndroidManifest.ItemSpec));                
            }

            AddResources(AnimationResources, ToolOptions.AnimationResource, builder);
            AddResources(DrawableResources, ToolOptions.DrawableResource, builder);
            AddResources(LayoutResources, ToolOptions.LayoutResource, builder);
            AddResources(MenuResources, ToolOptions.MenuResource, builder);
            AddResources(ValuesResources, ToolOptions.ValuesResource, builder);
            AddResources(XmlResources, ToolOptions.XmlResource, builder);
            AddResources(RawResources, ToolOptions.RawResource, builder);

            if (AppWidgetProviders != null)
            {
                foreach (var x in AppWidgetProviders)
                {
                    builder.Add(ToolOptions.AppWidgetProvider.AsArg());
                    builder.Add(x.ItemSpec);
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

            if (ReferenceFolders != null)
            {
                foreach (var x in ReferenceFolders.GetReferenceFolders())
                {
                    builder.Add(ToolOptions.ReferenceFolder.AsArg());
                    builder.Add(x);
                }
            }

            builder.AddTarget();
            return builder.ToArray();
        }
    }
}
