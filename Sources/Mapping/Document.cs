using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Refers to a source code file
    /// </summary>
    public sealed class Document
    {
        private readonly string path;
        private readonly List<DocumentPosition> positions = new List<DocumentPosition>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public Document(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        public Document(XElement element)
        {
            path = element.GetAttribute("path");
            positions.AddRange(element.Elements("p").Select(x => new DocumentPosition(x)));
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal XElement ToXml(string elementName)
        {
            var element = new XElement(elementName,
                                       new XAttribute("path", path));
            element.Add(positions.OrderBy(x => x).Select(x => x.ToXml("p")));
            return element;
        }

        /// <summary>
        /// Full path of the source file.
        /// </summary>
        public string Path
        {
            get { return path; }
        }

        /// <summary>
        /// Get all positions within this document
        /// </summary>
        public List<DocumentPosition> Positions
        {
            get { return positions; }
        }

        /// <summary>
        /// Try to find the position that has the best match with the given parameters.
        /// </summary>
        /// <returns>Null if not found</returns>
        public DocumentPosition Find(int startLine, int startCol, int endLine, int endCol)
        {
      		return positions.FirstOrDefault(x => x.Intersects(startLine, startCol, endLine, endCol));
        }

        /// <summary>
        /// Finds all position that has match with the given parameters.
        /// </summary>
        /// <returns>Null if not found</returns>
        public IEnumerable<DocumentPosition> FindAll(int startLine, int startCol, int endLine, int endCol)
        {
            return positions.Where(x => x.Intersects(startLine, startCol, endLine, endCol));
        }

        /// <summary>
        /// Perform size and speed optimizations.
        /// </summary>
        public void Optimize()
        {
            // Sort
            positions.Sort();

            // Remove positions that are equal except for offset
            var i = 1;
            while (i < positions.Count)
            {
                var last = positions[i - 1];
                var pos = positions[i];

                if (last.EqualExceptOffset(pos) && !pos.IsReturn)
                {
                    // Remove current position
                    positions.RemoveAt(i);
                }
                else
                {
                    // Go to next
                    i++;
                }
            }
        }
    }
}
