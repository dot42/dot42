using System.Collections.Generic;
using System.Threading.Tasks;
using Dot42.DexLib;
using Dot42.Mapping;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain method information
    /// </summary>
    public class DalvikMethod
    {
        public readonly MethodId Id;
        private readonly DalvikReferenceType declaringType;
        private readonly MethodInfo info;
        private List<VariableInfo> variableTable;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DalvikMethod(DalvikReferenceType declaringType, MethodInfo info)
        {
            Id = info.Id;
            this.declaringType = declaringType;
            this.info = info;
        }

        /// <summary>
        /// Gets the method name
        /// </summary>
        public string Name { get { return info.Name; } }

        /// <summary>
        /// Gets the method signature
        /// </summary>
        public string Signature { get { return info.Signature; } }

        /// <summary>
        /// Gets the method access flags.
        /// </summary>
        public AccessFlags AccessFlags { get { return (AccessFlags) info.AccessFlags; } }

        /// <summary>
        /// Is this a native method?
        /// </summary>
        public bool IsNative { get { return (AccessFlags.HasFlag(AccessFlags.Native)); } }

        public DalvikReferenceType DeclaringType { get { return declaringType; } }

        /// <summary>
        /// Gets the list of local variables and arguments
        /// </summary>
        public Task<List<VariableInfo>> GetVariableTableAsync()
        {
            if (variableTable != null) return variableTable.AsTask();
            if (IsNative)
            {
                variableTable = new List<VariableInfo>();
                return variableTable.AsTask();
            }
            return declaringType.Debugger.Method.VariableTableWithGenericAsync(declaringType.Id, Id).SaveAndReturn(x => variableTable = x);
        }

        /// <summary>
        /// Does this method method the name and signature in the given method entry?
        /// </summary>
        public bool IsMatch(MethodEntry entry)
        {
            return (Name == entry.DexName) && (Signature == entry.DexSignature);
        }
    }
}
