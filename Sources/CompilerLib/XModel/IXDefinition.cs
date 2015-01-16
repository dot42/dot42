namespace Dot42.CompilerLib.XModel
{
    public interface IXDefinition
    {
        /// <summary>
        /// Gets the type that contains this member
        /// </summary>
        XTypeDefinition DeclaringType { get; }
    }
}
