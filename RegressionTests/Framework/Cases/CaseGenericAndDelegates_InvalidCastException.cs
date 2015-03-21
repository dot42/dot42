using System;
using System.ComponentModel;
using Junit.Framework;

namespace Dot42.Tests.Cases
{
    public class CaseGenericAndDelegates_InvalidCastException : TestCase
    {
        public void testConvertToObjectCallObject()
        {
            new Test00_<Class1>().Test(new Class1());
        }

        public void testConvertToObjectCallGeneric()
        {
            new Test01_<Class1>().Test(new Class1());
        }

        public void testCallMethodOfGeneric()
        {
            new Test10_<Class1>().Test(new Class1());
        }


        class Class1 : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
        }



    }

    internal class Test00_<TSource> where TSource : INotifyPropertyChanged
    {
        public void Test(TSource source)
        {
            // retrieve source property getter
            var objSource = (object)source;
            var sourceType = objSource.GetType();

            Func<object, object> g = obj => obj.ToString();
            Func<object> getSourceValue = () => g(objSource);


        }
    }

    internal class Test01_<TSource> where TSource : INotifyPropertyChanged
    {
        public void Test(TSource source)
        {
            // retrieve source property getter
            var objSource = (object)source;
            var sourceType = objSource.GetType();

            Func<object, object> g = obj => obj.ToString();
            Func<object> getSourceValue = () => g(source);


        }
    }

    internal class Test10_<TSource> where TSource : INotifyPropertyChanged
    {
        public void Test(TSource source)
        {
            // retrieve source property getter
            var sourceType = source.GetType();

            Func<object, object> g = obj => obj.ToString();
            Func<object> getSourceValue = () => g(source);
        }
    }
}
