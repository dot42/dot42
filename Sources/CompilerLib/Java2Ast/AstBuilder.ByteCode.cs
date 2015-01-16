using System.Collections.Generic;
using System.Text;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.XModel;
using Dot42.JvmClassLib.Bytecode;

namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        internal sealed class ByteCode
        {
            public Category Category;
            private AstLabel label;      // Non-null only if needed
            public int Offset;
            public int EndOffset;
            public AstCode Code;
            public object Operand;
            public int? PopCount;   // Null means pop all
            public int PushCount;
            public string Name { get { return "J_" + Offset.ToString("X2"); } }
            public ByteCode Next;
            public StackSlot[] StackBefore;     // Unique per bytecode; not shared
            public VariableSlot[] VariablesBefore; // Unique per bytecode; not shared
            public List<AstVariable> StoreTo;         // Store result of instruction to those AST variables
            public SourceLocation SourceLocation; // Source code reference
            public XTypeReference Type;

            public bool IsVariableDefinition
            {
                get { return (Code == AstCode.Stloc); }
            }

            public AstLabel Label(bool create)
            {
                if ((label != null) || !create)
                    return label;
                return (label = new AstLabel(SourceLocation, Name)); 
            }

            /// <summary>
            /// Does this bytecode contain the given offset?
            /// </summary>
            public bool ContainsOffset(int value)
            {
                return (value >= Offset) && (value < EndOffset);
            }

            /// <summary>
            /// Create the stack after this bytecode has executed, based on StackBefore.
            /// </summary>
            public StackSlot[] CreateNewStack()
            {
                switch (Code)
                {
                    case AstCode.Dup_x1:
                        return StackSlot.ModifyStackDupX1(StackBefore);
                    case AstCode.Dup_x2:
                        return StackSlot.ModifyStackDupX2(StackBefore);
                    case AstCode.Dup2:
                        return StackSlot.ModifyStackDup2(StackBefore);
                    case AstCode.Dup2_x1:
                        return StackSlot.ModifyStackDup2X1(StackBefore);
                    case AstCode.Dup2_x2:
                        return StackSlot.ModifyStackDup2X2(StackBefore);
                    case AstCode.Swap:
                        return StackSlot.ModifyStackSwap(StackBefore);
                    case AstCode.Pop2:
                        return StackSlot.ModifyStackPop2(StackBefore);
                    default:
                        return StackSlot.ModifyStack(StackBefore, PopCount ?? StackBefore.Length, PushCount, this);
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                // Label
                sb.Append(this.Name);
                sb.Append(':');
                if (this.label != null)
                    sb.Append('*');

                // Name
                sb.Append(' ');
                sb.Append(this.Code.GetName());

                if (this.Operand != null)
                {
                    sb.Append(' ');
                    if (this.Operand is Instruction)
                    {
                        sb.Append("J_" + ((Instruction)this.Operand).Offset.ToString("X2"));
                    }
                    else if (this.Operand is Instruction[])
                    {
                        foreach (Instruction inst in (Instruction[])this.Operand)
                        {
                            sb.Append("J_" + inst.Offset.ToString("X2"));
                            sb.Append(" ");
                        }
                    }
                    else if (this.Operand is AstLabel)
                    {
                        sb.Append(((AstLabel)this.Operand).Name);
                    }
                    else if (this.Operand is AstLabel[])
                    {
                        foreach (var label in (AstLabel[])this.Operand)
                        {
                            sb.Append(label.Name);
                            sb.Append(" ");
                        }
                    }
                    else
                    {
                        sb.Append(this.Operand.ToString());
                    }
                }

                if (this.StackBefore != null)
                {
                    sb.Append(" StackBefore={");
                    bool first = true;
                    foreach (StackSlot slot in this.StackBefore)
                    {
                        if (!first) sb.Append(",");
                        bool first2 = true;
                        foreach (ByteCode defs in slot.Definitions)
                        {
                            if (!first2) sb.Append("|");
                            sb.AppendFormat("J_{0:X2}", defs.Offset);
                            first2 = false;
                        }
                        first = false;
                    }
                    sb.Append("}");
                }

                if (this.StoreTo != null && this.StoreTo.Count > 0)
                {
                    sb.Append(" StoreTo={");
                    bool first = true;
                    foreach (AstVariable stackVar in this.StoreTo)
                    {
                        if (!first) sb.Append(",");
                        sb.Append(stackVar.Name);
                        first = false;
                    }
                    sb.Append("}");
                }

                if (this.VariablesBefore != null)
                {
                    sb.Append(" VarsBefore={");
                    bool first = true;
                    foreach (VariableSlot varSlot in this.VariablesBefore)
                    {
                        if (!first) sb.Append(",");
                        if (varSlot.UnknownDefinition)
                        {
                            sb.Append("?");
                        }
                        else
                        {
                            bool first2 = true;
                            foreach (ByteCode storedBy in varSlot.Definitions)
                            {
                                if (!first2) sb.Append("|");
                                sb.AppendFormat("IL_{0:X2}", storedBy.Offset);
                                first2 = false;
                            }
                        }
                        first = false;
                    }
                    sb.Append("}");
                }

                return sb.ToString();
            }
        }
    }
}
