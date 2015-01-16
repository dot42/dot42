using System.Threading.Tasks;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP DDMS command set.
    /// </summary>
    partial class Debugger
    {
        public readonly DdmsCommandSet Ddms;

        public class DdmsCommandSet : CommandSet
        {
            internal const int ServerProtocolVersion = 1;
            internal static readonly int HeloType = Chunk.GetType("HELO");
            internal static readonly int ApnmType = Chunk.GetType("APNM");
            internal static readonly int WaitType = Chunk.GetType("WAIT");
            internal static readonly int ThenType = Chunk.GetType("THEN");
            internal static readonly int ThstType = Chunk.GetType("THST");

            internal DdmsCommandSet(Debugger debugger)
                : base(debugger, 0xC7)
            {
            }

            /// <summary>
            /// Enable/disable sending of thread creation notifications.
            /// </summary>
            public Task SetThreadNotificationsAsync(bool enable)
            {
                var conn = ConnectionOrError;
                return conn.SendAsync(Chunk.CreateChunk(conn, ThenType, 1, x => x.Data.SetBoolean(enable)));
            }

            /// <summary>
            /// Trigger thread status.
            /// </summary>
            public void SendThreadStatus()
            {
                var conn = ConnectionOrError;
                conn.AddToWriteQueue(Chunk.CreateChunk(conn, ThstType, 0));
            }
        }
    }
}
