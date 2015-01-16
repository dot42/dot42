using System;

namespace Dot42.VStudio.XmlEditor
{
    public interface IXmlModel : IDisposable
    {
        event EventHandler BufferReloaded;

        string Name { get; }
    }
}
