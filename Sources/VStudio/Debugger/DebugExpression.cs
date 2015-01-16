using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal class DebugExpression : IDebugExpression2
    {
        private readonly DebugProperty property;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugExpression(DebugProperty property)
        {
            this.property = property;
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
        public int EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback, out IDebugProperty2 ppResult)
        {
            ppResult = property;
            return VSConstants.S_OK;
        }
    }
}
