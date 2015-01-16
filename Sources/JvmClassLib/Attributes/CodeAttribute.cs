using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.JvmClassLib.Bytecode;
using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Attributes
{
    public class CodeAttribute : Attribute, IModifiableAttributeProvider
    {
        internal const string AttributeName = "Code";

        private readonly List<Attribute> attributes = new List<Attribute>();
        private readonly List<ExceptionHandler> exceptionHandlers = new List<ExceptionHandler>();
        private readonly MethodDefinition method;
        private readonly ConstantPool cp;
        private readonly int maxStack;
        private readonly int maxLocals;
        private readonly byte[] code;
        private List<Instruction> instructions;
        private readonly List<Tuple<LocalVariable, LocalVariableReference>> localVariableMap;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal CodeAttribute(MethodDefinition method, ConstantPool cp, int maxStack, int maxLocals, byte[] code)
        {
            this.method = method;
            this.cp = cp;
            this.maxStack = maxStack;
            this.maxLocals = maxLocals;
            this.code = code;

            localVariableMap = new List<Tuple<LocalVariable, LocalVariableReference>>();
        }

        /// <summary>
        /// Maximum depth of the operand stack
        /// </summary>
        public int MaxStack { get { return maxStack; } }

        /// <summary>
        /// Number of local variables
        /// </summary>
        public int MaxLocals { get { return maxLocals; } }

        /// <summary>
        /// Gets the byte code
        /// </summary>
        public byte[] Code { get { return code; } }

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }

        /// <summary>
        /// Gets all attributes of this code attribute.
        /// </summary>
        public IEnumerable<Attribute> Attributes { get { return attributes; } }

        /// <summary>
        /// Adds the given attribute to this provider.
        /// </summary>
        void IModifiableAttributeProvider.Add(Attribute attribute)
        {
            attributes.Add(attribute);
        }

        /// <summary>
        /// Signals that all attributes have been loaded.
        /// </summary>
        void IModifiableAttributeProvider.AttributesLoaded()
        {
            var localVarAttrs = Attributes.OfType<LocalVariableTableAttribute>().ToList();
            if (!localVarAttrs.Any()) 
                return;

            // Use local variables table to make a map.
            /*var map = new Dictionary<string, LocalVariableReference>();
            foreach (var attr in localVarAttrs)
            {
                foreach (var v in attr.Variables)
                {
                    var key = "" + v.Index + "-" + v.Name + v.VariableType.ClrTypeName;
                    LocalVariableReference varRef;
                    if (!map.TryGetValue(key, out varRef))
                    {
                        varRef = new LocalVariableReference(method, v.Index);
                        map[key] = varRef;
                    }
                    localVariableMap.Add(Tuple.Create(v, varRef));
                }
            }*/
        }

        /// <summary>
        /// Gets all attributes of this class.
        /// </summary>
        public IEnumerable<ExceptionHandler> ExceptionHandlers
        {
            get { return exceptionHandlers; }
        }

        /// <summary>
        /// Adds the given exception handler to my list.
        /// </summary>
        internal void Add(ExceptionHandler handler)
        {
            exceptionHandlers.Add(handler);
        }

        /// <summary>
        /// Gets all instructions
        /// </summary>
        public IEnumerable<Instruction> Instructions
        {
            get
            {
                ResolveInstructions();
                return instructions;
            }
        }

        /// <summary>
        /// Gets an instruction by it's offset.
        /// </summary>
        public Instruction GetInstruction(int offset)
        {
            ResolveInstructions();
            var result = instructions.FirstOrDefault(x => x.Offset == offset);
            if (result == null)
                throw new ArgumentException("Unknown instruction offset");
            return result;
        }

        /// <summary>
        /// Gets the instruction that follows the given instruction.
        /// Returns null for the last instruction.
        /// </summary>
        public Instruction GetNext(Instruction ins)
        {
            ResolveInstructions();
            var index = instructions.IndexOf(ins);
            if (index < 0)
                throw new ArgumentException("Unknown instruction");
            return (index + 1 < instructions.Count) ? instructions[index + 1] : null;
        }

        /// <summary>
        /// Gets the filename of the source file.
        /// Returns null if not available.
        /// </summary>
        public string SourceFile
        {
            get
            {
                var attr = attributes.OfType<SourceFileAttribute>().FirstOrDefault();
                return (attr != null) ? attr.Value : null;
            }
        }

        /// <summary>
        /// Gets a local variable reference used in an instruction starting at the given offset, to a variable with given index in the stack frame.
        /// </summary>
        public LocalVariableReference GetLocalVariable(int instructionOffset, int index)
        {
            return GetLocalVariable(instructionOffset, index, "local" + index, null);
        }

        /// <summary>
        /// Gets a local variable reference used in an instruction starting at the given offset, to a variable with given index in the stack frame.
        /// </summary>
        private LocalVariableReference GetLocalVariable(int instructionOffset, int index, string name, TypeReference type)
        {
            foreach (var entry in localVariableMap)
            {
                if (/*entry.Item1.IsValidForOffset(instructionOffset) &&*/ (entry.Item1.Index == index))
                {
                    return entry.Item2;
                }
            }

            // Not found, create a new one
            var v = new LocalVariable(0, code.Length, name, type, index);
            var varRef = new LocalVariableReference(method, index);
            localVariableMap.Add(Tuple.Create(v, varRef));
            return varRef;
        }

        /// <summary>
        /// Gets the local variable holding "this".
        /// Returns null if this method is static.
        /// </summary>
        public LocalVariableReference ThisParameter
        {
            get { return method.IsStatic ? null : GetLocalVariable(0, 0, "this", new ObjectTypeReference(method.DeclaringClass.ClassName, null)); }
        }

        /// <summary>
        /// Gets the local variable holding all normal parameters (not including "this").
        /// </summary>
        public IEnumerable<Tuple<TypeReference, LocalVariableReference>> Parameters
        {
            get
            {
                var paramCount = method.Parameters.Count;
                var localIndex = method.IsStatic ? 0 : 1;
                for (var i = 0; i < paramCount; i++)
                {
                    var type = method.Parameters[i];
                    yield return Tuple.Create(type, GetLocalVariable(0, localIndex, "p" + i, type));
                    localIndex += method.Parameters[i].IsWide ? 2 : 1;
                }
            }
        }

        /// <summary>
        /// Gets all local variables that are not parameters.
        /// </summary>
        public IEnumerable<LocalVariableReference> Variables
        {
            get
            {
                for (var i = method.GetParametersLocalVariableSlots(); i < maxLocals; i++)
                {
                    var index = i;
                    foreach (var pair in localVariableMap.Where(x => x.Item2.Index == index))
                    {
                        yield return pair.Item2;
                    }
                }
            }
        }

        /// <summary>
        /// Make sure the instructions are loaded.
        /// </summary>
        internal void ResolveInstructions()
        {
            if (instructions == null)
            {
                var reader = new CodeReader(method, this, code, cp);
                var tmpInstructions = reader.Read();
                exceptionHandlers.ForEach(x => ((IResolveable)x).Resolve(tmpInstructions, null));
                instructions = tmpInstructions;
            }            
        }
    }
}
