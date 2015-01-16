using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.Utility;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.IL2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        private readonly MethodDefinition methodDef;
        private readonly bool optimize;
        private readonly DecompilerContext context;
        private readonly XModule module;

        // Virtual instructions to load exception on stack
        private readonly Dictionary<ExceptionHandler, ByteCode> ldexceptions = new Dictionary<ExceptionHandler, ByteCode>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstBuilder(MethodDefinition methodDef, bool optimize, DecompilerContext context)
        {
            this.methodDef = methodDef;
            this.optimize = optimize;
            this.context = context;
            module = context.CurrentModule;
        }

        /// <summary>
        /// Convert the given method to a list of Ast nodes.
        /// </summary>
        public List<AstNode> Build()
        {
            if (methodDef.Body.Instructions.Count == 0)
                return new List<AstNode>();

            var body = StackAnalysis();
            var ast = ConvertToAst(body, new HashSet<ExceptionHandler>(methodDef.Body.ExceptionHandlers));

            return ast;
        }

        /// <summary>
        /// Analyse the instructions in the method code and convert them to a ByteCode list.
        /// </summary>
        private List<ByteCode> StackAnalysis()
        {
            var instrToByteCode = new Dictionary<Instruction, ByteCode>();

            // Create temporary structure for the stack analysis
            var body = new List<ByteCode>(methodDef.Body.Instructions.Count);
            List<Instruction> prefixes = null;
            foreach (var inst in methodDef.Body.Instructions)
            {
                if (inst.OpCode.OpCodeType == OpCodeType.Prefix)
                {
                    if (prefixes == null)
                        prefixes = new List<Instruction>(1);
                    prefixes.Add(inst);
                    continue;
                }
                var code = (AstCode)inst.OpCode.Code;
                var operand = inst.Operand;
                AstCodeUtil.ExpandMacro(ref code, ref operand, methodDef.Body);
                var byteCode = new ByteCode
                {
                    Offset = inst.Offset,
                    EndOffset = inst.Next != null ? inst.Next.Offset : methodDef.Body.CodeSize,
                    Code = code,
                    Operand = operand,
                    PopCount = inst.GetPopDelta(methodDef),
                    PushCount = inst.GetPushDelta(),
                    SequencePoint = SequencePointWrapper.Wrap(inst.SequencePoint)
                };
                if (prefixes != null)
                {
                    instrToByteCode[prefixes[0]] = byteCode;
                    byteCode.Offset = prefixes[0].Offset;
                    byteCode.Prefixes = prefixes.ToArray();
                    prefixes = null;
                }
                else
                {
                    instrToByteCode[inst] = byteCode;
                }
                body.Add(byteCode);
            }
            for (int i = 0; i < body.Count - 1; i++)
            {
                body[i].Next = body[i + 1];
            }

            var agenda = new Stack<ByteCode>();
            var varCount = methodDef.Body.Variables.Count;

            var exceptionHandlerStarts = new HashSet<ByteCode>(methodDef.Body.ExceptionHandlers.Select(eh => instrToByteCode[eh.HandlerStart]));

            // Add known states
            if (methodDef.Body.HasExceptionHandlers)
            {
                foreach (var ex in methodDef.Body.ExceptionHandlers)
                {
                    var handlerStart = instrToByteCode[ex.HandlerStart];
                    handlerStart.StackBefore = new StackSlot[0];
                    handlerStart.VariablesBefore = VariableSlot.MakeUknownState(varCount);
                    if (ex.HandlerType == ExceptionHandlerType.Catch || ex.HandlerType == ExceptionHandlerType.Filter)
                    {
                        // Catch and Filter handlers start with the exeption on the stack
                        var ldexception = new ByteCode()
                        {
                            Code = AstCode.Ldexception,
                            Operand = ex.CatchType,
                            PopCount = 0,
                            PushCount = 1
                        };
                        ldexceptions[ex] = ldexception;
                        handlerStart.StackBefore = new[] { new StackSlot(new[] { ldexception }, null) };
                    }
                    agenda.Push(handlerStart);

                    if (ex.HandlerType == ExceptionHandlerType.Filter)
                    {
                        var filterStart = instrToByteCode[ex.FilterStart];
                        var ldexception = new ByteCode
                        {
                            Code = AstCode.Ldexception,
                            Operand = ex.CatchType,
                            PopCount = 0,
                            PushCount = 1
                        };
                        // TODO: ldexceptions[ex] = ldexception;
                        filterStart.StackBefore = new[] { new StackSlot(new[] { ldexception }, null) };
                        filterStart.VariablesBefore = VariableSlot.MakeUknownState(varCount);
                        agenda.Push(filterStart);
                    }
                }
            }

            body[0].StackBefore = new StackSlot[0];
            body[0].VariablesBefore = VariableSlot.MakeUknownState(varCount);
            agenda.Push(body[0]);

            // Process agenda
            while (agenda.Count > 0)
            {
                var byteCode = agenda.Pop();

                // Calculate new stack
                var newStack = StackSlot.ModifyStack(byteCode.StackBefore, byteCode.PopCount ?? byteCode.StackBefore.Length, byteCode.PushCount, byteCode);

                // Calculate new variable state
                var newVariableState = VariableSlot.CloneVariableState(byteCode.VariablesBefore);
                if (byteCode.IsVariableDefinition)
                {
                    newVariableState[((VariableReference)byteCode.Operand).Index] = new VariableSlot(new[] { byteCode }, false);
                }

                // After the leave, finally block might have touched the variables
                if (byteCode.Code == AstCode.Leave)
                {
                    newVariableState = VariableSlot.MakeUknownState(varCount);
                }

                // Find all successors
                var branchTargets = new List<ByteCode>();
                if (!byteCode.Code.IsUnconditionalControlFlow())
                {
                    if (exceptionHandlerStarts.Contains(byteCode.Next))
                    {
                        // Do not fall though down to exception handler
                        // It is invalid IL as per ECMA-335 §12.4.2.8.1, but some obfuscators produce it
                    }
                    else
                    {
                        branchTargets.Add(byteCode.Next);
                    }
                }
                if (byteCode.Operand is Instruction[])
                {
                    foreach (var inst in (Instruction[])byteCode.Operand)
                    {
                        var target = instrToByteCode[inst];
                        branchTargets.Add(target);
                        // The target of a branch must have label
                        if (target.Label == null)
                        {
                            target.Label = new AstLabel(target.SequencePoint, target.Name);
                        }
                    }
                }
                else if (byteCode.Operand is Instruction)
                {
                    var target = instrToByteCode[(Instruction)byteCode.Operand];
                    branchTargets.Add(target);
                    // The target of a branch must have label
                    if (target.Label == null)
                    {
                        target.Label = new AstLabel(target.SequencePoint, target.Name);
                    }
                }

                // Apply the state to successors
                foreach (var branchTarget in branchTargets)
                {
                    if (branchTarget.StackBefore == null && branchTarget.VariablesBefore == null)
                    {
                        if (branchTargets.Count == 1)
                        {
                            branchTarget.StackBefore = newStack;
                            branchTarget.VariablesBefore = newVariableState;
                        }
                        else
                        {
                            // Do not share data for several bytecodes
                            branchTarget.StackBefore = StackSlot.ModifyStack(newStack, 0, 0, null);
                            branchTarget.VariablesBefore = VariableSlot.CloneVariableState(newVariableState);
                        }
                        agenda.Push(branchTarget);
                    }
                    else
                    {
                        if (branchTarget.StackBefore.Length != newStack.Length)
                        {
                            throw new Exception("Inconsistent stack size at " + byteCode.Name);
                        }

                        // Be careful not to change our new data - it might be reused for several branch targets.
                        // In general, be careful that two bytecodes never share data structures.

                        bool modified = false;

                        // Merge stacks - modify the target
                        for (int i = 0; i < newStack.Length; i++)
                        {
                            var oldDefs = branchTarget.StackBefore[i].Definitions;
                            var newDefs = oldDefs.Union(newStack[i].Definitions);
                            if (newDefs.Length > oldDefs.Length)
                            {
                                branchTarget.StackBefore[i] = new StackSlot(newDefs, null);
                                modified = true;
                            }
                        }

                        // Merge variables - modify the target
                        for (int i = 0; i < newVariableState.Length; i++)
                        {
                            var oldSlot = branchTarget.VariablesBefore[i];
                            var newSlot = newVariableState[i];
                            if (!oldSlot.UnknownDefinition)
                            {
                                if (newSlot.UnknownDefinition)
                                {
                                    branchTarget.VariablesBefore[i] = newSlot;
                                    modified = true;
                                }
                                else
                                {
                                    ByteCode[] oldDefs = oldSlot.Definitions;
                                    ByteCode[] newDefs = CollectionExtensions.Union(oldDefs, newSlot.Definitions);
                                    if (newDefs.Length > oldDefs.Length)
                                    {
                                        branchTarget.VariablesBefore[i] = new VariableSlot(newDefs, false);
                                        modified = true;
                                    }
                                }
                            }
                        }

                        if (modified)
                        {
                            agenda.Push(branchTarget);
                        }
                    }
                }
            }

            // Occasionally the compilers or obfuscators generate unreachable code (which might be intentonally invalid)
            // I belive it is safe to just remove it
            body.RemoveAll(b => b.StackBefore == null);

            // Genertate temporary variables to replace stack
            foreach (var byteCode in body)
            {
                int argIdx = 0;
                int popCount = byteCode.PopCount ?? byteCode.StackBefore.Length;
                for (int i = byteCode.StackBefore.Length - popCount; i < byteCode.StackBefore.Length; i++)
                {
                    var tmpVar = new AstGeneratedVariable(string.Format("arg_{0:X2}_{1}", byteCode.Offset, argIdx), null);
                    byteCode.StackBefore[i] = new StackSlot(byteCode.StackBefore[i].Definitions, tmpVar);
                    foreach (ByteCode pushedBy in byteCode.StackBefore[i].Definitions)
                    {
                        if (pushedBy.StoreTo == null)
                        {
                            pushedBy.StoreTo = new List<AstVariable>(1);
                        }
                        pushedBy.StoreTo.Add(tmpVar);
                    }
                    argIdx++;
                }
            }

            // Try to use single temporary variable insted of several if possilbe (especially useful for dup)
            // This has to be done after all temporary variables are assigned so we know about all loads
            foreach (var byteCode in body)
            {
                if (byteCode.StoreTo != null && byteCode.StoreTo.Count > 1)
                {
                    var locVars = byteCode.StoreTo;
                    // For each of the variables, find the location where it is loaded - there should be preciesly one
                    var loadedBy = locVars.Select(locVar => body.SelectMany(bc => bc.StackBefore).Single(s => s.LoadFrom == locVar)).ToList();
                    // We now know that all the variables have a single load,
                    // Let's make sure that they have also a single store - us
                    if (loadedBy.All(slot => slot.Definitions.Length == 1 && slot.Definitions[0] == byteCode))
                    {
                        // Great - we can reduce everything into single variable
                        var tmpVar = new AstGeneratedVariable(string.Format("expr_{0:X2}", byteCode.Offset), locVars.Select(x => x.OriginalName).FirstOrDefault());
                        byteCode.StoreTo = new List<AstVariable>() { tmpVar };
                        foreach (var bc in body)
                        {
                            for (int i = 0; i < bc.StackBefore.Length; i++)
                            {
                                // Is it one of the variable to be merged?
                                if (locVars.Contains(bc.StackBefore[i].LoadFrom))
                                {
                                    // Replace with the new temp variable
                                    bc.StackBefore[i] = new StackSlot(bc.StackBefore[i].Definitions, tmpVar);
                                }
                            }
                        }
                    }
                }
            }

            // Split and convert the normal local variables
            ConvertLocalVariables(body);

            // Convert branch targets to labels and references to xreferences
            foreach (var byteCode in body)
            {
                if (byteCode.Operand is Instruction[])
                {
                    byteCode.Operand = (from target in (Instruction[])byteCode.Operand select instrToByteCode[target].Label).ToArray();
                }
                else if (byteCode.Operand is Instruction)
                {
                    byteCode.Operand = instrToByteCode[(Instruction)byteCode.Operand].Label;
                }
                else if (byteCode.Operand is FieldReference)
                {
                    byteCode.Operand = XBuilder.AsFieldReference(module, (FieldReference)byteCode.Operand);
                }
                else if (byteCode.Operand is MethodReference)
                {
                    byteCode.Operand = XBuilder.AsMethodReference(module, (MethodReference)byteCode.Operand);
                }
                else if (byteCode.Operand is TypeReference)
                {
                    byteCode.Operand = XBuilder.AsTypeReference(module, (TypeReference)byteCode.Operand);
                }
            }

            // Convert parameters to ILVariables
            ConvertParameters(body);

            return body;
        }

        static bool IsDeterministicLdloca(ByteCode b)
        {
            var v = b.Operand;
            b = b.Next;
            if (b.Code == AstCode.Initobj) return true;

            // instance method calls on value types use the variable ref deterministically
            int stack = 1;
            while (true)
            {
                if (b.PopCount == null) return false;
                stack -= b.PopCount.GetValueOrDefault();
                if (stack == 0) break;
                if (stack < 0) return false;
                if (b.Code.IsConditionalControlFlow() || b.Code.IsUnconditionalControlFlow()) return false;
                switch (b.Code)
                {
                    case AstCode.Ldloc:
                    case AstCode.Ldloca:
                    case AstCode.Stloc:
                        if (b.Operand == v) return false;
                        break;
                }
                stack += b.PushCount;
                b = b.Next;
                if (b == null) return false;
            }
            if (b.Code == AstCode.Ldfld || b.Code == AstCode.Stfld)
                return true;
            return (b.Code == AstCode.Call || b.Code == AstCode.Callvirt) && ((MethodReference)b.Operand).HasThis;
        }

        /// <summary>
        /// If possible, separates local variables into several independent variables.
        /// It should undo any compilers merging.
        /// </summary>
        void ConvertLocalVariables(List<ByteCode> body)
        {
            foreach (var varDef in methodDef.Body.Variables)
            {

                // Find all definitions and uses of this variable
                var defs = body.Where(b => b.Operand == varDef && b.IsVariableDefinition).ToList();
                var uses = body.Where(b => b.Operand == varDef && !b.IsVariableDefinition).ToList();

                List<VariableInfo> newVars;

                // If the variable is pinned, use single variable.
                // If any of the uses is from unknown definition, use single variable
                // If any of the uses is ldloca with a nondeterministic usage pattern, use  single variable
                if (!optimize || varDef.IsPinned || uses.Any(b => b.VariablesBefore[varDef.Index].UnknownDefinition || (b.Code == AstCode.Ldloca && !IsDeterministicLdloca(b))))
                {
                    newVars = new List<VariableInfo>(1) { new VariableInfo() {
						Variable = new AstILVariable(
							string.IsNullOrEmpty(varDef.Name) ? "var_" + varDef.Index : varDef.Name,
							XBuilder.AsTypeReference(module, varDef.IsPinned ? ((PinnedType)varDef.VariableType).ElementType : varDef.VariableType),
							varDef),
						Defs = defs,
						Uses = uses
					}};
                }
                else
                {
                    // Create a new variable for each definition
                    newVars = defs.Select(def => new VariableInfo()
                    {
                        Variable = new AstILVariable(
                            (string.IsNullOrEmpty(varDef.Name) ? "var_" + varDef.Index : varDef.Name) + "_" + def.Offset.ToString("X2"),
                            XBuilder.AsTypeReference(module, varDef.VariableType),
                            varDef),
                        Defs = new List<ByteCode> { def },
                        Uses = new List<ByteCode>()
                    }).ToList();

                    // VB.NET uses the 'init' to allow use of uninitialized variables.
                    // We do not really care about them too much - if the original variable
                    // was uninitialized at that point it means that no store was called and
                    // thus all our new variables must be uninitialized as well.
                    // So it does not matter which one we load.

                    // TODO: We should add explicit initialization so that C# code compiles.
                    // Remember to handle cases where one path inits the variable, but other does not.

                    // Add loads to the data structure; merge variables if necessary
                    foreach (ByteCode use in uses)
                    {
                        ByteCode[] useDefs = use.VariablesBefore[varDef.Index].Definitions;
                        if (useDefs.Length == 1)
                        {
                            VariableInfo newVar = newVars.Single(v => v.Defs.Contains(useDefs[0]));
                            newVar.Uses.Add(use);
                        }
                        else
                        {
                            List<VariableInfo> mergeVars = newVars.Where(v => v.Defs.Intersect(useDefs).Any()).ToList();
                            VariableInfo mergedVar = new VariableInfo()
                            {
                                Variable = mergeVars[0].Variable,
                                Defs = mergeVars.SelectMany(v => v.Defs).ToList(),
                                Uses = mergeVars.SelectMany(v => v.Uses).ToList()
                            };
                            mergedVar.Uses.Add(use);
                            newVars = newVars.Except(mergeVars).ToList();
                            newVars.Add(mergedVar);
                        }
                    }
                }

                // Set bytecode operands
                foreach (VariableInfo newVar in newVars)
                {
                    foreach (ByteCode def in newVar.Defs)
                    {
                        def.Operand = newVar.Variable;
                    }
                    foreach (ByteCode use in newVar.Uses)
                    {
                        use.Operand = newVar.Variable;
                    }
                }
            }
        }

        public List<AstVariable> Parameters = new List<AstVariable>();

        void ConvertParameters(List<ByteCode> body)
        {
            AstVariable thisParameter = null;
            if (methodDef.HasThis)
            {
                TypeReference type = methodDef.DeclaringType;
                thisParameter = new AstILVariable("this", XBuilder.AsTypeReference(module, type.IsValueType ? new ByReferenceType(type) : type), methodDef.Body.ThisParameter);
            }
            foreach (var p in methodDef.Parameters)
            {
                Parameters.Add(new AstILVariable(p.Name, XBuilder.AsTypeReference(module, p.ParameterType), p));
            }
            if (Parameters.Count > 0 && (methodDef.IsSetter || methodDef.IsAddOn || methodDef.IsRemoveOn))
            {
                // last parameter must be 'value', so rename it
                Parameters.Last().Name = "value";
            }
            foreach (ByteCode byteCode in body)
            {
                ParameterDefinition p;
                switch (byteCode.Code)
                {
                    case AstCode.__Ldarg:
                        p = (ParameterDefinition)byteCode.Operand;
                        byteCode.Code = AstCode.Ldloc;
                        byteCode.Operand = p.Index < 0 ? thisParameter : this.Parameters[p.Index];
                        break;
                    case AstCode.__Starg:
                        p = (ParameterDefinition)byteCode.Operand;
                        byteCode.Code = AstCode.Stloc;
                        byteCode.Operand = p.Index < 0 ? thisParameter : this.Parameters[p.Index];
                        break;
                    case AstCode.__Ldarga:
                        p = (ParameterDefinition)byteCode.Operand;
                        byteCode.Code = AstCode.Ldloca;
                        byteCode.Operand = p.Index < 0 ? thisParameter : this.Parameters[p.Index];
                        break;
                }
            }
            if (thisParameter != null)
                this.Parameters.Add(thisParameter);
        }

        List<AstNode> ConvertToAst(List<ByteCode> body, HashSet<ExceptionHandler> ehs)
        {
            var ast = new List<AstNode>();

            while (ehs.Any())
            {
                var tryCatchBlock = new AstTryCatchBlock(null);

                // Find the first and widest scope
                var tryStart = ehs.Min(eh => eh.TryStart.Offset);
                var tryEnd = ehs.Where(eh => eh.TryStart.Offset == tryStart).Max(eh => eh.TryEnd.Offset);
                var handlers = ehs.Where(eh => eh.TryStart.Offset == tryStart && eh.TryEnd.Offset == tryEnd).OrderBy(eh => eh.TryStart.Offset).ToList();

                // Remember that any part of the body migt have been removed due to unreachability

                // Cut all instructions up to the try block
                {
                    var tryStartIdx = 0;
                    while (tryStartIdx < body.Count && body[tryStartIdx].Offset < tryStart) tryStartIdx++;
                    ast.AddRange(ConvertToAst(CollectionExtensions.CutRange(body, 0, tryStartIdx)));
                }

                // Cut the try block
                {
                    var nestedEHs = new HashSet<ExceptionHandler>(
                        ehs.Where(eh => (tryStart <= eh.TryStart.Offset && eh.TryEnd.Offset < tryEnd) || (tryStart < eh.TryStart.Offset && eh.TryEnd.Offset <= tryEnd)));
                    ehs.ExceptWith(nestedEHs);
                    var tryEndIdx = 0;
                    while (tryEndIdx < body.Count && body[tryEndIdx].Offset < tryEnd) tryEndIdx++;
                    var converted = ConvertToAst(CollectionExtensions.CutRange(body, 0, tryEndIdx), nestedEHs);
                    tryCatchBlock.TryBlock = new AstBlock(converted.Select(x => x.SourceLocation).FirstOrDefault(), converted);
                }

                // Cut all handlers
                tryCatchBlock.CatchBlocks.Clear();
                foreach (var eh in handlers)
                {
                    var handlerEndOffset = eh.HandlerEnd == null ? methodDef.Body.CodeSize : eh.HandlerEnd.Offset;
                    var startIdx = 0;
                    while (startIdx < body.Count && body[startIdx].Offset < eh.HandlerStart.Offset) startIdx++;
                    var endIdx = 0;
                    while (endIdx < body.Count && body[endIdx].Offset < handlerEndOffset) endIdx++;
                    var nestedEHs = new HashSet<ExceptionHandler>(ehs.Where(e => (eh.HandlerStart.Offset <= e.TryStart.Offset && e.TryEnd.Offset < handlerEndOffset) || (eh.HandlerStart.Offset < e.TryStart.Offset && e.TryEnd.Offset <= handlerEndOffset)));
                    ehs.ExceptWith(nestedEHs);
                    var handlerAst = ConvertToAst(CollectionExtensions.CutRange(body, startIdx, endIdx - startIdx), nestedEHs);
                    if (eh.HandlerType == ExceptionHandlerType.Catch)
                    {
                        var catchType = eh.CatchType.IsSystemObject() ? module.TypeSystem.Exception : XBuilder.AsTypeReference(module, eh.CatchType);
                        var catchBlock = new AstTryCatchBlock.CatchBlock(handlerAst.Select(x => x.SourceLocation).FirstOrDefault(), tryCatchBlock)
                        {
                            ExceptionType = catchType,
                            Body = handlerAst
                        };
                        // Handle the automatically pushed exception on the stack
                        var ldexception = ldexceptions[eh];
                        if (ldexception.StoreTo == null || ldexception.StoreTo.Count == 0)
                        {
                            // Exception is not used
                            catchBlock.ExceptionVariable = null;
                        }
                        else if (ldexception.StoreTo.Count == 1)
                        {
                            var first = catchBlock.Body[0] as AstExpression;
                            if (first != null &&
                                first.Code == AstCode.Pop &&
                                first.Arguments[0].Code == AstCode.Ldloc &&
                                first.Arguments[0].Operand == ldexception.StoreTo[0])
                            {
                                // The exception is just poped - optimize it all away;
                                if (context.Settings.AlwaysGenerateExceptionVariableForCatchBlocks)
                                    catchBlock.ExceptionVariable = new AstGeneratedVariable("ex_" + eh.HandlerStart.Offset.ToString("X2"), null);
                                else
                                    catchBlock.ExceptionVariable = null;
                                catchBlock.Body.RemoveAt(0);
                            }
                            else
                            {
                                catchBlock.ExceptionVariable = ldexception.StoreTo[0];
                            }
                        }
                        else
                        {
                            var exTemp = new AstGeneratedVariable("ex_" + eh.HandlerStart.Offset.ToString("X2"), null);
                            catchBlock.ExceptionVariable = exTemp;
                            foreach (var storeTo in ldexception.StoreTo)
                            {
                                catchBlock.Body.Insert(0, new AstExpression(catchBlock.SourceLocation, AstCode.Stloc, storeTo, new AstExpression(catchBlock.SourceLocation, AstCode.Ldloc, exTemp)));
                            }
                        }
                        tryCatchBlock.CatchBlocks.Add(catchBlock);
                    }
                    else if (eh.HandlerType == ExceptionHandlerType.Finally)
                    {
                        tryCatchBlock.FinallyBlock = new AstBlock(handlerAst);
                    }
                    else if (eh.HandlerType == ExceptionHandlerType.Fault)
                    {
                        tryCatchBlock.FaultBlock = new AstBlock(handlerAst);
                    }
                    else
                    {
                        // TODO: ExceptionHandlerType.Filter
                    }
                }

                ehs.ExceptWith(handlers);

                ast.Add(tryCatchBlock);
            }

            // Add whatever is left
            ast.AddRange(ConvertToAst(body));

            return ast;
        }

        private List<AstNode> ConvertToAst(IEnumerable<ByteCode> body)
        {
            var ast = new List<AstNode>();

            // Convert stack-based IL code to ILAst tree
            foreach (var byteCode in body)
            {
                var ilRange = new InstructionRange(byteCode.Offset, byteCode.EndOffset);

                if (byteCode.StackBefore == null)
                {
                    // Unreachable code
                    continue;
                }

                var expr = new AstExpression(byteCode.SequencePoint, byteCode.Code, byteCode.Operand);
                expr.ILRanges.Add(ilRange);
                if (byteCode.Prefixes != null && byteCode.Prefixes.Length > 0)
                {
                    var prefixes = new AstExpressionPrefix[byteCode.Prefixes.Length];
                    for (var i = 0; i < prefixes.Length; i++)
                    {
                        var operand = byteCode.Prefixes[i].Operand;
                        if (operand is FieldReference)
                        {
                            operand = XBuilder.AsFieldReference(module, (FieldReference) operand);
                        }
                        else if (operand is MethodReference)
                        {
                            operand = XBuilder.AsMethodReference(module, (MethodReference)operand);
                        }
                        else if (operand is TypeReference)
                        {
                            operand = XBuilder.AsTypeReference(module, (TypeReference) operand);
                        }
                        prefixes[i] = new AstExpressionPrefix((AstCode)byteCode.Prefixes[i].OpCode.Code, operand);
                    }
                    expr.Prefixes = prefixes;
                }

                // Label for this instruction
                if (byteCode.Label != null)
                {
                    ast.Add(byteCode.Label);
                }

                // Reference arguments using temporary variables
                var popCount = byteCode.PopCount ?? byteCode.StackBefore.Length;
                for (var i = byteCode.StackBefore.Length - popCount; i < byteCode.StackBefore.Length; i++)
                {
                    var slot = byteCode.StackBefore[i];
                    expr.Arguments.Add(new AstExpression(byteCode.SequencePoint, AstCode.Ldloc, slot.LoadFrom));
                }

                // Store the result to temporary variable(s) if needed
                if (byteCode.StoreTo == null || byteCode.StoreTo.Count == 0)
                {
                    ast.Add(expr);
                }
                else if (byteCode.StoreTo.Count == 1)
                {
                    ast.Add(new AstExpression(byteCode.SequencePoint, AstCode.Stloc, byteCode.StoreTo[0], expr));
                }
                else
                {
                    var tmpVar = new AstGeneratedVariable("expr_" + byteCode.Offset.ToString("X2"), byteCode.StoreTo.Select(x => x.OriginalName).FirstOrDefault());
                    ast.Add(new AstExpression(byteCode.SequencePoint, AstCode.Stloc, tmpVar, expr));
                    foreach (var storeTo in byteCode.StoreTo.AsEnumerable().Reverse())
                    {
                        ast.Add(new AstExpression(byteCode.SequencePoint, AstCode.Stloc, storeTo, new AstExpression(byteCode.SequencePoint, AstCode.Ldloc, tmpVar)));
                    }
                }
            }

            return ast;
        }

        private sealed class VariableInfo
        {
            public AstVariable Variable;
            public List<ByteCode> Defs;
            public List<ByteCode> Uses;
        }
    }
}
