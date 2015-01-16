using Dot42.Ide.Serialization.Nodes.Layout;
using Dot42.Ide.Serialization.Nodes.Menu;
using Dot42.Ide.Serialization.Nodes.XmlResource;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Visitor pattern
    /// </summary>
    public abstract class SerializationNodeVisitor<TReturn, TData>
    {
        // Layout
        public abstract TReturn Visit(ViewNode node, TData data);
        public virtual TReturn Visit(TextViewNode node, TData data) { return Visit((ViewNode)node, data); }

        public virtual TReturn Visit(ViewGroupNode node, TData data) { return Visit((ViewNode)node, data); }
        public virtual TReturn Visit(LinearLayoutNode node, TData data) { return Visit((ViewGroupNode)node, data); }

        // menu
        public abstract TReturn Visit(MenuNode node, TData data);
        public abstract TReturn Visit(MenuGroupNode node, TData data);
        public abstract TReturn Visit(MenuItemNode node, TData data);

        // values
        public abstract TReturn Visit(BoolNode node, TData data);
        public abstract TReturn Visit(ColorNode node, TData data);
        public abstract TReturn Visit(DimensionNode node, TData data);
        public abstract TReturn Visit(IdNode node, TData data);
        public abstract TReturn Visit(IntegerNode node, TData data);
        public abstract TReturn Visit(IntegerArrayNode node, TData data);
        public abstract TReturn Visit(IntegerArrayItemNode node, TData data);
        public abstract TReturn Visit(PluralsNode node, TData data);
        public abstract TReturn Visit(PluralsItemNode node, TData data);
        public abstract TReturn Visit(ResourcesNode node, TData data);
        public abstract TReturn Visit(StringNode node, TData data);
        public abstract TReturn Visit(StringArrayNode node, TData data);
        public abstract TReturn Visit(StringArrayItemNode node, TData data);
        public abstract TReturn Visit(StyleNode node, TData data);
        public abstract TReturn Visit(StyleItemNode node, TData data);
        public abstract TReturn Visit(TypedArrayNode node, TData data);
        public abstract TReturn Visit(TypedArrayItemNode node, TData data);
    }
}
