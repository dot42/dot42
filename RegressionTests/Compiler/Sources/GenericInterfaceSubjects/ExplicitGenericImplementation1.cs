namespace Dot42.Tests.Compiler.Sources.GenericInterfaceSubjects
{
    internal class ExplicitGenericImplementation1<T> : IGenericInterface<T>
    {
        T IGenericInterface<T>.FooReturn()
        {
            return default(T);
        }

        void IGenericInterface<T>.Foo(T x)
        {
        }
    }
}
