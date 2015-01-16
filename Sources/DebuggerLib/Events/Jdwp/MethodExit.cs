namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of a method return in the target VM. This event is generated after all code in the method has executed, but the location of this 
    /// event is the last executed location in the method. Method exit events are generated for both native and non-native methods. Method exit events 
    /// are not generated if the method terminates with a thrown exception.  
    /// </summary>
    public sealed class MethodExit : BaseLocationEvent
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public MethodExit(JdwpPacket.DataReaderWriter reader)
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
