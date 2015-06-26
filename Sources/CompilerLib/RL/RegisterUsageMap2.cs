using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;

namespace Dot42.CompilerLib.RL
{
    class RegisterUsageMap2
    {
        private readonly ControlFlowGraph2 _graph;
        private readonly Dictionary<Register, RegisterUsage> _usages;
        public List<RegisterUsage> BasicUsages { get { return _usages.Values.ToList(); }}
        
        internal class RegisterUsage
        {
            public readonly Register Register;
            public readonly List<InstructionInBlock> Reads = new List<InstructionInBlock>();
            public readonly List<InstructionInBlock> Writes = new List<InstructionInBlock>();
            public readonly List<InstructionInBlock> MovesFromOtherRegisters = new List<InstructionInBlock>();
            public readonly List<InstructionInBlock> CheckCasts = new List<InstructionInBlock>();
            public readonly HashSet<BasicBlock> Blocks = new HashSet<BasicBlock>();

            public RegisterUsage(Register r)
            {
                Register = r;
            }

            public override string ToString()
            {
                return Register.ToString();
            }

            public IEnumerable<InstructionInBlock> Instructions
            {
                get { return Reads.Union(Writes).ToList(); }
            }
            
            private void AddWrite(InstructionInBlock ins)
            {
                Writes.Add(ins);
                if (ins.Instruction.Code.IsMove())
                    MovesFromOtherRegisters.Add(ins);
            }

            public void Remove(InstructionInBlock ins)
            {
                Reads.RemoveAll(i => i == ins);
                Writes.RemoveAll(i => i == ins);
                MovesFromOtherRegisters.RemoveAll(i => i == ins);
                CheckCasts.RemoveAll(i => i == ins);
            }

            public void Add(InstructionInBlock ins, int registerIndex)
            {
                var isDest = ins.IsDestinationRegister(registerIndex);
                var isSource = ins.IsSourceRegister(registerIndex);

                if (isDest)   AddWrite(ins);
                if (isSource) Reads.Add(ins); ;

                if(ins.Instruction.Code == RCode.Check_cast)
                    CheckCasts.Add(ins);
                
                if(!isDest && !isSource)
                    Debug.Assert(false);
            }

            public void Clear()
            {
                Reads.Clear();
                Writes.Clear();
                MovesFromOtherRegisters.Clear();
                CheckCasts.Clear();
            }
        }

        public RegisterUsageMap2(ControlFlowGraph2 graph)
        {
            _graph = graph;
            _usages = new Dictionary<Register, RegisterUsage>();
            CollectBasicUsages(graph, _usages);
            // TODO: seperate independant usages.
        }

        public RegisterUsage GetBasicUsage(Register r)
        {
            RegisterUsage ret;
            if (_usages.TryGetValue(r, out ret))
                return ret;

            if(!_graph.Body.Registers.Contains(r))
                throw new InvalidOperationException();

            ret = new RegisterUsage(r);
            _usages.Add(r, ret);
            return ret;
        }

        private static void CollectBasicUsages(ControlFlowGraph2 graph, Dictionary<Register, RegisterUsage> usages)
        {
            foreach (var block in graph.BasicBlocks)
            {
                foreach (var ins in block.Instructions)
                {
                    var instructionInBlock = new InstructionInBlock(ins, block);

                    for (int i = 0; i < ins.Registers.Count; ++i)
                    {
                        var reg = ins.Registers[i];

                        RegisterUsage u;
                        if (!usages.TryGetValue(reg, out u))
                        {
                            u = new RegisterUsage(reg);
                            usages.Add(reg, u);
                        }

                        u.Blocks.Add(block);
                        u.Add(instructionInBlock, i);
                    }
                }
            }
        }

        /// <summary>
        /// Will replace all read and write usages of 'replaced' with 'replacement',
        /// and nop resulting assigments to itself.
        /// </summary>
        public void ReplaceRegister(RegisterUsage replaced, RegisterUsage replacement)
        {
            if (replaced.Register.KeepWith != replacement.Register.KeepWith)
                throw new ArgumentException("New register has different keep-with value");
            if (replaced.Register.KeepWith == RFlags.KeepWithPrev)
                throw new ArgumentException("can not replace a keep with prev register");

            Register replR = replaced.Register, replR2 = null;

            RegisterUsage replaced2 = null, replacement2 = null;

            if (replaced.Register.KeepWith == RFlags.KeepWithNext)
            {
                replaced2 = GetBasicUsage(_graph.Body.GetNext(replaced.Register));
                replacement2 = GetBasicUsage(_graph.Body.GetNext(replacement.Register));
                replR2 = replaced2.Register;
            }

            foreach (var ins in replaced.Instructions)
            {
                var instr = ins.Instruction;
                var regs = instr.Registers;

                for (int i = 0; i < regs.Count; ++i)
                {
                    if (regs[i] == replR)
                    {
                        regs[i] = replacement.Register;
                        replacement.Add(ins, i);
                    }
                    else if (regs[i] == replR2)
                    {
                        Debug.Assert(replacement2 != null, "replacement2 != null");
                        regs[i] = replacement2.Register;
                        replacement2.Add(ins, i);
                    }
                }

                if (instr.Code.IsMove() && instr.Registers[0] == instr.Registers[1])
                {
                    ConvertToNop(ins);
                }

            }

            replaced.Clear();
            _usages.Remove(replaced.Register);

            if (replaced2 != null)
            {
                replaced2.Clear();
                _usages.Remove(replaced2.Register);
            }
            
        }

        private void ConvertToNop(InstructionInBlock ins)
        {
            // update registers usages.
            foreach (var r in ins.Instruction.Registers)
            {
                _usages[r].Remove(ins);
            }
            ins.Instruction.ConvertToNop();
        }
    }
}
