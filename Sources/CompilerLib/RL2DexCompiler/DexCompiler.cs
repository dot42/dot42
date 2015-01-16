using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib.Instructions;
using Catch = Dot42.DexLib.Instructions.Catch;
using ExceptionHandler = Dot42.DexLib.Instructions.ExceptionHandler;
using Instruction = Dot42.CompilerLib.RL.Instruction;
using MethodBody = Dot42.DexLib.Instructions.MethodBody;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    internal class DexCompiler
    {
        private readonly RL.MethodBody rlBody;
        private readonly MethodBody dexBody;
        private readonly InvocationFrame frame;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexCompiler(RL.MethodBody rlBody, MethodBody dexBody, InvocationFrame frame)
        {
            this.rlBody = rlBody;
            this.dexBody = dexBody;
            this.frame = frame;
        }

        /// <summary>
        /// Compile RL into Dex.
        /// </summary>
        public RegisterMapper Compile()
        {
            // Map registers
            var regMapper = new RegisterMapper(rlBody, frame);

            // Create instructions
            var maxOutgoingArguments = 0;
            var target = dexBody.Instructions;
            var insMap = new Dictionary<Instruction, Tuple<DexLib.Instructions.Instruction, DexLib.Instructions.Instruction>>();
            foreach (var ins in rlBody.Instructions)
            {
                DexLib.Instructions.Instruction first = null;
                DexLib.Instructions.Instruction last = null;
                foreach (var dexIns in RL2Dex.Convert(ins, regMapper))
                {
                    target.Add(dexIns);
                    if (first == null) first = dexIns;
                    last = dexIns;
                }
                if ((first == null) || (last == null))
                    throw new InvalidOperationException();
                insMap[ins] = Tuple.Create(first, last);

                // Gather statistics
                if (ins.Code.IsInvoke())
                {
                    maxOutgoingArguments = Math.Max(maxOutgoingArguments, ins.Registers.Count);
                }
            }

            // Update targets
            foreach (var dexIns in target)
            {
                var instruction = dexIns.Operand as Instruction;
                if (instruction != null)
                {
                    dexIns.Operand = insMap[instruction].Item1;
                }
                if (dexIns.OpCode == OpCodes.Packed_switch)
                {
                    var instructions = (Instruction[])dexIns.Operand;
                    dexIns.Operand = new PackedSwitchData(instructions.Select(x => insMap[x].Item1));
                }
                else if (dexIns.OpCode == OpCodes.Sparse_switch)
                {
                    var targetPairs = (Tuple<int, Instruction>[])dexIns.Operand;
                    var data = new SparseSwitchData();
                    foreach (var pair in targetPairs.OrderBy(x => x.Item1))
                    {
                        data.Targets.Add(pair.Item1, insMap[pair.Item2].Item1);
                    }
                    dexIns.Operand = data;
                }
            }

            // Create exception handlers 
            foreach (var handler in rlBody.Exceptions)
            {
                var dhandler = new ExceptionHandler();
                dhandler.TryStart = insMap[handler.TryStart].Item1;
                dhandler.TryEnd = insMap[handler.TryEnd].Item2;
                dhandler.CatchAll = (handler.CatchAll != null) ? insMap[handler.CatchAll].Item1 : null;
                dexBody.Exceptions.Add(dhandler);

                foreach (var catchBlock in handler.Catches)
                {
                    var dcatchBlock = new Catch();
                    dcatchBlock.Type = catchBlock.Type;
                    dcatchBlock.Instruction = insMap[catchBlock.Instruction].Item1;
                    dhandler.Catches.Add(dcatchBlock);
                }
            }

            // Add register spilling code (if needed)
            if (regMapper.SpillingRequired)
            {
                RegisterSpillingOptimizer.Transform(dexBody);
                RegisterSpilling.AddSpillingCode(dexBody, regMapper);
            }

            // Set statistics
            dexBody.Registers.AddRange(regMapper.All);
            dexBody.IncomingArguments = (ushort) regMapper.ArgumentCount;
            dexBody.OutgoingArguments = (ushort) maxOutgoingArguments;

            return regMapper;
        }
    }
}
