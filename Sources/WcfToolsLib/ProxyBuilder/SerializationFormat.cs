using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dot42.WcfTools.ProxyBuilder
{
    internal enum SerializationFormat
    {
        [Obfuscation]
        DataContract,
        [Obfuscation]
        XmlSerializer
    }
}
