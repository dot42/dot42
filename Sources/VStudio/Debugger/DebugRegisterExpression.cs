using System.Linq;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// An expression referencing a register, possibly with side effects (e.g. 
    /// crashing the virtual machineon object/string casts).
    /// </summary>
    internal class DebugRegisterExpression : IDebugExpression2
    {
        private readonly DebugStackFrame _stackFrame;
        private readonly string _expression;
        private readonly string _registerType;
        private readonly int _index;
        private readonly string _castExpr;

        /// <param name="stackFrame"></param>
        /// <param name="expression"></param>
        /// <param name="registerType">can be 'p' for parameter or 'r' for register</param>
        /// <param name="index"></param>
        /// <param name="castExpr"></param>
        public DebugRegisterExpression(DebugStackFrame stackFrame, string expression, string registerType, int index, string castExpr)
        {
            _stackFrame = stackFrame;
            _expression = expression;
            _registerType = registerType.ToLowerInvariant();
            _index = index;
            _castExpr = castExpr;
        }

        /// <summary>
        /// This method evaluates the expression asynchronously.
        /// </summary>
        public int EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// This method cancels asynchronous expression evaluation as started by a call to the IDebugExpression2::EvaluateAsync method.
        /// </summary>
        public int Abort()
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// This method evaluates the expression synchronously.
        /// </summary>
        public int EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback,
            out IDebugProperty2 ppResult)
        {
            ppResult = null;
            int idx = _index;

            if (string.IsNullOrEmpty(_castExpr))
            {
                var reg = _stackFrame.GetRegistersAsync().Await((int) dwTimeout)
                                     .FirstOrDefault(r => r.Name == _registerType + _index);
                if (reg != null)
                {
                    ppResult = new DebugStackFrameValueProperty(reg, null, _stackFrame);
                    return VSConstants.S_OK;
                }
            }
            else
            {
                var tag = GetTagFromString(_castExpr);

                if (!tag.IsPrimitive() && (dwFlags & enum_EVALFLAGS.EVAL_NOSIDEEFFECTS) != 0)
                {
                    // this evaluation has "side effects" in that it might crash the VM
                    // if the cast is to "object" or "string", but the register does not 
                    // hold an object or string.
                    ppResult = new DebugConstProperty(_expression, "(this cast might crash the VM if the register is not of the casted type)", _castExpr, null)
                                                     { HasSideEffects = true };
                    return VSConstants.E_FAIL;
                }

                var isParam = _registerType == "p";

                if (isParam)
                {
                    var loc = _stackFrame.GetDocumentLocationAsync().Await((int) dwTimeout);
                    if (loc == null)
                        return VSConstants.E_FAIL;
                    var methodDiss = _stackFrame.Thread.Program.DisassemblyProvider.GetFromLocation(loc);
                    if (methodDiss == null)
                        return VSConstants.E_FAIL;
                    idx += methodDiss.Method.Body.Registers.Count - methodDiss.Method.Body.IncomingArguments;
                }

                var reg = _stackFrame.GetRegistersAsync(false, tag, idx).Await((int) dwTimeout);

                if (reg != null && reg.Count > 0)
                {
                    ppResult = new DebugStackFrameValueProperty(reg[0], null, _stackFrame, _expression)
                    {
                        HasSideEffects = !tag.IsPrimitive()
                    };
                    return VSConstants.S_OK;
                }
            }

            return VSConstants.E_FAIL;
        }

        /// <summary>
        /// TODO: this belongs somewhere else.
        /// </summary>
        private static Jdwp.Tag GetTagFromString(string expr)
        {
            Jdwp.Tag tag;
            switch (expr)
            {
                case "double":
                    tag = Jdwp.Tag.Double;
                    break;
                case "float":
                case "single":
                    tag = Jdwp.Tag.Float;
                    break;
                case "char":
                    tag = Jdwp.Tag.Char;
                    break;
                case "long":
                    tag = Jdwp.Tag.Long;
                    break;
                case "object":
                    tag = Jdwp.Tag.Object;
                    break;
                case "string":
                    tag = Jdwp.Tag.String;
                    break;
                case "byte":
                    tag = Jdwp.Tag.Byte;
                    break;
                case "short":
                    tag = Jdwp.Tag.Short;
                    break;
                case "Type":
                    tag = Jdwp.Tag.ClassObject;
                    break;
                case "int":
                    tag = Jdwp.Tag.Int;
                    break;
                default:
                    tag = Jdwp.Tag.Int;
                    break;
            }
            return tag;
        }
    }
}
