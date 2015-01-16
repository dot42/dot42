namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of a field modification in the target VM. Requires canWatchFieldModification capability - see CapabilitiesNew. 
    /// </summary>
    public sealed class FieldModification : BaseFieldEvent
    {
        private readonly Value value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public FieldModification(JdwpPacket.DataReaderWriter reader)
            : base(reader)
        {
            value = new Value(reader);
        }

        /// <summary>
        /// Value to be assigned 
        /// </summary>
        public Value Value
        {
            get { return value; }
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
