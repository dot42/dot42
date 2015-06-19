using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib.Instructions;
using Instruction = Dot42.CompilerLib.RL.Instruction;
using MethodBody = Dot42.CompilerLib.RL.MethodBody;
using Register = Dot42.CompilerLib.RL.Register;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    /// <summary>
    /// Map RL registers onto Dex registers.
    /// </summary>
    internal class RegisterMapper
    {
        private readonly InvocationFrame frame;
        private const int MaxSpillRegisters = 7;

        private readonly Dictionary<Register, DexLib.Instructions.Register> map = new Dictionary<Register, DexLib.Instructions.Register>();
        private Dictionary<IVariable, VariableMapping> variableMap;
        private readonly Dictionary<DexLib.Instructions.Register, RType> dRegisterTypes = new Dictionary<DexLib.Instructions.Register, RType>();
        private readonly DexLib.Instructions.Register[] invokeRangeRegisters;
        private readonly DexLib.Instructions.Register[] spillRangeRegisters;
        private readonly RegisterSpillingMap spillingMap = new RegisterSpillingMap();

        /// <summary>
        /// Default ctor
        /// </summary>
        public RegisterMapper(MethodBody body, InvocationFrame frame)
        {
            this.frame = frame;
            // Select all non-argument registers that are in use.
            Dictionary<Register, RegisterFlags> used;
            int maxOutgoingRegisters;
            bool largeInvokeInsFound;
            GetUsedArgumentRegisters(body, out used, out maxOutgoingRegisters, out largeInvokeInsFound);
            //var used = body.Registers.Where(x => (x.Category != RCategory.Argument) && body.Instructions.Uses(x)).ToList();
            var sorted = used.OrderBy(x => x.Value).ThenBy(x => x.Key.Index).Select(x => x.Key).ToList();
            AddKeepWithNext(sorted, body);
            // Collect argument registers
            var arguments = body.Registers.Where(x => x.Category == RCategory.Argument).ToList();

            // Prepare various decisions
            //var maxOutgoingRegisters = body.Instructions.Max(x => x.Code.IsInvoke() ? x.Registers.Count : 0);
            var required = sorted.Count + arguments.Count;
            //var largeInvokeInsFound = body.Instructions.Any(x => x.Code.IsInvoke() && x.Registers.Count > 5);

            // Allocate Dex registers for each
            var index = 0;

            // Allocate spill registers (if needed)
            if ((required >= 16) || (largeInvokeInsFound && (required + maxOutgoingRegisters >= 16)))
            {
                spillRangeRegisters = new DexLib.Instructions.Register[MaxSpillRegisters];
                for (var i = 0; i < MaxSpillRegisters; i++)
                {
                    spillRangeRegisters[i] = new DexLib.Instructions.Register(index++);                    
                }
            }

            // Allocate Dex registers for each temp register.
            foreach (var r in sorted)
            {
                var dreg = new DexLib.Instructions.Register(index++);
                map[r] = dreg;
                dRegisterTypes[dreg] = r.Type;
            }

            // Allocate outgoing invocation frame (if needed)
            if ((required >= 16) || largeInvokeInsFound)
            {
                invokeRangeRegisters = new DexLib.Instructions.Register[maxOutgoingRegisters];
                for (var i = 0; i < maxOutgoingRegisters; i++)
                {
                    invokeRangeRegisters[i] = new DexLib.Instructions.Register(index++);
                }
            }

            // Allocate Dex registers for each argument
            foreach (var r in arguments)
            {
                var dreg = new DexLib.Instructions.Register(index++);
                map[r] = dreg;
                dRegisterTypes[dreg] = r.Type;
            }
            ArgumentCount = arguments.Count;
        }

        /// <summary>
        /// Gets the total number of mapped registers
        /// </summary>
        public int Count { get { return map.Count; } }

        /// <summary>
        /// Gets the number of registers used for arguments.
        /// </summary>
        public int ArgumentCount { get; private set; }

        /// <summary>
        /// Gets the Dex register for the given RL register.
        /// </summary>
        public Dot42.DexLib.Instructions.Register this[Register source]
        {
            get { return map[source]; }
        }

        /// <summary>
        /// Gets the mappings between high-index registers and low-index registers.
        /// </summary>
        public RegisterSpillingMap RegisterSpillingMap { get { return spillingMap; } }

        /// <summary>
        /// Gets the Dex register for the given RL register.
        /// </summary>
        public bool ContainsKey(Register source)
        {
            return map.ContainsKey(source); 
        }

        /// <summary>
        /// Gets the type of the value in the given register.
        /// </summary>
        public RType GetType(DexLib.Instructions.Register register)
        {
            return dRegisterTypes[register];
        }

        /// <summary>
        /// Will it be needed to add register spilling code?
        /// </summary>
        public bool SpillingRequired { get { return (spillRangeRegisters != null) || (invokeRangeRegisters != null); } }

        /// <summary>
        /// Gets all registers allocated for spilling purposes.
        /// </summary>
        public IEnumerable<DexLib.Instructions.Register> SpillRegisters { get { return spillRangeRegisters ?? Enumerable.Empty<DexLib.Instructions.Register>(); } }

        /// <summary>
        /// Gets all registers allocated for outgoing invocation frames.
        /// </summary>
        public IEnumerable<DexLib.Instructions.Register> InvocationFrameRegisters { get { return invokeRangeRegisters ?? Enumerable.Empty<DexLib.Instructions.Register>(); } }

        /// <summary>
        /// Gets all Dex registers being used.
        /// </summary>
        public IEnumerable<Dot42.DexLib.Instructions.Register> All { get { return map.Values.Concat(SpillRegisters).Concat(InvocationFrameRegisters); } }

        /// <summary>
        /// Gets all registers holding a local variable.
        /// </summary>
        public IEnumerable<VariableMapping> VariableRegisters
        {
            get
            {
                EnsureVariableMap();
                return variableMap.Values;
            }
        }

        /// <summary>
        /// Make sure <see cref="variableMap"/> is initialized.
        /// </summary>
        private void EnsureVariableMap()
        {
            if (variableMap != null) return;
            var result = new Dictionary<IVariable, VariableMapping>();
            foreach (var x in frame.Variables)
            {
                if (map.ContainsKey(x.Register))
                {
                    var dreg = map[x.Register];
                    result[x.Variable] = new VariableMapping(dreg, x.Variable);
                }
            }
            variableMap = result;
        }

        /// <summary>
        /// For each register in the given list, ensure that all keep-with-next registers are also added.
        /// </summary>
        private static void AddKeepWithNext(List<Register> registers, MethodBody body)
        {
            var i = 0;
            while (i < registers.Count)
            {
                var r = registers[i++];
                if (r.KeepWith != RFlags.KeepWithNext)
                    continue;
                var next = body.Registers.First(x => x.Index == r.Index + 1);
                if (!registers.Contains(next))
                {
                    registers.Insert(i, next);
                }
            }
        }

        /// <summary>
        /// Gets the lowest size (in bitsX) that is available for the given register in the given instruction.
        /// </summary>
        private static RegisterFlags GetLowestSize(Instruction instruction, Register r)
        {
            var result = RegisterFlags.Bits16;
            var info = OpCodeInfo.Get(instruction.Code.ToDex());
            var registers = instruction.Registers;
            for (var i = 0; i < registers.Count; i++)
            {
                if (registers[i] == r)
                {
                    var size = info.GetUsage(i) & RegisterFlags.SizeMask;
                    if (size < result) result = size;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all registers of category!=Argument that are used in an instruction.
        /// </summary>
        private static void GetUsedArgumentRegisters(MethodBody body, out Dictionary<Register, RegisterFlags> used, out int maxOutgoingRegisters, out bool largeInvokeInsFound)
        {
            //maxOutgoingRegisters = body.Instructions.Max(x => x.Code.IsInvoke() ? x.Registers.Count : 0);
            //largeInvokeInsFound = body.Instructions.Any(x => x.Code.IsInvoke() && x.Registers.Count > 5);
            maxOutgoingRegisters = 0;
            largeInvokeInsFound = false;

            used = new Dictionary<Register, RegisterFlags>();
            foreach (var ins in body.Instructions)
            {
                if (ins.Code.IsInvoke())
                {
                    maxOutgoingRegisters = Math.Max(maxOutgoingRegisters, ins.Registers.Count);
                    if (ins.Registers.Count > 5)
                    {
                        largeInvokeInsFound = true;
                    }
                }
                foreach (var reg in ins.Registers)
                {
                    if (reg.Category != RCategory.Argument)
                    {
                        var lowestSize = GetLowestSize(ins, reg);
                        RegisterFlags currentLowestSize;
                        if (used.TryGetValue(reg, out currentLowestSize))
                        {
                            if (lowestSize < currentLowestSize)
                            {
                                used[reg] = lowestSize;
                            }
                        }
                        else
                        {
                            used.Add(reg, lowestSize);
                        }
                    }
                }
            }

            // Add second register of wide registers.
            Register prevReg = null;
            foreach (var reg in body.Registers)
            {
                if (prevReg != null)
                {
                    if (reg.Type == RType.Wide2)
                    {
                        RegisterFlags flags;
                        if (used.TryGetValue(prevReg, out flags))
                        {
                            used[reg] = flags;
                        }
                    }
                }
                prevReg = reg;
            }
        }
    }
}
