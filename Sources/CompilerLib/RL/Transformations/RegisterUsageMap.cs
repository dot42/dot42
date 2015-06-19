using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Map that contains all instructions that use the given register.
    /// </summary>
    internal class RegisterUsageMap
    {
        private readonly Dictionary<Register, HashSet<Instruction>> map = new Dictionary<Register, HashSet<Instruction>>();

        /// <summary>
        /// Create a map.
        /// </summary>
        public RegisterUsageMap(IEnumerable<Instruction> instructions)
        {
            foreach (var ins in instructions)
            {
                foreach (var reg in ins.Registers)
                {
                    Add(reg, ins);
                }
            }
        }

        /// <summary>
        /// Gets all instructions that are found to use the given register.
        /// </summary>
        public IEnumerable<Instruction> this[Register register]
        {
            get
            {
                HashSet<Instruction> list;
                return map.TryGetValue(register, out list) ? list : Enumerable.Empty<Instruction>();
            }
        }

        /// <summary>
        /// Add the given instruction to the set of usages for the given register.
        /// </summary>
        public void Add(Register register, Instruction instruction)
        {
            HashSet<Instruction> list;
            if (!map.TryGetValue(register, out list))
            {
                list = new HashSet<Instruction>();
                map.Add(register, list);
            }
            list.Add(instruction);
        }

        /// <summary>
        /// Add the given instructions to the set of usages for the given register.
        /// </summary>
        public void AddRange(Register register, IEnumerable<Instruction> instructions)
        {
            HashSet<Instruction> list;
            if (!map.TryGetValue(register, out list))
            {
                list = new HashSet<Instruction>();
                map.Add(register, list);
            }
            instructions.ForEach(x => list.Add(x));
        }

        /// <summary>
        /// Replace all references to oldRegister with newRegister in the given instruction set.
        /// </summary>
        public void ReplaceRegisterWith(Register oldRegister, Register newRegister, MethodBody body)
        {
            var oldRegister2 = (oldRegister.Type == RType.Wide) ? body.GetNext(oldRegister) : null;
            var newRegister2 = (newRegister.Type == RType.Wide) ? body.GetNext(newRegister) : null;

            if (oldRegister.KeepWith != newRegister.KeepWith)
                throw new ArgumentException("New register has different keep-with-next value");

            HashSet<Instruction> list;
            if (map.TryGetValue(oldRegister, out list))
            {
                list.ForEach(x => x.ReplaceRegisterWith(oldRegister, newRegister));
                // Update newRegister
                AddRange(newRegister, list);
            }
            if (oldRegister2 != null)
            {
                if (map.TryGetValue(oldRegister2, out list))
                {
                    list.ForEach(x => x.ReplaceRegisterWith(oldRegister2, newRegister2));
                    // Update newRegister2
                    AddRange(newRegister2, list);
                }                
            }
        }
    }
}
