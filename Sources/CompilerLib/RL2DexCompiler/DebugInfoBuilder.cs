using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL2DexCompiler.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    internal partial class DebugInfoBuilder
    {
        private readonly CompiledMethod compiledMethod;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugInfoBuilder(CompiledMethod compiledMethod)
        {
            this.compiledMethod = compiledMethod;
        }

        /// <summary>
        /// Create debug info for the given (otherwise completed) body.
        /// </summary>
        internal void CreateDebugInfo(MethodBody dbody, RegisterMapper regMapper, DexTargetPackage targetPackage)
        {
            var source = compiledMethod.ILSource;
            if ((source == null) || !source.HasBody || (source.Body.Instructions.Count == 0))
                return;

            // Initialize
            var info = new DebugInfo(dbody);
            info.Parameters.AddRange(dbody.Owner.Prototype.Parameters.Select(x => x.Name ?? "?"));

            // Should be at a better location perhaps
            info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.SetPrologueEnd));

            // Get instructions with valid sequence points
            var validTuples = dbody.Instructions.Select(x => Tuple.Create(x, x.SequencePoint as ISourceLocation)).Where(x => x.Item2 != null && !x.Item2.IsSpecial).ToList();

            // Assign line numbers
            var lineNumbers = AssignLineNumbers(validTuples.Select(x => x.Item2));

            // Set default document
            string lastUrl = null;
            var setLineStart = true;
            int lastLine = 0;
            int lastOffset = 0;                      
            var firstDocument = validTuples.Select(x => x.Item2).FirstOrDefault(x => x != null && x.Document != null);
            if (firstDocument != null)
            {
                lastUrl = firstDocument.Document;
                lastLine = firstDocument.StartLine;
                info.LineStart = (uint) lastLine;
                setLineStart = false;

                // Set on type or when different in debug info.
                var type = dbody.Owner.Owner;
                if(!type.SetSourceFile(lastUrl))
                {
                    // Make sure the file is set when needed
                    lastUrl = null;
                }
            }

            // Build intermediate list
            var entries = new List<Entry>();

            // Add line numbers
            foreach (var tuple in validTuples)
            {
                var ins = tuple.Item1;
                var seqp = tuple.Item2;
                var lineNumber = GetLineNumber(seqp, lineNumbers);

                // Line number
                if (setLineStart)
                {
                    info.LineStart = (uint)lineNumber;
                    setLineStart = false;
                }

                var url = seqp.Document;
                entries.Add(new PositionEntry(ins.Offset, lineNumber, url));
            }

            // Add variables
            ControlFlowGraph cfg = null;
            foreach (var tuple in regMapper.VariableRegisters)
            {
                var reg = tuple.Register;
                var variable = tuple.Variable;
                if (variable.IsCompilerGenerated)
                    continue;
                var dexType = variable.GetType(targetPackage);
                if (dexType == null)
                    continue;

                // Find out in which basic blocks the variable is live
                cfg = cfg ?? new ControlFlowGraph(dbody);
                var startMap = new Dictionary<BasicBlock, Instruction>();
                foreach (var block in cfg)
                {
                    // First instruction from that first writes to the register.
                    var firstWrite = block.Instructions.FirstOrDefault(reg.IsDestinationIn);
                    if (firstWrite == null)
                        continue;

                    // The variable is valid the first instruction after the first write.
                    Instruction start;
                    if (!firstWrite.TryGetNext(dbody.Instructions, out start))
                        continue;
                    startMap.Add(block, start);
                    block.AddLiveRegisterAtExit(reg);                    
                }

                if (startMap.Count == 0)
                    continue;

                // Generate start-restart-end entries
                VariableEndEntry lastBlockEndEntry = null;
                foreach (var block in cfg)
                {
                    Instruction start;
                    var started = false;
                    if (block.IsLiveAtEntry(reg))
                    {
                        // Live in the entire block
                        if (lastBlockEndEntry == null)
                        {
                            // We have to start/restart
                            entries.Add(new VariableStartEntry(block.Entry.Offset, reg, variable, dexType));
                        }
                        else
                        {
                            // Remove the end-entry of the previous block
                            entries.Remove(lastBlockEndEntry);
                        }
                        started = true;
                    }
                    else if (startMap.TryGetValue(block, out start))
                    {
                        // Live from "start"
                        entries.Add(new VariableStartEntry(start.Offset, reg, variable, dexType));
                        started = true;
                    }

                    Instruction next;
                    lastBlockEndEntry = null;
                    if (started && block.Exit.TryGetNext(dbody.Instructions, out next))
                    {
                        // Add end block
                        entries.Add(lastBlockEndEntry = new VariableEndEntry(next.Offset, reg, variable));
                    }
                }

                // Weave in splilling info
                var spillMappings = regMapper.RegisterSpillingMap.Find(reg).ToList();
                if (spillMappings.Count > 0)
                {
                    foreach (var mapping in spillMappings)
                    {
                        var alreadyStarted = IsStartedAt(entries, variable, mapping.FirstInstruction.Offset);
                        if (alreadyStarted)
                        {
                            // Stop now
                            var prev = mapping.FirstInstruction.GetPrevious(dbody.Instructions);
                            entries.Add(new VariableEndEntry(prev.Offset, mapping.HighRegister, variable));
                        }

                        // Add mappings for low register
                        entries.Add(new VariableStartEntry(mapping.FirstInstruction.Offset, mapping.LowRegister, variable, dexType));
                        entries.Add(new VariableEndEntry(mapping.LastInstruction.Offset, mapping.LowRegister, variable));

                        if (alreadyStarted)
                        {
                            // Restart on high register
                            Instruction next;
                            if (mapping.LastInstruction.TryGetNext(dbody.Instructions, out next))
                            {
                                entries.Add(new VariableStartEntry(next.Offset, mapping.HighRegister, variable, dexType));
                            }
                        }
                    }
                }
            }

            // Generate instructions
            entries.Sort();
            var firstPositionEntry = true;
            var startedVarirables = new HashSet<Register>();
            foreach (var entry in entries)
            {
                entry.Generate(info, ref lastLine, ref lastOffset, ref lastUrl, ref firstPositionEntry, startedVarirables);
            }

            // Terminate
            info.DebugInstructions.Add(new DebugInstruction(DebugOpCodes.EndSequence));

            // Attached
            dbody.DebugInfo = info;
        }

        /// <summary>
        /// Is the given variable started at the given offset?
        /// </summary>
        private static bool IsStartedAt(List<Entry> entries, IVariable variable, int offset)
        {
            var started = false;
            foreach (var entry in entries.OrderBy(x => x))
            {
                if (entry.Offset > offset)
                    break;
                var startEntry = entry as VariableStartEntry;
                VariableEndEntry endEntry;
                if (startEntry != null)
                {
                    if (startEntry.Variable == variable)
                        started = true;
                }
                else if ((endEntry = entry as VariableEndEntry) != null)
                {
                    if (endEntry.Variable == variable)
                        started = false;
                }
            }
            return started;
        }

        /// <summary>
        /// Calculate a special debug opcode that sets a position entry.
        /// </summary>
        /// <returns>True if the special opcode is valid, false otherwise</returns>
        private static bool TryCalculateSpecialOpcode(int lineAdv, int addressAdv, out DebugOpCodes opcode)
        {
            const int lineBase = -4;
            const int lineRange = 15;

            opcode = DebugOpCodes.Special;
            if ((lineAdv < -4) || (lineAdv > 10) || (addressAdv > 16))
                return false;

            if (addressAdv < 0)
                throw new ArgumentOutOfRangeException("addressAdv > 0");

            var adjusted = (lineAdv - lineBase) + (addressAdv * lineRange);
            var opcodeAsInt = (int)DebugOpCodes.Special + adjusted;
            var result = (opcodeAsInt >= 0x0A) && (opcodeAsInt <= 0xFF);
            opcode = result ? (DebugOpCodes) opcodeAsInt : DebugOpCodes.Special;
            return result;
        }

        /// <summary>
        /// Add document and position data to the given map file.
        /// </summary> 
        internal void AddDocumentMapping(MapFile mapFile)
        {
            var source = compiledMethod.DexMethod;
            if ((source == null) || (source.Body == null) || (source.Body.Instructions.Count == 0))
                return;

            if (this.compiledMethod.DexMethod.Name == "testTryCatch")
            {
                
            }

            var sequencePointsInstr = source.Body.Instructions.Where(x => x.SequencePoint != null);
            foreach (var seqPointIns in sequencePointsInstr)
            {
                // Get document
                var seqPoint = (ISourceLocation)seqPointIns.SequencePoint;
                var doc = mapFile.GetOrCreateDocument(seqPoint.Document, true);

                // Add position
                var docPos = new DocumentPosition(seqPoint.StartLine, seqPoint.StartColumn, seqPoint.EndLine,
                                                  seqPoint.EndColumn, 
                                                  compiledMethod.DexMethod.Owner.MapFileId,
                                                  compiledMethod.DexMethod.MapFileId,
                                                  seqPointIns.Offset) { AlwaysKeep = seqPointIns.OpCode.IsReturn() };
                doc.Positions.Add(docPos);
            }            
        }

        /// <summary>
        /// Assign unique line numbers each sequence point.
        /// </summary>
        private static List<Tuple<ISourceLocation, int>> AssignLineNumbers(IEnumerable<ISourceLocation> sourceLocations)
        {
            // Create a list of unique sequence points
            var list = new List<ISourceLocation>();
            foreach (var iterator in sourceLocations)
            {
                var sp = iterator;
                if (!list.Any(x => x.IsEqual(sp)))
                {
                    list.Add(sp);
                }
            }

            // Order the list
            list.Sort(SourceLocationComparer.Instance);

            // Assign line numbers
            var result = new List<Tuple<ISourceLocation, int>>();
            var lastNumber = int.MinValue;
            foreach (var sp in list)
            {
                var number = sp.StartLine;
                while (number <= lastNumber)
                    number++;
                result.Add(Tuple.Create(sp, number));
                lastNumber = number;

            }
            return result;
        }

        /// <summary>
        /// Gets the line number assigned to the given sequence point.
        /// </summary>
        private static int GetLineNumber(ISourceLocation sp, List<Tuple<ISourceLocation, int>> lineNumbers)
        {
            foreach (var tuple in lineNumbers)
            {
                if (tuple.Item1.IsEqual(sp))
                    return tuple.Item2;
            }
            throw new ArgumentException("Unknown sequence point");
        }
    }
}
