using System;
using System.Diagnostics;

namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        /// <summary> Immutable </summary>
        [DebuggerDisplay("{UnknownDefinition} {Definitions}")]
        internal struct VariableSlot
        {
            public readonly ByteCode[] Definitions;       // Reaching deinitions of this variable
            public readonly bool UnknownDefinition; // Used for initial state and exceptional control flow

            static readonly VariableSlot UnknownInstance = new VariableSlot(new ByteCode[0], true);

            public VariableSlot(ByteCode[] definitions, bool unknownDefinition)
            {
                Definitions = definitions;
                UnknownDefinition = unknownDefinition;
            }

            public static VariableSlot[] CloneVariableState(VariableSlot[] state)
            {
                var clone = new VariableSlot[state.Length];
                Array.Copy(state, clone, state.Length);
                return clone;
            }

            public static VariableSlot[] MakeUnknownState(int varCount)
            {
                var unknownVariableState = new VariableSlot[varCount];
                for (int i = 0; i < unknownVariableState.Length; i++)
                {
                    unknownVariableState[i] = UnknownInstance;
                }
                return unknownVariableState;
            }
        }
    }
}
