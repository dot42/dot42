using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestNestedFinallySpecial : TestCase
    {
        private readonly object _serializer = new object();
        private int ActiveRequests;
        private int _logId = 1;

        public void testHandleRequestImpl()
        {
            HandleRequestImpl(new MemoryStream(), CancellationToken.None);
        }

        private class CommitNotFoundException : Exception
        {
        }

        private static class Log
        {
            public static void Debug(string forma, params object[] args)
            {
            }
        }

        public void RunInTransaction(Action a)
        {
            a();
        }

        private void HandleRequestImpl(Stream stream, CancellationToken cancel)
        {
            var reader = new StreamReader(stream);

            var writer = new StreamWriter(stream) {NewLine = "\n"};

            int numRequestsHandled = 0;

            int finallyCalled = 0;

            SemaphoreSlim accountSemaphore = _logId == -1 ? new SemaphoreSlim(1) : null;

            lock (_serializer)
                ActiveRequests += 1;

            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    try
                    {
                        Log.Debug("{0}: handling request: waiting for request.", _logId);

                        string line;

                        // allow empty lines from previous requests.
                        while ((line = reader.ReadLine()) == "" && numRequestsHandled > 0)
                            continue;

                        if (line == null)
                        {
                            if (numRequestsHandled == 0)
                                throw new Exception("premature end of input");
                            return;
                        }

                        Log.Debug("{0}: request: {1}", _logId, line);

                        if (Regex.IsMatch(line, "^PUT /ninjasync HTTP/1.[01]$"))
                        {
                            Log.Debug("{0}: deserializing payload.", _logId);

                            RunInTransaction(() =>
                            {
                                Log.Debug("{0}: in transaction.", _logId);

                            });

                            // don't dispose.
                            Log.Debug("{0}: writing response.", _logId);
                            writer.WriteLine("HTTP/1.0 201 Created");
                            writer.WriteLine("X-NinjaSync-Version: 1.0");
                            writer.WriteLine();
                            writer.WriteLine();
                            writer.Flush();


                            // stop listening after put.
                            // note: if we ever continue listening, we have to release the account here. 
                            return;
                        }
                        else if (Regex.IsMatch(line, "^GET /ninjasync HTTP/1.[01]$"))
                        {
                            Log.Debug("{0}: get: {1}", _logId, line);

                            object status = line == "" ? null : line;

                            if (status == null)
                            {
                                Log.Debug("not local account for remote StorageId {0}", 2);
                            }
                            else
                            {
                                if (line == "throw")
                                {
                                    throw new InvalidOperationException("reverse sync already in progress");
                                }
                            }


                            Log.Debug("{0}: writing response.", _logId);
                            writer.WriteLine("HTTP/1.0 200 OK");
                            writer.WriteLine("X-NinjaSync-Version: 1.0");
                            writer.WriteLine("Content-Type: application/json; charset=UTF-8");
                            writer.WriteLine();
                            writer.Flush();
                        }
                        else
                            throw new Exception("Invalid request: " + line);

                        ++numRequestsHandled;
                        Log.Debug("{0}: done.", _logId);
                    }
                    catch (CommitNotFoundException ex)
                    {
                        writer.WriteLine("HTTP/1.0 412 Precondition Failed");
                        writer.WriteLine();
                        writer.Flush();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Log.Debug("operation cancelled");
            }
            catch (Exception ex)
            {
                ReportError(writer, ex);
            }
            finally
            {
                AssertEquals(0, finallyCalled);
                ++finallyCalled ;
                // should not throw an exception, but better be on the save side.
                try
                {
                    stream.Dispose();
                }
                catch (Exception)
                {
                }

                if (accountSemaphore != null)
                    accountSemaphore.Release();

                lock (_serializer) ActiveRequests -= 1;
            }

            AssertEquals(1, finallyCalled);
        }


        private void ReportError(StreamWriter writer, Exception exception)
        {
        }

    }
}

