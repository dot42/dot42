using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.XmlEditor;

namespace Dot42.VStudio.XmlEditor
{
    internal class XmlModel : IXmlModel
    {
        private readonly Microsoft.VisualStudio.XmlEditor.XmlModel model;

        public event EventHandler BufferReloaded
        {
            add { model.BufferReloaded += value; }
            remove { model.BufferReloaded -= value; }
        }

        internal XmlModel(Microsoft.VisualStudio.XmlEditor.XmlModel model)
        {
            this.model = model;
        }

        public void Dispose()
        {
            model.Dispose();
        }

        public string Name { get { return model.Name; } }
    }
}
