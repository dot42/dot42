using Dot42.Ide.Serialization.Nodes.Layout;
using Dot42.Ide.Serialization.Nodes.Menu;
using Dot42.Ide.Serialization.Nodes.XmlResource;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Visitor pattern
    /// </summary>
    public class DefaultSerializationNodeVisitor<TReturn, TData> : SerializationNodeVisitor<TReturn, TData>
    {
        // Layout
        public override TReturn Visit(ViewNode node, TData data) { return default(TReturn); }

        // menu
        public override TReturn Visit(MenuNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(MenuGroupNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(MenuItemNode node, TData data) { return default(TReturn); }

        // values
        public override TReturn Visit(BoolNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(ColorNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(DimensionNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(IdNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(IntegerNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(IntegerArrayNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(IntegerArrayItemNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(PluralsNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(PluralsItemNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(ResourcesNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(StringNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(StringArrayNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(StringArrayItemNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(StyleNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(StyleItemNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(TypedArrayNode node, TData data) { return default(TReturn); }
        public override TReturn Visit(TypedArrayItemNode node, TData data) { return default(TReturn); }
    }
}
