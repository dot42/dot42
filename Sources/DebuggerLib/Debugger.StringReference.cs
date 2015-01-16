using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP StringReference command set.
    /// </summary>
    partial class Debugger
    {
        public readonly StringReferenceCommandSet StringReference;

        public class StringReferenceCommandSet : CommandSet
        {
            internal StringReferenceCommandSet(Debugger debugger)
                : base(debugger, 10)
            {
            }

            /// <summary>
            /// Returns the characters contained in the string.
            /// </summary>
            public Task<string> ValueAsync(ObjectId objectId)
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, objectId.Size, 
                    x => {
                        var data = x.Data;
                        objectId.WriteTo(data);
                    }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return result.Data.GetString();
                });
            }
        }
    }
}
