using System;

namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of a class unload in the target VM.
    /// There are severe constraints on the debugger back-end during garbage collection, so unload information is greatly limited.	 
    /// </summary>
    public sealed class ClassUnload : JdwpEvent
    {
        private readonly int requestId;
        private readonly string signature;

        public ClassUnload(JdwpPacket.DataReaderWriter reader)
        {
            requestId = reader.GetInt();
            signature = reader.GetString();
        }

        public string Signature
        {
            get { return signature; }
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
