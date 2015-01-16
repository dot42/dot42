using System;

namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of the VM death.
    /// </summary>
    public sealed class VmDeath : JdwpEvent
    {
        private readonly int requestId;

        public VmDeath(JdwpPacket.DataReaderWriter reader)
        {
            requestId = reader.GetInt();
        }

        public int RequestId
        {
            get { return requestId; }
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
