using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugStackFrameValueProperty : DebugValueProperty
    {
        private readonly DebugStackFrame _stackFrame;
        private readonly bool _forceHex;

        public DebugStackFrameValueProperty(DalvikValue value, DebugProperty parent, DebugStackFrame stackFrame,bool forceHex=false)
            : base(value, parent)
        {
            _stackFrame = stackFrame;
            _forceHex = forceHex;
        }

        internal override DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix)
        {
            var info = base.ConstructDebugPropertyInfo(dwFields, _forceHex ? 16:dwRadix);

            if (info.dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB))
            {
                // allow editing values.
                if(Value.IsPrimitive && Value is DalvikStackFrameValue)
                    info.dwAttrib &= ~enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;
                info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_STORAGE_REGISTER;
            }
            return info;
        }

        public override int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            var stackFrameVal = Value as DalvikStackFrameValue;

            if (stackFrameVal == null || !Value.IsPrimitive )
                return VSConstants.E_FAIL;

            object val = ParsePrimitive(pszValue, Value.tag, (int)dwRadix);

            // is there as simpler way to grab the debugger?
            var debugger = _stackFrame.Thread.Program.Process.Debugger;

            var slotValue = new SlotValue(stackFrameVal.Variable.Slot, Value.tag, val);
            debugger.StackFrame.SetValuesAsync(_stackFrame.Thread.Id, _stackFrame.Id, slotValue)
                               .Await((int)dwTimeout);
            _stackFrame.InvalidateVariablesValueCache();
            return VSConstants.S_OK;
        }
    }
}
