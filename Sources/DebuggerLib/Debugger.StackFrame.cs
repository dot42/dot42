using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP Stackframe command set.
    /// </summary>
    partial class Debugger
    {
        public readonly StackFrameCommandSet StackFrame;

        public class StackFrameCommandSet : CommandSet
        {
            internal StackFrameCommandSet(Debugger debugger)
                : base(debugger, 16)
            {
            }

            /// <summary>
            /// Returns the value of one or more local variables in a given frame. Each variable must be visible at the frame's code index. 
            /// Even if local variable information is not available, values can be retrieved if the front-end is able to determine the correct 
            /// local variable index. (Typically, this index can be determined for method arguments from the method signature without access to 
            /// the local variable table information.)
            /// </summary>
            public Task<List<Value>> GetValuesAsync(ThreadId threadId, FrameId frameId, SlotRequest[] slots)
            {
                var conn = ConnectionOrError;
                DLog.Debug(DContext.DebuggerLibCommands, () => string.Format("StackFrame.GetValues {0}", string.Join(", ", slots.Select(x => x.ToString()))));
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, threadId.Size + frameId.Size + 4 + (slots.Length * 5), 
                    x => {
                        var data = x.Data;
                        threadId.WriteTo(data);
                        frameId.WriteTo(data);
                        data.SetInt(slots.Length);
                        foreach (var slot in slots)
                        {
                            data.SetInt(slot.Slot);
                            data.SetByte((byte) slot.Tag);
                        }
                    }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var count = data.GetInt();
                    var list = new List<Value>(count);
                    for (var i = 0; i < count; i++ )
                    {
                        var value = new Value(data);
                        list.Add(value);
                    }
                    return list;
                });
            }
        }
    }
}
