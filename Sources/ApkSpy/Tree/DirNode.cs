namespace Dot42.ApkSpy.Tree
{
    internal class DirNode : Node
    {
        private readonly string dir;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DirNode(string dir)
        {
            this.dir = dir;
            Text = dir;
        }

        /// <summary>
        /// Gets the directory
        /// </summary>
        public string Directory { get { return dir; } }
    }
}
