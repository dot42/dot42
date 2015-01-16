namespace Dot42.DebuggerLib.Events.Dalvik
{
    public class WaitForDebugger : DalvikEvent
    {
        private readonly int reason;

        public WaitForDebugger(int reason)
        {
            this.reason = reason;
        }

        /// <summary>
        /// Wait reason (0 == wait for debugger)
        /// </summary>
        public int Reason
        {
            get { return reason; }
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
