using System;
using System.Diagnostics;
using Dot42.Utility;
using Microsoft.Build.Utilities;

namespace Dot42.Compiler
{
    /// <summary>
    /// Library wrapper around the compiler.
    /// </summary>
    public static class CompilerService
    {
        /// <summary>
        /// Execute the compiler.
        /// </summary>
        public static bool Execute(string[] args, TaskLoggingHelper msbuildLog)
        {
            var log = (msbuildLog != null) ? new MSBuildLog(msbuildLog) : null;
            if (log != null) DLog.AddAdditionalLogger(log);
            try
            {
                var options = new CommandLineOptions(args);
                if (options.ShowHelp)
                {
                    throw new ArgumentOutOfRangeException(options.GetUsage());
                }
#if DEBUG
            //Debugger.Launch();
#endif
                return Program.MainCode(options);
            }
            finally
            {
                if (log != null)
                {
                    DLog.RemoveAdditionalLogger(log);
                }
            }
        }

        private class MSBuildLog : DLog
        {
            private readonly TaskLoggingHelper log;

            /// <summary>
            /// Default ctor
            /// </summary>
            public MSBuildLog(TaskLoggingHelper log)
            {
                this.log = log;
            }

            protected override void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception, object[] args)
            {
                if (level < Levels.Info)
                    return;
                if ((msg == null) && (exception != null)) msg = exception.Message;
                if (msg == null)
                    return;
                lineNr = Math.Max(0, lineNr);
                column = Math.Max(0, column);
                switch (level)
                {
                    //case Levels.Info:
                    //        log.LogMessage(msg, args);
                    //    break;
                    case Levels.Info: //TODO: get LogMessage to work.
                    case Levels.Warning:
                        if (url != null)
                            log.LogWarning(null, null, null, url, lineNr, column, 0, 0, msg, args);
                        else
                            log.LogWarning(msg, args);
                        break;
                    case Levels.Error:
                        if (url != null)
                            log.LogError(null, null, null, url, lineNr, column, 0, 0, msg, args);
                        else
                            log.LogError(msg, args);
                        break;
                }
            }

            protected override void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder)
            {
                if (level < Levels.Info)
                    return;
                lineNr = Math.Max(0, lineNr);
                column = Math.Max(0, column);
                switch (level)
                {
                    //case Levels.Info:
                    //        log.LogMessage(messageBuilder());
                    //    break;
                    case Levels.Info: //TODO: get LogMessage to work.
                    case Levels.Warning:
                        if (url != null)
                            log.LogWarning(null, null, null, url, lineNr, column, 0, 0, messageBuilder());
                        else
                            log.LogWarning(messageBuilder());
                        break;
                    case Levels.Error:
                        if (url != null)
                            log.LogError(null, null, null, url, lineNr, column, 0, 0, messageBuilder());
                        else
                            log.LogError(messageBuilder());
                        break;
                }
            }
        }
    }
}
