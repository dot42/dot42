namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    public class ExtSimpleAndClashImplementation2 : IExtSimpleInterface, IClashInterface2
    {
        public void Foo()
        {
        }

        int IExtSimpleInterface.FooInt()
        {
            return 100;
        }

        public int FooInt()
        {
            return 2;
        }
    }
}
