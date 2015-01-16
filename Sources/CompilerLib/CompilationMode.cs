namespace Dot42.CompilerLib
{
    /// <summary>
    /// Modes in which the code compiler can operate
    /// </summary>
    public enum CompilationMode
    {
        /// <summary>
        /// All types that are important for an application are roots. Only those members that are reachable are included.
        /// Important types are:
        /// - Marked by an manifest related attribute
        /// - Extend a testcase base class.
        /// </summary>
        /// <remarks>
        /// This is the default mode.
        /// </remarks>
        Application,

        /// <summary>
        /// All publicly visible types and members are roots.
        /// All reachable types/members are also included.
        /// </summary>
        ClassLibrary,

        /// <summary>
        /// Everything in the assembly is included
        /// </summary>
        All
    }
}
