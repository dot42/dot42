namespace Dot42.Tests.Compiler.Sources.GenericInterfaceSubjects
{
    public interface IGenericInterface<T>
    {
        T FooReturn();

        void Foo(T x);
    }
}
