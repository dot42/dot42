namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    public class ExtSimpleAndClashImplementation1 : IExtSimpleInterface, IClashInterface2
    {
        public void Foo()
        {
        }

        public int FooInt()
        {
            return 100;
        }

        int IClashInterface2.FooInt()
        {
            return 2;
        }
    }
}
