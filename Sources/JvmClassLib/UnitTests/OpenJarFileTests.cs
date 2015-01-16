using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dot42.JvmClassLib.UnitTests
{
    [TestClass]
    public class OpenJarFileTests
    {
        [TestMethod]
        public void Test()
        {
            var jf = new JarFile(new MemoryStream(Resources.android), "test", null);
            foreach (var name in jf.ClassNames)
            {
                ClassFile cf;
                jf.TryLoadClass(name, out cf);
            }
        }
    }
}
