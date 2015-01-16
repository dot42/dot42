namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of a method invocation in the target VM. This event is generated before any code in the invoked method has executed. 
    /// Method entry events are generated for both native and non-native methods.
    /// In some VMs method entry events can occur for a particular thread before its thread start event occurs if methods are called as 
    /// part of the thread's initialization.  
    /// </summary>
    public sealed class MethodEntry : BaseLocationEvent
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public MethodEntry(JdwpPacket.DataReaderWriter reader)
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
