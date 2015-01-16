namespace Dot42.CompilerLib
{
    public interface IParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// Can be null
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is this parameter equal to the given variable?
        /// </summary>
        bool Equals(IVariable variable);
    }
}
