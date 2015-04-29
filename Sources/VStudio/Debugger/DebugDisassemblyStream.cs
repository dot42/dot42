using System;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    class DebugDisassemblyStream : IDebugDisassemblyStream2
    {
        private readonly DocumentLocation _loc;
        private readonly MethodDisassembly _method;

        private int _instructionPointer;

        Tuple<Document, DocumentPosition> _prevSource = null;
        int _prevSourceInstructionOffset = -1;

        public DebugDisassemblyStream(DebugProgram program, DebugCodeContext documentContext)
        {
            _loc = documentContext.DocumentContext.DocumentLocation;
            _method = program.DisassemblyProvider.GetFromLocation(_loc);
        }

        public int Read(uint dwInstructions, enum_DISASSEMBLY_STREAM_FIELDS dwFields, out uint pdwInstructionsRead, DisassemblyData[] prgDisassembly)
        {
            pdwInstructionsRead = 0;

            if (_method == null)
                return HResults.E_DISASM_NOTAVAILABLE;

            var method = _method.Method;

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
                    insd.bstrAddress = _method.FormatAddress(ins);
                    insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_ADDRESS;
                }

                insd.dwFields = enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPCODE 
                              | enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS 
                              | enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODELOCATIONID
                              | enum_DISASSEMBLY_STREAM_FIELDS.DSF_FLAGS;

                insd.bstrOpcode = _method.FormatOpCode(ins);
                insd.bstrOperands = _method.FormatOperands(ins);
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
                    Tuple<Document, DocumentPosition> source = _method.GetSourceFromOffset(ins.Offset);

                    if(source != null && !source.Item2.IsSpecial)
                    {
                        bool isSameDocAsPrevious = _prevSource != null && _prevSource.Item1.Path == source.Item1.Path;

                        if (!isSameDocAsPrevious)
                        {
                            _prevSource = null;
                            insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_DOCUMENTCHANGE;
                        }

                        if (wantsDocumentUrl || wantsPosition)
                        {
                            insd.bstrDocumentUrl = "file://" + source.Item1.Path;
                            insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_DOCUMENTURL;
                        }

                        insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_HASSOURCE;

                        if (wantsByteOffset)
                            insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_BYTEOFFSET;

                        if (_prevSource == null || !_prevSource.Item2.Start.Equals(source.Item2.Start) || !_prevSource.Item2.End.Equals(source.Item2.End))
                        {
                            if (wantsPosition)
                            {
                                // For reasons unknown to me, this crashes sometimes VS in debug mode,
                                // and does not work in release mode. I've come to the conclusion
                                // that this might be a bug in visual studio.
                                insd.posBeg.dwLine = (uint)(source.Item2.Start.Line - 1);
                                insd.posBeg.dwColumn = 0;
                                insd.posEnd.dwLine = (uint)(source.Item2.End.Line - 1);
                                insd.posEnd.dwColumn = uint.MaxValue;
                                if (insd.posEnd.dwLine - insd.posBeg.dwLine > 3) // never show more then 3 lines.
                                    insd.posEnd.dwLine = insd.posBeg.dwLine + 3;
                                
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
                            _prevSource = source;
                            insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_DOCUMENTCHANGE;

                            if (source != null)
                            {
                                if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS) != 0)
                                {
                                    insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS;
                                    insd.bstrSymbol = "(special instruction(s))";
                                }
                            }
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
            if (_method == null)
                return HResults.E_DISASM_NOTAVAILABLE;

            var method = _method.Method;

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
            ppCodeContext = null;

            if (_method == null)
                return HResults.E_DISASM_NOTAVAILABLE;

            var location = _loc.Location.GetAtIndex((uint)uCodeLocationId);
            var ctx = new DebugCodeContext(location);

            // try to set source code.
            Tuple<Document, DocumentPosition> source = _method.GetSourceFromOffset((int)uCodeLocationId);
            if(source != null)
            { 
                var docLoc = new DocumentLocation(location, source.Item1, source.Item2, _loc.ReferenceType, _loc.Method, _method.TypeEntry, _method.MethodEntry);
                ctx.DocumentContext = new DebugDocumentContext(docLoc, ctx);
            }

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

            if (_method == null)
                return HResults.E_DISASM_NOTAVAILABLE;

            pnSize = (ulong)_method.Method.Body.Instructions.Count;
            return VSConstants.S_OK;
        }

       
    }
}
