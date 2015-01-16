using System.Collections.Generic;
using System.Diagnostics;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    partial class DebugInfoBuilder
    {
        /// <summary>
        /// End of local variable entry in the debug information.
        /// </summary>
        [DebuggerDisplay("{Offset} {register}")]
        public sealed class VariableEndEntry : Entry
        {
            private readonly Register register;
            public readonly IVariable Variable;

            /// <summary>
            /// Default ctor
            /// </summary>
            public VariableEndEntry(int offset, Register register, IVariable variable) : base(offset)
            {
                this.register = register;
                Variable = variable;
            }

            /// <summary>
            /// Sort priority (higher values come first)
            /// </summary>
            public override int Priority
            {
                get { return 50; }
            }

            /// <summary>
            /// Generate debug info for this entry.
            /// </summary>
            public override void Generate(DebugInfo info, ref int lastLine, ref int lastAddress, ref string lastUrl, ref bool firstPositionEntry, HashSet<Register> startedVariables)
            {
                var offsetAdv = Offset - lastAddress;
                if (offsetAdv > 0)
                {
                    info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.AdvancePc, offsetAdv));
                    lastAddress = Offset;
                }

                info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.EndLocal, register));
            }
        }
    }
}
