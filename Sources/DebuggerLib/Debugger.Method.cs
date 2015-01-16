using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP Method command set.
    /// </summary>
    partial class Debugger
    {
        public readonly MethodCommandSet  Method;

        public class MethodCommandSet : CommandSet
        {
            internal MethodCommandSet(Debugger debugger)
                : base(debugger, 6)
            {
            }

            /// <summary>
            /// Returns variable information for the method, including generic signatures for the variables. The variable table includes arguments and 
            /// locals declared within the method. For instance methods, the "this" reference is included in the table. Also, synthetic variables may be 
            /// present. Generic signatures are described in the signature attribute section in the Java Virtual Machine Specification, 3rd Edition. 
            /// Since JDWP version 1.5.
            /// </summary>
            public Task<List<VariableInfo>> VariableTableWithGenericAsync(ReferenceTypeId typeId, MethodId methodId)
            {
                var conn = ConnectionOrError;
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 5, typeId.Size + methodId.Size, 
                    x => {
                        var data = x.Data;
                        typeId.WriteTo(data);
                        methodId.WriteTo(data);
                    }));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var argCnt = data.GetInt();
                    var count = data.GetInt();
                    var list = new List<VariableInfo>(count);
                    for (var i = 0; i < count; i++ )
                    {
                        var codeIndex = data.GetLong();
                        var name = data.GetString();
                        var signature = data.GetString();
                        var genericSignature = data.GetString();
                        var length = data.GetInt();
                        var slot = data.GetInt();
                        list.Add(new VariableInfo(codeIndex, name, signature, genericSignature, length, slot));
                    }
                    return list;
                });
            }
        }
    }
}
