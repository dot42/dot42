using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout
{
    public interface ILayoutViewModel : IViewModel
    {
        /// <summary>
        /// Gets the layout root.
        /// </summary>
        ViewGroupNode Root { get; }
    }
}
