using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Dot42.Tests.System.Threading.Tasks
{
    [TestFixture]
    class TestCancellationToken
    {
        [Test]
        public void CancellationTokenCanceled()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                cts.Cancel();
            }, TaskCreationOptions.LongRunning);

            var task = Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("in task");

                for (var i = 0; i < 20; i++)
                {
                    Thread.Sleep(100);
                    var ct = cts.Token;
                    if (ct.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancellation was requested from within task");
                        ct.ThrowIfCancellationRequested();
                    }
                    Debug.WriteLine("in loop");
                }

                Debug.WriteLine("task finished without cancellation");
            }, cts.Token);

            var exceptionThrown = false;

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is AggregateException, "Expected AggregateException");

                Assert.IsNotNull(ex.InnerException, "Expected InnerException");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("inner Exception: {0} - {1}", ex.InnerException.GetType().Name, ex.InnerException.Message);
                    Assert.IsTrue(ex.InnerException is TaskCanceledException, "Expected TaskCanceledException");
                }
            }

            Assert.IsTrue(exceptionThrown, "Expected that an exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);

            sw.Stop();
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 499 && sw.ElapsedMilliseconds < 700, "Expected time between 499 and 700 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.Canceled, task.Status, "Expected TaskStatus.Canceled");
        }

        [Test]
        public void CancellationTokenFaulted()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                cts.Cancel();
            }, TaskCreationOptions.LongRunning);

            var task = Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("in task");

                for (var i = 0; i < 20; i++)
                {
                    Thread.Sleep(100);
                    var ct = cts.Token;
                    if (ct.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancellation was requested from within task");
                        ct.ThrowIfCancellationRequested();
                    }
                    Debug.WriteLine("in loop");
                }

                Debug.WriteLine("task finished without cancellation");
            });

            var exceptionThrown = false;

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is AggregateException, "Expected AggregateException");

                Assert.IsNotNull(ex.InnerException, "Expected InnerException");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("inner Exception: {0} - {1}", ex.InnerException.GetType().Name, ex.InnerException.Message);
                    Assert.IsTrue(ex.InnerException is OperationCanceledException, "Expected OperationCanceledException");
                }
            }

            Assert.IsTrue(exceptionThrown, "Expected that an exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);

            sw.Stop();
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 499 && sw.ElapsedMilliseconds < 700, "Expected time between 499 and 700 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.Faulted, task.Status); 
        }

        [Test]
        public void CancellationTokenCanceledwithExceptionHandler()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                cts.Cancel();
            }, TaskCreationOptions.LongRunning);

            var task = Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("in task");

                for (var i = 0; i < 20; i++)
                {
                    Thread.Sleep(100);
                    var ct = cts.Token;
                    if (ct.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancellation was requested from within task");
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Task Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                        }
                        
                    }
                    Debug.WriteLine("in loop");
                }

                Debug.WriteLine("task finished without cancellation");
            }, cts.Token);

            var exceptionThrown = false;

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is AggregateException, "Expected AggregateException");

                Assert.IsNotNull(ex.InnerException, "Expected InnerException");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("inner Exception: {0} - {1}", ex.InnerException.GetType().Name, ex.InnerException.Message);
                    Assert.IsTrue(ex.InnerException is TaskCanceledException, "Expected TaskCanceledException");
                }
            }

            Assert.IsFalse(exceptionThrown, "Expected that no exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);

            sw.Stop();
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 1999 && sw.ElapsedMilliseconds < 2250, "Expected time between 1999 and 2250 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status, "Expected TaskStatus.Canceled");
        }

        [Test]
        public void CancellationTokenWait1()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                cts.Cancel();
            }, TaskCreationOptions.LongRunning);

            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                Debug.WriteLine("Ready with task");
            });

            var exceptionThrown = false;

            try
            {
                task.Wait(cts.Token);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is OperationCanceledException, "Expected OperationCanceledException");
            }

            Assert.IsTrue(exceptionThrown, "Expected that an exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 499 && sw.ElapsedMilliseconds < 750, "Expected time between 499 and 750 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.Running, task.Status);

            task.Wait();

            Debug.WriteLine("status of task: {0}", task.Status);
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 1999 && sw.ElapsedMilliseconds < 2050, "Expected time between 1999 and 2050 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public void CancellationTokenWait2()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                cts.Cancel();
            }, TaskCreationOptions.LongRunning);

            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                Debug.WriteLine("Ready with task");
            }, cts.Token);

            var exceptionThrown = false;

            try
            {
                task.Wait(cts.Token);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is OperationCanceledException, "Expected OperationCanceledException");
            }

            Assert.IsTrue(exceptionThrown, "Expected that an exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 499 && sw.ElapsedMilliseconds < 750, "Expected time between 499 and 750 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.Running, task.Status);

            task.Wait();

            Debug.WriteLine("status of task: {0}", task.Status);
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 1999 && sw.ElapsedMilliseconds < 2050, "Expected time between 1999 and 2050 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public void CancellationTokenDelay1()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                cts.Cancel();
            }, TaskCreationOptions.LongRunning);

            var task = Task.Delay(2000, cts.Token);

            var exceptionThrown = false;

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is AggregateException, "Expected AggregateException");

                Assert.IsNotNull(ex.InnerException, "Expected InnerException");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("inner Exception: {0} - {1}", ex.InnerException.GetType().Name, ex.InnerException.Message);
                    Assert.IsTrue(ex.InnerException is TaskCanceledException, "Expected TaskCanceledException");
                }
            }

            Assert.IsTrue(exceptionThrown, "Expected that an exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 499 && sw.ElapsedMilliseconds < 750, "Expected time between 499 and 750 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }

        [Test]
        public void CancellationTokenDelay2()
        {
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Debug.WriteLine("** Task canceled **"));
            cts.Cancel();
           
            var task = Task.Delay(2000, cts.Token);

            var exceptionThrown = false;

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                Debug.WriteLine("Exception: {0} - {1}", ex.GetType().Name, ex.Message);
                Assert.IsTrue(ex is AggregateException, "Expected AggregateException");

                Assert.IsNotNull(ex.InnerException, "Expected InnerException");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("inner Exception: {0} - {1}", ex.InnerException.GetType().Name, ex.InnerException.Message);
                    Assert.IsTrue(ex.InnerException is TaskCanceledException, "Expected TaskCanceledException");
                }
            }

            Assert.IsTrue(exceptionThrown, "Expected that an exception whas thrown");

            Debug.WriteLine("status of task: {0}", task.Status);
            Debug.WriteLine("elapsed: {0}ms", sw.ElapsedMilliseconds);

            Assert.IsTrue(sw.ElapsedMilliseconds > 0 && sw.ElapsedMilliseconds < 100, "Expected time between 0 and 100 ms however was: " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(cts.IsCancellationRequested, "Expected IsCancellationRequested");
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }
    }
}
