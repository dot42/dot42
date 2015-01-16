namespace Dot42.DebuggerLib.Events.Dalvik
{
    /// <summary>
    /// Thread creation notification (THCR)
    /// </summary>
    public class ThreadCreation : DalvikEvent
    {
        private readonly ThreadId id;
        private readonly string name;

        public ThreadCreation(ThreadId id, string name)
        {
            this.name = name;
            this.id = id;
        }

        /// <summary>
        /// Gets the threads name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Thread id.
        /// </summary>
        public ThreadId Id
        {
            get { return id; }
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
