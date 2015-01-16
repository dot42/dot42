using System.Linq;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP Thread Reference command set.
    /// </summary>
    partial class Debugger
    {
        public readonly EventRequestCommandSet EventRequest;

        public class EventRequestCommandSet : CommandSet
        {
            internal EventRequestCommandSet(Debugger debugger)
                : base(debugger, 15)
            {
            }

            /// <summary>
            /// Set an event request. When the event described by this request occurs, an event is sent from the target VM. 
            /// If an event occurs that has not been requested then it is not sent from the target VM. The two exceptions to this are the 
            /// VM Start Event and the VM Death Event which are automatically generated events - see Composite Command for further details.
            /// </summary>
            /// <returns>Task that returns a requestId</returns>
            public Task<int> SetAsync(Jdwp.EventKind eventKind, Jdwp.SuspendPolicy suspendPolicy, params EventModifier[] modifiers)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var size = 6 + modifiers.Sum(x => x.DataSize);
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, size, x => {
                    var data = x.Data;
                    data.SetByte((byte) eventKind);
                    data.SetByte((byte)suspendPolicy);
                    data.SetInt(modifiers.Length);
                    foreach (var modifier in modifiers)
                    {
                        modifier.WriteTo(data);
                    }
                }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return result.Data.GetInt();
                });
            }

            /// <summary>
            /// Clear an event request. See JDWP.EventKind for a complete list of events that can be cleared. Only the event request matching the 
            /// specified event kind and requestID is cleared. If there isn't a matching event request the command is a no-op and does not result 
            /// in an error. Automatically generated events do not have a corresponding event request and may not be cleared using this command.
            /// </summary>
            public Task ClearAsync(Jdwp.EventKind eventKind, int requestId)
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 2, 5, x => {
                    var data = x.Data;
                    data.SetByte((byte)eventKind);
                    data.SetInt(requestId);
                }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                });
            }

            /// <summary>
            /// Removes all set breakpoints, a no-op if there are no breakpoints set.
            /// </summary>
            public Task ClearAllBreakpointsAsync()
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 3, 0));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                });
            }
        }
    }
}
