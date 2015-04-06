namespace Dot42.Tests.Compiler.Sources.GenericInterfaceSubjects
{
    public interface IInterfaceWithGenericMethod
    {
        T FooReturn<T>();
    }

    public class ClassImplementGenericMethodFromInterface : IInterfaceWithGenericMethod
    {
        public T FooReturn<T>()
        {
            return default(T);
        }
    }

    public class ClassCallsGenericMethodFromGenericMethod
    {
        public T FooReturn<T>()
        {
            return FooReturn1<T>();
        }

        private T FooReturn1<T>()
        {
            return default (T);
        }
    }
}
