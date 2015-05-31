using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib.OpcodeHelp;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    public class DebugStackFrame : DalvikStackFrame, IDebugStackFrame2, IDebugExpressionContext2
    {
        private static readonly DalvikOpcodeHelpLookup Opcodes = new DalvikOpcodeHelpLookup();

        private DebugDocumentContext documentContext;
        private List<DalvikValue> values;
        private static readonly Regex castRegisterExpression = new Regex(@"^(?:[ ]*\([ ]*([^)]*)[ ]*\))?[ ]*([rRpP])([0-9]+)[ ]*$", RegexOptions.CultureInvariant); 

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugStackFrame(FrameId frameId, Location location, DebugThread thread)
            : base(frameId, location, thread)
        {
        }

        /// <summary>
        /// Gets the containing thread.
        /// </summary>
        public new DebugThread Thread
        {
            get { return (DebugThread) base.Thread; }
        }

        /// <summary>
        /// Construct a FRAMEINFO for this stack frame with the requested information.
        /// </summary>
        internal void SetFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, out FRAMEINFO info)
        {
            info = new FRAMEINFO();

            info.m_pFrame = this;
            info.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;

            info.m_bstrModule = Thread.Program.MainModule.Name;
            info.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME))
            {
                info.m_bstrFuncName = GetDocumentContext().DocumentLocation.Description;
                info.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
            }
        }

        public int GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetCodeContext");
            ppCodeCxt = GetDocumentContext().CodeContext;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Get or create the document context.
        /// </summary>
        private DebugDocumentContext GetDocumentContext()
        {
            if (documentContext == null)
            {
                documentContext = GetDocumentLocationAsync().Select(x => new DebugDocumentContext(x, new DebugCodeContext(Location))).Await(DalvikProcess.VmTimeout);
            }
            return documentContext;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppCxt)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetDocumentContext");
            ppCxt = GetDocumentContext();
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the name of the stack frame, typically the method name.
        /// </summary>
        public int GetName(out string pbstrName)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetName");
            FRAMEINFO info;
            SetFrameInfo(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME, out info);
            pbstrName = info.m_bstrFuncName;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Parses an expression in text form for later evaluation.
        /// </summary>
        public int ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError, out uint pichError)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.ParseText");
            ppExpr = null;
            pbstrError = null;
            pichError = 0;

            // Try to match a local variable
            var locals = GetValues();
            var localVariable = locals.FirstOrDefault(x => x.Name == pszCode);
            if (localVariable != null)
            {
                ppExpr = new DebugExpression(new DebugStackFrameValueProperty(localVariable, null, this));
                return VSConstants.S_OK;
            }

            // special registers group?
            if (pszCode.StartsWith("$reg", StringComparison.InvariantCultureIgnoreCase))
            {
                ppExpr = new DebugExpression(new DebugRegisterGroupProperty(this, false));
                return VSConstants.S_OK;
            }

            // try to match any of the registers.
            var match = castRegisterExpression.Match(pszCode);
            if (match.Success)
            {
                var castExpr = match.Groups[1].Value.Trim();
                var registerType = match.Groups[2].Value;
                int index = int.Parse(match.Groups[3].Value);

                ppExpr = new DebugRegisterExpression(this, pszCode, registerType, index, castExpr);
                return VSConstants.S_OK;   
            }

            // try to match opcode help (in disassembly)
            var opHelp = Opcodes.Lookup(pszCode);
            if (opHelp != null)
            {
                ppExpr = new DebugExpression(new DebugConstProperty(pszCode, opHelp.Syntax + "\r\n\r\n" + opHelp.Arguments + "\r\n\r\n" + opHelp.Description, "(opcode)", null));
                return VSConstants.S_OK;                
            }

            return VSConstants.E_FAIL;
        }

        public int GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetInfo");
            SetFrameInfo(dwFieldSpec, out pFrameInfo[0]);
            return VSConstants.S_OK;
        }

        public int GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetPhysicalStackRange");
            paddrMin = 0;
            paddrMax = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetExpressionContext");
            ppExprCxt = this;
            return VSConstants.S_OK;
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetLanguageInfo");
            pguidLanguage = AD7Guids.guidLanguageCSharp;
            return VSConstants.S_OK;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetDebugProperty");
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.EnumProperties");
            var list = new List<DebugProperty>();

            if (guidFilter == AD7Guids.guidFilterLocalsPlusArgs)
            {
                AddLocalProperties(list);
                AddParameterProperties(list);
            }
            else if (guidFilter == AD7Guids.guidFilterAllLocalsPlusArgs)
            {
                AddLocalProperties(list);
                AddParameterProperties(list);
                AddRegisters(list, false);
            }
            else if(guidFilter == AD7Guids.guidFilterAllLocals)
            {
                AddLocalProperties(list);
                AddRegisters(list, false);
            }
            else if (guidFilter == AD7Guids.guidFilterLocals)
            {
                AddLocalProperties(list);
            }
            else if (guidFilter == AD7Guids.guidFilterArgs)
            {
                AddParameterProperties(list);
            }
            else if (guidFilter == AD7Guids.guidFilterRegisters)
            {
                AddRegisters(list, true);
            }
            else
            {
                pcelt = 0;
                ppEnum = null;
                return VSConstants.E_NOTIMPL;
            }

            pcelt = (uint) list.Count;
            ppEnum = new PropertyEnum(list.Select(x => x.ConstructDebugPropertyInfo(dwFields, nRadix)));
            return VSConstants.S_OK;
        }

        public int GetThread(out IDebugThread2 ppThread)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugStackFrame.GetThread");
            ppThread = Thread;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Add all local variables to the list.
        /// </summary>
        private void AddLocalProperties(List<DebugProperty> list)
        {
            list.AddRange(GetValues().Select(x => new DebugStackFrameValueProperty(x, null, this))); // TODO
        }

        private void AddRegisters(List<DebugProperty> list, bool forceHexDisplay)
        {
            var loc = GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);
            var method = Debugger.Process.DisassemblyProvider.GetFromLocation(loc);

            if (method == null)
                return;

            list.Add(new DebugRegisterGroupProperty(this, forceHexDisplay));
        }

        /// <summary>
        /// Add all parameters to the list.
        /// </summary>
        private void AddParameterProperties(List<DebugProperty> list)
        {
            //list.AddRange(GetValues().Where(x => x.Entry is Parameter).Select(x => new DebugProperty(x))); // TODO
        }

        /// <summary>
        /// Gets all values on the stack frame
        /// </summary>
        private List<DalvikValue> GetValues()
        {
            if (values != null) return values;
            values = base.GetValuesAsync().Await(DalvikProcess.VmTimeout);
            return values;
        }
    }
}
