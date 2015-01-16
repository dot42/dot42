using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to build resource items in a temporary resources folder.
    /// This task only creates MSBuild items, it does not generate / modify any files.
    /// </summary>
    public class AssignLibraryName : Task
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public AssignLibraryName()
        {
            Utils.InitializeLocations();
        }

        /// <summary>
        /// Items to assign a library name to
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
            AddLibraryName(Items, outputList);
            OutputItems = outputList.ToArray();
            return true;
        }

        /// <summary>
        /// Add all items to the command line
        /// </summary>
        private static void AddLibraryName(IEnumerable<ITaskItem> items, List<ITaskItem> outputItems)
        {
            if (items == null) return;
            foreach (var x in items)
            {
                var outputItem = new TaskItem(x);
                x.CopyMetadataTo(outputItem);
                if (string.IsNullOrEmpty(outputItem.GetMetadata("LibraryName")))
                {
                    outputItem.SetMetadata("LibraryName", Path.GetFileNameWithoutExtension(x.ItemSpec));                    
                }
                outputItems.Add(outputItem);
            }
        }
    }
}
