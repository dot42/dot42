namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Response element to a ReferenceType.Fields/Methods command.
    /// </summary>
    public abstract class MemberInfo<T> 
        where T : Id<T>
    {
        public readonly T Id;
        public readonly string Name;
        public readonly string Signature;
        public readonly string GenericSignature;
        public readonly int AccessFlags;

        protected MemberInfo(T id, string name, string signature, string genericSignature, int accessFlags)
        {
            Id = id;
            Name = name;
            Signature = signature;
            AccessFlags = accessFlags;
            GenericSignature = genericSignature;
        }
    }
}
