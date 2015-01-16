using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Optimizer;
using Dot42.CompilerLib.XModel;
using Dot42.JvmClassLib;
using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Bytecode;
using Dot42.Utility;
using MethodDefinition = Dot42.JvmClassLib.MethodDefinition;

namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts java bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        private readonly XModule module;
        private readonly XTypeSystem typeSystem;
        private readonly MethodDefinition methodDef;
        private readonly XTypeDefinition declaringType;
        private readonly bool optimize;
        private readonly CodeAttribute codeAttr;
        private readonly List<AstVariable> parameters = new List<AstVariable>();
        private readonly List<ExceptionHandler> validExceptionHandlers;

        // Virtual instructions to load exception on stack
        private readonly Dictionary<ExceptionHandler, ByteCode> ldexceptions = new Dictionary<ExceptionHandler, ByteCode>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstBuilder(XModule module, MethodDefinition methodDef, XTypeDefinition declaringType, bool optimize)
        {
            this.module = module;
            typeSystem = module.TypeSystem;
            this.methodDef = methodDef;
            this.declaringType = declaringType;
            this.optimize = optimize;
            codeAttr = methodDef.Attributes.OfType<CodeAttribute>().FirstOrDefault();
            validExceptionHandlers = (codeAttr != null) ? codeAttr.ExceptionHandlers.Where(IsValid).ToList() : null;
        }

        /// <summary>
        /// Gets the return type of the method as .NET type reference.
        /// </summary>
        public XTypeReference ReturnType
        {
            get { return AsTypeReference(methodDef.ReturnType, XTypeUsageFlags.ReturnType); }
        }

        /// <summary>
        /// Convert the given method to a list of Ast nodes.
        /// </summary>
        public AstBlock Build()
        {
            if ((codeAttr == null) || (codeAttr.Code.Length == 0))
                return new AstBlock((ISourceLocation)null);

            var body = StackAnalysis();
            var offset2ByteCode = new Dictionary<int, ByteCode>();
            foreach (var bc in body)
            {
                offset2ByteCode[bc.Offset] = bc;
            }
            var blockStarts = SplitInBlocks(body, validExceptionHandlers);
            var list = ConvertToAst(body, new HashSet<ExceptionHandler>(validExceptionHandlers), blockStarts, 0, offset2ByteCode);
            var ast = new AstBlock(list.Select(x => x.SourceLocation).FirstOrDefault(), list);

            if (methodDef.IsSynchronized)
            {
                // Wrap in synchronization block
                var sl = ast.SourceLocation;
                var tryCatch = new AstTryCatchBlock(sl);
                // try-lock(this)
                var lockExpr = methodDef.IsStatic ?
                    new AstExpression(sl, AstCode.TypeOf, declaringType) :
                    new AstExpression(sl, AstCode.Ldthis, null);
                ast.Body.Insert(0, new AstExpression(sl, AstCode.Call, MonitorMethodReference("Enter"), new AstExpression(lockExpr)));
                tryCatch.TryBlock = ast;
                // finally-unlock(this)
                tryCatch.FinallyBlock = new AstBlock(sl, 
                    new AstExpression(sl, AstCode.Call, MonitorMethodReference("Exit"), new AstExpression(lockExpr)),
                    new AstExpression(sl, AstCode.Endfinally, null));
                // Wrap try/catch in block
                ast = new AstBlock(sl, tryCatch);
            }

            return ast;
        }

        /// <summary>
        /// Is the given exception handler a valid one?
        /// The java compiler tends to add rediculous handlers.
        /// </summary>
        private static bool IsValid(ExceptionHandler handler)
        {
            return (handler.StartPc != handler.HandlerPc);
        }

        /// <summary>
        /// Analyse the instructions in the method code and convert them to a ByteCode list.
        /// </summary>
        private List<ByteCode> StackAnalysis()
        {
            // Map from instruction to bytecode.
            var instrToByteCode = new Dictionary<Instruction, ByteCode>();

            // Create temporary structure for the stack analysis
            var body = new List<ByteCode>(codeAttr.Code.Length);
            foreach (var inst in codeAttr.Instructions)
            {
                var first = true;
                foreach (var byteCode in Create(inst, module))
                {
                    if (first)
                    {
                        instrToByteCode[inst] = byteCode;
                        first = false;
                    }
                    body.Add(byteCode);
                }
            }
            // Connect bytecodes to the next
            for (var i = 0; i < body.Count - 1; i++)
            {
                body[i].Next = body[i + 1];
            }

            var agenda = new Stack<ByteCode>();
            var localVarsCount = codeAttr.MaxLocals; // methodDef.GetParametersLocalVariableSlots();

            // All bytecodes that are the start of an exception handler.
            var exceptionHandlerStarts = new HashSet<ByteCode>(validExceptionHandlers.Select(eh => instrToByteCode[eh.Handler]));
            var exceptionTryStarts = new DefaultDictionary<ByteCode, List<ExceptionHandler>>(x => new List<ExceptionHandler>());
            foreach (var eh in validExceptionHandlers)
            {
                exceptionTryStarts[instrToByteCode[eh.Start]].Add(eh);
            }

            // Add known states
            var ldExceptionByHandlerPc = new Dictionary<int, ByteCode>();
            foreach (var ex in validExceptionHandlers)
            {
                ByteCode ldexception;
                if (ldExceptionByHandlerPc.TryGetValue(ex.HandlerPc, out ldexception))
                {
                    // Re-use ldexception (that we've created for the same handler PC for another exception handler before)
                }
                else
                {
                    // No handler at handlerPc processed before, do that now
                    var handlerStart = instrToByteCode[ex.Handler];
                    handlerStart.StackBefore = new StackSlot[0];
                    handlerStart.VariablesBefore = VariableSlot.MakeUnknownState(localVarsCount);
                    {
                        // Catch handlers start with the exeption on the stack
                        ldexception = new ByteCode {
                            Code = AstCode.Ldexception,
                            Operand = ex.CatchType,
                            PopCount = 0,
                            PushCount = 1,
                            Offset = handlerStart.Offset,
                            Next = handlerStart,
                            StackBefore = new StackSlot[0],
                            VariablesBefore = handlerStart.VariablesBefore
                        };
                        handlerStart.StackBefore = new[] { new StackSlot(new[] { ldexception }, null) };
                    }
                    ldExceptionByHandlerPc[ex.HandlerPc] = ldexception;
                    agenda.Push(handlerStart);
                }
                // Store ldexception by exception handler
                ldexceptions[ex] = ldexception;
            }

            // At the start of the method the stack is empty and all local variables have unknown state
            body[0].StackBefore = new StackSlot[0];
            body[0].VariablesBefore = VariableSlot.MakeUnknownState(localVarsCount);
            agenda.Push(body[0]);

            // Process agenda
            while (agenda.Count > 0)
            {
                var byteCode = agenda.Pop();

                // Calculate new stack
                var newStack = byteCode.CreateNewStack();

                // Calculate new variable state
                var newVariableState = VariableSlot.CloneVariableState(byteCode.VariablesBefore);
                if (byteCode.IsVariableDefinition)
                {
                    newVariableState[((LocalVariableReference)byteCode.Operand).Index] = new VariableSlot(new[] { byteCode }, false);
                }

                // After the leave, finally block might have touched the variables
                if (byteCode.Code == AstCode.Leave)
                {
                    newVariableState = VariableSlot.MakeUnknownState(localVarsCount);
                }

                // Find all successors
                var branchTargets = FindBranchTargets(byteCode, instrToByteCode, exceptionHandlerStarts);

                // Apply the state to successors
                foreach (var branchTarget in branchTargets)
                {
                    UpdateBranchTarget(byteCode, branchTarget, (branchTargets.Count == 1), newStack, newVariableState, agenda);
                }

                // Apply state to handlers when a branch target is the start of an exception handler
                foreach (var branchTarget in branchTargets.Where(exceptionTryStarts.ContainsKey))
                {
                    // The branch target is the start of a try block.
                    UpdateTryStartBranchTarget(branchTarget, exceptionTryStarts[branchTarget], instrToByteCode, newVariableState, agenda);
                }
            }

            // Occasionally the compilers or obfuscators generate unreachable code (which might be intentonally invalid)
            // I believe it is safe to just remove it
            body.RemoveAll(b => b.StackBefore == null);

            // Generate temporary variables to replace stack
            foreach (var byteCode in body)
            {
                var argIdx = 0;
                var popCount = byteCode.PopCount ?? byteCode.StackBefore.Length;
                for (var i = byteCode.StackBefore.Length - popCount; i < byteCode.StackBefore.Length; i++)
                {
                    var tmpVar = new AstGeneratedVariable(string.Format("arg_{0:X2}_{1}", byteCode.Offset, argIdx), null);
                    byteCode.StackBefore[i] = new StackSlot(byteCode.StackBefore[i].Definitions, tmpVar);
                    foreach (var pushedBy in byteCode.StackBefore[i].Definitions)
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

            // Try to use single temporary variable insted of several if possible (especially useful for dup)
            // This has to be done after all temporary variables are assigned so we know about all loads
            foreach (var byteCode in body)
            {
                if ((byteCode.StoreTo == null) || (byteCode.StoreTo.Count <= 1)) 
                    continue;

                var locVars = byteCode.StoreTo;
                // For each of the variables, find the location where it is loaded - there should be preciesly one
                var loadedBy = locVars.Select(locVar => body.SelectMany(bc => bc.StackBefore).Single(s => s.LoadFrom == locVar)).ToList();
                // We now know that all the variables have a single load,
                // Let's make sure that they have also a single store - us
                if (loadedBy.All(slot => (slot.Definitions.Length == 1) && (slot.Definitions[0] == byteCode)))
                {
                    // Great - we can reduce everything into single variable
                    var tmpVar = new AstGeneratedVariable(string.Format("expr_{0:X2}", byteCode.Offset), locVars.Select(x => x.OriginalName).FirstOrDefault());
                    byteCode.StoreTo = new List<AstVariable> { tmpVar };
                    foreach (var bc in body)
                    {
                        for (var i = 0; i < bc.StackBefore.Length; i++)
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

            // Split and convert the normal local variables
            ConvertLocalVariables(body);

            // Convert branch targets to labels
            foreach (var byteCode in body)
            {
                if (byteCode.Operand is Instruction[])
                {
                    byteCode.Operand = (from target in (Instruction[])byteCode.Operand select instrToByteCode[target].Label(true)).ToArray();
                }
                else if (byteCode.Operand is Instruction)
                {
                    byteCode.Operand = instrToByteCode[(Instruction)byteCode.Operand].Label(true);
                }
                else if (byteCode.Operand is LookupSwitchData)
                {
                    var data = (LookupSwitchData) byteCode.Operand;
                    byteCode.Operand = data.Pairs.Select(x => new AstLabelKeyPair(instrToByteCode[x.Target].Label(true), x.Match)).ToArray();
                }
            }

            // Convert parameters to ILVariables
            ConvertParameters(body);

            // Replace temporary opcodes
            foreach (var byteCode in body)
            {
                switch (byteCode.Code)
                {
                    case AstCode.Dup_x1:
                    case AstCode.Dup_x2:
                    case AstCode.Dup2:
                    case AstCode.Dup2_x1:
                    case AstCode.Dup2_x2:
                    case AstCode.Swap:
                        byteCode.Code = AstCode.Dup;
                        break;
                    case AstCode.Pop2:
                        byteCode.Code = AstCode.Pop;
                        break;
                }
            }

            return body;
        }

        /// <summary>
        /// Find all targets the given bytecode can branch to.
        /// </summary>
        private static List<ByteCode> FindBranchTargets(ByteCode byteCode, Dictionary<Instruction, ByteCode> instrToByteCode, HashSet<ByteCode> exceptionHandlerStarts)
        {
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
            var operandAsInstructionArray = byteCode.Operand as Instruction[];
            if (operandAsInstructionArray != null)
            {
                branchTargets.AddRange(operandAsInstructionArray.Select(inst => instrToByteCode[inst]));
            }
            else
            {
                var operandAsInstruction = byteCode.Operand as Instruction;
                if (operandAsInstruction != null)
                {
                    var target = instrToByteCode[operandAsInstruction];
                    branchTargets.Add(target);
                }
                else
                {
                    var operandAsLookupSwitchData = byteCode.Operand as LookupSwitchData;
                    if (operandAsLookupSwitchData != null)
                    {
                        branchTargets.AddRange(operandAsLookupSwitchData.Pairs.Select(pair => instrToByteCode[pair.Target]));
                        // Default target is ignored here because a a Br follows.
                    }
                }
            }
            return branchTargets;
        }

        /// <summary>
        /// Update the state of the given branch targets (of the given bytecode).
        /// Add modified branch targets to the given agenda.
        /// </summary>
        private static void UpdateBranchTarget(ByteCode byteCode, ByteCode branchTarget, bool canShareNewState, StackSlot[] newStack, VariableSlot[] newVariableState, Stack<ByteCode> agenda)
        {
            if ((branchTarget.StackBefore == null) && (branchTarget.VariablesBefore == null))
            {
                // Branch target has not been processed at all
                if (canShareNewState)
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
                return;
            }

            // If we get here, the branch target has been processed before.
            // See the the stack size is the same and merge the state where needed.
            if ((branchTarget.StackBefore == null) || (branchTarget.StackBefore.Length != newStack.Length))
            {
                throw new Exception("Inconsistent stack size at " + byteCode.Name);
            }

            // Be careful not to change our new data - it might be reused for several branch targets.
            // In general, be careful that two bytecodes never share data structures.
            var modified = false;

            // Merge stacks - modify the target
            for (var i = 0; i < newStack.Length; i++)
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
            for (var i = 0; i < newVariableState.Length; i++)
            {
                var oldSlot = branchTarget.VariablesBefore[i];
                var newSlot = newVariableState[i];
                if (oldSlot.UnknownDefinition)
                    continue;

                if (newSlot.UnknownDefinition)
                {
                    branchTarget.VariablesBefore[i] = newSlot;
                    modified = true;
                }
                else
                {
                    var oldDefs = oldSlot.Definitions;
                    var newDefs = oldDefs.Union(newSlot.Definitions);
                    if (newDefs.Length > oldDefs.Length)
                    {
                        branchTarget.VariablesBefore[i] = new VariableSlot(newDefs, false);
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                agenda.Push(branchTarget);
            }
        }

        /// <summary>
        /// Update the variable state of handlers for the exception handlers that start at the given branch target.
        /// </summary>
        private void UpdateTryStartBranchTarget(ByteCode branchTarget, IEnumerable<ExceptionHandler> exceptionHandlers, Dictionary<Instruction, ByteCode> instrToByteCode, VariableSlot[] newVariableState, Stack<ByteCode> agenda)
        {
            // The branch target is the start of a try block.
            // Collection all exception handler that share the same handler as this try block.
            var ehs = new HashSet<ExceptionHandler>(exceptionHandlers);
            foreach (var eh in validExceptionHandlers.Where(x => ehs.Any(y => y.HandlerPc == x.HandlerPc)))
            {
                ehs.Add(eh);
            }

            // Collection the variables defined in all try block's of the collected exception handlers.
            var defs = new HashSet<LocalVariableReference>();
            foreach (var eh in ehs)
            {
                GetDefinedVariables(instrToByteCode[eh.Start], instrToByteCode[eh.End], defs);
            }

            // Merge variables of handlers - modify the target
            foreach (var handler in ehs.Select(x => instrToByteCode[x.Handler]))
            {
                var modified = false;
                for (var i = 0; i < newVariableState.Length; i++)
                {
                    // Update variables unless it is defined
                    if (defs.Any(x => x.Index == i))
                        continue;

                    var oldSlot = handler.VariablesBefore[i];
                    var newSlot = newVariableState[i];

                    if (newSlot.UnknownDefinition)
                    {
                        if (!oldSlot.UnknownDefinition)
                        {
                            handler.VariablesBefore[i] = newSlot;
                            modified = true;
                        }
                    }
                    else
                    {
                        var oldDefs = oldSlot.Definitions;
                        var newDefs = oldDefs.Union(newSlot.Definitions);
                        if (newDefs.Length > oldDefs.Length)
                        {
                            handler.VariablesBefore[i] = new VariableSlot(newDefs, false);
                            modified = true;
                        }
                    }
                }
                if (modified)
                {
                    agenda.Push(handler);
                }
            }
        }

        /// <summary>
        /// Gets all variables that are defined in the given range of bytes (start - (exclusive) end).
        /// </summary>
        private static void GetDefinedVariables(ByteCode start, ByteCode end, HashSet<LocalVariableReference> defs)
        {
            while ((start != null) && (start != end))
            {
                if (start.IsVariableDefinition)
                {
                    var varDef = start.Operand as LocalVariableReference;
                    if (varDef != null) defs.Add(varDef);
                }
                start = start.Next;
            }
        }

        /// <summary>
        /// If possible, separates local variables into several independent variables.
        /// It should undo any compilers merging.
        /// </summary>
        private void ConvertLocalVariables(List<ByteCode> body)
        {
            foreach (var varDef in methodDef.Body.Variables)
            {
                // Find all definitions and uses of this variable
                var refs = body.Where(b => (b.Operand is LocalVariableReference) && (((LocalVariableReference)b.Operand) == varDef)).ToList();
                var defs = refs.Where(b => b.IsVariableDefinition).ToList();
                var uses = refs.Where(b => !b.IsVariableDefinition).ToList();

                List<VariableInfo> newVars;

                // If any of the uses is from unknown definition, use single variable
                if (!optimize || uses.Any(b => b.VariablesBefore[varDef.Index].UnknownDefinition))
                {
                    newVars = new List<VariableInfo>(1) {
                        new VariableInfo {
                            Variable = new AstJavaVariable(null, varDef, "var_" + varDef.Index),
                            Defs = defs,
                            Uses = uses
                        }
                    };
                }
                else
                {
                    // Create a new variable for each definition
                    newVars = defs.Select(def => new VariableInfo {
                        Variable = new AstJavaVariable(null, varDef, "var_" + varDef.Index + "_" + def.Offset.ToString("X2")),
                        Defs = new List<ByteCode> { def },
                        Uses = new List<ByteCode>()
                    }).ToList();

                    // Add loads to the data structure; merge variables if necessary
                    foreach (var use in uses)
                    {
                        var useDefs = use.VariablesBefore[varDef.Index].Definitions;
                        if (useDefs.Length == 1)
                        {
                            var newVar = newVars.Single(v => v.Defs.Contains(useDefs[0]));
                            newVar.Uses.Add(use);
                        }
                        else
                        {
                            var mergeVars = newVars.Where(v => v.Defs.Intersect(useDefs).Any()).ToList();
                            var mergedVar = new VariableInfo {
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
                foreach (var newVar in newVars)
                {
                    foreach (var def in newVar.Defs)
                    {
                        def.Operand = newVar.Variable;
                        if ((def.Type != null) && (newVar.Variable.Type == null))
                        {
                            newVar.Variable.Type = def.Type;
                        }
                    }
                    foreach (var use in newVar.Uses)
                    {
                        use.Operand = newVar.Variable;
                    }
                }
            }
        }

        /// <summary>
        /// Fill the parameters list.
        /// </summary>
        private void ConvertParameters(List<ByteCode> body)
        {
            AstVariable thisParameter = null;
            if (methodDef.HasThis)
            {
                var type = AsTypeReference(methodDef.DeclaringClass, XTypeUsageFlags.DeclaringType);
                thisParameter = new AstJavaVariable(type, codeAttr.ThisParameter);
            }
            foreach (var p in codeAttr.Parameters)
            {
                var type = AsTypeReference(p.Item1, XTypeUsageFlags.ParameterType);
                parameters.Add(new AstJavaVariable(type, p.Item2));
            }
            if (thisParameter != null)
            {
                parameters.Add(thisParameter);
            }
            foreach (var byteCode in body)
            {
                LocalVariableReference p;
                switch (byteCode.Code)
                {
                    case AstCode.Ldloc:
                    case AstCode.Stloc:
                        p = byteCode.Operand as LocalVariableReference;
                        if (p != null)
                        {
                            var astP = parameters.Cast<AstJavaVariable>().FirstOrDefault(x => x.LocalVariableReference.Index == p.Index);
                            if (astP != null) byteCode.Operand = astP;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Convert the given set of bytecodes to an Ast node list.
        /// Split exception handlers into Ast try/catch blocks.
        /// </summary>
        private List<AstNode> ConvertToAst(List<ByteCode> body, HashSet<ExceptionHandler> ehs, List<ByteCodeBlock> blockStarts, int nestingLevel, Dictionary<int, ByteCode> offset2ByteCode)
        {
            var ast = new List<AstNode>();

            // Split body in blocks

            while (ehs.Any())
            {
                var tryCatchBlock = new AstTryCatchBlock(null);

                // Find the first and widest scope
                var tryStart = ehs.Min(eh => eh.StartPc);
                var tryEnd = ehs.Where(eh => eh.StartPc == tryStart).Max(eh => eh.EndPc);
                var handlers = ehs.Where(eh => (eh.StartPc == tryStart) && (eh.EndPc == tryEnd)).OrderBy(eh => eh.HandlerPc).ToList();

                // Remember that any part of the body migt have been removed due to unreachability

                // Cut all instructions up to the try block
                {
                    var tryStartIdx = 0;
                    while ((tryStartIdx < body.Count) && (body[tryStartIdx].Offset < tryStart))
                    {
                        tryStartIdx++;
                    }
                    if (tryStartIdx > 0)
                    {
                        ast.AddRange(ConvertRangeToAst(body.CutRange(0, tryStartIdx)));
                        // Make sure the block before the try block ends with an unconditional control flow
                        AddUnconditionalBranchToNext(ast, body, 0);
                    }
                }

                // Cut the try block
                {
                    var nestedEHs = new HashSet<ExceptionHandler>(
                        ehs.Where(eh => ((tryStart <= eh.StartPc) && (eh.EndPc < tryEnd)) || ((tryStart < eh.StartPc) && (eh.EndPc <= tryEnd))));
                    ehs.ExceptWith(nestedEHs);
                    var tryEndIdx = 0;
                    while ((tryEndIdx < body.Count) && (body[tryEndIdx].Offset < tryEnd))
                    {
                        tryEndIdx++;
                    }
                    var converted = ConvertToAst(body.CutRange(0, tryEndIdx), nestedEHs, blockStarts, nestingLevel + 1, offset2ByteCode);
                    tryCatchBlock.TryBlock = new AstBlock(converted.Select(x => x.SourceLocation).FirstOrDefault(), converted);
                    // Make sure the try block ends with an unconditional control flow
                    AddUnconditionalBranchToNext(tryCatchBlock.TryBlock.Body, body, 0);
                }

                // Cut all handlers
                tryCatchBlock.CatchBlocks.Clear();
                foreach (var iterator in handlers)
                {
                    var eh = iterator;
                    var handler = offset2ByteCode[eh.HandlerPc]; // body.First(x => x.Offset == eh.HandlerPc);
                    var catchType = eh.IsCatchAll ? typeSystem.Object : AsTypeReference(eh.CatchType, XTypeUsageFlags.CatchType);
                    var catchBlock = new AstTryCatchBlock.CatchBlock(handler.SourceLocation, tryCatchBlock)
                    {
                        ExceptionType = catchType,
                        Body = new List<AstNode>()
                    };

                    // Create catch "body" (actually a jump to the handler)
                    // Handle the automatically pushed exception on the stack
                    var ldexception = ldexceptions[eh];
                    catchBlock.Body.Add(new AstExpression(handler.SourceLocation, AstCode.Br, handler.Label(true)));
                    if (ldexception.StoreTo == null || ldexception.StoreTo.Count == 0)
                    {
                        // Exception is not used
                        catchBlock.ExceptionVariable = null;
                    }
                    else if (ldexception.StoreTo.Count == 1)
                    {
                        /*var first = catchBlock.Body.FirstOrDefault() as AstExpression;
                        if (first != null &&
                            first.Code == AstCode.Pop &&
                            first.Arguments[0].Code == AstCode.Ldloc &&
                            first.Arguments[0].Operand == ldexception.StoreTo[0])
                        {
                            // The exception is just poped - optimize it all away;
                            catchBlock.ExceptionVariable = new AstGeneratedVariable("ex_" + eh.HandlerPc.ToString("X2"));
                            catchBlock.Body.RemoveAt(0);
                        }
                        else*/
                        {
                            catchBlock.ExceptionVariable = ldexception.StoreTo[0];
                        }
                    }
                    else
                    {
                        var exTemp = new AstGeneratedVariable("ex_" + eh.HandlerPc.ToString("X2"), null);
                        catchBlock.ExceptionVariable = exTemp;
                        foreach (var storeTo in ldexception.StoreTo)
                        {
                            catchBlock.Body.Insert(0, new AstExpression(catchBlock.SourceLocation, AstCode.Stloc, storeTo, new AstExpression(catchBlock.SourceLocation, AstCode.Ldloc, exTemp)));
                        }
                    }
                    tryCatchBlock.CatchBlocks.Add(catchBlock);
                }

                ehs.ExceptWith(handlers);

                ast.Add(tryCatchBlock);
            }

            // Add whatever is left
            ast.AddRange(ConvertRangeToAst(body));

            return ast;
        }

        /// <summary>
        /// Add an unconditional branch to the end of the given block (to the label at nextIndex in body), if the last instruction
        /// of the given block is not already an unconditional branch.
        /// </summary>
        private static void AddUnconditionalBranchToNext(List<AstNode> block, List<ByteCode> body, int nextIndex)
        {
            if (block.Count == 0)
                return;
            if (block.Last().IsUnconditionalControlFlow())
                return;
            if (nextIndex >= body.Count)
                return;

            // Add unconditional branch
            var target = body[nextIndex].Label(true);
            block.Add(new AstExpression(block.Last().SourceLocation, AstCode.Br, target));
        }

        /// <summary>
        /// Convert the given set of bytecodes into a list of Ast nodes.
        /// </summary>
        private static IEnumerable<AstNode> ConvertRangeToAst(IEnumerable<ByteCode> range)
        {
            var ast = new List<AstNode>();

            // Convert stack-based java bytecode code to Ast tree
            foreach (var byteCode in range)
            {
                var ilRange = new InstructionRange(byteCode.Offset, byteCode.EndOffset);
                if (byteCode.StackBefore == null)
                {
                    // Unreachable code
                    continue;
                }

                var expr = new AstExpression(byteCode.SourceLocation, byteCode.Code, byteCode.Operand);
                expr.ILRanges.Add(ilRange);

                // Label for this instruction
                if (byteCode.Label(false) != null)
                {
                    ast.Add(byteCode.Label(false));
                }

                // Reference arguments using temporary variables
                var popCount = byteCode.PopCount ?? byteCode.StackBefore.Length;
                for (var i = byteCode.StackBefore.Length - popCount; i < byteCode.StackBefore.Length; i++)
                {
                    var slot = byteCode.StackBefore[i];
                    expr.Arguments.Add(new AstExpression(byteCode.SourceLocation, AstCode.Ldloc, slot.LoadFrom));
                }

                // Store the result to temporary variable(s) if needed
                if (byteCode.StoreTo == null || byteCode.StoreTo.Count == 0)
                {
                    ast.Add(expr);
                }
                else if (byteCode.StoreTo.Count == 1)
                {
                    ast.Add(new AstExpression(byteCode.SourceLocation, AstCode.Stloc, byteCode.StoreTo[0], expr));
                }
                else
                {
                    var tmpVar = new AstGeneratedVariable("expr_" + byteCode.Offset.ToString("X2"), byteCode.StoreTo.Select(x => x.OriginalName).FirstOrDefault());
                    ast.Add(new AstExpression(byteCode.SourceLocation, AstCode.Stloc, tmpVar, expr));
                    foreach (var storeTo in byteCode.StoreTo.AsEnumerable().Reverse())
                    {
                        ast.Add(new AstExpression(byteCode.SourceLocation, AstCode.Stloc, storeTo, new AstExpression(byteCode.SourceLocation, AstCode.Ldloc, tmpVar)));
                    }
                }
            }

            return ast;
        }

        /// <summary>
        /// Split the given body into blocks.
        /// The returned set contains the indexes into body of the start of each block (sorted by offset).
        /// </summary>
        private static List<ByteCodeBlock> SplitInBlocks(List<ByteCode> body, IEnumerable<ExceptionHandler> exceptionHandlers)
        {
            var blockStarts = new HashSet<ByteCode>();
            if (blockStarts.Count > 0)
                blockStarts.Add(body[0]);

            var startNewBlock = false;
            foreach (var byteCode in body)
            {
                if (startNewBlock)
                {
                    blockStarts.Add(byteCode);
                    startNewBlock = false;
                }

                if (byteCode.Code.IsUnconditionalControlFlow() || byteCode.Code.IsConditionalControlFlow())
                {
                    startNewBlock = true;
                }

                var operand = byteCode.Operand;
                if (operand is AstLabel)
                {
                    var target = (AstLabel)operand;
                    blockStarts.Add(body.First(x => x.Label(false) == target));
                }
                else if (operand is AstLabel[])
                {
                    var targets = (AstLabel[])operand;
                    foreach (var target in targets)
                    {
                        blockStarts.Add(body.First(x => x.Label(false) == target));
                    }
                }
            }

            foreach (var eh in exceptionHandlers)
            {
                blockStarts.Add(body.First(x => x.ContainsOffset(eh.StartPc)));
                blockStarts.Add(body.First(x => x.ContainsOffset(eh.EndPc)));
                blockStarts.Add(body.First(x => x.ContainsOffset(eh.HandlerPc)));
            }

            // Collect start points of blocks
            var startByteCodes = body.Where(blockStarts.Contains).ToList();

            // Insert unconditional control flow at end of block
            /*for (var i = 0; i < startByteCodes.Count; i++)
            {
                var next = ((i + 1) < startByteCodes.Count) ? startByteCodes[i + 1] : null;
                if (next != null)
                {
                    var nextIndex = body.IndexOf(next);
                    var last = body[nextIndex - 1];
                    if (!last.Code.IsUnconditionalControlFlow())
                    {
                        // Add unconditional control flow transfer to next block
                        var extra = new ByteCode {
                            Code = AstCode.Br,
                            Operand = next.Label,
                            PopCount = 0,
                            PushCount = 0,
                            Next = next,
                            SourceLocation = last.SourceLocation,
                            Offset = last.Offset
                        };
                        body.Insert(nextIndex, extra);
                    }
                }
            }*/

            // Create blocks
            var result = new List<ByteCodeBlock>();
            for (var i = 0; i < startByteCodes.Count; i++)
            {
                var first = startByteCodes[i];
                var next = ((i + 1) < startByteCodes.Count) ? startByteCodes[i + 1] : null;
                var last = (next != null) ? body[body.IndexOf(next) - 1] : body[body.Count - 1];
                result.Add(new ByteCodeBlock(first, last));
            }
            return result;
        }

        private sealed class VariableInfo
        {
            public AstVariable Variable;
            public List<ByteCode> Defs;
            public List<ByteCode> Uses;
        }
    }
}
