using System.Text;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.JvmClassLib;
using Dot42.Mapping;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
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
        
        private MethodDefinition _methodDef;
        private ClassDefinition  _classDef;
        private string _methodName;
        private string _className;
        

        private int _instructionPointer;

        public DebugDisassemblyStream(DebugProgram program, DebugCodeContext documentContext, Dex dexFile)
        {
            _program = program;
            _documentContext = documentContext;
            _dexFile = dexFile;

            _loc = documentContext.DocumentContext.DocumentLocation;

            LoadMethod(_loc);
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

                insd.dwFields = enum_DISASSEMBLY_STREAM_FIELDS.DSF_ADDRESS | enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPCODE |
                                enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS | enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODELOCATIONID;

                insd.bstrAddress = ins.Offset.ToString("X3");
                insd.bstrOpcode = ins.OpCode.ToString().PadLeft(20) + " ";
                insd.bstrOperands = FormatOperands(ins, ip, method.Body);
                insd.uCodeLocationId = (ulong)ins.Offset;

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

            if (newPos < 0 || newPos >= insCount)
                return VSConstants.S_FALSE;

            return VSConstants.S_OK;
        }

        public int GetCodeLocationId(IDebugCodeContext2 pCodeContext, out ulong puCodeLocationId)
        {
            var dl = ((DebugCodeContext) pCodeContext).DocumentContext.DocumentLocation;
            puCodeLocationId = dl.Location.Index;
            return VSConstants.S_OK;
        }

        public int GetCodeContext(ulong uCodeLocationId, out IDebugCodeContext2 ppCodeContext)
        {
            var location = new Location(_loc.Location.Class, _loc.Location.Method, uCodeLocationId);

            var ctx = new DebugCodeContext(location);
            //var docLoc = new DocumentLocation(location, new Document(_classDef.SourceFile),
                
            //    new DocumentPosition(_methodDef.MapFileId), 
                
            //    )
            //ctx.DocumentContext = new DebugDocumentContext();

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

                    if (i > 8)
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
                    ops.Append(target.Offset.ToString("X3"));

                    int idx = method.Instructions.IndexOf(target);
                    ops.Append(" ; ");
                    ops.Append((idx - ip).ToString("+0;-0;+0"));                    
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
                //throw new InvalidOperationException("not a known method: " + loc.Description);
                return false;

            _classDef = _dexFile.GetClass(_className);
            if (_classDef == null)
                //throw new NotSupportedException("class not in dex: " + loc.Description);
                return false;

            _methodDef = _classDef.GetMethod(_methodName);

            if (_methodDef == null)
                return false;
                //throw new NotSupportedException("method not in dex:" + loc.Description);

            return true;
        }
    }
}
