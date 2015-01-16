using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP Virtual Machine command set.
    /// </summary>
    partial class Debugger
    {
        public readonly VirtualMachineCommandSet VirtualMachine;

        public class VirtualMachineCommandSet : CommandSet
        {
            internal VirtualMachineCommandSet(Debugger debugger)
                : base(debugger, 1)
            {
            }

            /// <summary>
            /// Returns reference types that match the given signature.
            /// </summary>
            public Task<List<ClassInfo>> ClassBySignatureAsync(string signature)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 2, JdwpPacket.DataReaderWriter.GetStringSize(signature), x => x.Data.SetString(signature)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var count = data.GetInt();
                    var list = new List<ClassInfo>();
                    while (count > 0)
                    {
                        count--;
                        var typeId = ReferenceTypeId.Read(data);
                        var status = data.GetInt();
                        list.Add(new ClassInfo(typeId, signature, signature, (Jdwp.ClassStatus) status));
                    }
                    return list;
                });
            }

            /// <summary>
            /// Returns reference types for all classes currently loaded by the target VM.
            /// </summary>
            public Task<List<ClassInfo>> AllClassesWithGenericAsync()
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 20, 0));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var count = data.GetInt();
                    var list = new List<ClassInfo>();
                    while (count > 0)
                    {
                        count--;
                        var typeId = ReferenceTypeId.Read(data);
                        var signature = data.GetString();
                        var genericSignature = data.GetString();
                        var status = data.GetInt();
                        list.Add(new ClassInfo(typeId, signature, genericSignature, (Jdwp.ClassStatus)status));
                    }
                    return list;
                });
            }

            /// <summary>
            /// Gets all threads in the VM.
            /// </summary>
            public Task<List<ThreadId>> AllThreadsAsync()
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 4, 0));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var count = result.Data.GetInt();
                    var list = new List<ThreadId>();
                    while (count > 0)
                    {
                        count--;
                        list.Add(new ThreadId(result.Data));
                    }
                    return list;
                });
            }

            /// <summary>
            /// Suspend the execution of the application running in the target VM.
            /// </summary>
            public Task SuspendAsync()
            {
                var conn = ConnectionOrError;
                return conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 8, 0));
            }

            /// <summary>
            /// Suspend the execution of the application running in the target VM.
            /// </summary>
            public Task ResumeAsync()
            {
                var conn = ConnectionOrError;
                return conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 9, 0));
            }

            /// <summary>
            /// Request exit.
            /// </summary>
            public void Exit(int exitCode)
            {
                // Send exit command
                var conn = ConnectionOrError;
                conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 10, 4, x => x.Data.SetInt(exitCode)));

                // Wait until write queue is empty
                conn.WaitUntilWriteQueueEmpty();

                // Disconnect
                Debugger.Disconnect();
            }
        }
    }
}
