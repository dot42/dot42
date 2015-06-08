using System.Linq;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.Target.CompilerCache
{
    /// <summary>
    /// Clones Dex Method bodies.
    /// </summary>
    internal static class DexMethodBodyCloner
    {
        /// <summary>
        /// Note that referencing operands, i.e. TypeReferences, FieldReference or
        /// MethodRefrences are not cloned, but only shallow copied.
        /// If you indend to modify those operand values themself, be sure to clone
        /// them beforehand. 
        /// </summary>
        public static MethodBody Clone(MethodDefinition targetMethod, MethodDefinition sourceMethod)
        {
            var sourceBody = sourceMethod.Body;

            MethodBody body = new MethodBody(targetMethod, sourceBody.Registers.Count)
            {
                IncomingArguments = sourceBody.IncomingArguments,
                OutgoingArguments = sourceBody.OutgoingArguments
            };

            foreach (var sourceIns in sourceBody.Instructions)
            {
                var ins = new Instruction(sourceIns.OpCode);

                foreach (var r in sourceIns.Registers)
                    ins.Registers.Add(body.Registers[r.Index]);

                ins.Offset = sourceIns.Offset;
                ins.SequencePoint = ins.SequencePoint;

                ins.Operand = sourceIns.Operand;

                body.Instructions.Add(ins);
            }

            // fix instruction references

            var insByOffset = body.Instructions.ToDictionary(i => i.Offset);

            foreach (var ins in body.Instructions)
            {
                var targetIns = ins.Operand as Instruction;
                var packedSwitch = ins.Operand as PackedSwitchData;
                var sparseSwitch = ins.Operand as SparseSwitchData;

                if (targetIns != null)
                {
                    ins.Operand = insByOffset[targetIns.Offset];
                }
                else if (packedSwitch != null)
                {
                    var ps = new PackedSwitchData(packedSwitch.Targets.Select(si => insByOffset[si.Offset]));
                    ps.FirstKey = packedSwitch.FirstKey;
                    ins.Operand = ps;
                }
                else if (sparseSwitch != null)
                {
                    var ss = new SparseSwitchData();
                    foreach (var ssd in sparseSwitch.Targets)
                    {
                        ss.Targets.Add(ssd.Key, insByOffset[ssd.Value.Offset]);
                    }
                    ins.Operand = ss;
                }
            }


            foreach (var sourceEx in sourceBody.Exceptions)
            {
                var ex = new ExceptionHandler();
                if (sourceEx.TryStart != null)
                    ex.TryStart = insByOffset[sourceEx.TryStart.Offset];
                if (sourceEx.TryEnd != null)
                    ex.TryEnd   = insByOffset[sourceEx.TryEnd.Offset];
                if (sourceEx.CatchAll != null)
                    ex.CatchAll = insByOffset[sourceEx.CatchAll.Offset];

                foreach (var sourceCatch in sourceEx.Catches)
                {
                    var c = new Catch
                    {
                        Type = sourceCatch.Type,
                        Instruction = insByOffset[sourceCatch.Instruction.Offset]
                    };
                    ex.Catches.Add(c);
                }

                body.Exceptions.Add(ex);
            }

            if (sourceBody.DebugInfo != null)
            {
                var di = body.DebugInfo = new DebugInfo(body);

                di.LineStart = sourceBody.DebugInfo.LineStart;
                di.Parameters = sourceBody.DebugInfo.Parameters.ToList();

                foreach (var sourceInstruction in sourceBody.DebugInfo.DebugInstructions)
                {
                    di.DebugInstructions.Add(new DebugInstruction(sourceInstruction.OpCode,
                        sourceInstruction.Operands.ToList()));
                }
            }

            return body;
        }
    }
}
