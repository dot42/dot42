using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout
{
    public interface IViewNodeControl
    {
        /// <summary>
        /// Gets access to the XML node
        /// </summary>
        ViewNode Node { get; }
    }
}
