using Java.Lang;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestLock : TestCase
    {
        public void testLock1()
        {
            var tmp = new object();
            lock (tmp)
            {
                AssertNotNull(tmp);
            }
        }

        public void testLock2()
        {
            runTest(false);
        }

        public void testLock3()
        {
            runTest(true);
        }

        public void runTest(bool useLock)
        {
            var sharedObject = new SharedObject(useLock);

            var threadWork1 = new Work(sharedObject, useLock);
            var newThread1 = new System.Threading.Thread(threadWork1, "Thread 1");

            var threadWork2 = new Work(sharedObject, useLock);
            var newThread2 = new System.Threading.Thread(threadWork2, "Thread 2");

            newThread1.Start();
            newThread2.Start();

            newThread1.Join();
            newThread2.Join();

            AssertEquals(sharedObject.Counter, 2);
        }

        class Work: IRunnable
        {
            private readonly SharedObject _sharedObject;
            private readonly bool _useLock;

            public Work(SharedObject sharedObject, bool useLock)
            {
                _sharedObject = sharedObject;
                _useLock = useLock;
            }

            public void Run()
            {
                lock (_useLock?_sharedObject:new object())
                {
                    _sharedObject.DoIt();
                }
            }
        }

        private class SharedObject
        {
            private readonly bool _useLock;
            private int _counter;
            
            public SharedObject(bool useLock)
            {
                _useLock = useLock;
            }

            public int Counter
            {
                get { return _counter; }
            }

            internal void DoIt()
            {
                var current = _counter;
                _counter++;

                System.Threading.Thread.Sleep(500L);

                if (_useLock || current == 1)
                {
                    AssertEquals(_counter, current + 1);
                }
                else
                {
                    //no lock and current is 0, the other thread runs parallel, 
                    //so the 'current' should be incremented twice.
                    AssertEquals(_counter, current + 2);
                }
            }
        }

    }
}
