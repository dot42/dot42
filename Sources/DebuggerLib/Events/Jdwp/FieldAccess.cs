namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of a field access in the target VM. Field modifications are not considered field accesses. 
    /// Requires canWatchFieldAccess capability - see CapabilitiesNew. 
    /// </summary>
    public sealed class FieldAccess : BaseFieldEvent
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public FieldAccess(JdwpPacket.DataReaderWriter reader)
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
