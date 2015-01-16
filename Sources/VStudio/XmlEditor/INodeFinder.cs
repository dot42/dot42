using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dot42.VStudio.XmlEditor
{
    public interface INodeFinder
    {
        bool IsXmlStartTag { get; }
        bool IsElement { get; }
        bool IsAttribute { get; }

        List<string> GetPath();
        List<string> GetParentPath();
        string GetLocalName();
    }
}
