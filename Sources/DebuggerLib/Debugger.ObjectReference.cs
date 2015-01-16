using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP ObjectReference command set.
    /// </summary>
    partial class Debugger
    {
        public readonly ObjectReferenceCommandSet ObjectReference;

        public class ObjectReferenceCommandSet : CommandSet
        {
            internal ObjectReferenceCommandSet(Debugger debugger)
                : base(debugger, 9)
            {
            }

            /// <summary>
            /// Returns the runtime type of the object. The runtime type will be a class or an array.
            /// </summary>
            public Task<ReferenceTypeId> ReferenceTypeAsync(ObjectId objectId)
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
                    return ReferenceTypeId.Read(result.Data);
                });
            }

            /// <summary>
            /// Returns the value of one or more instance fields. Each field must be member of the object's type or one of its superclasses, 
            /// superinterfaces, or implemented interfaces. Access control is not enforced; for example, the values of private fields can be obtained.
            /// </summary>
            public Task<List<Value>> GetValuesAsync(ObjectId objectId, FieldId[] fields)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 2, objectId.Size + 4 + (sizeInfo.FieldIdSize * fields.Length),
                    x => {
                        var data = x.Data;
                        objectId.WriteTo(data);
                        data.SetInt(fields.Length);
                        foreach (var fieldId in fields)
                        {
                            fieldId.WriteTo(data);
                        }
                    }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var count = data.GetInt();
                    var list = new List<Value>(count);
                    for (var i = 0; i < count; i++)
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
