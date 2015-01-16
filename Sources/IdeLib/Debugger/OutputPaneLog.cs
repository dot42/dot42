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

        /// <summary>
        /// Make sure this log is registered.
        /// </summary>
        internal static void EnsureLoaded(IIde ide)
        {
            if (log != null)
                return;
            var outputPane = ide.CreateDebugOutputPane();
            var add = false;
            lock (logLock)
            {
                if (log == null)
                {
                    log = new OutputPaneLog(outputPane);
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
        private OutputPaneLog(IIdeOutputPane outputPane)
        {
            this.outputPane = outputPane;
        }

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected override void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception,
                                      object[] args)
        {
            if (level < Levels.Error)
                return;
            if ((msg == null) && (exception != null)) msg = exception.Message;
            if (msg == null)
                return;
            if (!Show(level, context))
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
