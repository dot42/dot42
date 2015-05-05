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
        string BaksmaliCommand { get; }
        string BaksmaliParameters { get; }

        bool ShowControlFlow { get; }

        bool EmbedSourceCodePositions { get; }
        bool EmbedSourceCode { get; }
    }
}
