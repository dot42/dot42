using System;
using Dot42.CompilerLib.Ast;

namespace Dot42.CompilerLib.Java2Ast
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

            /// <summary>
            /// Does this slot hold a category2 (long/double) value?
            /// </summary>
            public bool IsCategory2
            {
                get
                {
                    if ((Definitions == null) || (Definitions.Length == 0))
                        return false;
                    return (Definitions[0].Category == Category.Category2);
                }
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

            public static StackSlot[] ModifyStackDupX1(StackSlot[] stack)
            {
                var value1 = stack[stack.Length - 1].Definitions;
                var value2 = stack[stack.Length - 2].Definitions;

                var newStack = new StackSlot[stack.Length - 2 + 3];
                Array.Copy(stack, newStack, stack.Length - 2);
                newStack[newStack.Length - 3] = new StackSlot(value1, null);
                newStack[newStack.Length - 2] = new StackSlot(value2, null);
                newStack[newStack.Length - 1] = new StackSlot(value1, null);
                return newStack;
            }

            public static StackSlot[] ModifyStackDupX2(StackSlot[] stack)
            {
                if (stack[stack.Length - 2].IsCategory2)
                {
                    // Form 2
                    return ModifyStackDupX1(stack);
                }
                else
                {
                    // Form 1
                    var value1 = stack[stack.Length - 1].Definitions;
                    var value2 = stack[stack.Length - 2].Definitions;
                    var value3 = stack[stack.Length - 3].Definitions;

                    var newStack = new StackSlot[stack.Length - 3 + 4];
                    Array.Copy(stack, newStack, stack.Length - 3);
                    newStack[newStack.Length - 4] = new StackSlot(value1, null);
                    newStack[newStack.Length - 3] = new StackSlot(value3, null);
                    newStack[newStack.Length - 2] = new StackSlot(value2, null);
                    newStack[newStack.Length - 1] = new StackSlot(value1, null);
                    return newStack;
                }
            }

            public static StackSlot[] ModifyStackDup2(StackSlot[] stack)
            {
                if (stack[stack.Length - 1].IsCategory2)
                {
                    // Form 2
                    var value = stack[stack.Length - 1].Definitions;

                    var newStack = new StackSlot[stack.Length - 1 + 2];
                    Array.Copy(stack, newStack, stack.Length - 1);
                    newStack[newStack.Length - 2] = new StackSlot(value, null);
                    newStack[newStack.Length - 1] = new StackSlot(value, null);
                    return newStack;
                }
                else
                {
                    // Form 1
                    var value1 = stack[stack.Length - 1].Definitions;
                    var value2 = stack[stack.Length - 2].Definitions;

                    var newStack = new StackSlot[stack.Length - 2 + 4];
                    Array.Copy(stack, newStack, stack.Length - 2);
                    newStack[newStack.Length - 4] = new StackSlot(value2, null);
                    newStack[newStack.Length - 3] = new StackSlot(value1, null);
                    newStack[newStack.Length - 2] = new StackSlot(value2, null);
                    newStack[newStack.Length - 1] = new StackSlot(value1, null);
                    return newStack;
                }
            }

            public static StackSlot[] ModifyStackDup2X1(StackSlot[] stack)
            {
                if (stack[stack.Length - 1].IsCategory2)
                {
                    // Form 2
                    return ModifyStackDupX1(stack);
                }
                else
                {
                    // Form 1
                    var value1 = stack[stack.Length - 1].Definitions;
                    var value2 = stack[stack.Length - 2].Definitions;
                    var value3 = stack[stack.Length - 3].Definitions;

                    var newStack = new StackSlot[stack.Length - 3 + 5];
                    Array.Copy(stack, newStack, stack.Length - 3);
                    newStack[newStack.Length - 5] = new StackSlot(value2, null);
                    newStack[newStack.Length - 4] = new StackSlot(value1, null);
                    newStack[newStack.Length - 3] = new StackSlot(value3, null);
                    newStack[newStack.Length - 2] = new StackSlot(value2, null);
                    newStack[newStack.Length - 1] = new StackSlot(value1, null);
                    return newStack;
                }
            }

            public static StackSlot[] ModifyStackDup2X2(StackSlot[] stack)
            {
                var s1 = stack[stack.Length - 1];
                var s2 = stack[stack.Length - 2];
                if (s1.IsCategory2 && s2.IsCategory2)
                {
                    // Form 4
                    return ModifyStackDupX1(stack);
                }

                var s3 = stack[stack.Length - 3];
                if (s3.IsCategory2 && !s1.IsCategory2 && !s2.IsCategory2)
                {
                    // Form 3
                    var value1 = stack[stack.Length - 1].Definitions;
                    var value2 = stack[stack.Length - 2].Definitions;
                    var value3 = stack[stack.Length - 3].Definitions;

                    var newStack = new StackSlot[stack.Length - 3 + 5];
                    Array.Copy(stack, newStack, stack.Length - 3);
                    newStack[newStack.Length - 5] = new StackSlot(value2, null);
                    newStack[newStack.Length - 4] = new StackSlot(value1, null);
                    newStack[newStack.Length - 3] = new StackSlot(value3, null);
                    newStack[newStack.Length - 2] = new StackSlot(value2, null);
                    newStack[newStack.Length - 1] = new StackSlot(value1, null);
                    return newStack;
                }
                
                if (s1.IsCategory2)
                {
                    // Form 2
                    var value1 = stack[stack.Length - 1].Definitions;
                    var value2 = stack[stack.Length - 2].Definitions;
                    var value3 = stack[stack.Length - 3].Definitions;

                    var newStack = new StackSlot[stack.Length - 3 + 4];
                    Array.Copy(stack, newStack, stack.Length - 3);
                    newStack[newStack.Length - 4] = new StackSlot(value1, null);
                    newStack[newStack.Length - 3] = new StackSlot(value3, null);
                    newStack[newStack.Length - 2] = new StackSlot(value2, null);
                    newStack[newStack.Length - 1] = new StackSlot(value1, null);
                    return newStack;
                }
                else
                {
                    var value1 = stack[stack.Length - 1].Definitions;
                    var value2 = stack[stack.Length - 2].Definitions;
                    var value3 = stack[stack.Length - 3].Definitions;
                    var value4 = stack[stack.Length - 4].Definitions;

                    var newStack = new StackSlot[stack.Length - 4 + 6];
                    Array.Copy(stack, newStack, stack.Length - 4);
                    newStack[newStack.Length - 6] = new StackSlot(value2, null);
                    newStack[newStack.Length - 5] = new StackSlot(value1, null);
                    newStack[newStack.Length - 4] = new StackSlot(value4, null);
                    newStack[newStack.Length - 3] = new StackSlot(value3, null);
                    newStack[newStack.Length - 2] = new StackSlot(value2, null);
                    newStack[newStack.Length - 1] = new StackSlot(value1, null);
                    return newStack;
                }
            }

            public static StackSlot[] ModifyStackSwap(StackSlot[] stack)
            {
                var value1 = stack[stack.Length - 1].Definitions;
                var value2 = stack[stack.Length - 2].Definitions;

                var newStack = new StackSlot[stack.Length - 2 + 2];
                Array.Copy(stack, newStack, stack.Length - 2);
                newStack[newStack.Length - 2] = new StackSlot(value1, null);
                newStack[newStack.Length - 1] = new StackSlot(value2, null);
                return newStack;
            }

            public static StackSlot[] ModifyStackPop2(StackSlot[] stack) 
            {
                if (stack[stack.Length - 1].IsCategory2)
                {
                    // Form 2
                    return ModifyStack(stack, 1, 0, null);
                }
                else
                {
                    // Form 1
                    return ModifyStack(stack, 2, 0, null);
                }
            }
        }
    }
}
