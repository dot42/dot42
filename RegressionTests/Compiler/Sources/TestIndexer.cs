using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestIndexer : TestCase
    {
        public void test1a()
        {
            var pages = new PageCollection();

            var page = pages[42];

            AssertEquals(42, page.Index);
        }

        public void test1b()
        {
            IReadOnlyCollection<Page> pages = new PageCollection();

            var page = pages[42];

            AssertEquals(42, page.Index);
        }

        public class Page
        {
            public Page(int index)
            {
                Index = index;
            }

            public int Index { get; private set; }
        }

        public interface IReadOnlyCollection<T> 
        {
            T this[int index] { get; }
        }

        public class PageCollection : IReadOnlyCollection<Page>
        {
            public Page this[int index]
            {
                get
                {
                    return new Page(index);
                }
            }
        }
    }
}
