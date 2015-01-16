using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP ClassType command set.
    /// </summary>
    partial class Debugger
    {
        public readonly ClassTypeCommandSet ClassType;

        public class ClassTypeCommandSet : CommandSet
        {
            internal ClassTypeCommandSet(Debugger debugger)
                : base(debugger, 3)
            {
            }

            /// <summary>
            /// Returns the immediate superclass of a class.
            /// </summary>
            public Task<ClassId> SuperclassAsync(ReferenceTypeId id)
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, id.Size, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return new ClassId(result.Data);
                });
            }
        }
    }
}
