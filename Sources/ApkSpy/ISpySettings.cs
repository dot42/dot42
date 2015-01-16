namespace Dot42.ApkSpy
{
    internal interface ISpySettings
    {
#if DEBUG
        /// <summary>
        /// Show abstract syntax tree
        /// </summary>
        bool ShowAst { get; }
#endif
    }
}
