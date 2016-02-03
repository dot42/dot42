using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Inline calls to ctor's of java types that are not available in those java types.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class InlineAndroidExtensionCtors : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 10; }
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            private ReachableContext reachableContext;

            /// <summary>
            /// Convert calls to android extension ctors.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                this.reachableContext = reachableContext;

                // Collect all names
                var methodsWithBody =
                    reachableContext.ReachableTypes.SelectMany(x => x.Methods).Where(m => m.HasBody).ToList();
                if (methodsWithBody.Count == 0)
                    return;

                foreach (var method in methodsWithBody)
                {
                    Convert(method.Body);
                }
            }

            /// <summary>
            /// Convert calls to android extension ctors in the given method body.
            /// </summary>
            private static void Convert(MethodBody body)
            {
                var instructionsToCheck =
                    body.Instructions.Where(x => x.OpCode.Code == Mono.Cecil.Cil.Code.Newobj).ToList();
                if (instructionsToCheck.Count == 0)
                    return;

                foreach (var ins in instructionsToCheck)
                {
                    var ctorRef = ins.Operand as MethodReference;
                    var ctor = (ctorRef != null) ? ctorRef.Resolve() : null;
                    if (ctor == null)
                        continue;
                    if (ctor.DeclaringType.HasDexImportAttribute() && !ctor.HasDexImportAttribute())
                    {
                        // Inline this call
                        InlineNewObjCall(ins, body, ctor);
                    }
                }
            }

            /// <summary>
            /// Inline the call to the given ctor
            /// </summary>
            private static void InlineNewObjCall(Instruction instruction, MethodBody body, MethodDefinition ctor)
            {
                // Prepare 
                var prefixSeq = new ILSequence();
                ctor.Body.SimplifyMacros();

                // Create "this" variable
                var thisVariable = new VariableDefinition(ctor.DeclaringType);
                body.Variables.Add(thisVariable);
                body.InitLocals = true;

                // Store argument in variables
                var paramVariables = new List<VariableDefinition>();
                foreach (var parameter in ctor.Parameters.Reverse())
                {
                    // Create variable
                    var paramVariable = new VariableDefinition(parameter.ParameterType);
                    body.Variables.Add(paramVariable);
                    paramVariables.Insert(0, paramVariable);

                    // Pop 
                    prefixSeq.Emit(OpCodes.Stloc, paramVariable);
                }

                // Clone variables first
                var source = ctor.Body;
                var variables = new List<VariableDefinition>();
                foreach (var sv in source.Variables)
                {
                    var clone = new VariableDefinition(sv.VariableType);
                    variables.Add(clone);
                    body.Variables.Add(clone);
                }

                // Now clone instructions
                var seq = new ILSequence();
                foreach (var instr in source.Instructions)
                {
                    var ni = new Instruction(instr.OpCode, instr.Operand);
                    seq.Append(ni);
                    ni.Offset = instr.Offset;

                    // Convert variable opcodes
                    switch (instr.OpCode.OperandType)
                    {
                        case OperandType.InlineVar:
                        case OperandType.ShortInlineVar:
                            {
                                var index = source.Variables.IndexOf((VariableDefinition) instr.Operand);
                                ni.Operand = variables[index];
                            }
                            break;
                    }

                    // Convert parameter opcodes
                    switch (instr.OpCode.Code)
                    {
                        case Mono.Cecil.Cil.Code.Ldarg:
                            {
                                var index = ctor.Parameters.IndexOf((ParameterDefinition) instr.Operand);
                                ni.Operand = (index >= 0) ? paramVariables[index] : thisVariable;
                                ni.OpCode = OpCodes.Ldloc;
                            }
                            break;
                        case Mono.Cecil.Cil.Code.Ldarga:
                            {
                                var index = ctor.Parameters.IndexOf((ParameterDefinition) instr.Operand);
                                ni.Operand = (index >= 0) ? paramVariables[index] : thisVariable;
                                ni.OpCode = OpCodes.Ldloca;
                            }
                            break;
                        case Mono.Cecil.Cil.Code.Starg:
                            {
                                var index = ctor.Parameters.IndexOf((ParameterDefinition) instr.Operand);
                                ni.Operand = (index >= 0) ? paramVariables[index] : thisVariable;
                                ni.OpCode = OpCodes.Stloc;
                            }
                            break;
                    }
                }

                // Update branch targets
                for (var i = 0; i < seq.Length; i++)
                {
                    var instr = seq[i];
                    var oldi = source.Instructions[i];

                    if (instr.OpCode.OperandType == OperandType.InlineSwitch)
                    {
                        var olds = (Instruction[]) oldi.Operand;
                        var targets = new Instruction[olds.Length];

                        for (int j = 0; j < targets.Length; j++)
                        {
                            targets[j] = GetClone(seq, source.Instructions, olds[j]);
                        }

                        instr.Operand = targets;
                    }
                    else if (instr.OpCode.OperandType == OperandType.ShortInlineBrTarget ||
                             instr.OpCode.OperandType == OperandType.InlineBrTarget)
                    {
                        instr.Operand = GetClone(seq, source.Instructions, (Instruction) oldi.Operand);
                    }
                }

                // Clone exception handlers
                if (source.HasExceptionHandlers)
                {
                    CloneInstructions(seq, source.Instructions, body.ExceptionHandlers, source.ExceptionHandlers);
                }

                // Find call to "this" ctor
                var callToCtors = seq.Where(x => IsCallToThisCtor(x, ctor)).ToList();
                if (callToCtors.Count == 0)
                    throw new CompilerException(string.Format("No call to another this ctor found in {0}", ctor));
                if (callToCtors.Count > 1)
                    throw new CompilerException(string.Format("Multiple calls to another this ctor found in {0}", ctor));
                var callToCtor = callToCtors[0];

                // Change "ld this" to nop
                var args = callToCtor.GetCallArguments(seq, true);
                args[0].ChangeToNop(); // Replace ldarg.0

                // Replace call to this ctor with newobj
                var callSeq = new ILSequence();
                callSeq.Emit(OpCodes.Newobj, (MethodReference) callToCtor.Operand);
                callSeq.Emit(OpCodes.Stloc, thisVariable); // Save new object
                callToCtor.ChangeToNop();
                callSeq.InsertToBefore(callToCtor, seq);

                // Replace ret instructions
                var end = seq.Emit(OpCodes.Ldloc, thisVariable);
                var retInstructions = seq.Where(x => x.OpCode.Code == Mono.Cecil.Cil.Code.Ret).ToList();
                foreach (var ins in retInstructions)
                {
                    ins.OpCode = OpCodes.Br;
                    ins.Operand = end;
                }

                // Insert cloned instructions
                prefixSeq.InsertTo(0, seq);
                seq.InsertToAfter(instruction, body);

                // Change replaced instruction to nop
                instruction.ChangeToNop();

                // Update offsets
                body.ComputeOffsets();
            }

            /// <summary>
            /// Is the given instruction a call to a ctor of the same class as the given ctor?
            /// </summary>
            private static bool IsCallToThisCtor(Instruction ins, MethodDefinition ctor)
            {
                if (ins.OpCode.Code != Mono.Cecil.Cil.Code.Call)
                    return false;
                var ctorRef = ins.Operand as MethodReference;
                if (ctorRef == null)
                    return false;
                if (ctorRef.Name != ".ctor")
                    return false;
                var resolver = new GenericsResolver(ctor.DeclaringType);
                return ctorRef.DeclaringType.AreSame(ctor.DeclaringType, resolver.Resolve);
            }

            /// <summary>
            /// Clone all exception handlers from source to target.
            /// </summary>
            private static void CloneInstructions(IList<Instruction> targetInstructions,
                                                  IList<Instruction> sourceInstructions, IList<ExceptionHandler> target,
                                                  IList<ExceptionHandler> source)
            {
                foreach (var x in source)
                {
                    var clone = new ExceptionHandler(x.HandlerType) {
                        CatchType = x.CatchType,
                        //FilterEnd = GetClone(targetInstructions, sourceInstructions, x.FilterEnd),
                        FilterStart = GetClone(targetInstructions, sourceInstructions, x.FilterStart),
                        HandlerEnd = GetClone(targetInstructions, sourceInstructions, x.HandlerEnd),
                        HandlerStart = GetClone(targetInstructions, sourceInstructions, x.HandlerStart),
                        TryEnd = GetClone(targetInstructions, sourceInstructions, x.TryEnd),
                        TryStart = GetClone(targetInstructions, sourceInstructions, x.TryStart),
                    };
                    target.Add(clone);
                }
            }

            /// <summary>
            /// Find the instruction clone in the given list.
            /// </summary>
            private static Instruction GetClone(IList<Instruction> targetInstructions,
                                                IList<Instruction> sourceInstructions, Instruction source)
            {
                if (source == null)
                {
                    return null;
                }
                return targetInstructions[sourceInstructions.IndexOf(source)];
            }
        }
    }
}