using System;
using System.Threading;
using System.Threading.Tasks;

namespace TallComponents.Common.Extensions
{
    /// <summary>
    /// Common task extension methods
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Is the given task completed succesfully.
        /// </summary>
        public static bool CompletedOk(this Task t)
        {
            return (t.Status == TaskStatus.RanToCompletion);
        }

        /// <summary>
        /// If there is an exception in the given task, throw it in an AggregateException.
        /// </summary>
        public static void ForwardException(this Task t)
        {
            var ex = t.Exception;
            if (ex != null)
            {
                throw new AggregateException(ex).Flatten();
            }
        }

        /// <summary>
        /// Wait for the given task to complete and return it's result.
        /// Exceptions are thrown if there was an exception in the task.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds. Use -1 to wait without timeout.</param>
        public static T Await<T>(this Task<T> t, int millisecondsTimeout, CancellationToken cancel=default(CancellationToken))
        {
            if(!t.Wait(millisecondsTimeout, cancel))
                throw new OperationCanceledException("timeout");
            return t.Result;
        }

        /// <summary>
        /// Wait for the given task to complete and return it's result.
        /// Exceptions are thrown if there was an exception in the task.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds. Use -1 to wait without timeout.</param>
        public static void Await(this Task t, int millisecondsTimeout, CancellationToken cancel = default(CancellationToken))
        {
            if (!t.Wait(millisecondsTimeout, cancel))
                throw new OperationCanceledException("timeout");
        }

        /// <summary>
        /// Wait for the given task to complete and return it's result.
        /// Exceptions are thrown if there was an exception in the task.
        /// </summary>
        public static Task<T> SaveAndReturn<T>(this Task<T> t, Action<T> saveAction)
        {
            return t.ContinueWith(x => {
                x.ForwardException();
                saveAction(x.Result);
                return x.Result;
            });
        }

        /// <summary>
        /// Create a short lived task that directly returns the given value.
        /// </summary>
        public static Task<T> AsTask<T>(this T value)
        {
            var source = new TaskCompletionSource<T>();
            source.SetResult(value);
            return source.Task;
        }

        /// <summary>
        /// Create a task that waits for the result of the given tasks and combined the results with the given selector.
        /// </summary>
        public static Task<TResult> Concat<T1, T2, TResult>(this Task<T1> t1, Task<T2> t2, Func<T1, T2, TResult> selector)
        {
            return t1.ContinueWith(r1 => {
                r1.ForwardException();
                return t2.ContinueWith(r2 => {
                    r2.ForwardException();
                    return selector(r1.Result, r2.Result);
                });
            }).Unwrap();
        }

        /// <summary>
        /// Create a task that waits for the result of the given tasks and combined the results with the given selector.
        /// </summary>
        public static Task<TResult> Concat<T1, T2, T3, TResult>(this Task<T1> t1, Task<T2> t2, Task<T3> t3, Func<T1, T2, T3, TResult> selector)
        {
            return t1.ContinueWith(r1 => {
                r1.ForwardException();
                return t2.ContinueWith(r2 => {
                    r2.ForwardException();
                    return t3.ContinueWith(r3 => {
                        r3.ForwardException();
                        return selector(r1.Result, r2.Result, r3.Result);
                    });
                });
            }).Unwrap().Unwrap();
        }

        /// <summary>
        /// Creates a task that selects a part of the result of the given task.
        /// </summary>
        public static Task<TResult> Select<TSource, TResult>(this Task<TSource> task, Func<TSource, TResult> selector)
        {
            return task.ContinueWith(t => {
                t.ForwardException();
                return selector(t.Result);
            });
        }
    }
}
