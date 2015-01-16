using Dot42.Ide.Serialization.Nodes.Menu;

namespace Dot42.Ide.Editors.Menu
{
    public interface IMenuViewModel : IViewModel
    {
        /// <summary>
        /// Gets the menu root.
        /// </summary>
        MenuNode Menu { get; }
    }
}
