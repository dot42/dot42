using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP ClassType command set.
    /// </summary>
    partial class Debugger
    {
        public readonly ClassReferenceCommandSet ClassReference;

        public class ClassReferenceCommandSet : CommandSet
        {
            internal ClassReferenceCommandSet(Debugger debugger)
                : base(debugger, 17)
            {
            }

            /// <summary>
            /// Returns the reference type reflected by this class object. 
            /// </summary>
            public Task<ReferenceTypeId> ReflectedTypeAsync(ObjectId classObjectId)
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, classObjectId.Size, x => classObjectId.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();

                    var kind = (Jdwp.TypeTag) x.Result.Data.GetByte();
                    if(kind == Jdwp.TypeTag.Array)
                        return (ReferenceTypeId)new ArrayTypeId(result.Data);
                    if(kind == Jdwp.TypeTag.Interface) 
                        return new InterfaceId(result.Data);
                    return new ClassId(result.Data);

                });
            }
        }
    }
}
