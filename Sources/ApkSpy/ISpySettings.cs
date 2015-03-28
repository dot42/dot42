namespace Dot42.ApkSpy
{
    internal interface ISpySettings
    {
#if DEBUG || ENABLE_SHOW_AST
        /// <summary>
        /// Show abstract syntax tree
        /// </summary>
        bool ShowAst { get; }
#endif
        bool EnableBaksmali { get; }
        string BacksmaliCommand { get; }
        string BacksmaliParameters { get; }
    }
}
