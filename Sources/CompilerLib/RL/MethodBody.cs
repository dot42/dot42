using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Data about 1 method body
    /// </summary>
    public class MethodBody : IRegisterAllocator
    {
        private readonly MethodSource method;
        private readonly InstructionList instructions = new InstructionList();
        private readonly List<ExceptionHandler> exceptionHandlers = new List<ExceptionHandler>();
        private readonly List<Register> registers = new List<Register>();
        private int lastRegisterIndex;

        internal MethodBody(MethodSource method)
        {
            this.method = method;
        }

        /// <summary>
        /// Gets the source method
        /// </summary>
        public MethodSource Method
        {
            get { return method; }
        }

        /// <summary>
        /// Instruction code
        /// </summary>
        public InstructionList Instructions { get { return instructions; } }

        /// <summary>
        /// Exception handlers
        /// </summary>
        public List<ExceptionHandler> Exceptions { get { return exceptionHandlers; } }

        /// <summary>
        /// Allocate a single register.
        /// </summary>
        public Register AllocateRegister(RCategory category, RType type)
        {
            // Allow an additional register
            var result = new Register(lastRegisterIndex++, category, type);
            registers.Add(result);

            return result;
        }

        /// <summary>
        /// Allocate a double register used for wide arguments.
        /// </summary>
        public Tuple<Register, Register> AllocateWideRegister(RCategory category)
        {
            // Allow an additional register
            var first = new Register(lastRegisterIndex++, category, RType.Wide) { Flags = RFlags.KeepWithNext };
            var second = new Register(lastRegisterIndex++, category, RType.Wide2);
            registers.Add(first);
            registers.Add(second);

            return Tuple.Create(first, second);
        }

        /// <summary>
        /// Gets all allocated registers
        /// </summary>
        public IEnumerable<Register> Registers { get { return registers; } }

        /// <summary>
        /// Gets the register immediately following the given register.
        /// </summary>
        public Register GetNext(Register r) { return registers.First(x => x.Index == r.Index + 1); }

        /// <summary>
        /// Change all references to the given old register with the given new register.
        /// </summary>
        public void ReplaceRegisterWith(Register oldRegister, Register newRegister)
        {
            foreach (var ins in instructions)
            {
                ins.ReplaceRegisterWith(oldRegister, newRegister);
            }
            registers.Remove(oldRegister);
            registers.Add(newRegister);
        }

        /// <summary>
        /// Allocate a register for the given type for use as temporary calculation value.
        /// </summary>
        RegisterSpec IRegisterAllocator.AllocateTemp(TypeReference type)
        {
            var primitiveType = type as PrimitiveType;
            if (primitiveType != null)
            {
                if (primitiveType.IsWide)
                {
                    var tuple = AllocateWideRegister(RCategory.Temp);
                    return new RegisterSpec(tuple.Item1, tuple.Item2, type);
                }
                return new RegisterSpec(AllocateRegister(RCategory.Temp, RType.Value), null, type);
            }
            // Object type
            return new RegisterSpec(AllocateRegister(RCategory.Temp, RType.Object), null, type);
        }
    }
}
