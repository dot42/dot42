namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Response element to a ReferenceType.Methods command.
    /// </summary>
    public sealed class MethodInfo : MemberInfo<MethodId>
    {
        public MethodInfo(MethodId id, string name, string signature, string genericSignature, int accessFlags)
            : base(id, name, signature, genericSignature, accessFlags)
        {
        }
    }
}
