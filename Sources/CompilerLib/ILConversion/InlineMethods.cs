using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Reachable;
using Dot42.FrameworkDefinitions;
using Dot42.Utility;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Inline calls to methods marked with the InlineAttribute.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class InlineMethods : ILConverterFactory
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
            /// Convert calls to methods marked with InlineAttribute.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                this.reachableContext = reachableContext;

                // Collect all names
                var methodsWithBody = reachableContext.ReachableTypes.SelectMany(x => x.Methods).Where(m => m.HasBody).ToList();
                if (methodsWithBody.Count == 0)
                    return;

                // Find all methods marked with InlineAttribute
                var inlineMethods = new HashSet<MethodDefinition>(methodsWithBody.Where(x =>x.HasCustomAttributes &&
                        x.CustomAttributes.Any(c => (c.AttributeType.Name == AttributeConstants.InlineAttributeName) &&
                                                    (c.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace))));
                if (inlineMethods.Count == 0)
                    return;

                // Prepare inline methods
                inlineMethods.ForEach(x => x.Body.SimplifyMacros());

                // Now inline all applicable calls
                var notAlwaysConverted = new HashSet<MethodDefinition>();
                foreach (var method in methodsWithBody)
                {
                    if (!inlineMethods.Contains(method))
                    {
                        Convert(method.Body, inlineMethods, notAlwaysConverted);
                    }
                }

                // Mark all methods that have been inlined in all calls not reachable.
                /*foreach (var method in inlineMethods)
                {
                    if (!notAlwaysConverted.Contains(method))
                    {
                        //method.SetNotReachable();
                    }
                }*/
            }

            /// <summary>
            /// Convert calls to android extension ctors in the3 given method body.
            /// </summary>
            private void Convert(MethodBody body, HashSet<MethodDefinition> inlineMethods, HashSet<MethodDefinition> notAlwaysConverted)
            {
                var callInstructions = body.Instructions.Where(x => (x.OpCode == OpCodes.Call || x.OpCode == OpCodes.Callvirt)).ToList();
                foreach (var ins in callInstructions)
                {
                    var methodRef = ins.Operand as MethodReference;
                    var targetMethod = (methodRef != null) ? methodRef.Resolve() : null;
                    if ((targetMethod == null) || !inlineMethods.Contains(targetMethod))
                        continue;
                    if (targetMethod == body.Method)
                    {
                        notAlwaysConverted.Add(targetMethod);
                        DLog.Warning(DContext.CompilerILConverter, "Cannot inline recursive call to {0}", targetMethod.FullName);
                        continue;
                    }
                    // Inline this call
                    InlineCall(ins, body, targetMethod);
                }
            }

            /// <summary>
            /// Inline the call to the given method
            /// </summary>
            private static void InlineCall(Instruction instruction, MethodBody body, MethodDefinition targetMethod)
            {
                // Prepare 
                var prefixSeq = new ILSequence();

                // Create "this" variable
                VariableDefinition thisVariable = null;
                if (targetMethod.HasThis)
                {
                    thisVariable = new VariableDefinition(targetMethod.DeclaringType);
                    body.Variables.Add(thisVariable);
                    body.InitLocals = true;
                }

                // Store argument in variables
                var paramVariables = new List<VariableDefinition>();
                foreach (var parameter in targetMethod.Parameters.Reverse())
                {
                    // Create variable
                    var paramVariable = new VariableDefinition(parameter.ParameterType);
                    body.Variables.Add(paramVariable);
                    paramVariables.Insert(0, paramVariable);

                    // Pop 
                    prefixSeq.Emit(OpCodes.Stloc, paramVariable);
                }

                // Store this argument (if any)
                if (thisVariable != null)
                {
                    // Pop 
                    prefixSeq.Emit(OpCodes.Stloc, thisVariable);
                }

                // Clone variables first
                var source = targetMethod.Body;
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
                                var index = source.Variables.IndexOf((VariableDefinition)instr.Operand);
                                ni.Operand = variables[index];
                            }
                            break;
                    }

                    // Convert parameter opcodes
                    switch (instr.OpCode.Code)
                    {
                        case Code.Ldarg:
                            {
                                var index = targetMethod.Parameters.IndexOf((ParameterDefinition)instr.Operand);
                                ni.Operand = (index >= 0) ? paramVariables[index] : thisVariable;
                                ni.OpCode = OpCodes.Ldloc;
                            }
                            break;
                        case Code.Ldarga:
                            {
                                var index = targetMethod.Parameters.IndexOf((ParameterDefinition)instr.Operand);
                                ni.Operand = (index >= 0) ? paramVariables[index] : thisVariable;
                                ni.OpCode = OpCodes.Ldloca;
                            }
                            break;
                        case Code.Starg:
                            {
                                var index = targetMethod.Parameters.IndexOf((ParameterDefinition)instr.Operand);
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
                        var olds = (Instruction[])oldi.Operand;
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
                        instr.Operand = GetClone(seq, source.Instructions, (Instruction)oldi.Operand);
                    }
                }

                // Clone exception handlers
                if (source.HasExceptionHandlers)
                {
                    body.ComputeOffsets();
                    CloneInstructions(seq, source.Instructions, body.ExceptionHandlers, source.ExceptionHandlers);
                }

                // Replace ret instructions
                var end = seq.Emit(OpCodes.Nop);
                var retInstructions = seq.Where(x => x.OpCode.Code == Code.Ret).ToList();
                foreach (var ins in retInstructions)
                {
                    ins.OpCode = OpCodes.Br;
                    ins.Operand = end;
                }

                // cast return type of a generic method. TODO: better might be to
                // correctly resolve calls to generic instances as well (above)
                if (targetMethod.ReturnType.IsGenericParameter)
                {
                    var methodRef = (MethodReference)instruction.Operand;
                    TypeReference returnType = null;

                    var gp = (GenericParameter)methodRef.ReturnType;
                    if (gp.Type == GenericParameterType.Type)
                    {
                        var gi = methodRef.DeclaringType as IGenericInstance;
                        if (gi != null && gi.HasGenericArguments)
                            returnType = gi.GenericArguments[gp.Position];
                    }
                    else if (gp.Type == GenericParameterType.Method)
                    {
                        var gi = methodRef as IGenericInstance;
                        if (gi != null && gi.HasGenericArguments)
                            returnType = gi.GenericArguments[gp.Position];
                    }

                    if (returnType != null)
                    {
                        if (!returnType.IsPrimitive)
                        {
                            seq.Emit(OpCodes.Castclass, returnType);
                        }
                        // todo: handle primitive types. unbox them? are structs correctly handled? enums?
                    }
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
            /// Clone all exception handlers from source to target.
            /// </summary>
            private static void CloneInstructions(IList<Instruction> targetInstructions,
                                                  IList<Instruction> sourceInstructions,
                                                  IList<ExceptionHandler> target,
                                                  IEnumerable<ExceptionHandler> source)
            {
                var index = 0;
                foreach (var x in source)
                {
                    var clone = new ExceptionHandler(x.HandlerType)
                    {
                        CatchType = x.CatchType,
                        //FilterEnd = GetClone(targetInstructions, sourceInstructions, x.FilterEnd),
                        FilterStart = GetClone(targetInstructions, sourceInstructions, x.FilterStart),
                        HandlerEnd = GetClone(targetInstructions, sourceInstructions, x.HandlerEnd),
                        HandlerStart = GetClone(targetInstructions, sourceInstructions, x.HandlerStart),
                        TryEnd = GetClone(targetInstructions, sourceInstructions, x.TryEnd),
                        TryStart = GetClone(targetInstructions, sourceInstructions, x.TryStart),
                    };
                    target.Insert(index++, clone);
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