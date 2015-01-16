// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Text;
using Dot42.CompilerLib.Ast;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.IL2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        internal sealed class ByteCode
        {
            public AstLabel Label;      // Non-null only if needed
            public int Offset;
            public int EndOffset;
            public AstCode Code;
            public object Operand;
            public int? PopCount;   // Null means pop all
            public int PushCount;
            public string Name { get { return "IL_" + this.Offset.ToString("X2"); } }
            public ByteCode Next;
            public Instruction[] Prefixes;        // Non-null only if needed
            public StackSlot[] StackBefore;     // Unique per bytecode; not shared
            public VariableSlot[] VariablesBefore; // Unique per bytecode; not shared
            public List<AstVariable> StoreTo;         // Store result of instruction to those AST variables
            public ISourceLocation SequencePoint; // Source code reference

            public bool IsVariableDefinition
            {
                get
                {
                    return (Code == AstCode.Stloc) || ((Code == AstCode.Ldloca) && (Next != null) && (Next.Code == AstCode.Initobj));
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                // Label
                sb.Append(this.Name);
                sb.Append(':');
                if (this.Label != null)
                    sb.Append('*');

                // Name
                sb.Append(' ');
                if (this.Prefixes != null)
                {
                    foreach (var prefix in this.Prefixes)
                    {
                        sb.Append(prefix.OpCode.Name);
                        sb.Append(' ');
                    }
                }
                sb.Append(this.Code.GetName());

                if (this.Operand != null)
                {
                    sb.Append(' ');
                    if (this.Operand is Instruction)
                    {
                        sb.Append("IL_" + ((Instruction)this.Operand).Offset.ToString("X2"));
                    }
                    else if (this.Operand is Instruction[])
                    {
                        foreach (Instruction inst in (Instruction[])this.Operand)
                        {
                            sb.Append("IL_" + inst.Offset.ToString("X2"));
                            sb.Append(" ");
                        }
                    }
                    else if (this.Operand is AstLabel)
                    {
                        sb.Append(((AstLabel)this.Operand).Name);
                    }
                    else if (this.Operand is AstLabel[])
                    {
                        foreach (AstLabel label in (AstLabel[])this.Operand)
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
                            sb.AppendFormat("IL_{0:X2}", defs.Offset);
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
