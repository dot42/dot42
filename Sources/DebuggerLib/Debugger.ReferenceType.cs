using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP Thread Reference command set.
    /// </summary>
    partial class Debugger
    {
        public readonly ReferenceTypeCommandSet ReferenceType;

        public class ReferenceTypeCommandSet : CommandSet
        {
            internal ReferenceTypeCommandSet(Debugger debugger)
                : base(debugger, 2)
            {
            }

            /// <summary>
            /// Returns the JNI signature of a reference type. JNI signature formats are described in the Java Native Inteface Specification
            /// For primitive classes the returned signature is the signature of the corresponding primitive type; for example, "I" is returned 
            /// as the signature of the class represented by java.lang.Integer.TYPE.
            /// </summary>
            public Task<string> SignatureAsync(ReferenceTypeId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 1, sizeInfo.ReferenceTypeIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return result.Data.GetString();
                });
            }

            /// <summary>
            /// Returns information for each field in a reference type. Inherited fields are not included. The field list will include any synthetic 
            /// fields created by the compiler. Fields are returned in the order they occur in the class file.
            /// </summary>
            public Task<List<FieldInfo>> FieldsAsync(ReferenceTypeId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 14, sizeInfo.ReferenceTypeIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var count = data.GetInt();
                    var list = new List<FieldInfo>(count);
                    for (var i = 0; i < count; i++ )
                    {
                        var fieldId = new FieldId(data);
                        var name = data.GetString();
                        var signature = data.GetString();
                        var genericSignature = data.GetString();
                        var accessFlags = data.GetInt();
                        list.Add(new FieldInfo(fieldId, name, signature, genericSignature, accessFlags));
                    }
                    return list;
                });
            }

            /// <summary>
            /// Returns information for each method in a reference type. Inherited methods are not included. The list of methods will include constructors 
            /// (identified with the name "&lt;init&gt;"), the initialization method (identified with the name "&lt;clinit&gt;") if present, and any synthetic methods 
            /// created by the compiler. Methods are returned in the order they occur in the class file.
            /// </summary>
            public Task<List<MethodInfo>> MethodsAsync(ReferenceTypeId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 15, sizeInfo.ReferenceTypeIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    var data = result.Data;
                    var count = data.GetInt();
                    var list = new List<MethodInfo>(count);
                    for (var i = 0; i < count; i++)
                    {
                        var methodId = new MethodId(data);
                        var name = data.GetString();
                        var signature = data.GetString();
                        var genericSignature = data.GetString();
                        var accessFlags = data.GetInt();
                        list.Add(new MethodInfo(methodId, name, signature, genericSignature, accessFlags));
                    }
                    return list;
                });
            }

            /// <summary>
            /// Returns the value of one or more static fields of the reference type. Each field must be member of the reference type or 
            /// one of its superclasses, superinterfaces, or implemented interfaces. Access control is not enforced; for example, the values 
            /// of private fields can be obtained.
            /// </summary>
            public Task<List<Value>> GetValuesAsync(ReferenceTypeId typeId, FieldId[] fields)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 6, typeId.Size + 4 + (sizeInfo.FieldIdSize * fields.Length),
                    x => {
                        var data = x.Data;
                        typeId.WriteTo(data);
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


            /// <summary>
            /// Returns the current status of the reference type. The status indicates the extent to which the reference type has been initialized, 
            /// as described in the VM specification. If the class is linked the PREPARED and VERIFIED bits in the returned status bits will be set. 
            /// If the class is initialized the INITIALIZED bit in the returned status bits will be set. If an error occured during initialization then 
            /// the ERROR bit in the returned status bits will be set. The returned status bits are undefined for array types and for primitive classes 
            /// (such as java.lang.Integer.TYPE).
            /// </summary>
            public Task<Jdwp.ClassStatus> StatusAsync(ReferenceTypeId id)
            {
                var conn = ConnectionOrError;
                var sizeInfo = conn.GetIdSizeInfo();
                var t = conn.SendAsync(JdwpPacket.CreateCommand(conn, Nr, 9, sizeInfo.ReferenceTypeIdSize, x => id.WriteTo(x.Data)));
                return t.ContinueWith(x => {
                    x.ForwardException();
                    var result = x.Result;
                    result.ThrowOnError();
                    return (Jdwp.ClassStatus)result.Data.GetInt();
                });
            }
        }
    }
}
