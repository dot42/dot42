namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    public class VirtualExtSimpleImplementation : IExtSimpleInterface
    {
        public void Foo()
        {
        }

        public virtual int FooInt()
        {
            return 2;
        }
    }
}
