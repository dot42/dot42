using VSXmlDocument = Microsoft.XmlEditor.XmlDocument;

namespace Dot42.VStudio.XmlEditor
{
    internal class XmlDocument : IXmlDocument
    {
        private readonly VSXmlDocument document;

        public XmlDocument(VSXmlDocument document)
        {
            this.document = document;
        }

        public INodeFinder SearchForNodeAtPosition(int line, int column)
        {
            var nf = document.SearchForNodeAtPosition(line, column);
            return (nf != null) ? new NodeFinder(nf) : null;
        }
    }
}
