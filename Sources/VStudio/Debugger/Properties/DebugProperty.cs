using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal abstract class DebugProperty : IDebugProperty2
    {
        private readonly DebugProperty parent;
        private List<DebugProperty> children;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected DebugProperty(DebugProperty parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
        /// </summary>
        /// <param name="dwFields"></param>
        /// <param name="dwRadix"></param>
        /// <returns></returns>
        internal abstract DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix);

        /// <summary>
        /// Does this property have children
        /// </summary>
        protected abstract bool HasChildren { get; }

        /// <summary>
        /// Create all child properties
        /// </summary>
        protected abstract List<DebugProperty> CreateChildren();

        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields, dwRadix);
            return VSConstants.S_OK;
        }

        public virtual int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
        {
            ppEnum = null;
            if (!HasChildren)
                return VSConstants.E_FAIL;

            if (children == null)
            {
                children = CreateChildren();
            }

            ppEnum = new PropertyInfoEnum(children.Select(x => x.ConstructDebugPropertyInfo(dwFields, dwRadix)));
            return VSConstants.S_OK;
        }

        public int GetParent(out IDebugProperty2 ppParent)
        {
            ppParent = parent;
            return VSConstants.S_OK;
        }

        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            ppDerivedMost = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            ppMemory = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetSize(out uint pdwSize)
        {
            pdwSize = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetReference(out IDebugReference2 ppReference)
        {
            ppReference = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            pExtendedInfo = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}
