using System;
using Dot42.Ide;
using Dot42.Utility;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// Plugin for DLog that outputs in the IDE output pane.
    /// </summary>
    internal class StatusBarLog : DLog
    {
        private readonly IVsStatusbar _statusBar;
        private static readonly object logLock = new object();
        private static DLog log;
        private string lastMessageText;
        private DateTime lastTime;
        /// <summary>
        /// Make sure this log is registered.
        /// </summary>
        internal static void EnsureLoaded()
        {
            if (log != null)
                return;

            var status = (IVsStatusbar)VSUtilities.ServiceProvider().GetService(typeof (SVsStatusbar));

            var add = false;
            lock (logLock)
            {
                if (log == null)
                {
                    log = new StatusBarLog(status);
                    add = true;
                }
            }
            if (add)
            {
                DLog.AddAdditionalLogger(log);                
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        private StatusBarLog(IVsStatusbar statusBar)
        {
            _statusBar = statusBar;
        }

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected override void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception,
                                      object[] args)
        {
            if (context != DContext.VSDebuggerStatusBarMessage)
                return;

            if ((msg == null) && (exception != null)) msg = exception.Message;
            if (msg == null)
                return;
            if (!Show(level, context))
                return;

            string text = string.Format(msg, args);

            SetText(level, text);
        }

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected override void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder)
        {
            if (context != DContext.VSDebuggerStatusBarMessage)
                return;
            SetText(level, messageBuilder());
        }

        private void SetText(Levels level, string text)
        {
            if (level != Levels.Info)
                text = FormatLevel(level) + text;

            int frozen;
            _statusBar.IsFrozen(out frozen);

            if (frozen != 0)
                return;

            _statusBar.SetText(text);
        }
    }
}
