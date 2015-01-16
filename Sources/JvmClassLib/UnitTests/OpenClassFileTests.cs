using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dot42.JvmClassLib.UnitTests
{
    [TestClass]
    public class OpenClassFileTests
    {
        [TestMethod]
        public void Test()
        {
            var cf = new ClassFile(new MemoryStream(Resources.HalloAndroidActivity), null);
            Assert.AreEqual("org/hello/HalloAndroidActivity", cf.ClassName);

        }
    }
}
