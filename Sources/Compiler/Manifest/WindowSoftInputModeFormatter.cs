using System;

namespace Dot42.Compiler.Manifest
{
    internal class WindowSoftInputModeFormatter : EnumFormatter
    {
        private readonly EnumFormatter stateOptions;
        private readonly EnumFormatter adjustOptions;

        public WindowSoftInputModeFormatter()
        {
            stateOptions = new EnumFormatter(
                Tuple.Create(0x0001, "stateUnspecified"),
                Tuple.Create(0x0002, "stateUnchanged"),
                Tuple.Create(0x0003, "stateHidden"),
                Tuple.Create(0x0004, "stateAlwaysHidden"),
                Tuple.Create(0x0005, "stateVisible"),
                Tuple.Create(0x0006, "stateAlwaysVisible")
                );
            adjustOptions = new EnumFormatter(
                Tuple.Create(0x0100, "adjustUnspecified"),
                Tuple.Create(0x0200, "adjustResize"),
                Tuple.Create(0x0300, "adjustPan")
                );
        }

        public override string Format(int value)
        {
            var stateValue = value & 0x00FF;
            var adjustValue = value & 0xFF00;
            var state = (stateValue != 0) ? stateOptions.Format(stateValue) : null;
            var adjust = (adjustValue != 0) ? adjustOptions.Format(adjustValue) : null;

            if (!string.IsNullOrEmpty(state) || !string.IsNullOrEmpty(adjust))
                return state + "|" + adjust;
            if (!string.IsNullOrEmpty(state))
                return state;
            if (!string.IsNullOrEmpty(adjust))
                return adjust;
            return string.Empty;
        }
    }
}
