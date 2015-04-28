using System.Collections.Generic;
using System.Linq;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugRegisterGroupProperty : DebugProperty
    {
        private readonly DebugStackFrame _stackFrame;
        private readonly bool _forceHexDisplay;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugRegisterGroupProperty(DebugStackFrame stackFrame, bool forceHexDisplay) : base(null)
        {
            _stackFrame = stackFrame;
            _forceHexDisplay = forceHexDisplay;
        }

        /// <summary>
        /// Does this property have children
        /// </summary>
        protected override bool HasChildren
        {
            get { return true; }
        }

        /// <summary>
        /// Create all child properties
        /// </summary>
        protected override List<DebugProperty> CreateChildren()
        {
            List<DebugProperty> list =new List<DebugProperty>();

            var registers = _stackFrame.GetRegistersAsync().Await(DalvikProcess.VmTimeout);
            foreach(var value in registers)
                list.Insert(0, new DebugStackFrameValueProperty(value, this, _stackFrame, _forceHexDisplay));
            list.Reverse();
            return list;
        }

        /// <summary>
        /// Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
        /// </summary>
        /// <param name="dwFields"></param>
        /// <param name="dwRadix"></param>
        /// <returns></returns>
        internal override DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix)
        {
            var info = new DEBUG_PROPERTY_INFO();

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME))
            {
                info.bstrFullName = "$regs";
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME))
            {
                info.bstrName = "$regs";
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE))
            {
                info.bstrValue = "(local registers)";
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }
            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB))
            {
                info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE))
            {
                info.bstrType = "VM Registers";
                info.dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP))
            {
                info.pProperty = this;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
            }

            return info;
        }
    }
}
