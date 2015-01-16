using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP ArrayReference command set.
    /// </summary>
    partial class Debugger
    {
        public readonly ArrayReferenceCommandSet ArrayReference;

        public class ArrayReferenceCommandSet : CommandSet
        {
            internal ArrayReferenceCommandSet(Debugger debugger)
                : base(debugger, 13)
            {
            }

            /// <summary>
            /// Returns the number of components in a given array.
            /// </summary>
            public Task<int> LengthAsync(ObjectId objectId)
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
                    return result.Data.GetInt();
                });
            }

            /// <summary>
            /// Returns a range of array components. The specified range must be within the bounds of the array.
            /// </summary>
            public Task<List<Value>> GetValuesAsync(ObjectId objectId, string elementSignature, int firstIndex, int length)
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 2, objectId.Size + 8, 
                    x => {
                        var data = x.Data;
                        objectId.WriteTo(data);
                        data.SetInt(firstIndex);
                        data.SetInt(length);
                    }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var arrayElementTag = (Jdwp.Tag)data.GetByte();
                    var arrayElementIsObject = !arrayElementTag.IsPrimitive();
                    var count = data.GetInt();
                    var list = new List<Value>(count);
                    for (var i = 0; i < count; i++)
                    {
                        var value = arrayElementIsObject ? new Value(data) : new Value(data, arrayElementTag);
                        list.Add(value);
                    }
                    return list;
                });
            }
        }
    }
}
