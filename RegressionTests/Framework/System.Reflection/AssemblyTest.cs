using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Dot42.Tests.System.Reflection
{
    [TestFixture]
    public class AssemblyTest
    {
        [Test]
        public void TestAssemblyOfType()
        {
            Assert.AreEqual("FrameworkTests", typeof(AssemblyTest).Assembly.FullName);
        }

        [Test]
        public void TestGetExecutingAssembly()
        {
            Assert.AreEqual(typeof(AssemblyTest).Assembly, Assembly.GetExecutingAssembly());
        }

        [Test]
        public void TestAssemblyGetTypes()
        {
            Assert.IsTrue(typeof(AssemblyTest).Assembly.DefinedTypes.Any(t => t.IsAssignableFrom(typeof (AssemblyTest).GetTypeInfo())));
        }
        
    }
}

