namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Response to a AllClasses command.
    /// </summary>
    public sealed class ClassInfo
    {
        public readonly ReferenceTypeId TypeId;
        public readonly string Signature;
        public readonly string GenericSignature;
        public readonly Jdwp.ClassStatus Status;

        public ClassInfo(ReferenceTypeId typeId, string signature, string genericSignature, Jdwp.ClassStatus status)
        {
            TypeId = typeId;
            Signature = signature;
            GenericSignature = genericSignature;
            Status = status;
        }
    }
}
