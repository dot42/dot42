
using System;
using System.IO;
using System.Xml.Linq;
using ICSharpCode.SharpDevelop;

namespace Dot42.SharpDevelop.Editors
{
	/// <summary>
	/// Description of OpenedFileSerializer.
	/// </summary>
	internal sealed class OpenedFileSerializer
	{
		private readonly OpenedFile file;
		
		public OpenedFileSerializer(OpenedFile file)
		{
			this.file = file;
		}
		
		        /// <summary>
        /// Parse the contents of the buffer.
        /// </summary>
        public XDocument Parse(Stream stream)
        {
        	return XDocument.Load(stream);
        }

        /// <summary>
        /// Overwrite the text in the buffer with the given document
        /// </summary>
        public void Save(XDocument document, Stream stream)
        {
        	document.Save(stream);
        }
	}
}
