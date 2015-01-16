using System.IO;
using Junit.Framework;

namespace Dot42.Tests.System.IO
{
    public class TestMemoryStream : TestCase
    {
        public void testReadBytes()
        {
            var data = new byte[] { 1, 2, 3, 4 };
            var memStream = new MemoryStream(data);

            AssertEquals(data.Length, memStream.Length);
            AssertEquals(0, memStream.Position);

            for (var i = 0; i < data.Length; i++)
            {
                AssertEquals(data[i], memStream.ReadByte());
            }

            AssertEquals(-1, memStream.ReadByte());
        }
    }
}
