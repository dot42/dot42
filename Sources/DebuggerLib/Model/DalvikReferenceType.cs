using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.JvmClassLib;
using Dot42.Mapping;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain class information
    /// </summary>
    public class DalvikReferenceType
    {
        public readonly ReferenceTypeId Id;
        private readonly DalvikReferenceTypeManager manager;
        private Jdwp.ClassStatus? status;
        private string signature;
        private DalvikReferenceType superClass;
        private DalvikMemberList<FieldId, DalvikField> fields;
        private DalvikMemberList<MethodId, DalvikMethod> methods;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DalvikReferenceType(ReferenceTypeId id, DalvikReferenceTypeManager manager)
        {
            Id = id;
            this.manager = manager;
        }

        /// <summary>
        /// Gets the status of the class.
        /// Load's it if needed.
        /// </summary>
        public Task<Jdwp.ClassStatus> GetStatusAsync()
        {
            return status.HasValue ? status.Value.AsTask() : RefreshStatusAsync();
        }

        /// <summary>
        /// Sets the status of the class.
        /// </summary>
        internal void SetStatusIfNull(Jdwp.ClassStatus value)
        {
            if (!status.HasValue)
                status = value;
        }

        /// <summary>
        /// Force the status to be reloaded.
        /// </summary>
        public Task<Jdwp.ClassStatus> RefreshStatusAsync()
        {
            return Debugger.ReferenceType.StatusAsync(Id).SaveAndReturn(x => status = x);
        }

        /// <summary>
        /// Gets the signature of the class.
        /// Load's it if needed.
        /// </summary>
        public Task<string> GetSignatureAsync()
        {
            return (signature != null) ? signature.AsTask() : RefreshSignatureAsync();
        }

        /// <summary>
        /// Set the signature of this class.
        /// </summary>
        internal void SetSignatureIfNull(string value)
        {
            if (signature == null)
                signature = value;
        }

        /// <summary>
        /// Gets the currently known signature (can be null).
        /// </summary>
        internal string Signature { get { return signature; } }

        /// <summary>
        /// Gets the original class name.
        /// </summary>
        public Task<string> GetNameAsync()
        {
            return GetSignatureAsync().Select(manager.Process.SignatureToClrName);
        }

        /// <summary>
        /// Force a reload of the signature.
        /// </summary>
        protected Task<string> RefreshSignatureAsync()
        {
            return Debugger.ReferenceType.SignatureAsync(Id).SaveAndReturn(x => signature = x);
        }

        /// <summary>
        /// Gets the superclass of this type.
        /// </summary>
        /// <returns>Null if this type is java/lang/Object.</returns>
        public Task<DalvikReferenceType> GetSuperClassAsync()
        {
            if (superClass != null) return superClass.AsTask();
            return Debugger.ClassType.SuperclassAsync(Id).Select(x => x.IsNull ? null : Manager[x]).SaveAndReturn(x => superClass = x);
        }

        /// <summary>
        /// Load the fields of the class.
        /// </summary>
        public Task<DalvikMemberList<FieldId, DalvikField>> GetFieldsAsync()
        {
            if (fields != null) return fields.AsTask();
            return Debugger.ReferenceType.FieldsAsync(Id).Select(t => new DalvikMemberList<FieldId, DalvikField>(t.Select(CreateField), x => x.Id)).SaveAndReturn(x => fields = x);
        }

        /// <summary>
        /// Load the value of all instance fields of this class
        /// </summary>
        public Task<List<DalvikFieldValue>> GetInstanceFieldValuesAsync(DalvikObjectReference objectReference)
        {
            return GetFieldsAsync().Select(list => {
                var process = manager.Process;
                var instanceFields = list.Where(x => !x.IsStatic).ToList();
                var instanceFieldIds = instanceFields.Select(x => x.Id).ToArray();
                var values = Debugger.ObjectReference.GetValuesAsync(objectReference.Id, instanceFieldIds).Await(DalvikProcess.VmTimeout);
                var result = new List<DalvikFieldValue>();
                for (var i = 0; i < instanceFields.Count; i++)
                {
                    result.Add(new DalvikFieldValue(values[i], instanceFields[i], process));
                }
                return result;
            });
        }

        /// <summary>
        /// Load the methods of the class.
        /// </summary>
        public Task<DalvikMemberList<MethodId, DalvikMethod>> GetMethodsAsync()
        {
            if (methods != null) return methods.AsTask();
            return Debugger.ReferenceType.MethodsAsync(Id).Select(t => new DalvikMemberList<MethodId, DalvikMethod>(t.Select(CreateMethod), x => x.Id)).SaveAndReturn(x => methods = x);
        }

        /// <summary>
        /// Create a field instance.
        /// </summary>
        protected virtual DalvikField CreateField(FieldInfo info)
        {
            return new DalvikField(this, info);
        }

        /// <summary>
        /// Create a method instance.
        /// </summary>
        protected virtual DalvikMethod CreateMethod(MethodInfo info)
        {
            return new DalvikMethod(this, info);
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            return GetSignatureAsync().Await(DalvikProcess.VmTimeout);
        }

        /// <summary>
        /// Provide access to the containing manager.
        /// </summary>
        protected internal DalvikReferenceTypeManager Manager { get { return manager; } }

        /// <summary>
        /// Provide access to the low level debugger.
        /// </summary>
        protected internal Debugger Debugger { get { return manager.Debugger; } }
    }
}
