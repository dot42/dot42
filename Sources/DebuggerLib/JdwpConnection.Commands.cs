using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// TCP/IP connection to a debugger.
    /// </summary>
    partial class JdwpConnection 
    {
        /// <summary>
        /// Request version data
        /// </summary>
        public Task<VersionInfo> VersionAsync()
        {
            var t = SendAsync(JdwpPacket.CreateCommand(this, 1, 1, 0));
            return t.ContinueWith(x => {
                x.ForwardException();
                var result = x.Result;
                result.ThrowOnError();
                var description = result.Data.GetString();
                var major = result.Data.GetInt();
                var minor = result.Data.GetInt();
                var vmVersion = result.Data.GetString();
                var vmName = result.Data.GetString();
                return new VersionInfo(description, major, minor, vmVersion, vmName);                
            });
        }

        /// <summary>
        /// Request ID size information
        /// </summary>
        public Task<IdSizeInfo> IdSizesAsync()
        {
            var t = SendAsync(JdwpPacket.CreateCommand(this, 1, 7, 0));
            return t.ContinueWith(x => {
                x.ForwardException();
                var result = x.Result;
                result.ThrowOnError();
                var fieldIdSize = result.Data.GetInt();
                var methodIdSize = result.Data.GetInt();
                var objectIdSize = result.Data.GetInt();
                var referenceTypeIdSize = result.Data.GetInt();
                var frameIdSize = result.Data.GetInt();
                return new IdSizeInfo(fieldIdSize, methodIdSize, objectIdSize, referenceTypeIdSize, frameIdSize);
            });
        }

        /// <summary>
        /// Send a Helo command.
        /// </summary>
        public void SendHelo()
        {
            AddToWriteQueue(Chunk.CreateChunk(this, Debugger.DdmsCommandSet.HeloType, 4, x => x.Data.SetInt(Debugger.DdmsCommandSet.ServerProtocolVersion)));
        }
    }
}
