using System.IO;
using Dot42.ApkLib.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dot42.ApkLib.UnitTests
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public void LoadTableTest()
        {
            var arsc = new Table(new MemoryStream(TestResource.resources));
        }

        [TestMethod]
        public void LoadTableTestFull()
        {
            var resources = new Table(new MemoryStream(TestResource.android_jar_resources));
            var index = resources.Strings.IndexOf("name", -1);
            var pkg = resources.Packages[0];
            var index2 = pkg.KeyStrings.IndexOf("name", -1);
        }
    }
}
