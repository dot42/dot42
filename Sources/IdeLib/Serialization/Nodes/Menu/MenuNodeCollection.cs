namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// Collection of <see cref="MenuNode"/>
    /// </summary>
    public class MenuNodeCollection : NodeCollection<MenuNode>
    {
        public MenuNodeCollection()
            : base("Sub menu's")
        {
        }
    }
}
