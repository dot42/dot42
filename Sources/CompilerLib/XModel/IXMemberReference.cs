namespace Dot42.CompilerLib.XModel
{
    public interface IXMemberReference 
    {
        /// <summary>
        /// Gets the type that contains this member
        /// </summary>
        XTypeReference DeclaringType { get; }
    }
}
