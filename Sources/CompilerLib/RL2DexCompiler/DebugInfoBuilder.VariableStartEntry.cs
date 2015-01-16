using System.Collections.Generic;
using System.Diagnostics;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    partial class DebugInfoBuilder
    {
        /// <summary>
        /// Start of local variable entry in the debug information.
        /// </summary>
        [DebuggerDisplay("{Offset} {register} {name}")]
        public sealed class VariableStartEntry : Entry
        {
            private readonly Register register;
            public readonly IVariable Variable;
            private readonly string name;
            private readonly TypeReference type;

            /// <summary>
            /// Default ctor
            /// </summary>
            public VariableStartEntry(int offset, Register register, IVariable variable, TypeReference type) : base(offset)
            {
                this.register = register;
                Variable = variable;
                name = variable.OriginalName;
                this.type = type;
            }

            /// <summary>
            /// Sort priority (higher values come first)
            /// </summary>
            public override int Priority
            {
                get { return 100; }
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

                if (startedVariables.Contains(register))
                {
                    info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.RestartLocal, register));
                }
                else
                {
                    info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.StartLocal, register, name, type));
                    startedVariables.Add(register);
                }
            }
        }
    }
}
