using System;
using Dot42.Utility;

namespace Dot42.Ide.Debugger
{
    /// <summary>
    /// Plugin for DLog that outputs in the IDE output pane.
    /// </summary>
    internal class OutputPaneLog : DLog
    {
        private static readonly object logLock = new object();
        private static OutputPaneLog log;
        private readonly IIdeOutputPane outputPane;
        private readonly Levels minLevel;
        private readonly DContext? _limitToContext;

        /// <summary>
        /// Make sure this log is registered.
        /// </summary>
        internal static void EnsureLoaded(IIde ide, bool vsDebugPane)
        {
            if (log != null)
                return;

            var outputPane = !vsDebugPane ? ide.CreateDot42OutputPane() : ide.CreateDebugOutputPane();

            var add = false;
            lock (logLock)
            {
                if (log == null)
                {
                    log = new OutputPaneLog(outputPane, vsDebugPane?Levels.Info : Levels.Error, vsDebugPane?DContext.VSDebuggerMessage:(DContext?) null);
                    add = true;
                }
            }
            if (add)
            {
                AddAdditionalLogger(log);
                if(vsDebugPane)
                    AddToContext(DContext.VSDebuggerMessage, log);
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        private OutputPaneLog(IIdeOutputPane outputPane, Levels minLevel,DContext? limitToContext)
        {
            this.outputPane = outputPane;
            this.minLevel = minLevel;
            _limitToContext = limitToContext;
        }

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected override void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception,
                                      object[] args)
        {
            if (level < minLevel)
                return;
            if (_limitToContext != null && context != _limitToContext.Value)
                return;

            if ((msg == null) && (exception != null)) msg = exception.Message;
            if (msg == null)
                return;

            outputPane.EnsureLoaded();
            outputPane.LogLine(FormatLevel(level) + FormatContext(context) + string.Format(msg, args));
        }

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected override void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder)
        {
            if (level < Levels.Error)
                return;
            outputPane.EnsureLoaded();
            outputPane.LogLine(FormatLevel(level) + messageBuilder());
        }
    }
}
