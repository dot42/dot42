using System.IO;
using Junit.Framework;
using JFile = Java.Io.File;
using JEnvironment = Android.OS.Environment;

namespace Dot42.Tests.System.IO
{
    public class TestFile : TestCase
    {
        public void testWriteAllBytes1()
        {
            var data = new byte[] { 1, 2, 3, 4 };
            var path = JFile.CreateTempFile("dot42.TestFile-WriteAllBytes1", "txt");
            File.WriteAllBytes(path.AbsolutePath, data);
            AssertTrue(path.Exists());
            var readData = File.ReadAllBytes(path.AbsolutePath);
            path.Delete();
            AssertFalse(path.Exists());
            AssertEquals(data.Length, readData.Length);
            for (var i = 0; i < data.Length; i++)
            {
                AssertEquals(data[i], readData[i]);
            }
        }

        public void testWriteAllLines1()
        {
            var data = new string[] { "Line 1", "Line 2", "Line3" };
            var path = JFile.CreateTempFile("dot42.TestFile-WriteAllLines1", "txt");
            File.WriteAllLines(path.AbsolutePath, data);
            AssertTrue(path.Exists());
            var readData = File.ReadAllLines(path.AbsolutePath);
            path.Delete();
            AssertFalse(path.Exists());
            AssertEquals(data.Length, readData.Length);
            for (var i = 0; i < data.Length; i++)
            {
                AssertEquals(data[i], readData[i]);
            }
        }

        public void testWriteAllText1()
        {
            var path = JFile.CreateTempFile("dot42.TestFile-WriteAllText1", "txt");
            File.WriteAllText(path.AbsolutePath, "Hello world");
            AssertTrue(path.Exists());
            path.Delete();
            AssertFalse(path.Exists());
        }

        public void testWriteAllText2()
        {
            const string Data = "Hello worldAllText2";
            var path = JFile.CreateTempFile("dot42.TestFile-WriteAllText2", "txt");
            File.WriteAllText(path.AbsolutePath, Data);
            AssertTrue(path.Exists());
            var content = File.ReadAllText(path.AbsolutePath);
            path.Delete();
            AssertEquals(content, Data);
        }
    }
}
