using Dot42.Ide.Editors.Layout.Controls;
using Dot42.Ide.Serialization;
using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout
{
    /// <summary>
    /// Build controls for view nodes.
    /// </summary>
    internal class ControlBuilder : DefaultSerializationNodeVisitor<IViewNodeControl, IXmlLayoutDesigner>
    {
        public static readonly ControlBuilder Instance = new ControlBuilder();

        public override IViewNodeControl Visit(TextViewNode node, IXmlLayoutDesigner data)
        {
            return new TextViewNodeControl(node, data);
        }
        public override IViewNodeControl Visit(LinearLayoutNode node, IXmlLayoutDesigner data)
        {
            return new LinearLayoutControl(node, data);
        }
    }
}
