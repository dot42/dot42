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

        SourceCodePosition _prevSource = null;
        int _prevSourceInstructionOffset = -1;
        bool _justSeeked = true;

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

            bool wantsDocumentUrl = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_DOCUMENTURL) != 0;
            bool wantsPosition = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_POSITION) != 0;
            bool wantsByteOffset = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_BYTEOFFSET) != 0;
            bool wantsFlags = (dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_FLAGS) != 0;

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

                if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODELOCATIONID) != 0)
                {
                    insd.uCodeLocationId = (ulong)ins.Offset;    
                    insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODELOCATIONID;
                }

                if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPCODE) != 0)
                {
                    insd.bstrOpcode = _method.FormatOpCode(ins);
                    insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPCODE;
                }

                if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS) != 0)
                {
                    insd.bstrOperands = _method.FormatOperands(ins);
                    insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS;
                }

                if (wantsDocumentUrl || wantsPosition || wantsByteOffset || wantsFlags)
                {
                    var source = _method.FindSourceCode(ins.Offset);
                    var hasSource = source != null && !source.IsSpecial;

                    bool isSameDocAsPrevious, isSameDocPos;
                    
                    if (hasSource)
                    {
                        isSameDocAsPrevious = _prevSource != null
                                              && !_prevSource.IsSpecial
                                              && _prevSource.Document.Path == source.Document.Path;
                        isSameDocPos = _prevSource != null && !_prevSource.IsSpecial
                                              && isSameDocAsPrevious
                                              && source.Position.CompareTo(_prevSource.Position) == 0;

                    }
                    else
                    {
                        isSameDocAsPrevious = (source == null && _prevSource == null) || (source != null && _prevSource != null && _prevSource.IsSpecial);
                        isSameDocPos = isSameDocAsPrevious;
                    }

                    if (wantsDocumentUrl && (!isSameDocAsPrevious || _justSeeked))
                    {
                        if (hasSource)
                        {
                            insd.bstrDocumentUrl = "file://" + source.Document.Path;
                            insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_DOCUMENTURL;
                        }
                        // how do I clear the document?
                    }

                    int byteOffset = 0;

                    if ((wantsByteOffset || wantsPosition) && hasSource)
                    {
                        if (isSameDocPos)
                            byteOffset = ins.Offset - _prevSourceInstructionOffset;
                        else
                        {
                            byteOffset = 0;
                            _prevSourceInstructionOffset = ins.Offset;
                        }
                    }

                    if (wantsPosition && hasSource && !isSameDocPos)
                    {
                        var pos = source.Position;

                        insd.posBeg.dwLine      = (uint)(pos.Start.Line - 1);
                        insd.posBeg.dwColumn    = (uint)(pos.Start.Column - 1);
                        insd.posEnd.dwLine      = (uint)(pos.End.Line - 1);
                        insd.posEnd.dwColumn    = (uint)(pos.End.Column - 1);

                        if (insd.posEnd.dwLine - insd.posBeg.dwLine > 3) // never show more than 3 lines.
                            insd.posEnd.dwLine = insd.posBeg.dwLine + 3;

                        // Is this just me? I have no idea why my VS throws an exception when using this.
                        //insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_POSITION;
                    }

                    if (wantsByteOffset && hasSource)
                    {
                        insd.dwByteOffset = (uint)byteOffset;
                        insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_BYTEOFFSET;
                    }

                    if (wantsFlags)
                    {
                        if(!isSameDocAsPrevious)
                            insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_DOCUMENTCHANGE;
                        if(hasSource)
                            insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_HASSOURCE;

                        //if (_loc.Location.Index == (ulong)ins.Offset)
                        //    insd.dwFlags |= enum_DISASSEMBLY_FLAGS.DF_INSTRUCTION_ACTIVE;


                        insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_FLAGS;
                    }

                    if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS) != 0)
                    {
                        if (!hasSource && !isSameDocPos)
                        {
                            insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS;
                            insd.bstrSymbol = "(generated instructions)";
                        }
                        else if (hasSource && !isSameDocPos)
                        {
                            // workaround to show something at least
                            if ((dwFields & enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS) != 0)
                            {
                                insd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS_SYMBOLS;
                                insd.bstrSymbol = "File Position: "  + source.Position.Start + " - " + source.Position.End;
                            }
                            
                        }
                    }

                    _justSeeked = false;
                    _prevSource = source;
                }

                prgDisassembly[pdwInstructionsRead] = insd;
            }

            return pdwInstructionsRead == 0 || _instructionPointer >= insCount ? VSConstants.S_FALSE : VSConstants.S_OK;
        }

        public int Seek(enum_SEEK_START dwSeekStart, IDebugCodeContext2 pCodeContext, ulong uCodeLocationId, long iInstructions)
        {
            _justSeeked = true;
            _prevSource = null;
            _prevSourceInstructionOffset = -1;

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
            var source = _method.FindSourceCode((int)uCodeLocationId);
            if(source != null)
            { 
                var docLoc = new DocumentLocation(location, source, _loc.ReferenceType, _loc.Method, _method.TypeEntry, _method.MethodEntry);
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
