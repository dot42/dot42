using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dot42.Tests.System.Threading.Tasks
{
    [TestFixture]
    class TestTask
    {
        [Test]
        public void StartNewTest1()
        {
            bool result = false;
            var task = Task.Factory.StartNew(() => result = true);

            lock (this)
            {
                this.JavaWait(200); //wait 0.2 seconds in main thread
            }

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);

            task.Wait();
            Assert.IsTrue(result);


        }

        [Test]
        public void StartNewTest2()
        {
            bool result = false;
            Task.Factory.StartNew(() => result = true).Wait();
            Assert.IsTrue(result);
        }

        [Test]
        public void StartNewTest3()
        {
            bool result = false;
            var task = Task.Factory.StartNew(() =>
                {
                    var o = new object();
                    lock (o)
                    {
                        o.JavaWait(200); //0.2 seconds
                    }
                    result = true;
                });

            lock (this)
            {
                this.JavaWait(100); //wait 0.1 seconds in main thread, so the task can start
            }

            Assert.IsTrue(task.Status == TaskStatus.Running);

            task.Wait();

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(result);
        }

        [Test]
        public void StartNewTest4()
        {
            int result = 0;
            var task1 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(200); //0.2 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task1");
            });
            var task2 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(150); //0.15 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task2");
            });
            var task3 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(100); //0.1 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task3");
            });
            var task4 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(50); //0.05 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task4");
            });

            
            task1.Wait();
            task2.Wait();
            task3.Wait();
            task4.Wait();

            Assert.IsTrue(task1.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task2.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task3.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task4.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(4, result);
        }

        [Test]
        public void StartNewTest5()
        {
            int result = 0;
            var task1 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(200); //0.2 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task1");
            }, TaskCreationOptions.LongRunning);
            var task2 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(150); //0.15 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task2");
            }, TaskCreationOptions.LongRunning);
            var task3 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(100); //0.1 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task3");
            }, TaskCreationOptions.LongRunning);
            var task4 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(50); //0.05 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task4");
            }, TaskCreationOptions.LongRunning);


            task1.Wait();
            task2.Wait();
            task3.Wait();
            task4.Wait();

            Assert.IsTrue(task1.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task2.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task3.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task4.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(4, result);
        }

        [Test]
        public void StartWaitAll1()
        {
            int result = 0;
            var task1 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(200); //0.2 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task1");
            }, TaskCreationOptions.LongRunning);
            var task2 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(150); //0.15 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task2");
            }, TaskCreationOptions.LongRunning);
            var task3 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(100); //0.1 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task3");
            }, TaskCreationOptions.LongRunning);
            var task4 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(50); //0.05 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task4");
            }, TaskCreationOptions.LongRunning);

            Task.WaitAll(task1, task2, task3, task4);

            Assert.IsTrue(task1.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task2.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task3.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task4.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(4, result);
        }

        [Test]
        public void StartWaitAny1()
        {
            int result = 0;
            var task1 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(200); //0.2 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task1");
            }, TaskCreationOptions.LongRunning);
            var task2 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(150); //0.15 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task2");
            }, TaskCreationOptions.LongRunning);
            var task3 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(100); //0.1 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task3");
            }, TaskCreationOptions.LongRunning);
            var task4 = Task.Factory.StartNew(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(50); //0.05 seconds
                }
                result += 1;
                global::System.Diagnostics.Debug.WriteLine("Ready task4");
            }, TaskCreationOptions.LongRunning);

            var taskIndex = Task.WaitAny(task1, task2, task3, task4);

            Assert.IsTrue(task1.Status == TaskStatus.Running);
            Assert.IsTrue(task2.Status == TaskStatus.Running);
            Assert.IsTrue(task3.Status == TaskStatus.Running);
            Assert.IsTrue(task4.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(3, taskIndex);

            Assert.AreSame(1, result);
        }

        [Test]
        public void RunSyncronously()
        {
            bool result = false;
            var task =  new Task( () =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(200); //0.2 seconds
                }
                result = true;
            });

            Assert.IsTrue(task.Status == TaskStatus.Created);

            task.RunSynchronously();

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(result);


        }

        [Test]
        public void Start1()
        {
            bool result = false;
            var task = new Task(() =>
            {
                var o = new object();
                lock (o)
                {
                    o.JavaWait(200); //0.2 seconds
                }
                result = true;
            });

            Assert.IsTrue(task.Status == TaskStatus.Created);

            task.Start();

            lock (this)
            {
                this.JavaWait(100); //wait 0.1 seconds in main thread, so the task can start
            }

            Assert.IsTrue(task.Status == TaskStatus.Running);

            task.Wait();

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(result);


        }

        [Test]
        public void Delay1()
        {
            var startTime = global::Java.Lang.System.CurrentTimeMillis();
            var task = Task.Delay(500);
            task.Wait();
            var endTime = global::Java.Lang.System.CurrentTimeMillis();
            Assert.IsTrue((endTime - startTime) >= 500);
        }

        [Test]
        public void Delay2()
        {
            var startTime = global::Java.Lang.System.CurrentTimeMillis();
            var task1 = Task.Delay(100);
            var task2 = Task.Delay(300);
            Task.WaitAny(task1, task2);
            var endTime = global::Java.Lang.System.CurrentTimeMillis();
            Assert.IsTrue((endTime - startTime) >= 100);
        }

        [Test]
        public void Delay3()
        {
            var startTime = global::Java.Lang.System.CurrentTimeMillis();
            var task1 = Task.Delay(100);
            var task2 = Task.Delay(300);
            Task.WaitAll(task1, task2);
            var endTime = global::Java.Lang.System.CurrentTimeMillis();
            Assert.IsTrue((endTime - startTime) >= 300);
        }

        [Test]
        public void ExceptionTest()
        {
            bool exceptionThrown = false;

            try
            {
                var t = Task.Factory.StartNew(() => { throw new InvalidCastException("MyException"); });
                t.Wait();
                Assert.Fail("Issue!");
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                Assert.AssertTrue("Incorrect exception type : " + ex.GetType().FullName, ex is AggregateException);

                var aggregateException = ex as AggregateException;

                Assert.AssertEquals("Incorrect count", 1, aggregateException.InnerExceptions.Count);

                Assert.AssertEquals("MyException", ex.Message);

            }

            Assert.IsTrue(exceptionThrown, "No exception was thrown");
        }
    }
}
