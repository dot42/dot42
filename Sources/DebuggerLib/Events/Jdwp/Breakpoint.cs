namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// </summary>
    public sealed class Breakpoint : BaseLocationEvent
    {
        /// <summary>
        /// Notification of a breakpoint in the target VM. The breakpoint event is generated before the code at its location is executed.  
        /// </summary>
        public Breakpoint(JdwpPacket.DataReaderWriter reader)
            : base(reader)
        {
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
