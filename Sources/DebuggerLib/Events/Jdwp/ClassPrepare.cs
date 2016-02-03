namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of a class prepare in the target VM. See the JVM specification for a definition of class preparation. 
    /// Class prepare events are not generated for primitive classes (for example, java.lang.Integer.TYPE).  
    /// </summary>
    public sealed class ClassPrepare : BaseEvent
    {
        private readonly ReferenceTypeId typeId;
        private readonly string signature;
        private readonly DebuggerLib.Jdwp.ClassStatus status;

        public ClassPrepare(JdwpPacket.DataReaderWriter reader)
            : base(reader)
        {
            typeId = ReferenceTypeId.Read(reader);
            signature = reader.GetString();
            status = (DebuggerLib.Jdwp.ClassStatus) reader.GetInt();
        }

        public ReferenceTypeId TypeId
        {
            get { return typeId; }
        }

        public string Signature
        {
            get { return signature; }
        }

        public DebuggerLib.Jdwp.ClassStatus Status
        {
            get { return status; }
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
