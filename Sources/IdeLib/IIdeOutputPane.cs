namespace Dot42.Ide
{
    public interface IIdeOutputPane
    {
        /// <summary>
        /// Add the given line to this page.
        /// </summary>
        void LogLine(string line);

        /// <summary>
        /// Make sure this pane is loaded.
        /// </summary>
        void EnsureLoaded();
    }
}
