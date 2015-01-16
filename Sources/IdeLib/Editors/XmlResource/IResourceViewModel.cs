using Dot42.Ide.Serialization.Nodes.XmlResource;

namespace Dot42.Ide.Editors.XmlResource
{
    public interface IResourceViewModel : IViewModel
    {
        /// <summary>
        /// Gets the resources root.
        /// </summary>
        ResourcesNode Resources { get; }
    }
}
