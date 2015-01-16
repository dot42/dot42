using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to compile resources to binary resources.
    /// </summary>
    internal interface IResourcesTask
    {
        /// <summary>
        /// Manifest
        /// </summary>
        ITaskItem AndroidManifest { get; set; }

        /// <summary>
        /// Animation resources
        /// </summary>
        ITaskItem[] AnimationResources { get; set; }

        /// <summary>
        /// Drawable resources
        /// </summary>
        ITaskItem[] DrawableResources { get; set; }

        /// <summary>
        /// Menu resources
        /// </summary>
        ITaskItem[] MenuResources { get; set; }

        /// <summary>
        /// Layout resources
        /// </summary>
        ITaskItem[] LayoutResources { get; set; }

        /// <summary>
        /// Values resources
        /// </summary>
        ITaskItem[] ValuesResources { get; set; }

        /// <summary>
        /// Xml resources
        /// </summary>
        ITaskItem[] XmlResources { get; set; }

        /// <summary>
        /// Raw resources
        /// </summary>
        ITaskItem[] RawResources { get; set; }
    }
}
