namespace Dot42.VStudio.XmlEditor
{
    /// <summary>
    /// Visual Studio version independent XmlEditorService.
    /// </summary>
    public interface IXmlEditorService
    {
        /// <summary>
        /// Create an XML store.
        /// </summary>
        IXmlStore CreateXmlStore();

        /// <summary>
        /// Gets the XML language service.
        /// </summary>
        IXmlLanguageService GetLanguageService();
    }
}
