namespace Dot42.DebuggerLib.Events.Dalvik
{
    /// <summary>
    /// Thread death notification (THDE)
    /// </summary>
    public class ThreadDeath : DalvikEvent
    {
        private readonly ThreadId id;

        public ThreadDeath(ThreadId id)
        {
            this.id = id;
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
