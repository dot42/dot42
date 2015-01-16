using System.Collections.Generic;
using System.Diagnostics;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    partial class DebugInfoBuilder
    {
        /// <summary>
        /// Start of new line entry in the debug information.
        /// </summary>
        [DebuggerDisplay("{Offset}, {line}, {url}")]
        public sealed class PositionEntry : Entry
        {
            private readonly int line;
            private readonly string url;

            /// <summary>
            /// Default ctor
            /// </summary>
            public PositionEntry(int offset, int line, string url) : base(offset)
            {
                this.line = line;
                this.url = url;
            }

            /// <summary>
            /// Sort priority (higher values come first)
            /// </summary>
            public override int Priority
            {
                get { return 1; }
            }

            /// <summary>
            /// Generate debug info for this entry.
            /// </summary>
            public override void Generate(DebugInfo info, ref int lastLine, ref int lastAddress, ref string lastUrl, ref bool firstPositionEntry, HashSet<Register> startedVariables)
            {
                var documentChanged = false;
                if (!string.IsNullOrEmpty(url) && (url != lastUrl))
                {
                    lastUrl = url;
                    info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.SetFile, lastUrl));
                    documentChanged = true;
                }

                var lineAdv = line - lastLine;
                var offsetAdv = Offset - lastAddress;

                if ((lineAdv != 0) || documentChanged || firstPositionEntry)
                {
                    // Set position entry
                    DebugOpCodes opcode;
                    if (!TryCalculateSpecialOpcode(lineAdv, offsetAdv, out opcode))
                    {
                        // The line and/or offset advance does not fit in the special opcode, use ADVANCE opcodes
                        if (lineAdv != 0)
                            info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.AdvanceLine, lineAdv));
                        if (offsetAdv != 0)
                            info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.AdvancePc, offsetAdv));
                        TryCalculateSpecialOpcode(0, 0, out opcode);
                    }
                    info.DebugInstructions.Add(new DebugInstruction(opcode));

                    lastLine = line;
                    lastAddress = Offset;
                    firstPositionEntry = false;
                }
            }
        }
    }
}
