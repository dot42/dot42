namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    public class SimpleAndClashImplementation : ISimpleInterface, IClashInterface
    {
        public void Foo()
        {
        }

        int IClashInterface.Foo()
        {
            return 1;
        }
    }
}
