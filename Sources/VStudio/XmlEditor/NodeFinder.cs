using System.Collections.Generic;
using Microsoft.XmlEditor;
using VSNodeFinder = Microsoft.XmlEditor.NodeFinder;

namespace Dot42.VStudio.XmlEditor
{
    internal class NodeFinder : INodeFinder
    {
        private readonly VSNodeFinder nodeFinder;

        public NodeFinder(VSNodeFinder nodeFinder)
        {
            this.nodeFinder = nodeFinder;
        }

        /// <summary>
        /// Gets an array of all elements from the root upto (including) the given node.
        /// </summary>
        private static List<string> GetDocumentPath(XmlNode node)
        {
            if (node == null)
                return null;
            var list = new List<string>();
            while ((node != null) && (node.HasName))
            {
                list.Insert(0, node.Name.Name);
                node = node.Parent;
            }
            return list;
        }

        public bool IsXmlStartTag { get { return nodeFinder.Scope is XmlStartTag; } }
        public bool IsElement { get { return nodeFinder.Scope is XmlElement; } }
        public bool IsAttribute { get { return nodeFinder.Scope is XmlAttribute; } }

        public List<string> GetPath()
        {
            return GetDocumentPath(nodeFinder.Scope);
        }

        public List<string> GetParentPath()
        {
            return GetDocumentPath(nodeFinder.Scope.Parent);
        }

        public string GetLocalName()
        {
            return nodeFinder.Scope.LocalName;
        }
    }
}
