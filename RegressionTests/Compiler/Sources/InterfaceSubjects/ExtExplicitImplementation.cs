namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    public class ExtExplicitImplementation : IExtSimpleInterface
    {
        void ISimpleInterface.Foo()
        {
        }

        int IExtSimpleInterface.FooInt()
        {
            return 1;
        }
    }
}
