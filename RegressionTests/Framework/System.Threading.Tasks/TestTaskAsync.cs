using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Util;
using NUnit.Framework;

namespace Dot42.Tests.System.Threading.Tasks
{
    [TestFixture]
    class TestTaskAsync
    {
        [Test]
        public async void StartNewTest1()
        {
            bool result = false;
            var task = Task.Factory.StartNew(() => result = true);

            lock (this)
            {
                this.JavaWait(200); //wait 0.2 seconds in main thread
            }

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);

            await task;
            Assert.IsTrue(result);
        }

        [Test]
        public async void StartNewTest2()
        {
            bool result = false;
            await Task.Factory.StartNew(() => result = true);
            Assert.IsTrue(result);
        }

        [Test]
        public async void StartNewTest3()
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

            await task;

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(result);
        }

        [Test]
        public async void StartNewTest4()
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


            await task1;
            task2.Wait();
            await task3;
            task4.Wait();

            Assert.IsTrue(task1.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task2.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task3.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task4.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(4, result);
        }

        [Test]
        public async void StartNewTest5()
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


            await task1;
            task2.Wait();
            await task3;
            task4.Wait();

            Assert.IsTrue(task1.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task2.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task3.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(task4.Status == TaskStatus.RanToCompletion);
            Assert.AreSame(4, result);
        }

        [Test]
        public async void Start1()
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

            await task;

            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);
            Assert.IsTrue(result);


        }

        [Test]
        public async void Delay1()
        {
            var startTime = global::Java.Lang.System.CurrentTimeMillis();
            await Task.Delay(500);
            var endTime = global::Java.Lang.System.CurrentTimeMillis();
            Assert.IsTrue((endTime - startTime) >= 500);
        }

        [Test]
        public async void TestWaitAll()
        {
            var startTime = global::Java.Lang.System.CurrentTimeMillis();
            var task1 = Task.Delay(500);
            var task2 = Task.Delay(50);
            var task3 = Task.FromResult(0);
            await Task.WhenAll(new[] {task1, task2, task3});
            var endTime = global::Java.Lang.System.CurrentTimeMillis();
            Assert.IsTrue((endTime - startTime) >= 500);
        }

        [Test]
        public async void TestWaitAny()
        {
            var startTime = global::Java.Lang.System.CurrentTimeMillis();
            var task1 = Task.Delay(500);
            var task2 = Task.Delay(50);

            await Task.WhenAny(new[] { task1, task2});

            var endTime = global::Java.Lang.System.CurrentTimeMillis();
            var duration = (endTime - startTime);
            Assert.IsTrue(duration < 500 && duration >= 50);
        }

        [Test]
        public async void ExceptionTest()
        {
            bool exceptionThrown = false;

            try
            {
                await Task.Factory.StartNew(() => { throw new InvalidCastException("MyException"); });
                Assert.Fail("Issue!");
            }
			catch (InvalidCastException ex)
			{
                exceptionThrown = true;
			}
            catch (Exception ex)
            {
                Assert.Fail("Incorrect exception type : " + ex.GetType().FullName);
            }

            Assert.IsTrue(exceptionThrown, "No exception was thrown");
        }

       
    }
}
