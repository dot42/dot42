using Dot42.AdbLib;

namespace Dot42.DeviceLib.UI
{
    partial class LogCatControl
    {
        /// <summary>
        /// Filtering interface
        /// </summary>
        internal interface IColumnFilter
        {
            bool IsVisible(LogEntry entry);
            string Title { get; }
        }

        /// <summary>
        /// Filter on log level.
        /// </summary>
        internal class LogLevelFilter : IColumnFilter
        {
            private readonly LogCatControl control;

            /// <summary>
            /// Default ctor
            /// </summary>
            public LogLevelFilter(LogCatControl control)
            {
                this.control = control;
            }

            public bool IsVisible(LogEntry entry)
            {
                switch (entry.Level)
                {
                    case LogLevel.Verbose:
                        return control.miVerbose.Checked;
                    case LogLevel.Debug:
                        return control.miDebug.Checked;
                    case LogLevel.Info:
                        return control.miInfo.Checked;
                    case LogLevel.Warning:
                        return control.miWarning.Checked;
                    case LogLevel.Error:
                        return control.miError.Checked;
                    case LogLevel.Assert:
                        return control.miAssert.Checked;
                    default:
                        return true;
                }
            }

            public string Title { get { return "Log level"; } }
        }

        internal class PidFilter : IColumnFilter
        {
            private readonly int pid;

            /// <summary>
            /// Default ctor
            /// </summary>
            public PidFilter(string pid)
            {
                if (!int.TryParse(pid, out this.pid))
                    this.pid = -1;
            }

            public bool IsVisible(LogEntry entry)
            {
                return entry.Pid == pid;
            }

            public string Title { get { return string.Format("PID = {0}", pid); } }
        }

        internal class TidFilter : IColumnFilter
        {
            private readonly int tid;

            /// <summary>
            /// Default ctor
            /// </summary>
            public TidFilter(string tid)
            {
                if (!int.TryParse(tid, out this.tid))
                    this.tid = -1;
            }

            public bool IsVisible(LogEntry entry)
            {
                return entry.Tid == tid;
            }

            public string Title { get { return string.Format("TID = {0}", tid); } }
        }

        internal class SourceFilter : IColumnFilter
        {
            private readonly string source;

            /// <summary>
            /// Default ctor
            /// </summary>
            public SourceFilter(string source)
            {
                this.source = source;
            }

            public bool IsVisible(LogEntry entry)
            {
                return entry.Tag == source;
            }

            public string Title { get { return string.Format("Source = {0}", source); } }
        }
    }
}
