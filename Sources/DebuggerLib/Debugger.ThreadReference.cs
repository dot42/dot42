using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP Thread Reference command set.
    /// </summary>
    partial class Debugger
    {
        public readonly ThreadReferenceCommandSet ThreadReference;

        public class ThreadReferenceCommandSet : CommandSet
        {
            internal ThreadReferenceCommandSet(Debugger debugger)
                : base(debugger, 11)
            {
            }

            /// <summary>
            /// Gets the name of a thread.
            /// </summary>
            public Task<string> NameAsync(ThreadId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, sizeInfo.ObjectIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return result.Data.GetString();
                });
            }

            /// <summary>
            /// Suspend the thread with given id.
            /// </summary>
            public Task SuspendAsync(ThreadId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 2, sizeInfo.ObjectIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                });
            }

            /// <summary>
            /// Resume the thread with given id.
            /// </summary>
            public Task ResumeAsync(ThreadId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 3, sizeInfo.ObjectIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                });
            }

            /// <summary>
            /// Gets the status of a thread.
            /// </summary>
            public Task<ThreadStatusInfo> StatusAsync(ThreadId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 4, sizeInfo.ObjectIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var threadStatus = result.Data.GetInt();
                    var suspendStatus = result.Data.GetInt();
                    return new ThreadStatusInfo((Jdwp.ThreadStatus) threadStatus, (Jdwp.SuspendStatus) suspendStatus);
                });
            }

            /// <summary>
            /// Gets the group of a thread.
            /// </summary>
            public Task<ThreadGroupId> ThreadGroupAsync(ThreadId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 5, sizeInfo.ObjectIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return new ThreadGroupId(result.Data);
                });
            }


            /// <summary>
            /// Gets the current call stack of the thread with given id.
            /// </summary>
            public Task<List<Tuple<FrameId, Location>>> FramesAsync(ThreadId id, int length = -1)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 6, sizeInfo.ObjectIdSize + 8,
                    x => {
                        id.WriteTo(x.Data);
                        x.Data.SetInt(0); // start frame
                        x.Data.SetInt(length); // length -1 == all
                    }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();

                    var count = result.Data.GetInt();
                    var list = new List<Tuple<FrameId, Location>>(count);
                    for (var i = 0; i < count; i++)
                    {
                        var frame = new FrameId(result.Data, sizeInfo);
                        var location = new Location(result.Data);
                        list.Add(Tuple.Create(frame, location));
                    }
                    return list;
                });
            }

            /// <summary>
            /// Get the suspend count for this thread. The suspend count is the number of times the thread has been suspended through the thread-level or 
            /// VM-level suspend commands without a corresponding resume.
            /// </summary>
            public Task<int> SuspendCountAsync(ThreadId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 12, sizeInfo.ObjectIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return result.Data.GetInt();
                });
            }
        }
    }
}
