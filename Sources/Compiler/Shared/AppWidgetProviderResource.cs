namespace Dot42.Compiler.Shared
{
    /// <summary>
    /// Helper for AppWidgetProvider xml files.
    /// </summary>
    internal class AppWidgetProviderResource
    {
        /// <summary>
        /// Create a (hopefully) unique filename to contain the xml.
        /// </summary>
        internal static string GetResourceName(int index)
        {
            return "dot42_app_widget_provider_" + index;
        }
    }
}
