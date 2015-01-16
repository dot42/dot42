using System;
using System.Threading.Tasks;
using Dot42.DexLib;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain field information
    /// </summary>
    public class DalvikField
    {
        public readonly FieldId Id;
        private readonly DalvikReferenceType declaringType;
        private readonly FieldInfo info;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DalvikField(DalvikReferenceType declaringType, FieldInfo info)
        {
            Id = info.Id;
            this.declaringType = declaringType;
            this.info = info;
        }

        /// <summary>
        /// Gets the field's declaring type.
        /// </summary>
        public DalvikReferenceType DeclaringType { get { return declaringType; } }

        /// <summary>
        /// Gets the field name
        /// </summary>
        public string Name { get { return info.Name; } }

        /// <summary>
        /// Gets the field signature
        /// </summary>
        public string Signature { get { return info.Signature; } }

        /// <summary>
        /// Gets the field access flags.
        /// </summary>
        public AccessFlags AccessFlags { get { return (AccessFlags) info.AccessFlags; } }

        /// <summary>
        /// Is this a static field?
        /// </summary>
        public bool IsStatic { get { return AccessFlags.HasFlag(AccessFlags.Static); } }

        /// <summary>
        /// Gets the value of this field in the given object reference.
        /// If this field is static, the object reference is ignored.
        /// </summary>
        public Task<DalvikFieldValue> GetValueAsync(DalvikObjectReference objectReference)
        {
            var process = declaringType.Manager.Process;
            if (IsStatic)
            {
                // Get static field value
                return process.Debugger.ReferenceType.GetValuesAsync(declaringType.Id, new[] { Id }).Select(x => new DalvikFieldValue(x[0], this, process));
            }
            
            // Get instance field value
            if (objectReference == null)
                throw new ArgumentNullException("objectReference");
            return process.Debugger.ObjectReference.GetValuesAsync(objectReference.Id, new[] { Id }).Select(x => new DalvikFieldValue(x[0], this, process));
        }
    }
}
