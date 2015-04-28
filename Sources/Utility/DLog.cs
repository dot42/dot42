using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Win32;
using TallComponents.Common.Util;

namespace Dot42.Utility
{
    /// <summary>
    /// Generic logging
    /// </summary>
    public abstract class DLog
    {
        /// <summary>
        /// Log levels
        /// </summary>
        protected enum Levels
        {
            Debug,
            Info,
            Warning,
            Error
        };

        private static readonly DLog DebugInstance = new DebugLog();
        private static readonly DLog ConsoleInstance = new ConsoleLog();

        private static readonly ConcurrentDictionary<DContext, LogContext> contexts = new ConcurrentDictionary<DContext,LogContext>();
        private static readonly ConcurrentDictionary<DLog, DLog> additionalLoggers = new ConcurrentDictionary<DLog, DLog>();

        private static readonly DLog[] defaultLoggers = new[] { DebugInstance };

        /// <summary>
        /// Log the given error.
        /// </summary>
        public static void Error(DContext context, string msg, params object[] args)
        {
            GetContext(context).Write(Levels.Error, context, null, -1, -1, msg, null, args);
        }

        /// <summary>
        /// Log the given error.
        /// </summary>
        public static void Error(DContext context, string url, int column, int lineNr, string msg, params object[] args)
        {
            GetContext(context).Write(Levels.Error, context, url, column, lineNr, msg, null, args);
        }

        /// <summary>
        /// Log the given error.
        /// </summary>
        public static void Error(DContext context, string msg, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Error, context, null, -1, -1, msg, exception, args);
            ErrorLog.DumpError(exception);
        }

        /// <summary>
        /// Log the given error.
        /// </summary>
        public static void Error(DContext context, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Error, context, null, -1, -1, null, exception, args);
            ErrorLog.DumpError(exception);
        }

        /// <summary>
        /// Log the given warning.
        /// </summary>
        public static void Warning(DContext context, string msg, params object[] args)
        {
            GetContext(context).Write(Levels.Warning, context, null, -1, -1, msg, null, args);
        }

        /// <summary>
        /// Log the given warning.
        /// </summary>
        public static void Warning(DContext context, string msg, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Warning, context, null, -1, -1, msg, exception, args);
        }

        /// <summary>
        /// Log the given warning.
        /// </summary>
        public static void Warning(DContext context, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Warning, context, null, -1, -1, null, exception, args);
        }

        /// <summary>
        /// Log the given info message.
        /// </summary>
        public static void Info(DContext context, string msg, params object[] args)
        {
            GetContext(context).Write(Levels.Info, context, null, -1, -1, msg, null, args);
        }

        /// <summary>
        /// Log the given info message.
        /// </summary>
        public static void Info(DContext context, string msg, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Info, context, null, -1, -1, msg, exception, args);
        }

        /// <summary>
        /// Log the given info message.
        /// </summary>
        public static void Info(DContext context, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Info, context, null, -1, -1, null, exception, args);
        }

        /// <summary>
        /// Log the given debug message.
        /// </summary>
        public static void Debug(DContext context, string msg, params object[] args)
        {
            GetContext(context).Write(Levels.Debug, context, null, -1, -1, msg, null, args);
        }

        /// <summary>
        /// Log the given debug message.
        /// </summary>
        public static void Debug(DContext context, Func<string> messageBuilder)
        {
            GetContext(context).Write(Levels.Debug, context, null, -1, -1, messageBuilder);
        }

        /// <summary>
        /// Log the given debug message.
        /// </summary>
        public static void Debug(DContext context, string msg, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Debug, context, null, -1, -1, msg, exception, args);
        }

        /// <summary>
        /// Log the given debug message.
        /// </summary>
        public static void Debug(DContext context, Exception exception, params object[] args)
        {
            GetContext(context).Write(Levels.Debug, context, null, -1, -1, null, exception, args);
        }

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected abstract void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception, object[] args);

        /// <summary>
        /// Record the given log entry if the level is sufficient.
        /// </summary>
        protected abstract void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder);

        /// <summary>
        /// Show messages for the given leven and context?
        /// </summary>
        protected static bool Show(Levels level, DContext context)
        {
#if DEBUG
            return true;
#else
            return (level > Levels.Debug);
#endif
        }

        /// <summary>
        /// Format the given log level.
        /// </summary>
        protected static string FormatLevel(Levels level)
        {
            switch (level)
            {
                case Levels.Debug:
                    return "D: ";
                case Levels.Info:
                    return "I: ";
                case Levels.Warning:
                    return "W: ";
                case Levels.Error:
                    return "E: ";
                default:
                    throw new ArgumentException("Unknown level " + (int)level);
            }
        }

        /// <summary>
        /// Format the given context.
        /// </summary>
        protected static string FormatContext(DContext context)
        {
            return string.Format("[{0:D5}] ", (int) context);
        }

        /// <summary>
        /// Parse the log names
        /// </summary>
        private static DLog[] ParseLoggers(string value)
        {
            var result = new List<DLog>();
            foreach (var key in value.Split(','))
            {
                switch (key.Trim().ToLower())
                {
                    case "debug":
                        result.Add(DebugInstance);
                        break;
                    case "console":
                        result.Add(ConsoleInstance);
                        break;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Gets the logging context for the given context index.
        /// </summary>
        private static LogContext GetContext(DContext context)
        {
            LogContext logContext;
            if (contexts.TryGetValue(context, out logContext))
                return logContext;
            logContext = LogContext.LoadFromRegistry(context, defaultLoggers);
            contexts.TryAdd(context, logContext);
            return GetContext(context);
        }

        /// <summary>
        /// Register the given additional logger.
        /// </summary>
        public static void AddAdditionalLogger(DLog log)
        {
            additionalLoggers.TryAdd(log, log);
        }

        /// <summary>
        /// Unregister the given additional logger.
        /// </summary>
        public static void RemoveAdditionalLogger(DLog log)
        {
            DLog tmp;
            additionalLoggers.TryRemove(log, out tmp);
        }

        protected static void AddToContext(DContext ctx, DLog log)
        {
            GetContext(ctx).AddLogger(log);
            additionalLoggers.TryAdd(log, log);
        }

        public class DebugLog : DLog
        {
            /// <summary>
            /// Record the given log entry if the level is sufficient.
            /// </summary>
            protected override void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception, object[] args)
            {
                if ((msg == null) && (exception != null)) msg = exception.Message;
                if (msg == null) 
                    return;
                if (!Show(level, context))
                    return;
                System.Diagnostics.Debug.WriteLine(FormatLevel(level) + FormatContext(context) + string.Format(msg, args));
            }

            /// <summary>
            /// Record the given log entry if the level is sufficient.
            /// </summary>
            protected override void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder)
            {
                if (!Show(level, context))
                    return;
                System.Diagnostics.Debug.WriteLine(FormatLevel(level) + FormatContext(context) + messageBuilder());
            }
        }

        public class ConsoleLog : DLog
        {
            /// <summary>
            /// Record the given log entry if the level is sufficient.
            /// </summary>
            protected override void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception, object[] args)
            {
                if ((msg == null) && (exception != null)) msg = exception.Message;
                if (msg == null)
                    return;
                if (!Show(level, context))
                    return;
                Console.WriteLine(FormatLevel(level) + string.Format(msg, args));
            }

            /// <summary>
            /// Record the given log entry if the level is sufficient.
            /// </summary>
            protected override void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder)
            {
                if (!Show(level, context))
                    return;
                Console.WriteLine(FormatLevel(level) + messageBuilder());
            }
        }

        /// <summary>
        /// Loggers per context
        /// </summary>
        private sealed class LogContext
        {
            public readonly DContext Context;
            private List<DLog> loggers;

            private LogContext(DContext context, DLog[] loggers)
            {
                Context = context;
                var dLogs = ((loggers != null) && (loggers.Length > 0)) ? loggers : null;
                if(dLogs != null)
                    this.loggers = dLogs.ToList();
            }

            internal void AddLogger(DLog logger)
            {
                lock (this)
                {
                    if (loggers == null)
                        loggers = new List<DLog>();
                    loggers.Add(logger);
                }
            }

            /// <summary>
            /// Record the given log entry if the level is sufficient.
            /// </summary>
            internal void Write(Levels level, DContext context, string url, int column, int lineNr, string msg, Exception exception, object[] args)
            {
                //Console.WriteLine("LOGWRITE_A: " + (int)level);
                if (loggers != null)
                {
                    foreach (var log in loggers)
                    {
                        log.Write(level, context, url, column, lineNr, msg, exception, args);
                    }
                }
                foreach (var log in additionalLoggers.Keys)
                {
                    log.Write(level, context, url, column, lineNr, msg, exception, args);
                }
            }

            /// <summary>
            /// Record the given log entry if the level is sufficient.
            /// </summary>
            internal void Write(Levels level, DContext context, string url, int column, int lineNr, Func<string> messageBuilder)
            {
                //Console.WriteLine("LOGWRITE_B: " + (int)level);
                if (loggers != null)
                {
                    foreach (var log in loggers)
                    {
                        log.Write(level, context, url, column, lineNr, messageBuilder);
                    }
                }
                foreach (var log in additionalLoggers.Keys)
                {
                    log.Write(level, context, url, column, lineNr, messageBuilder);
                }
            }

            /// <summary>
            /// Load a context from current user registry.
            /// </summary>
            public static LogContext LoadFromRegistry(DContext context, DLog[] defaultLoggers)
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryConstants.LOGGING))
                {
                    if (key == null)
                        return new LogContext(context, defaultLoggers);
                    var valueName = ((int) context).ToString(CultureInfo.InvariantCulture);
                    var value = key.GetValue(valueName) as string;
                    if (value == null)
                    {
                        value = key.GetValue("") as string;
                    }
                    if (value == null)
                        return new LogContext(context, defaultLoggers);
                    return new LogContext(context, ParseLoggers(value));
                }                
            }
        }
    }
}
