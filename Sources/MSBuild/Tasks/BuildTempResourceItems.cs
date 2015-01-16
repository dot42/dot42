using System.Collections.Generic;
using System.IO;
using Dot42.ResourcesLib;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to build resource items in a temporary resources folder.
    /// This task only creates MSBuild items, it does not generate / modify any files.
    /// </summary>
    public class BuildTempResourceItems : Task, IResourcesTask
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
        /// Temporary folder for resource files
        /// </summary>
        [Required]
        public ITaskItem TempFolder { get; set; }

        /// <summary>
        /// The generated items
        /// </summary>
        [Output]
        public ITaskItem[] OutputItems { get; set; }

        /// <summary>
        /// The generated manifest items
        /// </summary>
        [Output]
        public ITaskItem OutputManifestItem { get; set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        public BuildTempResourceItems()
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
            var outputList = new List<ITaskItem>();
            var tempFolder = Path.GetFullPath(TempFolder.ItemSpec);

            var manifestItem = new TaskItem(Path.Combine(tempFolder, "AndroidManifest.xml"));
            OutputManifestItem = manifestItem;

            AddResources(AnimationResources, ResourceType.Animation, outputList, tempFolder);
            AddResources(DrawableResources, ResourceType.Drawable, outputList, tempFolder);
            AddResources(LayoutResources, ResourceType.Layout, outputList, tempFolder);
            AddResources(MenuResources, ResourceType.Menu, outputList, tempFolder);
            AddResources(ValuesResources, ResourceType.Values, outputList, tempFolder);
            AddResources(XmlResources, ResourceType.Xml, outputList, tempFolder);
            AddResources(RawResources, ResourceType.Raw, outputList, tempFolder);

            OutputItems = outputList.ToArray();
            return true;
        }

        /// <summary>
        /// Add all items to the command line
        /// </summary>
        private static void AddResources(IEnumerable<ITaskItem> items, ResourceType type, List<ITaskItem> outputItems, string tempFolder)
        {
            if (items == null) return;
            foreach (var x in items)
            {
                var resourceFile = x.ItemSpec;
                var outputPath = ResourceExtensions.GetNormalizedResourcePath(tempFolder, resourceFile, type);
                var outputItem = new TaskItem(outputPath);
                x.CopyMetadataTo(outputItem);
                outputItem.SetMetadata("TargetPath", outputPath);
                outputItems.Add(outputItem);
            }
        }
    }
}
