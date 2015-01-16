namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Response element to a ReferenceType.Fields command.
    /// </summary>
    public sealed class FieldInfo : MemberInfo<FieldId>
    {
        public FieldInfo(FieldId id, string name, string signature, string genericSignature, int accessFlags)
            : base(id, name, signature, genericSignature, accessFlags)
        {
        }
    }
}
