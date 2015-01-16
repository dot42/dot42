namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    /// <summary>
    /// FooInt is implemented implicitly here for 2 interfaces.
    /// However both are also explicitly implemented elsewhere.
    /// </summary>
    public class ExtSimpleAndClashImplementation3 : IExtSimpleInterface, IClashInterface2
    {
        public void Foo()
        {
        }

        public int FooInt()
        {
            return 3;
        }
    }
}
