namespace Dot42.DebuggerLib.Events.Dalvik
{
    /// <summary>
    /// Application name changed
    /// </summary>
    public class AppNameChanged : DalvikEvent
    {
        private readonly string name;

        public AppNameChanged(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the new name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TResult Accept<TResult, TData>(EventVisitor<TResult, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}
