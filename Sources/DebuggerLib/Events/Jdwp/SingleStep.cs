namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// </summary>
    public sealed class SingleStep : BaseLocationEvent
    {
        /// <summary>
        /// Notification of step completion in the target VM. The step event is generated before the code at its location is executed. 
        /// </summary>
        public SingleStep(JdwpPacket.DataReaderWriter reader)
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
