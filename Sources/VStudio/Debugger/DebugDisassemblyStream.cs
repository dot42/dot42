using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.JvmClassLib;
using Dot42.Mapping;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Text.Editor;
using NinjaTools.Collections;
using TallComponents.Common.Extensions;
using MethodDefinition = Dot42.DexLib.MethodDefinition;

namespace Dot42.VStudio.Debugger
{
    class DebugDisassemblyStream : IDebugDisassemblyStream2
    {
        private readonly DebugProgram _program;
        private readonly DebugCodeContext _documentContext;
        private readonly DocumentLocation _loc;
        
        private readonly Dex _dexFile;
        private readonly MapFile _mapFile;

        private MethodDefinition _methodDef;
        private ClassDefinition  _classDef;
        private string _methodName;
        private string _className;
        private readonly List<Tuple<Document,DocumentPosition>> _positions;
        

        private int _instructionPointer;
        private TypeEntry _typeEntry;
        private MethodEntry _methodEntry;

        Tuple<Document, DocumentPosition> _prevSource = null;
        int _prevSourceInstructionOffset = -1;

        public DebugDisassemblyStream(DebugProgram program, DebugCodeContext documentContext, Dex dexFile, MapFile mapFile)
        {
            _program = program;
            _documentContext = documentContext;
            _dexFile = dexFile;
            _mapFile = mapFile;

            _loc = documentContext.DocumentContext.DocumentLocation;

            LoadMethod(_loc);

            if (_typeEntry != null && _methodEntry != null)
            {
                _positions = _mapFile.GetLocations(_typeEntry, _methodEntry)
                                     .OrderBy(p=>p.Item2.MethodOffset)
                                     .ToList();
            }
        }

        public int Read(uint dwInstructions, enum_DISASSEMBLY_STREAM_FIELDS dwFields, out uint pdwInstructionsRead, DisassemblyData[] prgDisassembly)
        {
            pdwInstructionsRead = 0;
            
            var method = _methodDef;
            if (method == null)
                return VSConstants.DISP_E_MEMBERNOTFOUND;

            var insCount = method.Body.Instructions.Count;

            for (pdwInstructionsRead = 0; pdwInstructionsRead < dwInstructions; ++pdwInstructionsRead, ++_instructionPointer)
            {
                int ip = _instructionPointer;

                if (ip >= insCount)
                    break;

                var insd = new DisassemblyData();
                var ins = method.Body.Instructions[ip];

                if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_ADDRESS) != 0)
                {
                    insd.bstrAddress = ins.Offset.ToString("X3").PadLeft(4);
                    insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_ADDRESS;
                }

                insd.dwFields = enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPCODE 
                              | enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS 
                              | enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODELOCATIONID
                              | enum_DISASSEMBLY_STREAM_FIELDS.DSF_FLAGS;
                
                insd.bstrOpcode = ins.OpCode.ToString().PadLeft(20) + " ";
                insd.bstrOperands = FormatOperands(ins, ip, method.Body);
                insd.uCodeLocationId = (ulong)ins.Offset;

                //if (ins.Operand is IMemberReference && (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS) != 0)
                //{
                //    insd.bstrSymbol = ins.Operand.ToString();
                //    insd.dwFields|= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS;
                //}

                //if (_loc.Location.Index == (ulong)ins.Offset)
                //    insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_INSTRUCTION_ACTIVE;

                bool wantsDocumentUrl = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_DOCUMENTURL) != 0;
                bool wantsPosition = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_POSITION) != 0;
                bool wantsByteOffset = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_BYTEOFFSET) != 0;

                if (wantsDocumentUrl || wantsPosition || wantsByteOffset)
                {
                    Tuple<Document, DocumentPosition> source = GetSourceFromOffset(ins.Offset);

                    if(source != null && source.Item2.IsValid)
                    {
                        bool isSameDocAsPrevious = _prevSource != null && _prevSource.Item1.Path == source.Item1.Path;

                        if (!isSameDocAsPrevious)
                        {
                            _prevSource = null;
                            insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_DOCUMENTCHANGE;
                        }

                        if (wantsDocumentUrl)
                        {
                            insd.bstrDocumentUrl = "file:// " + source.Item1.Path;
                            insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_DOCUMENTURL;
                        }

                        insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_HASSOURCE;

                        if (wantsByteOffset)
                            insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_BYTEOFFSET;

                        if (_prevSource == null || !_prevSource.Item2.Start.Equals(source.Item2.Start) || !_prevSource.Item2.End.Equals(source.Item2.End))
                        {
                            // this does not work for unknown reasons.
                            // in DebugDocumentContext::GetSourceRange it works without problems.
                            if (wantsPosition)
                            {
                                insd.posBeg.dwLine = (uint)(source.Item2.Start.Line - 1);
                                insd.posBeg.dwColumn = (uint)source.Item2.Start.Column;
                                insd.posEnd.dwLine = (uint)(source.Item2.End.Line - 1);
                                insd.posEnd.dwColumn = (uint)source.Item2.End.Column;

                                insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_POSITION;
                            }

                            // workaround to show something at least
                            if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS) != 0)
                            {
                                insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS;
                                insd.bstrSymbol = "File Position: "  + source.Item2.Start.ToString() + " - " + source.Item2.End.ToString();
                            }

                            insd.dwByteOffset = 0;

                            _prevSource = source;
                            _prevSourceInstructionOffset = ins.Offset;
                        }
                        else
                        {
                            insd.dwByteOffset = (uint) (ins.Offset - _prevSourceInstructionOffset);
                        }
                    }
                    else
                    {
                        // no valid source
                        if (_prevSource != null)
                        {
                            _prevSource = null;
                            insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_DOCUMENTCHANGE;
                        }
                        
                        _prevSourceInstructionOffset = ins.Offset;
                    }
                }

                prgDisassembly[pdwInstructionsRead] = insd;
            }

            return pdwInstructionsRead == 0 || _instructionPointer >= insCount ? VSConstants.S_FALSE : VSConstants.S_OK;
        }

        public int Seek(enum_SEEK_START dwSeekStart, IDebugCodeContext2 pCodeContext, ulong uCodeLocationId, long iInstructions)
        {
            var method = _methodDef;
            if (method == null)
                return VSConstants.DISP_E_MEMBERNOTFOUND;

            var insCount = method.Body.Instructions.Count;
            int newPos;
            
            switch (dwSeekStart)
            {
                case enum_SEEK_START.SEEK_START_BEGIN:
                    newPos = (int)iInstructions;
                    break;
                case enum_SEEK_START.SEEK_START_END:
                    
                    newPos = insCount + (int)iInstructions;
                    break;
                case enum_SEEK_START.SEEK_START_CURRENT:
                    newPos = _instructionPointer + (int)iInstructions;
                    break;
                default:
                    return VSConstants.E_NOTIMPL;
            }

            _instructionPointer = newPos < 0 ? 0 : newPos >= insCount ? insCount - 1 : newPos;
            
            _prevSource = null;
            _prevSourceInstructionOffset = -1;

            if (newPos < 0 || newPos >= insCount)
                return VSConstants.S_FALSE;

            return VSConstants.S_OK;
        }

        public int GetCodeLocationId(IDebugCodeContext2 pCodeContext, out ulong puCodeLocationId)
        {
            var location = ((DebugCodeContext) pCodeContext).Location;
            puCodeLocationId = location.Index;
            return VSConstants.S_OK;
        }

        public int GetCodeContext(ulong uCodeLocationId, out IDebugCodeContext2 ppCodeContext)
        {
            Tuple<Document, DocumentPosition> source = GetSourceFromOffset((int) uCodeLocationId);
            if(source == null)
            {
                ppCodeContext = null;
                return VSConstants.E_FAIL;
            }

            if (!source.Item2.IsValid) // what is this marker!?
            {
                ppCodeContext = null;
                return VSConstants.E_FAIL;
            }

            var location = new Location(_loc.Location.Class, _loc.Location.Method, uCodeLocationId);
            var docLoc = new DocumentLocation(location, source.Item1, source.Item2, _loc.ReferenceType, _loc.Method, _typeEntry, _methodEntry);

            var ctx = new DebugCodeContext(location);
            ctx.DocumentContext = new DebugDocumentContext(docLoc, ctx);

            ppCodeContext = ctx;
            return VSConstants.S_OK;
        }

        public int GetCurrentLocation(out ulong puCodeLocationId)
        {
            puCodeLocationId = (ulong)_loc.Location.Index;
            return VSConstants.S_OK;
        }

        public int GetDocument(string bstrDocumentUrl, out IDebugDocument2 ppDocument)
        {
            ppDocument = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetScope(enum_DISASSEMBLY_STREAM_SCOPE[] pdwScope)
        {
            pdwScope[0] = enum_DISASSEMBLY_STREAM_SCOPE.DSS_FUNCTION;
            return VSConstants.S_OK;
        }

        public int GetSize(out ulong pnSize)
        {
            pnSize = 0;

            var method = _methodDef;
            if (method == null)
                return VSConstants.DISP_E_MEMBERNOTFOUND;

            pnSize = (ulong)method.Body.Instructions.Count;
            return VSConstants.S_OK;
        }

        private static string FormatOperands(Instruction ins, int ip, MethodBody method)
        {
            StringBuilder ops = new StringBuilder();

            foreach (var r in ins.Registers)
            {
                if (ops.Length > 0)
                {
                    ops.Append(",");
                    Align(ops, 4);
                }
                ops.Append(r);
            }
            if (ops.Length == 0)
                ops.Append(" ");

            if (ins.Operand != null)
            {
                AlignWithSpace(ops, 12);

                if (ins.Operand is string)
                {
                    ops.Append("\"");
                    ops.Append(ins.Operand);
                    ops.Append("\"");
                }
                else if (ins.Operand is int)
                {
                    var i = (int) ins.Operand;
                    ops.Append(i);

                    if (i > 8|| i < -8)
                    {
                        ops.Append(" (0x");
                        ops.Append(i.ToString("X4"));
                        ops.Append(")");
                    }
                }
                else if (ins.Operand is long)
                {
                    var l = (long)ins.Operand;
                    ops.Append(l);
                    
                    ops.Append(" (0x");
                    ops.Append(l.ToString("X8"));
                    ops.Append(")");
                }
                else if (ins.Operand is Instruction)
                {
                    var target = (Instruction) ins.Operand;
                    ops.Append("-> ");
                    ops.Append(target.Offset.ToString("X3"));

                    int idx = method.Instructions.IndexOf(target);
                    ops.Append(" ; ");

                    int offset = (idx - ip);                   
                    ops.Append(offset.ToString("+0;-0;+0"));
                    //insd.bstrAddressOffset = offset.ToString("+0;-0;+0");

                }
                else if (ins.Operand is IMemberReference)
                {
                    ops.Append(ins.Operand);
                }
                else
                {
                    ops.Append(ins.Operand);
                }
            }

            var bstrOperands = ops.ToString();
            return bstrOperands;
        }

        private static void AlignWithSpace(StringBuilder b, int tabSize)
        {
            if (b.Length > 0 && b[b.Length - 1] != ' ')
                b.Append(' ');
            Align(b, tabSize);
        }

        private static void Align(StringBuilder b, int tabSize)
        {
            int add = (tabSize - b.Length%tabSize) % tabSize;
            b.Append(' ', add);
        }

        private Tuple<Document, DocumentPosition> GetSourceFromOffset(int offset)
        {
            if (_positions != null)
            {
                int idx = _positions.FindLastIndexSmallerThanOrEqualTo(offset, p => p.Item2.MethodOffset);
                if (idx != -1)
                {
                    return _positions[idx];
                }
            }
            return null;
        }

        private bool LoadMethod(DocumentLocation loc)
        {
            if (loc.TypeEntry != null)
                _className = loc.TypeEntry.DexName;

            if (loc.Method != null)
            {
                if (_className == null)
                {
                    var type = loc.Method.DeclaringType.GetSignatureAsync().Await(DalvikProcess.VmTimeout);
                    var typeDef = Descriptors.ParseClassType(type);
                    _className = typeDef.ClassName.Replace("/", ".");
                }
                _methodName = loc.Method.Name;
            }

            if (_methodName == null && loc.MethodEntry != null)
            {
                _methodName = loc.MethodEntry.DexName;
            }

            if (_methodName == null || _className == null)
                return false;

            _classDef = _dexFile.GetClass(_className);
            if (_classDef == null)
                return false;

            _methodDef = _classDef.GetMethod(_methodName);

            if (_methodDef == null)
                return false;

            _typeEntry = loc.TypeEntry ?? _mapFile.GetTypeById(_classDef.MapFileId);
            _methodEntry = loc.MethodEntry ?? (_typeEntry == null?  null : _typeEntry.GetMethodById(_methodDef.MapFileId));

            return true;
        }
    }
}
