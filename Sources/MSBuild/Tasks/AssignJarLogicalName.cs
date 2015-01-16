using System.Collections.Generic;
using Dot42.Utility;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to build embedded resource items for jar references with imported code.
    /// This task only creates MSBuild items, it does not generate / modify any files.
    /// </summary>
    public class AssignJarLogicalName : Task
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public AssignJarLogicalName()
        {
            Utils.InitializeLocations();
        }

        /// <summary>
        /// Items to assign a manifest resource name to
        /// </summary>
        public ITaskItem[] Items { get; set; }

        /// <summary>
        /// The generated items
        /// </summary>
        [Output]
        public ITaskItem[] OutputItems { get; set; }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            var outputList = new List<ITaskItem>();
            AddManifestResourceName(Items, outputList);
            OutputItems = outputList.ToArray();
            return true;
        }

        /// <summary>
        /// Add all items to the command line
        /// </summary>
        private static void AddManifestResourceName(IEnumerable<ITaskItem> items, List<ITaskItem> outputItems)
        {
            if (items == null) return;
            foreach (var x in items)
            {
                var outputItem = new TaskItem(x);
                x.CopyMetadataTo(outputItem);
                var hash = JarReferenceHash.ComputeJarReferenceHash(x.ItemSpec);
                //outputItem.SetMetadata("ManifestResourceName", hash);
                outputItem.SetMetadata("LogicalName", hash);
                outputItems.Add(outputItem);
            }
        }
    }
}
