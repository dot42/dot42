using System;

namespace Dot42.VStudio.XmlEditor
{
    public interface IXmlDocument
    {
        INodeFinder SearchForNodeAtPosition(int line, int column);
    }
}
