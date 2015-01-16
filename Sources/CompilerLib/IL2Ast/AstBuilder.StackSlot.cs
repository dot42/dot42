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

using System;
using Dot42.CompilerLib.Ast;

namespace Dot42.CompilerLib.IL2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        /// <summary> Immutable </summary>
        internal struct StackSlot
        {
            public readonly ByteCode[] Definitions;  // Reaching definitions of this stack slot
            public readonly AstVariable LoadFrom;     // Variable used for storage of the value

            public StackSlot(ByteCode[] definitions, AstVariable loadFrom)
            {
                Definitions = definitions;
                LoadFrom = loadFrom;
            }

            public static StackSlot[] ModifyStack(StackSlot[] stack, int popCount, int pushCount, ByteCode pushDefinition)
            {
                var newStack = new StackSlot[stack.Length - popCount + pushCount];
                Array.Copy(stack, newStack, stack.Length - popCount);
                for (int i = stack.Length - popCount; i < newStack.Length; i++)
                {
                    newStack[i] = new StackSlot(new[] { pushDefinition }, null);
                }
                return newStack;
            }
        }
    }
}
