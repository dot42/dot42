using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CecilExtensions
{
    public static class Instructions
    {
        /// <summary>
        /// Gets the instructions that generate the arguments for the call in the given instructions.
        /// </summary>
        public static Instruction[] GetCallArguments(this Instruction call, ILSequence sequence, bool includeThis)
        {
            var method = (MethodReference)call.Operand;
            var count = method.Parameters.Count;
            if (includeThis && method.HasThis)
            {
                count++;
            }
            var result = new Instruction[count];

            var current = call;
            var height = count;
            for (int i = count - 1; i >= 0; i--)
            {
                // Look for the starting instruction where stack height is i
                while (result[i] == null)
                {
                    var prevIndex = sequence.IndexOf(current);
                    var prev = (prevIndex >= 1) ? sequence[prevIndex - 1] : null;
                    if (prev == null)
                        throw new ArgumentException(string.Format("Cannot find arguments for call to {0}", method));
                    height -= prev.GetPushDelta();
                    if (height == i)
                        result[i] = prev;
                    height += prev.GetPopDelta(0, true);
                    current = prev;
                }
            }

            return result;
        }

        public static int GetPushDelta(this Instruction instruction)
        {
            OpCode code = instruction.OpCode;
            switch (code.StackBehaviourPush)
            {
                case StackBehaviour.Push0:
                    return 0;

                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    return 1;

                case StackBehaviour.Push1_push1:
                    return 2;

                case StackBehaviour.Varpush:
                    if (code.FlowControl != FlowControl.Call)
                        break;

                    IMethodSignature method = (IMethodSignature)instruction.Operand;
                    return IsVoid(method.ReturnType) ? 0 : 1;
            }

            throw new NotSupportedException();
        }

        public static int? GetPopDelta(this Instruction instruction, MethodDefinition methodDef)
        {
            OpCode code = instruction.OpCode;
            switch (code.StackBehaviourPop)
            {
                case StackBehaviour.Pop0:
                    return 0;
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                case StackBehaviour.Pop1:
                    return 1;

                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    return 2;

                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    return 3;

                case StackBehaviour.PopAll:
                    return null;

                case StackBehaviour.Varpop:
                    if (code == OpCodes.Ret)
                        return methodDef.ReturnType.IsVoid() ? 0 : 1;

                    if (code.FlowControl != FlowControl.Call)
                        break;

                    IMethodSignature method = (IMethodSignature)instruction.Operand;
                    int count = method.HasParameters ? method.Parameters.Count : 0;
                    if (method.HasThis && code != OpCodes.Newobj)
                        ++count;
                    if (code == OpCodes.Calli)
                        ++count; // calli takes a function pointer in additional to the normal args

                    return count;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the number of stack entries popped by the given instruction
        /// </summary>
        internal static int GetPopDelta(this Instruction instruction, int stackHeight, bool methodHasVoidReturn)
        {
            var code = instruction.OpCode;
            switch (code.StackBehaviourPop)
            {
                case StackBehaviour.Pop0:
                    return 0;
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                case StackBehaviour.Pop1:
                    return 1;

                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    return 2;

                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    return 3;

                case StackBehaviour.PopAll:
                    return stackHeight;

                case StackBehaviour.Varpop:
                    if (code.FlowControl == FlowControl.Call)
                    {
                        var method = (IMethodSignature)instruction.Operand;
                        int count = method.Parameters.Count;
                        if (code.Code == Code.Calli)
                            count++;
                        if (method.HasThis && (code.Code != Code.Newobj))
                            ++count;


                        return (ushort)count;
                    }

                    if (code.Value == OpCodes.Ret.Value)
                    {
                        return (ushort)(methodHasVoidReturn ? 0 : 1);
                    }

                    break;
            }
            throw new ArgumentException(instruction.ToString());
        }

        /// <summary>
        /// Is the given type System.Byte?
        /// </summary>
        public static bool IsByte(this TypeReference type)
        {
            return type.Is(MetadataType.Byte);
        }

        /// <summary>
        /// Is the given type System.SByte?
        /// </summary>
        public static bool IsSByte(this TypeReference type)
        {
            return type.Is(MetadataType.SByte);
        }

        /// <summary>
        /// Is the given type System.Boolean?
        /// </summary>
        public static bool IsBoolean(this TypeReference type)
        {
            return type.Is(MetadataType.Boolean);
        }

        /// <summary>
        /// Is the given type System.Char?
        /// </summary>
        public static bool IsChar(this TypeReference type)
        {
            return type.Is(MetadataType.Char);
        }

        /// <summary>
        /// Is the given type System.Single?
        /// </summary>
        public static bool IsFloat(this TypeReference type)
        {
            return type.Is(MetadataType.Single);
        }

        /// <summary>
        /// Is the given type System.Double?
        /// </summary>
        public static bool IsDouble(this TypeReference type)
        {
            return type.Is(MetadataType.Double);
        }

        /// <summary>
        /// Is the given type System.Int16?
        /// </summary>
        public static bool IsInt16(this TypeReference type)
        {
            return type.Is(MetadataType.Int16);
        }

        /// <summary>
        /// Is the given type System.Int32?
        /// </summary>
        public static bool IsInt32(this TypeReference type)
        {
            return type.Is(MetadataType.Int32);
        }

        /// <summary>
        /// Is the given type System.Int64?
        /// </summary>
        public static bool IsInt64(this TypeReference type)
        {
            return type.Is(MetadataType.Int64);
        }

        /// <summary>
        /// Is the given type System.UInt16?
        /// </summary>
        public static bool IsUInt16(this TypeReference type)
        {
            return type.Is(MetadataType.UInt16);
        }

        /// <summary>
        /// Is the given type System.UInt32?
        /// </summary>
        public static bool IsUInt32(this TypeReference type)
        {
            return type.Is(MetadataType.UInt32);
        }

        /// <summary>
        /// Is the given type System.UInt64?
        /// </summary>
        public static bool IsUInt64(this TypeReference type)
        {
            return type.Is(MetadataType.UInt64);
        }

        /// <summary>
        /// Is the given type System.String?
        /// </summary>
        public static bool IsString(this TypeReference type)
        {
            return type.Is(MetadataType.String);
        }

        /// <summary>
        /// Is the given type System.Nullable&lt;T&gt;?
        /// </summary>
        public static bool IsNullableT(this TypeReference type)
        {
            return (type.FullName == "System.Nullable`1");
        }

        /// <summary>
        /// Is the given type System.Object?
        /// </summary>
        public static bool IsObject(this TypeReference type)
        {
            return type.Is(MetadataType.Object);
        }

        /// <summary>
        /// Is the given type System.Void?
        /// </summary>
        public static bool IsVoid(this TypeReference type)
        {
            return type.Is(MetadataType.Void) || (type.FullName == "System.Void");
        }

        /// <summary>
        /// Is the given type System.Type?
        /// </summary>
        public static bool IsSystemType(this TypeReference type)
        {
            return (type.FullName == "System.Type");
        }

        /// <summary>
        /// Is the given type System.ThreadingInterlocked?
        /// </summary>
        public static bool IsSystemThreadingInterlocked(this TypeReference type)
        {
            return (type.FullName == "System.Threading.Interlocked");
        }

        /// <summary>
        /// Is the given type System.Collections.ICollection or a derived interface?
        /// </summary>
        public static bool ExtendsICollection(this TypeReference type)
        {
            if (type.FullName == "System.Collections.ICollection")
            {
                return true;
            }

            var typeDef = type.GetElementType().Resolve();
            if ((typeDef == null) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Select(x => x.Interface).Any(ExtendsICollection);
        }

        /// <summary>
        /// Is the given type IEnumerable or a derived interface?
        /// </summary>
        public static bool ExtendsIEnumerable(this TypeReference type)
        {
            if (type.FullName == "System.Collections.IEnumerable")
            {
                return true;
            }

            var typeDef = type.GetElementType().Resolve();
            if ((typeDef == null) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Select(x => x.Interface).Any(ExtendsIEnumerable);
        }

        /// <summary>
        /// Is the given type System.Collections.IList or a derived interface?
        /// </summary>
        public static bool ExtendsIList(this TypeReference type)
        {
            if (type.FullName == "System.Collections.IList")
            {
                return true;
            }

            var typeDef = type.GetElementType().Resolve();
            if ((typeDef == null) || !typeDef.IsInterface)
                return false;
            return typeDef.Interfaces.Select(x => x.Interface).Any(ExtendsIList);
        }

        /// <summary>
        /// Is the given type System.Int64, UInt64 or Double?
        /// </summary>
        public static bool IsWide(this TypeReference type)
        {
            type = type.RemoveModifiers();
            return (type != null) &&
                   ((type.MetadataType == MetadataType.Int64) ||
                    (type.MetadataType == MetadataType.UInt64) ||
                    (type.MetadataType == MetadataType.Double));
        }

        /// <summary>
        /// Is the given type of the given metadata type?
        /// </summary>
        public static bool Is(this TypeReference type, MetadataType expectedType)
        {
            type = type.RemoveModifiers();
            return (type != null) && (type.MetadataType == expectedType);
        }

        /// <summary>
        /// Is the given type of the any of given metadata type?
        /// </summary>
        public static bool Is(this TypeReference type, params MetadataType[] expectedTypes)
        {
            type = type.RemoveModifiers();
            if (type == null) return false;
            var actual = type.MetadataType;
            foreach (var x in expectedTypes)
            {
                if (actual == x) return true;
            }
            return false;
        }

        /// <summary>
        /// Is the given type a value type of void?
        /// </summary>
        public static bool IsValueTypeOrVoid(this TypeReference type)
        {
            type = type.RemoveModifiers();
            if (type is ArrayType)
                return false;
            return type.IsValueType || type.IsVoid();
        }

        /// <summary>
        /// Return the given type without any (optional) modifiers.
        /// </summary>
        internal static TypeReference RemoveModifiers(this TypeReference type)
        {
            while (type is OptionalModifierType || type is RequiredModifierType)
                type = ((TypeSpecification)type).ElementType;
            return type;
        }

        /// <summary>
        /// Convert the given instruction to a NOP.
        /// </summary>
        public static void ChangeToNop(this Instruction source)
        {
            source.OpCode = OpCodes.Nop;
            source.Operand = null;
        }

        /// <summary>
        /// Update the offsets of all instructions following the given instruction.
        /// </summary>
        private static void ComputeOffsets(Instruction instruction)
        {
            var offset = instruction.Offset;
            while (instruction != null)
            {
                instruction.Offset = offset;
                offset += instruction.GetSize();
                instruction = instruction.Next;
            }
        }

        /// <summary>
        /// Update the offsets of all instructions following the given instruction.
        /// </summary>
        public static void ComputeOffsets(this MethodBody body)
        {
            if ((body == null) || (body.Instructions.Count == 0))
                return;
            ComputeOffsets(body.Instructions[0]);
        }
    }
}
