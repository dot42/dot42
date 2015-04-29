using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Reference to an object instance.
    /// </summary>
    public class DalvikObjectReference
    {
        public readonly ObjectId Id;
        private readonly DalvikProcess process;
        private DalvikReferenceType referenceType;

        /// <summary>
        /// Default ctor
        /// </summary
        public DalvikObjectReference(ObjectId id, DalvikProcess process)
        {
            Id = id;
            this.process = process;
        }

        /// <summary>
        /// Is the a NULL reference?
        /// </summary>
        public bool IsNull { get { return Id.IsNull; } }

        /// <summary>
        /// Gets or load the reference type of this id.
        /// </summary>
        public Task<DalvikReferenceType> GetReferenceTypeAsync()
        {
            if (IsNull || (referenceType != null)) return referenceType.AsTask();
            return process.Debugger.ObjectReference.ReferenceTypeAsync(Id).Select(x => process.ReferenceTypeManager[x]).SaveAndReturn(x => referenceType = x);
        }

        /// <summary>
        /// Gets the length of the array in this object.
        /// </summary>
        public Task<int> GetArrayLengthAsync()
        {
            return process.Debugger.ArrayReference.LengthAsync(Id);
        }

        /// <summary>
        /// Gets the values of an array.
        /// </summary>
        /// <param name="firstIndex">Index of the first element to retrieve</param>
        /// <param name="maxLength">The maximum number of elements to retrieve.</param>
        public Task<List<DalvikArrayElementValue>> GetArrayValuesAsync(int firstIndex, int maxLength)
        {
            return Task.Factory.StartNew(() => {
                // get element type
                var refType = GetReferenceTypeAsync().Await(DalvikProcess.VmTimeout);
                var signature = refType.GetSignatureAsync().Await(DalvikProcess.VmTimeout);
                var elementSignature = signature.Substring(1);
                // Get array length
                var arrayLength = GetArrayLengthAsync().Await(DalvikProcess.VmTimeout);
                var length = Math.Min(arrayLength - firstIndex, maxLength);
                // Get values
                var rawValues = process.Debugger.ArrayReference.GetValuesAsync(Id, elementSignature, firstIndex, length).Await(DalvikProcess.VmTimeout);
                // Convert
                var result = new List<DalvikArrayElementValue>(length);
                for (var i = 0; i < length; i++)
                {
                    result.Add(new DalvikArrayElementValue(rawValues[i], firstIndex + i, process));
                }
                return result;
            });
        }

        /// <summary>
        /// Gets the value of a string (assumes that the object is a string)
        /// </summary>
        public Task<string> GetStringValueAsync()
        {
            return process.Debugger.StringReference.ValueAsync(Id);
        }

        /// <summary>
        /// Gets the name of a class (assumes that the object is a ClassObject)
        /// </summary>
        public Task<string> GetClassObjectNameAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var refId = process.Debugger.ClassReference.ReflectedTypeAsync(Id).Await(DalvikProcess.VmTimeout);
                var refType = process.ReferenceTypeManager[refId];
                return refType.GetNameAsync().Await(DalvikProcess.VmTimeout);
            });
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
            if (IsNull) return "(null)";
            return string.Format("(object {0})", Id);
        }
    }
}
