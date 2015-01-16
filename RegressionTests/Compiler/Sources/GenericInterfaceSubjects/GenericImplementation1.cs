namespace Dot42.Tests.Compiler.Sources.GenericInterfaceSubjects
{
    internal class GenericImplementation1<T> : IGenericInterface<T>
    {
        public T FooReturn()
        {
            return default(T);
        }

        public void Foo(T x)
        {
        }
    }
}
